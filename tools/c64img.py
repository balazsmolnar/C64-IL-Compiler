#!/usr/bin/env python3
"""
c64img.py -- turns an ordinary PNG into a compressed C64 bitmap image (.img)
for C64Lib's [assembly: RawImage(...)] + C64.Screen.DrawImage(...).

    python tools/c64img.py convert room.png room.img [--preview room.c64.png]
    python tools/c64img.py info room.img
    python tools/c64img.py selftest

An image is a rectangle of 8x8 cells. Each cell carries up to three things
("planes"):
    b  bitmap      8 bytes
    m  matrix      1 byte   (two colours: high nibble / low nibble)
    c  colour RAM  1 byte   (one colour; multicolour only)
Cells can be left out ("transparent"): DrawImage does not touch them, so an
image can be an irregular overlay (a door, a prop) drawn over a backdrop.

Multicolour (the default): 160x200 double-wide pixels. Per cell: the screen
background colour (--bg, the game's $D021, default 0 = black) plus three
freely chosen colours. Hi-res (--mode hires): 320x200, two colours per cell.

The PNG may use any RGB values; every pixel is mapped to the nearest of the
16 C64 colours (PALETTE below -- draw with exactly these and the mapping is
exact). Where a cell uses more colours than the mode allows, the best
subset is kept, the other pixels are remapped to the nearest kept colour,
and the cell is listed in the report ("compromised"). --strict makes that an
error.

See Catacombs/TECH.md for the full reference.
"""

import argparse
import itertools
import sys

# The 16 C64 colours ("Colodore" palette), index = C64 colour number =
# C64Lib.Colors value.
PALETTE = [
    (0x00, 0x00, 0x00),  # 0 black
    (0xFF, 0xFF, 0xFF),  # 1 white
    (0x81, 0x33, 0x38),  # 2 red
    (0x75, 0xCE, 0xC8),  # 3 cyan
    (0x8E, 0x3C, 0x97),  # 4 violet (purple)
    (0x56, 0xAC, 0x4D),  # 5 green
    (0x2E, 0x2C, 0x9B),  # 6 blue
    (0xED, 0xF1, 0x71),  # 7 yellow
    (0x8E, 0x50, 0x29),  # 8 orange
    (0x55, 0x38, 0x00),  # 9 brown
    (0xC4, 0x6C, 0x71),  # 10 light red
    (0x4A, 0x4A, 0x4A),  # 11 dark grey   (Colors.Grey1)
    (0x7B, 0x7B, 0x7B),  # 12 mid grey    (Colors.Grey2)
    (0xA9, 0xFF, 0x9F),  # 13 light green
    (0x70, 0x6D, 0xEB),  # 14 light blue
    (0xB2, 0xB2, 0xB2),  # 15 light grey  (Colors.Grey3)
]
COLOR_NAMES = ["black", "white", "red", "cyan", "violet", "green", "blue", "yellow",
               "orange", "brown", "lightred", "grey1", "grey2", "lightgreen", "lightblue", "grey3"]

MAGIC = b"C64I"
VERSION = 1
MODE_MC = 0
MODE_HIRES = 1
PLANE_BITMAP = 1
PLANE_MATRIX = 2
PLANE_COLOR = 4
SCREEN_COLS = 40
SCREEN_ROWS = 25


# ---------------------------------------------------------------------------
# Colour matching
# ---------------------------------------------------------------------------

def _srgb_to_lab(rgb):
    def lin(c):
        c = c / 255.0
        return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4
    r, g, b = (lin(c) for c in rgb)
    x = (0.4124 * r + 0.3576 * g + 0.1805 * b) / 0.95047
    y = (0.2126 * r + 0.7152 * g + 0.0722 * b)
    z = (0.0193 * r + 0.1192 * g + 0.9505 * b) / 1.08883

    def f(t):
        return t ** (1 / 3) if t > 0.008856 else 7.787 * t + 16 / 116
    fx, fy, fz = f(x), f(y), f(z)
    return (116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz))


_PALETTE_LAB = [_srgb_to_lab(c) for c in PALETTE]
# Perceptual distance (squared) between every pair of C64 colours.
COLOR_DIST = [[sum((a - b) ** 2 for a, b in zip(_PALETTE_LAB[i], _PALETTE_LAB[j]))
               for j in range(16)] for i in range(16)]
_nearest_cache = {}


