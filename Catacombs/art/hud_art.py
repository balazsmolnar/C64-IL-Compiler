#!/usr/bin/env python3
"""
Art for the status panel (Hud.cs): cell rows 21-24 (y 168..199), multicolour
like the viewport above it -- 160x32 double-wide pixels, black ($D021) plus
three colours per 8x8 cell.

    python Catacombs/art/hud_art.py

writes
    art/src/hud_panel.png       the empty panel as the C64 shows it (320 wide)
    art/src/hud_mock.png        the panel in a few game states, x4, for review
    art/src/hud_backdrop.png    stand-in viewport for the scene builds only
    art/build/hud_panel.img     the panel for Screen.DrawImage
    art/build/hud_backdrop.img
    HudArt.cs                   the tiles Hud.cs copies into the bitmap
                                (Screen.DrawTiles) and the minimap's cells

Everything is authored the way the machine stores it: each double-wide pixel
is a slot -- '.' black, '1' '2' '3' the cell's three colours (video matrix
high nibble, low nibble, colour RAM) -- and each cell names its three
colours. Tiles copied in later only change pixels, so a cell's colours are
fixed by the panel picture and every tile drawn there is made for them.
"""
import os
import sys

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import c64img   # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
P = c64img.PALETTE

BLACK, WHITE, RED, CYAN, VIOLET, GREEN, BLUE, YELLOW = range(8)
ORANGE, BROWN, LRED, GREY1, GREY2, LGREEN, LBLUE, GREY3 = range(8, 16)

W, H = 160, 32            # double-wide pixels
COLS, ROWS = 40, 4

# ---------------------------------------------------------------------------
# Layout (cell columns). Compartments between skull-capped pillars.
# ---------------------------------------------------------------------------
PILLARS = (0, 11, 17, 22, 28, 39)
HEART_COL = 1             # 5 hearts, 2 cells each: 1-10
CHEST_COL = 12            # treasure: chest 12-13, "n/3" 14-16
COUNT_COL = 14
WEAPON_COL = 18           # weapon: icon 18-19, power 20-21
POWER_COL = 20
SKULL_COL = 23            # monsters slain: skull 23-24, "n/4" 25-27
KILLS_COL = 25
MAPBOX_COL = 29           # minimap: carved stone block 29-38 ...
MAP_COL = 31              # ... with the map window in 31-36

STONE = (GREY1, GREY2, GREY3)
HEART_PAL = (BROWN, RED, LRED)          # socket/rim, body, highlight
CHEST_PAL = (BROWN, ORANGE, YELLOW)     # dark wood, wood, gold
DIGIT_PAL = (GREY1, GREY3, WHITE)       # shadow, lower half, upper half
WEAPON_PAL = (GREY1, GREY3, BROWN)      # dark edge, steel, wood
MAP_PAL = (GREY1, GREY2, YELLOW)        # frame, explored room, you are here


