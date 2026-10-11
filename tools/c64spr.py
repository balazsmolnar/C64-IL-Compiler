#!/usr/bin/env python3
"""
c64spr.py -- turns a PNG sprite sheet into C64 sprite frames (.spr) for
C64Lib's [assembly: RawSprites(...)].

    python tools/c64spr.py convert coin.png coin.spr
    python tools/c64spr.py convert hero.png hero.spr --mc --mc1 0 --mc2 6
    python tools/c64spr.py info hero.spr

The sheet is a grid of frames, read left to right, top to bottom:
    hi-res        24x21 pixels per frame, one colour per frame
    multicolour   12x21 double-wide pixels per frame (--mc). The PNG frame is
                  12 wide (--px 1) or 24 wide with every pixel doubled
                  (--px 2). Up to three colours per frame: the frame's own
                  colour plus the two colours shared by ALL multicolour
                  sprites on screen (--mc1 = $D025, --mc2 = $D026).
Transparent = alpha below 128 (or the colour given with --transparent).
Colours are matched to the nearest C64 colour (see c64img.PALETTE).

The .spr file is 64 bytes per frame: 63 bytes of sprite data, then one byte
holding the frame's own colour in its low nibble (bit 7 set = multicolour),
which Sprite.Color can be set from; the tool also prints the colours.
"""

import argparse
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import c64img  # noqa: E402

FRAME_H = 21