def nearest_color(rgb):
    """Index of the C64 colour closest to an (r, g, b) triple."""
    rgb = tuple(rgb[:3])
    hit = _nearest_cache.get(rgb)
    if hit is None:
        if rgb in PALETTE:
            hit = PALETTE.index(rgb)
        else:
            lab = _srgb_to_lab(rgb)
            hit = min(range(16), key=lambda i: sum((a - b) ** 2 for a, b in zip(lab, _PALETTE_LAB[i])))
        _nearest_cache[rgb] = hit
    return hit


# ---------------------------------------------------------------------------
# The compressed stream ("LZ with skips")
# ---------------------------------------------------------------------------
# One stream per plane, decoded by asm/C64Graphics.asm's Image_Lz_Decode
# straight into its destination (bitmap / matrix / colour RAM): there is no
# scratch buffer, back-references read what was just written. The
# destination is linear in screen-cell order (cell k is at base + k * unit,
# unit = 8 for the bitmap and 1 for the two colour planes), so the encoder
# knows every byte's address relative to the image's top-left cell, and can
# say "skip n bytes" to jump over cells that are not part of the image (the
# rest of a screen row for a narrow image, or transparent cells).
#
#   $00            end of stream
#   $01-$3F        literal run: 1..63 bytes follow
#   $40-$5F        repeat match: length (t & $1F) + 2, same offset as the last match
#   $60-$7F nn     skip: advance the destination ((t & $1F) << 8 | nn) bytes
#   $80-$BF nn     match: length (t & $3F) + 2, offset nn + 1 (1..256)
#   $C0-$FF lo hi  match: length (t & $3F) + 3, offset hi:lo (1..65535)
#
# A match copies `length` bytes from (destination - offset), byte by byte,
# forwards (so offset < length repeats a pattern). The encoder only ever
# points a match at bytes this same stream wrote earlier.

LIT_MAX = 63
REP_MIN, REP_MAX = 2, 33
SHORT_MIN, SHORT_MAX = 2, 65
LONG_MIN, LONG_MAX = 3, 66
SKIP_MAX = 0x1FFF
MATCH_CAP = LONG_MAX


def _emit_skip(out, count):
    while count > 0:
        n = min(count, SKIP_MAX)
        out.append(0x60 | (n >> 8))
        out.append(n & 0xFF)
        count -= n