class Panel:
    def __init__(self):
        self.px = [[0] * W for _ in range(H)]
        self.pal = [[STONE] * COLS for _ in range(ROWS)]

    def copy(self):
        p = Panel()
        p.px = [r[:] for r in self.px]
        p.pal = [r[:] for r in self.pal]
        return p

    def colour(self, c0, r0, c1, r1, pal):
        for r in range(r0, r1 + 1):
            for c in range(c0, c1 + 1):
                self.pal[r][c] = pal

    def put(self, x, y, rows):
        """Slots '.', '1'-'3'; ' ' leaves a pixel alone. Clipped."""
        for dy, line in enumerate(rows):
            for dx, ch in enumerate(line):
                if ch != ' ' and 0 <= x + dx < W and 0 <= y + dy < H:
                    self.px[y + dy][x + dx] = 0 if ch == '.' else int(ch)

    def fill(self, x0, y0, x1, y1, v):
        for y in range(y0, y1 + 1):
            for x in range(x0, x1 + 1):
                self.px[y][x] = v

    def cell_bytes(self, c, r):
        out = []
        for y in range(8):
            b = 0
            for x in range(4):
                b = (b << 2) | self.px[r * 8 + y][c * 4 + x]
            out.append(b)
        return bytes(out)

    def image(self, scale=1):
        img = Image.new("RGB", (W * 2, H))
        pix = img.load()
        for y in range(H):
            for x in range(W):
                v = self.px[y][x]
                col = BLACK if v == 0 else self.pal[y // 8][x // 4][v - 1]
                pix[2 * x, y] = pix[2 * x + 1, y] = P[col]
        if scale != 1:
            img = img.resize((W * 2 * scale, H * scale), Image.NEAREST)
        return img


def pad(rows, width=8, height=16, top=0):
    """Places a picture in a width x height tile, `top` lines down."""
    out = ["." * width] * top + [r.ljust(width, ".") for r in rows]
    return out + ["." * width] * (height - len(out))


# ---------------------------------------------------------------------------
# Tiles: pictures 16 lines tall (cell rows 22-23), stored top cell row first.
# ---------------------------------------------------------------------------
HEART_FULL = pad([         # 6 wide, centred on the boundary between its two cells
    ".11..11.",
    ".132221.",
    ".132221.",
    ".122221.",
    ".122221.",
    ".122221.",
    "..1221..",
    "..1221..",
    "...11...",
    "...11...",
], top=3)
HEART_EMPTY = pad([        # a dark socket of the same size
    ".11..11.",
    ".111111.",
    ".111111.",
    ".111111.",
    ".111111.",
    ".111111.",
    "..1111..",
    "..1111..",
    "...11...",
    "...11...",
], top=3)

GLYPHS = {                 # 3x10, drawn in the upper colour, lower colour below the middle
    '0': [".#.", "#.#", "#.#", "#.#", "#.#", "#.#", "#.#", "#.#", "#.#", ".#."],
    '1': [".#.", "##.", ".#.", ".#.", ".#.", ".#.", ".#.", ".#.", ".#.", "###"],
    '2': [".#.", "#.#", "..#", "..#", ".#.", ".#.", "#..", "#..", "#..", "###"],
    '3': ["##.", "..#", "..#", "..#", ".#.", "..#", "..#", "..#", "..#", "##."],
    '4': ["#.#", "#.#", "#.#", "#.#", "###", "..#", "..#", "..#", "..#", "..#"],
    '5': ["###", "#..", "#..", "#..", "##.", "..#", "..#", "..#", "..#", "##."],
    '6': [".##", "#..", "#..", "#..", "##.", "#.#", "#.#", "#.#", "#.#", ".#."],
    '7': ["###", "..#", "..#", "..#", ".#.", ".#.", ".#.", "#..", "#..", "#.."],
    '8': [".#.", "#.#", "#.#", "#.#", ".#.", "#.#", "#.#", "#.#", "#.#", ".#."],
    '9': [".#.", "#.#", "#.#", "#.#", "#.#", ".##", "..#", "..#", "..#", "##."],
    '/': ["..#", "..#", "..#", ".#.", ".#.", ".#.", ".#.", "#..", "#..", "#.."],
    ' ': ["..."] * 10,
}
DIGIT_ORDER = "0123456789/ "


def digit_tile(ch):
    """4x16: the glyph in white from line 3."""
    return pad([line.replace("#", "3") for line in GLYPHS[ch]], width=4, top=3)


CHEST = pad([              # 7 wide + a gap before the digits
    ".33333.",
    "3222223",
    "2222222",
    "1113111",
    "2233322",
    "2231322",
    "2223222",
    "2222222",
    "2222222",
    "3111113",
], top=4)

SWORD = pad([              # point up: blade, crossguard, grip, pommel
    "...2....",
    "...21...",
    "...21...",
    "...21...",
    "...21...",
    "...21...",
    "...21...",
    "...21...",
    "...21...",
    ".222221.",
    "..1111..",
    "...33...",
    "...33...",
    "...31...",
    "..2221..",
    "...11...",
], top=0)
AXE = pad([                # a single bit on a wooden haft
    "....3...",
    ".1223...",
    "12223...",
    "22223...",
    "22223...",
    "12223...",
    ".1223...",
    "....3...",
    "....3...",
    "....3...",
    "....3...",
    "....3...",
    "....3...",
    "....3...",
    "....1...",
], top=1)
FIST = pad([               # unarmed: a mailed fist
    ".22222..",
    "2121212.",
    "2121212.",
    "2121212.",
    "2222222.",
    "2111122.",
    "2222212.",
    ".222222.",
    ".222222.",
    ".111111.",
    ".333333.",
    ".333333.",
], top=2)
WEAPONS = (FIST, SWORD, AXE)

SKULL_ICON = pad([          # monsters slain
    "..3332..",
    ".333332.",
    "33333322",
    "33333322",
    "3..32..2",
    "3..32..1",
    "3333.321",
    ".333221.",
    "..2.2...",
    "..2221..",
    "..2.2...",
], top=3)

SKULL_CAP = [              # tops every pillar, with a black outline
    "..3332..",
    ".333322.",
    ".3.32.2.",
    ".3.32.1.",
    ".333221.",
    "..3.2...",
    "..3221..",
    "........",
]


# ---------------------------------------------------------------------------
# The frame
# ---------------------------------------------------------------------------
def draw_frame(p):
    # Cornice, lines 1-6 (line 0 and 7 black): a lit top edge, stone in
    # blocks with offset joints, a dark lower edge, a row of dentils.
    for x in range(W):
        p.px[1][x] = 3
        for y in (2, 3):
            j = (x + (5 if y == 3 else 0)) % 10
            p.px[y][x] = 1 if j == 9 else 3 if j == 0 else 2
        p.px[4][x] = 1
        p.px[5][x] = 2 if x % 2 == 0 else 0
        p.px[6][x] = 1 if x % 2 == 0 else 0
    # Sill, lines 28-31: lit top edge, stone, dark bottom.
    for x in range(W):
        p.px[28][x] = 3
        j = x % 12
        p.px[29][x] = 1 if j == 11 else 2
        p.px[30][x] = 1 if j == 11 else 2
        p.px[31][x] = 1
    for c in PILLARS:
        x = c * 4
        for y in range(8, 28):          # shaft: lit left, dark right, black outline
            p.put(x, y, ["3221" if c != 39 else "3221"])
            p.px[y][x + 3] = 0
        p.put(x, 8, ["333."])            # necking under the skull
        p.put(x, 15, ["111.", "333."])   # a joint
        p.put(x, 25, ["3332", "2221"])   # plinth
        p.put(x, 27, ["1111"])
        p.put(x - 2, 0, SKULL_CAP)


def draw_static(p):
    p.put(CHEST_COL * 4, 8, CHEST)
    p.colour(CHEST_COL, 1, CHEST_COL + 1, 2, CHEST_PAL)
    p.put(SKULL_COL * 4, 8, SKULL_ICON)
    p.colour(SKULL_COL, 1, SKULL_COL + 1, 2, DIGIT_PAL)

    # The minimap's stone block: carved border, rivets, recessed window.
    x0, x1 = MAPBOX_COL * 4, (MAPBOX_COL + 10) * 4 - 1
    p.fill(x0, 8, x1, 27, 2)
    for y in range(8, 28):                   # masonry: two courses, offset joints
        for x in range(x0, x1 + 1):
            j = (x - x0 + (4 if y >= 18 else 0)) % 8
            if y in (17, 27) or j == 7:
                p.px[y][x] = 1
            elif y in (8, 18) or j == 0:
                p.px[y][x] = 3
    p.fill(x0, 27, x1, 27, 1)
    for rx in (x0 + 3, x1 - 4):
        for ry in (12, 21):
            p.put(rx, ry, ["32", "21"])   # rivets
    wx0, wx1 = MAP_COL * 4, (MAP_COL + 6) * 4 - 1
    p.fill(wx0, 8, wx1, 31, 0)
    # window: dark inset on the left, lit edge on the right (rooms start on its first line)
    p.fill(wx0, 8, wx0, 27, 1)
    p.fill(wx1, 8, wx1, 27, 2)
    p.fill(wx0, 28, wx1, 28, 2)           # its sill
    p.fill(wx0, 29, wx1, 30, 1)
    p.fill(wx0, 31, wx1, 31, 0)
    p.colour(MAP_COL, 1, MAP_COL + 5, 3, MAP_PAL)


def set_dynamic_colours(p):
    p.colour(HEART_COL, 1, HEART_COL + 9, 2, HEART_PAL)
    p.colour(COUNT_COL, 1, COUNT_COL + 2, 2, DIGIT_PAL)
    p.colour(WEAPON_COL, 1, WEAPON_COL + 1, 2, WEAPON_PAL)
    p.colour(POWER_COL, 1, POWER_COL + 1, 2, DIGIT_PAL)
    p.colour(KILLS_COL, 1, KILLS_COL + 2, 2, DIGIT_PAL)


def build_panel():
    p = Panel()
    draw_frame(p)
    draw_static(p)
    set_dynamic_colours(p)
    return p


# ---------------------------------------------------------------------------
# Tile data
# ---------------------------------------------------------------------------
def tile_bytes(rows):
    """rows: 16 lines of 4*n slots -> top n cells' bytes, then bottom n cells'."""
    n = len(rows[0]) // 4
    out = []
    for half in (0, 8):
        for c in range(n):
            for y in range(8):
                b = 0
                for ch in rows[half + y][c * 4:c * 4 + 4]:
                    b = (b << 2) | (0 if ch == '.' else int(ch))
                out.append(b)
    return out


# Tile numbers (Hud.cs): hearts 0-7 (full left/right, empty left/right),
# digits 8-31 ("0"-"9", "/", blank), weapons 32-43 (fist, sword, axe).
def tile_sets(panel):
    tiles = []
    for art in (HEART_FULL, HEART_EMPTY):
        tiles += tile_bytes([r[:4] for r in art])
        tiles += tile_bytes([r[4:] for r in art])
    for ch in DIGIT_ORDER:
        tiles += tile_bytes(digit_tile(ch))
    for art in WEAPONS:
        tiles += tile_bytes(art)
    # The minimap window's 6x3 cells as they start (nothing explored).
    mapcells = []
    for r in (1, 2, 3):
        for c in range(MAP_COL, MAP_COL + 6):
            mapcells += list(panel.cell_bytes(c, r))
    # hud_MapRow[y]: offset in hud_Map of maze row y's first byte (rooms are
    # 2 lines tall from line 2y+6 of the panel; the map's first cell row is
    # panel row 1).
    rows = []
    for y in range(12):
        line = max(8, min(26, 2 * y + 6))
        rows.append(((line >> 3) - 1) * 48 + (line & 7))
    return tiles, mapcells, rows


def write_cs(path, sets):
    names = ("hud_Tiles", "hud_Map", "hud_MapRow")
    lines = ["// Generated by art/hud_art.py -- do not edit. The status panel's tiles",
             "// (8 bytes a cell; a picture's top cell row, then its bottom one), the",
             "// minimap's cells and its row table.",
             "using C64Lib;",
             "",
             "[assembly: RawImage(Resource = \"hud_panel.img\", Label = \"HudPanel\")]",
             "[assembly: RawAssembly(Asm = @\""]
    total = 0
    for name, data in zip(names, sets):
        total += len(data)
        for i in range(0, len(data), 16):
            chunk = ",".join("$%02x" % b for b in data[i:i + 16])
            lines.append("%s .byte %s" % (name if i == 0 else " " * len(name), chunk))
    lines.append("\")]")
    with open(path, "w", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    return total


# ---------------------------------------------------------------------------
# Mock-ups: the panel as Hud.cs draws it in a given state.
# ---------------------------------------------------------------------------
MAZE = [
    "000000000000",
    "011101111110",
    "010101000010",
    "010111011110",
    "010001010010",
    "011101110110",
    "000100010100",
    "011111011110",
    "010001000010",
    "011101111110",
    "000111000010",
    "000000000000",
]


def mock(panel, hp, treasure, weapon, power, kills, seen, here, bright=True):
    p = panel.copy()
    halves = (hp + 9) // 10
    for h in range(10):
        art = HEART_FULL if h < halves else HEART_EMPTY
        p.put((HEART_COL + h) * 4, 8, [r[4:] if h & 1 else r[:4] for r in art])
    for i, ch in enumerate("%d/3" % treasure):
        p.put((COUNT_COL + i) * 4, 8, digit_tile(ch))
    p.put(WEAPON_COL * 4, 8, WEAPONS[weapon])
    for i, ch in enumerate("%2d" % power):
        p.put((POWER_COL + i) * 4, 8, digit_tile(ch))
    for i, ch in enumerate("%d/4" % kills):
        p.put((KILLS_COL + i) * 4, 8, digit_tile(ch))
    for (mx, my) in seen:
        if MAZE[my][mx] != '1':
            continue
        v = 3 if (mx, my) == here and bright else 2
        for dy in range(2):
            for dx in range(2):
                p.px[2 * my + 6 + dy][MAP_COL * 4 + 2 * mx + dx] = v
    return p


def flood(start, steps):
    seen, frontier = {start}, [start]
    while frontier and len(seen) < steps:
        x, y = frontier.pop(0)
        for dx, dy in ((1, 0), (0, 1), (-1, 0), (0, -1)):
            n = (x + dx, y + dy)
            if MAZE[n[1]][n[0]] == '1' and n not in seen:
                seen.add(n)
                frontier.append(n)
    return seen


BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def backdrop():
    """A stand-in viewport for the panel's scene builds (never in the game):
    160x168 multicolour pixels of dark, cold stone -- a brick wall over a
    flagstone floor, lit faintly from the middle."""
    w, h = 160, 168
    img = Image.new("RGB", (w, h), P[0])
    px = img.load()
    ramp = [BLACK, BLUE, GREY1, GREY2]
    gain = 1.35

    def pick(level, x, y):
        f = max(0.0, min(2.999, level * gain * 3))
        i = int(f)
        return ramp[i + (1 if (f - i) * 16 > BAYER[y & 3][x & 3] else 0)]

    for y in range(h):
        for x in range(w):
            glow = max(0.0, 1.0 - (((x - 80) * 2) ** 2 + (y - 70) ** 2) ** 0.5 / 190.0)
            if y < 120:
                row = y // 12
                bx = (x + (10 if row & 1 else 0)) % 20
                by = y % 12
                mortar = by == 11 or bx == 19
                edge = by == 0 or bx == 0
                level = glow * (0.12 if mortar else 0.62 if edge else 0.46)
            else:
                depth = (y - 120) / 48.0
                fx = (x - 80) / (0.4 + depth)
                line = int(fx + 400) % 30 == 0 or (y - 120) in (0, 6, 15, 28, 44)
                level = glow * (0.10 if line else 0.30 + 0.25 * depth)
            px[x, y] = P[pick(level, x, y)]
    return img


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    panel = build_panel()
    panel.image().save(os.path.join(SRC, "hud_panel.png"))

    image = c64img.Image(c64img.MODE_MC, COLS, ROWS, 0)
    for r in range(ROWS):
        for c in range(COLS):
            c1, c2, c3 = panel.pal[r][c]
            image.cells[r][c] = (panel.cell_bytes(c, r), (c1 << 4) | c2, c3)
    planes = c64img.PLANE_BITMAP | c64img.PLANE_MATRIX | c64img.PLANE_COLOR
    payload, sizes = c64img.encode_planes(image, planes)
    c64img.verify(image, payload)
    c64img.write_file(os.path.join(OUT, "hud_panel.img"), image, payload)

    sets = tile_sets(panel)
    data = write_cs(os.path.join(HERE, "..", "HudArt.cs"), sets)
    print("hud_panel.img: bitmap %d + matrix %d + colour %d = %d bytes; tiles+map %d bytes; assets %d bytes"
          % (sizes[c64img.PLANE_BITMAP], sizes[c64img.PLANE_MATRIX], sizes[c64img.PLANE_COLOR],
             len(payload), data, len(payload) + data))

    backdrop().save(os.path.join(SRC, "hud_backdrop.png"))
    c64img.convert(os.path.join(SRC, "hud_backdrop.png"), os.path.join(OUT, "hud_backdrop.img"), quiet=True)

    states = [
        mock(panel, 100, 0, 0, 6, 0, {(1, 1)}, (1, 1)),
        mock(panel, 55, 2, 1, 12, 1, flood((1, 1), 30), (5, 5)),
        mock(panel, 15, 3, 2, 20, 3, flood((1, 1), 52), (9, 9), bright=False),
    ]
    sheet = Image.new("RGB", (W * 2 * 4, (H * 4 + 16) * len(states)), (40, 40, 60))
    for i, p in enumerate(states):
        sheet.paste(p.image(4), (0, i * (H * 4 + 16)))
    sheet.save(os.path.join(SRC, "hud_mock.png"))


if __name__ == "__main__":
    main()