def _load(src, multicolor, px, transparent):
    from PIL import Image as PilImage
    img = PilImage.open(src).convert("RGBA")
    w, h = img.size
    if multicolor:
        if px == 0:
            px = 2 if w % 24 == 0 and w % 12 == 0 and _pairs_identical(img) else 1
        fw = 12 * px
    else:
        px = 1
        fw = 24
    if w % fw or h % FRAME_H:
        raise ValueError("%s is %dx%d: must be a grid of %dx%d frames" % (src, w, h, fw, FRAME_H))
    data = img.load()
    frames = []
    for fy in range(h // FRAME_H):
        for fx in range(w // fw):
            rows = []
            for y in range(FRAME_H):
                row = []
                for x in range(fw // px):
                    r, g, b, a = data[fx * fw + x * px, fy * FRAME_H + y]
                    v = None if a < 128 else c64img.nearest_color((r, g, b))
                    if v == transparent:
                        v = None
                    row.append(v)
                rows.append(row)
            frames.append(rows)
    return frames


def _pairs_identical(img):
    data = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(0, w - 1, 2):
            if data[x, y] != data[x + 1, y]:
                return False
    return True


def convert(src, dst, multicolor=False, px=0, mc1=None, mc2=None, transparent=None,
            count=None, preview=None, quiet=False):
    """Converts a sheet. Returns the list of per-frame colours."""
    frames = _load(src, multicolor, px, transparent)
    if count is not None:
        if not 0 < count <= len(frames):
            raise ValueError("--count %d: the sheet has %d frames" % (count, len(frames)))
        frames = frames[:count]
    notes = []

    if multicolor and (mc1 is None or mc2 is None):
        # The shared colours default to the ones used by the most frames.
        usage = {}
        for rows in frames:
            for col in {v for row in rows for v in row if v is not None}:
                usage[col] = usage.get(col, 0) + 1
        ranked = sorted(usage, key=lambda k: (-usage[k], k))
        picks = [c for c in ranked if c not in (mc1, mc2)]
        if mc1 is None:
            mc1 = picks.pop(0) if picks else 0
        if mc2 is None:
            mc2 = picks.pop(0) if picks else 0

    out = bytearray()
    colours = []
    for index, rows in enumerate(frames):
        counts = {}
        for row in rows:
            for v in row:
                if v is not None:
                    counts[v] = counts.get(v, 0) + 1
        own_candidates = {c: n for c, n in counts.items() if not (multicolor and c in (mc1, mc2))}
        own = max(own_candidates, key=lambda k: own_candidates[k]) if own_candidates else None
        if len(own_candidates) > 1:
            notes.append("frame %d: too many colours (%s); pixels remapped to %s"
                         % (index, c64img._names(sorted(own_candidates)), c64img.COLOR_NAMES[own]))
        avail = [own] if own is not None else []
        if multicolor:
            avail += [mc1, mc2]
        data = bytearray(64)
        for y, row in enumerate(rows):
            bits = 0
            for v in row:
                if v is not None and v not in avail:
                    v = min(avail, key=lambda a: c64img.COLOR_DIST[v][a])
                if multicolor:
                    # 00 transparent, 01 = $D025, 10 = the sprite's own colour, 11 = $D026
                    code = 0 if v is None else (1 if v == mc1 else 3 if v == mc2 else 2)
                    bits = (bits << 2) | code
                else:
                    bits = (bits << 1) | (0 if v is None else 1)
            data[y * 3] = (bits >> 16) & 0xFF
            data[y * 3 + 1] = (bits >> 8) & 0xFF
            data[y * 3 + 2] = bits & 0xFF
        colour = own if own is not None else 1
        data[63] = colour | (0x80 if multicolor else 0)
        colours.append(colour)
        out += data

    with open(dst, "wb") as f:
        f.write(out)
    if preview:
        render_preview(out, multicolor, mc1, mc2, preview)
    if not quiet:
        print("%s: %d %s frame(s) -> %s (%d bytes)"
              % (src, len(frames), "multicolour" if multicolor else "hi-res", dst, len(out)))
        if multicolor:
            print("  shared colours: $D025 (CommonColor1) = %d %s, $D026 (CommonColor2) = %d %s"
                  % (mc1, c64img.COLOR_NAMES[mc1], mc2, c64img.COLOR_NAMES[mc2]))
        print("  frame colours: " + " ".join("%d:%s" % (i, c64img.COLOR_NAMES[c]) for i, c in enumerate(colours)))
        for note in notes:
            print("  COMPROMISED " + note)
    return colours


def render_preview(data, multicolor, mc1, mc2, path, scale=3):
    from PIL import Image as PilImage
    n = len(data) // 64
    out = PilImage.new("RGB", (n * 25, FRAME_H), (40, 0, 40))
    pix = out.load()
    for f in range(n):
        frame = data[f * 64:f * 64 + 64]
        own = frame[63] & 15
        for y in range(FRAME_H):
            bits = (frame[y * 3] << 16) | (frame[y * 3 + 1] << 8) | frame[y * 3 + 2]
            for x in range(24):
                if multicolor:
                    code = (bits >> (22 - 2 * (x // 2))) & 3
                    idx = (None, mc1, own, mc2)[code]
                else:
                    idx = own if (bits >> (23 - x)) & 1 else None
                if idx is not None:
                    pix[f * 25 + x, y] = c64img.PALETTE[idx]
    out = out.resize((out.width * scale, out.height * scale), PilImage.NEAREST)
    out.save(path)


def info(path):
    with open(path, "rb") as f:
        data = f.read()
    if not data or len(data) % 64:
        raise ValueError("%s is %d bytes: not a multiple of 64" % (path, len(data)))
    n = len(data) // 64
    print("%s: %d frame(s)" % (path, n))
    for i in range(n):
        b = data[i * 64 + 63]
        print("  %2d: %s, colour %d %s" % (i, "multicolour" if b & 0x80 else "hi-res", b & 15, c64img.COLOR_NAMES[b & 15]))


def main(argv=None):
    parser = argparse.ArgumentParser(description="PNG sprite sheet -> C64 sprite frames (.spr)")
    sub = parser.add_subparsers(dest="command", required=True)
    c = sub.add_parser("convert")
    c.add_argument("src")
    c.add_argument("dst")
    c.add_argument("--mc", action="store_true", help="multicolour sprites (12x21 double-wide pixels)")
    c.add_argument("--px", type=int, choices=[1, 2], default=0,
                   help="multicolour: PNG pixels per sprite pixel (default: 2 if every pixel pair is identical, else 1)")
    c.add_argument("--mc1", type=int, help="shared multicolour 1 ($D025); default: the most used colour")
    c.add_argument("--mc2", type=int, help="shared multicolour 2 ($D026); default: the second most used colour")
    c.add_argument("--transparent", type=int, help="also treat this C64 colour as transparent (PNGs without alpha)")
    c.add_argument("--count", type=int, help="keep only the first N frames of the sheet")
    c.add_argument("--preview", help="write a PNG showing the converted frames")
    c.add_argument("--quiet", action="store_true")
    i = sub.add_parser("info")
    i.add_argument("path")
    args = parser.parse_args(argv)
    try:
        if args.command == "convert":
            convert(args.src, args.dst, args.mc, args.px, args.mc1, args.mc2, args.transparent,
                    args.count, args.preview, args.quiet)
        else:
            info(args.path)
    except (ValueError, OSError) as e:
        print("c64spr: error: %s" % e, file=sys.stderr)
        return 2
    return 0


if __name__ == "__main__":
    sys.exit(main())