def lz_encode(mem, segments, max_candidates=192):
    """
    mem: bytes covering the plane's whole address range (addresses outside
    the segments are ignored). segments: ascending, non-overlapping
    (start, end) address ranges that the image actually writes.
    """
    size = len(mem)
    valid_end = [0] * (size + 1)          # for an address in a segment: that segment's end
    for s, e in segments:
        for a in range(s, e):
            valid_end[a] = e

    out = bytearray()
    table = {}                            # 2-byte key -> ascending list of addresses
    inserted = 0                          # addresses below this are in `table`
    rep = None                            # the decoder's current match offset
    cursor = 0
    fixed_offsets = (1, 2, 4, 8, 16, 40, 80, 320, 640)

    def insert_upto(limit):
        nonlocal inserted
        a = inserted
        while a < limit:
            if valid_end[a] > a + 1:
                table.setdefault((mem[a], mem[a + 1]), []).append(a)
            a += 1
        inserted = max(inserted, limit)

    def match_len(p, q, e):
        # p: write address, q: source address (< p), e: end of p's segment.
        limit = min(MATCH_CAP, e - p, valid_end[q] - q)
        if limit <= 0:
            return 0
        if mem[p:p + limit] == mem[q:q + limit]:
            return limit
        k = 0
        while mem[p + k] == mem[q + k]:
            k += 1
        return k

    for s, e in segments:
        _emit_skip(out, s - cursor)
        n = e - s
        # Best match per position: (short_len, short_off, long_len, long_off)
        best = []
        for p in range(s, e):
            insert_upto(p)                # everything before p, minus pairs straddling p
            sl = so = ll = lo = 0
            seen = set()
            cands = []
            for off in fixed_offsets:
                q = p - off
                if q >= 0 and valid_end[q] > q:
                    cands.append(q)
            if p + 1 < e:
                lst = table.get((mem[p], mem[p + 1]))
                if lst:
                    cands.extend(reversed(lst[-max_candidates:]))
            for q in cands:
                if q in seen:
                    continue
                seen.add(q)
                length = match_len(p, q, e)
                off = p - q
                if off <= 256:
                    if length > sl:
                        sl, so = length, off
                elif length > ll:
                    ll, lo = length, off
            if sl >= ll:                  # a long match must beat the short one to matter
                ll = lo = 0
            best.append((sl, so, ll, lo))

        # Forward dynamic programme over the segment; one arrival per
        # position, carrying the repeat offset of the path that reached it.
        INF = 1 << 30
        cost = [INF] * (n + 1)
        back = [None] * (n + 1)           # (previous index, kind, length, offset)
        reps = [None] * (n + 1)
        cost[0] = 0
        reps[0] = rep
        for i in range(n):
            c = cost[i]
            r = reps[i]
            p = s + i
            # literals
            top = min(LIT_MAX, n - i)
            for k in range(1, top + 1):
                nc = c + 1 + k
                if nc < cost[i + k]:
                    cost[i + k] = nc
                    back[i + k] = (i, 'L', k, 0)
                    reps[i + k] = r
            # repeat match
            if r is not None and p - r >= 0 and valid_end[p - r] > p - r:
                length = min(match_len(p, p - r, e), REP_MAX)
                for k in range(REP_MIN, length + 1):
                    nc = c + 1
                    if nc < cost[i + k]:
                        cost[i + k] = nc
                        back[i + k] = (i, 'R', k, r)
                        reps[i + k] = r
            sl, so, ll, lo = best[i]
            for k in range(SHORT_MIN, min(sl, SHORT_MAX) + 1):
                nc = c + 2
                if nc < cost[i + k]:
                    cost[i + k] = nc
                    back[i + k] = (i, 'S', k, so)
                    reps[i + k] = so
            for k in range(LONG_MIN, min(ll, LONG_MAX) + 1):
                nc = c + 3
                if nc < cost[i + k]:
                    cost[i + k] = nc
                    back[i + k] = (i, 'G', k, lo)
                    reps[i + k] = lo

        steps = []
        i = n
        while i > 0:
            steps.append(back[i])
            i = back[i][0]
        steps.reverse()
        for prev, kind, length, off in steps:
            p = s + prev
            if kind == 'L':
                out.append(length)
                out += mem[p:p + length]
            elif kind == 'R':
                out.append(0x40 | (length - REP_MIN))
            elif kind == 'S':
                out.append(0x80 | (length - SHORT_MIN))
                out.append(off - 1)
            else:
                out.append(0xC0 | (length - LONG_MIN))
                out.append(off & 0xFF)
                out.append(off >> 8)
        rep = reps[n]
        cursor = e
        insert_upto(e - 1)
    out.append(0)
    return bytes(out)


def lz_decode(stream, pos, mem):
    """Python model of the 6502 decoder. Writes into mem (a dict address ->
    byte, address 0 = the image's top-left cell); returns the position
    after the end token."""
    dst = 0
    off = None
    while True:
        t = stream[pos]
        pos += 1
        if t == 0:
            return pos
        if t < 0x40:
            for _ in range(t):
                mem[dst] = stream[pos]
                pos += 1
                dst += 1
        elif t < 0x60:
            for _ in range((t & 0x1F) + 2):
                mem[dst] = mem[dst - off]
                dst += 1
        elif t < 0x80:
            dst += ((t & 0x1F) << 8) | stream[pos]
            pos += 1
        else:
            if t < 0xC0:
                length = (t & 0x3F) + 2
                off = stream[pos] + 1
                pos += 1
            else:
                length = (t & 0x3F) + 3
                off = stream[pos] | (stream[pos + 1] << 8)
                pos += 2
            for _ in range(length):
                mem[dst] = mem[dst - off]
                dst += 1


# ---------------------------------------------------------------------------
# Cells -> planes -> file
# ---------------------------------------------------------------------------

class Image:
    """A converted image: per cell either None (transparent) or
    (bitmap: 8 bytes, matrix: int, colour: int)."""

    def __init__(self, mode, cols, rows, bg):
        self.mode = mode
        self.cols = cols
        self.rows = rows
        self.bg = bg
        self.cells = [[None] * cols for _ in range(rows)]

    def opaque_count(self):
        return sum(1 for row in self.cells for c in row if c is not None)


def _plane_layout(image, unit):
    """(mem, segments) for one plane, addresses relative to the top-left cell."""
    size = ((image.rows - 1) * SCREEN_COLS + image.cols) * unit
    return bytearray(size), size


def encode_planes(image, planes):
    """Returns the payload: a flags byte, then one LZ stream per plane."""
    present = planes
    if image.mode == MODE_HIRES:
        present &= ~PLANE_COLOR
    payload = bytearray([present])
    sizes = {}
    for plane, unit in ((PLANE_BITMAP, 8), (PLANE_MATRIX, 1), (PLANE_COLOR, 1)):
        if not present & plane:
            continue
        mem, size = _plane_layout(image, unit)
        segments = []
        for r in range(image.rows):
            for c in range(image.cols):
                cell = image.cells[r][c]
                if cell is None:
                    continue
                a = (r * SCREEN_COLS + c) * unit
                if plane == PLANE_BITMAP:
                    mem[a:a + 8] = cell[0]
                elif plane == PLANE_MATRIX:
                    mem[a] = cell[1]
                else:
                    mem[a] = cell[2] & 0x0F
                if segments and segments[-1][1] == a:
                    segments[-1][1] = a + unit
                else:
                    segments.append([a, a + unit])
        stream = lz_encode(bytes(mem), [tuple(s) for s in segments])
        sizes[plane] = len(stream)
        payload += stream
    return bytes(payload), sizes


def decode_payload(payload):
    """Model decode: returns {plane: {address: byte}} for the planes present."""
    flags = payload[0]
    pos = 1
    result = {}
    for plane in (PLANE_BITMAP, PLANE_MATRIX, PLANE_COLOR):
        if flags & plane:
            mem = {}
            pos = lz_decode(payload, pos, mem)
            result[plane] = mem
    if pos != len(payload):
        raise ValueError("trailing bytes after the last plane")
    return result


def verify(image, payload):
    """The stream must decode to exactly the image's opaque cells."""
    decoded = decode_payload(payload)
    for plane, unit in ((PLANE_BITMAP, 8), (PLANE_MATRIX, 1), (PLANE_COLOR, 1)):
        if plane not in decoded:
            continue
        expected = {}
        for r in range(image.rows):
            for c in range(image.cols):
                cell = image.cells[r][c]
                if cell is None:
                    continue
                a = (r * SCREEN_COLS + c) * unit
                if plane == PLANE_BITMAP:
                    for k in range(8):
                        expected[a + k] = cell[0][k]
                elif plane == PLANE_MATRIX:
                    expected[a] = cell[1]
                else:
                    expected[a] = cell[2] & 0x0F
        if decoded[plane] != expected:
            raise AssertionError("internal error: plane %d does not round-trip" % plane)


def write_file(path, image, payload):
    header = bytearray(MAGIC)
    header += bytes([VERSION, image.mode, image.cols, image.rows, image.bg & 0xFF,
                     len(payload) & 0xFF, len(payload) >> 8])
    with open(path, "wb") as f:
        f.write(header + payload)


def read_file(path):
    with open(path, "rb") as f:
        data = f.read()
    if data[:4] != MAGIC or data[4] != VERSION:
        raise ValueError("%s is not a version %d .img file" % (path, VERSION))
    mode, cols, rows, bg = data[5], data[6], data[7], data[8]
    length = data[9] | (data[10] << 8)
    payload = data[11:11 + length]
    if len(payload) != length:
        raise ValueError("%s is truncated" % path)
    return mode, cols, rows, bg, payload


# ---------------------------------------------------------------------------
# PNG -> cells
# ---------------------------------------------------------------------------

class Report:
    def __init__(self):
        self.compromised = []     # (col, row, colours wanted, colours kept)
        self.pair_mismatch = 0    # multicolour: PNG pixel pairs that were not identical
        self.partial = []         # cells that were partly transparent


def load_pixels(path, mode, px, crop_cells, report):
    """Returns (cols, rows, grid) where grid[y][x] is a colour index or None
    (transparent), at the mode's own resolution (160-wide for multicolour)."""
    from PIL import Image as PilImage
    img = PilImage.open(path).convert("RGBA")
    w, h = img.size
    data = img.load()
    if mode == MODE_MC:
        if px == 0:
            px = 1 if w <= 160 else 2
        cell_w = 4 * px
    else:
        px = 1
        cell_w = 8
    if w % cell_w or h % 8:
        raise ValueError("%s is %dx%d: width must be a multiple of %d and height a multiple of 8"
                         % (path, w, h, cell_w))
    cols, rows = w // cell_w, h // 8
    x0 = y0 = 0
    if crop_cells:
        cx, cy, cw, ch = crop_cells
        if cx < 0 or cy < 0 or cw <= 0 or ch <= 0 or cx + cw > cols or cy + ch > rows:
            raise ValueError("--crop %s is outside the %dx%d-cell picture" % (crop_cells, cols, rows))
        x0, y0, cols, rows = cx * cell_w, cy * 8, cw, ch
    if cols > SCREEN_COLS or rows > SCREEN_ROWS:
        raise ValueError("%dx%d cells is larger than the screen (40x25)" % (cols, rows))
    lw = cols * (4 if mode == MODE_MC else 8)
    grid = []
    for y in range(rows * 8):
        line = []
        for x in range(lw):
            sx = x0 + x * px
            r, g, b, a = data[sx, y0 + y]
            v = None if a < 128 else nearest_color((r, g, b))
            if px == 2:
                r2, g2, b2, a2 = data[sx + 1, y0 + y]
                v2 = None if a2 < 128 else nearest_color((r2, g2, b2))
                if v2 != v:
                    report.pair_mismatch += 1
            line.append(v)
        grid.append(line)
    return cols, rows, grid


def _best_subset(counts, fixed, keep):
    """Chooses `keep` colours out of counts' keys minimising the remapping
    error; `fixed` colours are always available. Returns (kept, mapping)."""
    colours = sorted(counts)
    best = None
    for subset in itertools.combinations(colours, keep):
        avail = list(subset) + list(fixed)
        err = 0
        for col, n in counts.items():
            if col in subset:
                continue
            err += n * min(COLOR_DIST[col][a] for a in avail)
        if best is None or err < best[0]:
            best = (err, subset)
    subset = best[1]
    avail = list(subset) + list(fixed)
    mapping = {col: (col if col in subset else min(avail, key=lambda a: COLOR_DIST[col][a]))
               for col in counts}
    return subset, mapping


def build_image(mode, cols, rows, grid, bg, base_grid, report):
    cw = 4 if mode == MODE_MC else 8
    image = Image(mode, cols, rows, bg)
    cell_pixels = {}

    # Pass 1: per cell, the (possibly compromised) pixel colours.
    for r in range(rows):
        for c in range(cols):
            px = [grid[r * 8 + y][c * cw + x] for y in range(8) for x in range(cw)]
            if all(p is None for p in px):
                continue
            if base_grid is not None:
                bpx = [base_grid[r * 8 + y][c * cw + x] for y in range(8) for x in range(cw)]
                if bpx == px:
                    continue
            if any(p is None for p in px):
                report.partial.append((c, r))
                fill = bg if mode == MODE_MC else 0
                px = [fill if p is None else p for p in px]
            counts = {}
            for p in px:
                if mode == MODE_MC and p == bg:
                    continue
                counts[p] = counts.get(p, 0) + 1
            limit = 3 if mode == MODE_MC else 2
            if len(counts) > limit:
                kept, mapping = _best_subset(counts, [bg] if mode == MODE_MC else [], limit)
                report.compromised.append((c, r, sorted(counts), sorted(kept)))
                px = [mapping.get(p, p) for p in px]
            cell_pixels[(r, c)] = px

    if mode == MODE_MC:
        _assign_mc(image, cell_pixels, bg)
    else:
        _assign_hires(image, cell_pixels)
    _check_cells(image, cell_pixels)
    return image


def cell_colours(image, cell):
    """The colour index of each pixel of an encoded cell, row by row (4
    pixels a row in multicolour, 8 in hi-res): what the VIC will display."""
    bitmap, matrix, colour = cell
    out = []
    for y in range(8):
        if image.mode == MODE_MC:
            table = (image.bg, matrix >> 4, matrix & 15, colour & 15)
            out += [table[(bitmap[y] >> (6 - 2 * x)) & 3] for x in range(4)]
        else:
            out += [(matrix >> 4) if (bitmap[y] >> (7 - x)) & 1 else (matrix & 15) for x in range(8)]
    return out


def _check_cells(image, cell_pixels):
    """Every encoded cell must display exactly the pixels it was made from."""
    for (r, c), px in cell_pixels.items():
        if cell_colours(image, image.cells[r][c]) != px:
            raise AssertionError("internal error: cell (%d,%d) does not encode its pixels" % (c, r))


def _assign_mc(image, cell_pixels, bg):
    # Which of the three slots (bit pair 01 = matrix high nibble, 10 = matrix
    # low nibble, 11 = colour RAM) a colour goes to is free per cell. Giving
    # every colour one preferred slot for the whole picture makes cells that
    # look alike encode alike (same bitmap bytes, long runs in the colour
    # planes), which is what the compressor feeds on.
    freq = {}
    together = {}
    for px in cell_pixels.values():
        cols = sorted(set(px) - {bg})
        for a in cols:
            freq[a] = freq.get(a, 0) + px.count(a)
        for a, b in itertools.combinations(cols, 2):
            together[(a, b)] = together.get((a, b), 0) + 1
            together[(b, a)] = together.get((b, a), 0) + 1
    preferred = {}
    for col in sorted(freq, key=lambda k: -freq[k]):
        clash = [0, 0, 0]
        for other, slot in preferred.items():
            clash[slot] += together.get((col, other), 0)
        preferred[col] = min(range(3), key=lambda s: (clash[s], s))

    last = [0, 0, 0]                       # the previous cell's slot colours
    for r in range(image.rows):
        for c in range(image.cols):
            px = cell_pixels.get((r, c))
            if px is None:
                continue
            cols = sorted(set(px) - {bg}, key=lambda k: -freq[k])
            slots = [None, None, None]
            waiting = []
            for col in cols:
                s = preferred[col]
                if slots[s] is None:
                    slots[s] = col
                else:
                    waiting.append(col)
            for col in waiting:
                free = [s for s in range(3) if slots[s] is None]
                s = next((s for s in free if last[s] == col), free[0])
                slots[s] = col
            used = list(slots)
            for s in range(3):             # unused slots: repeat the previous cell's value
                if slots[s] is None:
                    slots[s] = last[s]
            last = slots
            code = {bg: 0}
            for s in range(3):
                if used[s] is not None:
                    code[used[s]] = s + 1
            bitmap = bytearray(8)
            for y in range(8):
                b = 0
                for x in range(4):
                    b = (b << 2) | code[px[y * 4 + x]]
                bitmap[y] = b
            image.cells[r][c] = (bytes(bitmap), (slots[0] << 4) | slots[1], slots[2])


def _assign_hires(image, cell_pixels):
    # Bit 1 = matrix high nibble, bit 0 = low nibble. The picture's most
    # common colour is made the "0" colour wherever it appears, so empty
    # areas are zero bytes.
    freq = {}
    for px in cell_pixels.values():
        for p in px:
            freq[p] = freq.get(p, 0) + 1
    order = sorted(freq, key=lambda k: -freq[k])
    rank = {col: i for i, col in enumerate(order)}
    last = [0, 0]                          # previous cell's (foreground, background)
    for r in range(image.rows):
        for c in range(image.cols):
            px = cell_pixels.get((r, c))
            if px is None:
                continue
            cols = sorted(set(px), key=lambda k: rank[k])
            if len(cols) == 2:
                back, fore = cols[0], cols[1]
            elif cols[0] == last[0] and cols[0] != order[0]:
                fore, back = cols[0], last[1]
            else:
                back, fore = cols[0], last[0]
            if fore == back:               # single-colour cell whose spare slot repeats it
                fore = last[0]
            last = [fore, back]
            bitmap = bytearray(8)
            for y in range(8):
                b = 0
                for x in range(8):
                    b = (b << 1) | (1 if px[y * 8 + x] == fore and fore != back else 0)
                bitmap[y] = b
            image.cells[r][c] = (bytes(bitmap), (fore << 4) | back, 0)


def render_preview(image, path, scale=2, checker=True):
    """Draws what the C64 will show. Transparent cells: a dark checkerboard."""
    from PIL import Image as PilImage
    w, h = image.cols * 8, image.rows * 8
    out = PilImage.new("RGB", (w, h))
    pix = out.load()
    cw = 4 if image.mode == MODE_MC else 8
    for r in range(image.rows):
        for c in range(image.cols):
            cell = image.cells[r][c]
            colours = cell_colours(image, cell) if cell is not None else None
            for y in range(8):
                for x in range(8):
                    if cell is None:
                        on = ((x // 4) + (y // 4)) & 1
                        rgb = (40, 0, 40) if (on and checker) else (20, 0, 20)
                    else:
                        rgb = PALETTE[colours[y * cw + x * cw // 8]]
                    pix[c * 8 + x, r * 8 + y] = rgb
    if scale != 1:
        out = out.resize((w * scale, h * scale), PilImage.NEAREST)
    out.save(path)


def convert(src, dst, mode=MODE_MC, px=0, bg=0, crop=None, base=None, planes="bmc",
            preview=None, strict=False, quiet=False):
    """Converts one PNG. Returns (image, payload, report)."""
    report = Report()
    cols, rows, grid = load_pixels(src, mode, px, crop, report)
    base_grid = None
    if base:
        bcols, brows, base_grid = load_pixels(base, mode, px, crop, Report())
        if (bcols, brows) != (cols, rows):
            raise ValueError("--base picture is %dx%d cells, the picture is %dx%d" % (bcols, brows, cols, rows))
    image = build_image(mode, cols, rows, grid, bg, base_grid, report)
    if image.opaque_count() == 0:
        raise ValueError("%s: nothing to draw (every cell is transparent or equal to --base)" % src)
    mask = 0
    for ch in planes:
        mask |= {"b": PLANE_BITMAP, "m": PLANE_MATRIX, "c": PLANE_COLOR}[ch]
    payload, sizes = encode_planes(image, mask)
    verify(image, payload)
    if dst:
        write_file(dst, image, payload)
    if preview:
        render_preview(image, preview)
    if not quiet:
        print_report(src, image, payload, sizes, report)
    if strict and report.compromised:
        raise ValueError("%d cell(s) needed a colour compromise (--strict)" % len(report.compromised))
    return image, payload, report


def print_report(src, image, payload, sizes, report):
    n = image.opaque_count()
    total = image.cols * image.rows
    raw = {PLANE_BITMAP: n * 8, PLANE_MATRIX: n, PLANE_COLOR: n}
    names = {PLANE_BITMAP: "bitmap", PLANE_MATRIX: "matrix", PLANE_COLOR: "colour"}
    print("%s: %dx%d cells (%dx%d px), %s, %d of %d cells drawn"
          % (src, image.cols, image.rows, image.cols * 8, image.rows * 8,
             "multicolour bg=%d" % image.bg if image.mode == MODE_MC else "hi-res", n, total))
    raw_total = 0
    for plane in (PLANE_BITMAP, PLANE_MATRIX, PLANE_COLOR):
        if plane in sizes:
            raw_total += raw[plane]
            print("  %-7s %5d -> %5d bytes" % (names[plane], raw[plane], sizes[plane]))
    print("  total   %5d -> %5d bytes (%d%%)" % (raw_total, len(payload), round(100 * len(payload) / max(1, raw_total))))
    if report.pair_mismatch:
        print("  note: %d pixel pair(s) were not two identical pixels; the left one was used"
              % report.pair_mismatch)
    if report.partial:
        print("  note: %d cell(s) are partly transparent; their transparent pixels became the background colour: %s"
              % (len(report.partial), _cell_list(report.partial)))
    if report.compromised:
        print("  COMPROMISED: %d cell(s) had too many colours:" % len(report.compromised))
        for c, r, wanted, kept in report.compromised[:40]:
            print("    cell (%d,%d) px (%d,%d): wanted %s, kept %s"
                  % (c, r, c * 8, r * 8, _names(wanted), _names(kept)))
        if len(report.compromised) > 40:
            print("    ... and %d more" % (len(report.compromised) - 40))
    else:
        print("  no colour compromises")


def _names(cols):
    return "+".join(COLOR_NAMES[c] for c in cols)


def _cell_list(cells):
    text = " ".join("(%d,%d)" % c for c in cells[:12])
    return text + (" ..." if len(cells) > 12 else "")


def info(path):
    mode, cols, rows, bg, payload = read_file(path)
    decoded = decode_payload(payload)
    print("%s: %dx%d cells, %s, payload %d bytes, planes: %s"
          % (path, cols, rows, "multicolour bg=%d" % bg if mode == MODE_MC else "hi-res", len(payload),
             ", ".join("%s (%d bytes out)" % (n, len(decoded[p]))
                       for p, n in ((1, "bitmap"), (2, "matrix"), (4, "colour")) if p in decoded)))


def _random_image(rnd, trial, cols, rows):
    """A synthetic image for the tests: random, flat, dithered or patterned
    cells, with holes (transparent cells) in every third one."""
    image = Image(MODE_MC, cols, rows, 0)
    style = trial % 4
    for r in range(rows):
        for c in range(cols):
            if rnd.random() < (0.3 if trial % 3 == 0 else 0.0):
                continue
            if style == 0:
                bm = bytes(rnd.randrange(256) for _ in range(8))
            elif style == 1:
                bm = bytes([rnd.choice([0, 0x55, 0xAA, 0xFF])] * 8)
            elif style == 2:
                a, b = rnd.choice([(0x66, 0x99), (0x11, 0x44), (0, 0)])
                bm = bytes([a, b] * 4)
            else:
                bm = bytes((c * 7 + r * 13 + k * (trial % 5)) & 0xFF for k in range(8))
            image.cells[r][c] = (bm, rnd.choice([0x12, 0xBC, 0xF1]), rnd.randrange(16))
    if image.opaque_count() == 0:
        image.cells[0][0] = (bytes(8), 0, 0)
    return image


def selftest():
    import random
    rnd = random.Random(1234)
    count = 0
    for trial in range(300):
        image = _random_image(rnd, trial, rnd.randint(1, 40), rnd.randint(1, 6))
        payload, _ = encode_planes(image, rnd.choice([7, 7, 7, 1, 6, 3]))
        verify(image, payload)
        count += 1
    print("selftest OK (%d random images round-tripped)" % count)


def testvectors(path):
    """Writes test cases for the 6502 decoder (SimpleEmulator.Test's
    GraphicsImageTests): for each, where the image is drawn, its payload,
    and every byte each plane must end up holding (offsets from the plane's
    base address, i.e. including the draw position)."""
    import json
    import random
    rnd = random.Random(99)
    sizes = [(40, 21), (40, 25), (1, 1), (3, 2), (17, 9), (40, 3), (39, 25), (8, 8),
             (25, 12), (2, 25), (40, 21), (12, 5)]
    cases = []
    for trial, (cols, rows) in enumerate(sizes):
        image = _random_image(rnd, trial, cols, rows)
        planes = [7, 7, 7, 7, 1, 6, 7, 3, 7, 5, 7, 2][trial]
        payload, _ = encode_planes(image, planes)
        verify(image, payload)
        x = rnd.randint(0, SCREEN_COLS - cols)
        y = rnd.randint(0, SCREEN_ROWS - rows)
        origin = y * SCREEN_COLS + x
        decoded = decode_payload(payload)
        case = {"x": x, "y": y, "cols": cols, "rows": rows, "payload": list(payload)}
        for plane, name, unit in ((PLANE_BITMAP, "bitmap", 8), (PLANE_MATRIX, "matrix", 1), (PLANE_COLOR, "colour", 1)):
            mem = decoded.get(plane, {})
            case[name] = [[origin * unit + a, v] for a, v in sorted(mem.items())]
        cases.append(case)
    with open(path, "w") as f:
        json.dump(cases, f)
    print("wrote %d test cases to %s" % (len(cases), path))


def main(argv=None):
    parser = argparse.ArgumentParser(description="PNG -> compressed C64 bitmap image (.img)")
    sub = parser.add_subparsers(dest="command", required=True)
    c = sub.add_parser("convert", help="convert a PNG")
    c.add_argument("src")
    c.add_argument("dst")
    c.add_argument("--mode", choices=["mc", "hires"], default="mc")
    c.add_argument("--px", type=int, choices=[1, 2], default=0,
                   help="multicolour: PNG pixels per C64 pixel (2 = a 320-wide PNG whose pixels come in "
                        "identical pairs, 1 = a 160-wide PNG). Default: 1 if the PNG is at most 160 wide, else 2")
    c.add_argument("--bg", type=int, default=0, help="multicolour background colour, the program's $D021 (default 0)")
    c.add_argument("--crop", help="X,Y,W,H in cells: convert only this part of the PNG")
    c.add_argument("--base", help="a PNG of the same size: cells identical to it are left out (overlay/delta image)")
    c.add_argument("--planes", default="bmc", help="which planes to store: any of b (bitmap) m (matrix) c (colour RAM)")
    c.add_argument("--preview", help="write a PNG of what the C64 will display")
    c.add_argument("--strict", action="store_true", help="fail if any cell needed a colour compromise")
    c.add_argument("--quiet", action="store_true")
    i = sub.add_parser("info", help="describe an .img file")
    i.add_argument("path")
    sub.add_parser("selftest", help="round-trip random images through the compressor")
    t = sub.add_parser("testvectors", help="write test cases for the 6502 decoder's unit tests")
    t.add_argument("path")
    args = parser.parse_args(argv)
    try:
        if args.command == "convert":
            if not 0 <= args.bg <= 15:
                raise ValueError("--bg must be 0..15")
            if not args.planes or any(ch not in "bmc" for ch in args.planes):
                raise ValueError("--planes takes letters from b, m, c")
            crop = tuple(int(v) for v in args.crop.split(",")) if args.crop else None
            if crop and len(crop) != 4:
                raise ValueError("--crop takes X,Y,W,H")
            convert(args.src, args.dst, MODE_MC if args.mode == "mc" else MODE_HIRES, args.px, args.bg,
                    crop, args.base, args.planes, args.preview, args.strict, args.quiet)
        elif args.command == "info":
            info(args.path)
        elif args.command == "testvectors":
            testvectors(args.path)
        else:
            selftest()
    except (ValueError, OSError) as e:
        print("c64img: error: %s" % e, file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
