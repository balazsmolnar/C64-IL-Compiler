#!/usr/bin/env python3
"""
Art for the status panel (Hud.cs): cell rows 21-24, shown hi-res under a
raster split (320x32 pixels, two colours per 8x8 cell).

    python Catacombs/art/hud_art.py

writes
    art/src/hud_panel.png        the empty panel, as the C64 shows it
    art/src/hud_mock_*.png       the panel in a few game states (x4), for review
    art/build/hud_panel.img      the panel for Screen.DrawImage (hi-res)
    HudArt.cs                    the tiles Hud.cs copies into the bitmap
                                 (hearts, digits, weapon icons), as .byte data

Hi-res art is authored the way the machine stores it: every pixel is on or
off, and every cell names its two colours (on, off). The panel is built
directly as c64img cells, so which colour is the "on" one is ours to say --
the tiles Hud.cs copies in later rely on it.
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

W, H = 320, 32
COLS, ROWS = 40, 4


class Panel:
    def __init__(self):
        self.bit = [[0] * W for _ in range(H)]
        self.col = [[(GREY2, BLACK)] * COLS for _ in range(ROWS)]

    def copy(self):
        p = Panel()
        p.bit = [row[:] for row in self.bit]
        p.col = [row[:] for row in self.col]
        return p

    def colour(self, c0, r0, c1, r1, fg, bg=BLACK):
        for r in range(r0, r1 + 1):
            for c in range(c0, c1 + 1):
                self.col[r][c] = (fg, bg)

    def put(self, x, y, rows):
        """'#' sets, '.' clears, ' ' leaves a pixel alone."""
        for dy, line in enumerate(rows):
            for dx, ch in enumerate(line):
                if ch == '#':
                    self.bit[y + dy][x + dx] = 1
                elif ch == '.':
                    self.bit[y + dy][x + dx] = 0

    def tile(self, x, y, rows):
        """Same, for a list of 0/1 lists."""
        for dy, line in enumerate(rows):
            for dx, v in enumerate(line):
                self.bit[y + dy][x + dx] = v

    def cell_bytes(self, c, r):
        return bytes(sum(self.bit[r * 8 + y][c * 8 + x] << (7 - x) for x in range(8)) for y in range(8))

    def image(self, scale=1):
        img = Image.new("RGB", (W, H))
        px = img.load()
        for y in range(H):
            for x in range(W):
                fg, bg = self.col[y // 8][x // 8]
                px[x, y] = P[fg if self.bit[y][x] else bg]
        if scale != 1:
            img = img.resize((W * scale, H * scale), Image.NEAREST)
        return img


def rows_to_bits(rows):
    return [[1 if ch == '#' else 0 for ch in line] for line in rows]


def mirror(rows):
    return [line[::-1] for line in rows]


# ---------------------------------------------------------------------------
# Layout (cell columns)
# ---------------------------------------------------------------------------
PILLARS = (0, 11, 17, 22, 32, 39)
HEART_COL = 1          # 5 hearts, 2 cells each: columns 1-10
CHEST_COL = 12         # treasure icon: columns 12-13
COUNT_COL = 14         # "n/m": columns 14-16
WEAPON_COL = 18        # weapon icon: columns 18-19
POWER_COL = 20         # attack power, two digits: columns 20-21
CREST_COL = 23         # crest: columns 23-31
MAP_COL = 33           # minimap: columns 33-38, rows 22-24; 4x2 pixels a maze cell

# ---------------------------------------------------------------------------
# Tiles (16 pixels tall = cell rows 22 and 23)
# ---------------------------------------------------------------------------
HEART_FULL = [
    "................",
    "................",
    "...###....###...",
    "..#####..#####..",
    ".##.##########..",
    ".#.###########.#"[:15] + ".",
    ".#.############.",
    ".##.###..###.##.",
    "..############..",
    "...##########...",
    "....########....",
    ".....######.....",
    "......####......",
    ".......##.......",
    "................",
    "................",
]
HEART_EMPTY = [
    "................",
    "................",
    "...###....###...",
    "..#...#..#...#..",
    ".#.....##.....#.",
    ".#............#.",
    ".#............#.",
    ".#............#.",
    "..#..........#..",
    "...#........#...",
    "....#......#....",
    ".....#....#.....",
    "......#..#......",
    ".......##.......",
    "................",
    "................",
]

# 7x12, drawn like lancet windows: pointed tops and feet, two-pixel stems.
DIGITS = {
    '0': ["..###..", ".##.##.", "##...##", "##...##", "##...##", "##...##",
          "##...##", "##...##", "##...##", "##...##", ".##.##.", "..###.."],
    '1': ["...##..", "..###..", ".####..", "...##..", "...##..", "...##..",
          "...##..", "...##..", "...##..", "...##..", "...##..", ".######"],
    '2': ["..###..", ".##.##.", "##...##", ".....##", ".....##", "....##.",
          "...##..", "..##...", ".##....", "##.....", "##...##", "#######"],
    '3': ["..###..", ".##.##.", "##...##", ".....##", "....##.", "..###..",
          "....##.", ".....##", ".....##", "##...##", ".##.##.", "..###.."],
    '4': ["....##.", "...###.", "..####.", ".##.##.", "##..##.", "##..##.",
          "#######", "#######", "....##.", "....##.", "....##.", "...####"],
    '5': ["#######", "##...##", "##.....", "##.....", "#####..", "##..##.",
          ".....##", ".....##", ".....##", "##...##", ".##.##.", "..###.."],
    '6': ["...###.", "..##...", ".##....", "##.....", "#####..", "###.##.",
          "##...##", "##...##", "##...##", "##...##", ".##.##.", "..###.."],
    '7': ["#######", "##...##", ".....##", "....##.", "....##.", "...##..",
          "...##..", "..##...", "..##...", "..##...", "..##...", ".####.."],
    '8': ["..###..", ".##.##.", "##...##", "##...##", ".##.##.", "..###..",
          ".##.##.", "##...##", "##...##", "##...##", ".##.##.", "..###.."],
    '9': ["..###..", ".##.##.", "##...##", "##...##", "##...##", "##...##",
          ".##.###", "..#####", ".....##", "....##.", "...##..", ".###..."],
    '/': [".....##", ".....##", "....##.", "....##.", "...##..", "...##..",
          "..##...", "..##...", ".##....", ".##....", "##.....", "##....."],
}
DIGIT_ORDER = "0123456789/"


def digit_tile(ch):
    """8x16: the 7x12 glyph, one pixel in from the left, two down."""
    rows = ["........"] * 2 + ["." + line for line in DIGITS[ch]] + ["........"] * 2
    return rows


CHEST = [                # iron-strapped: the straps are the dark vertical lines
    "................",
    "................",
    "...##########...",
    "..#.########.#..",
    ".##.###..###.##.",
    ".##.###..###.##.",
    ".##.###..###.##.",
    "................",
    ".##.###..###.##.",
    ".##.###..###.##.",
    ".##.###..###.##.",
    ".##.###..###.##.",
    ".##.###..###.##.",
    ".##.###..###.##.",
    "................",
    "................",
]

FIST = [
    "................",
    "................",
    "...##.##.##.##..",
    "..############..",
    "..#..#..#..#.##.",
    "..#..#..#..#.##.",
    "..#..#..#..#.##.",
    "..############..",
    ".##.........#...",
    ".#.########.#...",
    ".##........##...",
    "..##########....",
    "...#......#.....",
    "...########.....",
    "...#.#..#.#.....",
    "................",
]
SWORD = [
    "................",
    ".............##.",
    "............###.",
    "...........###..",
    "..........###...",
    ".........###....",
    "........###.....",
    ".......###......",
    ".##...###.......",
    "..##.###........",
    "...####.........",
    "...###..........",
    "..#####.........",
    ".###..##........",
    "###....#........",
    "##..............",
]
AXE = [
    "................",
    ".......##.......",
    "..#....##....#..",
    ".###...##...###.",
    ".##.###..###.##.",
    ".######..######.",
    ".######..######.",
    ".##.###..###.##.",
    ".###...##...###.",
    "..#....##....#..",
    ".......##.......",
    ".......##.......",
    ".......##.......",
    ".......##.......",
    "......####......",
    "................",
]
WEAPONS = (FIST, SWORD, AXE)


# ---------------------------------------------------------------------------
# The frame
# ---------------------------------------------------------------------------
SKULL_CAP = [            # tops each pillar: y 1..7 of the row 21 cell
    ".######.",
    "########",
    "#..##..#",
    "#..##..#",
    "###..###",
    ".######.",
    ".#.##.#.",
]

BONE = [                 # 22 wide, lying flat: one each side of the skull
    "......................",
    ".##................##.",
    "####..............####",
    ".####################.",
    ".####################.",
    "####..............####",
    ".##................##.",
    "......................",
]
SKULL = [                # 14 wide
    "....######....",
    "..##########..",
    ".############.",
    ".##..####..##.",
    ".##..####..##.",
    "..####..####..",
    "...########...",
    "...#.#..#.#...",
]

LETTERS = {
    'C': ["..###..", ".##.##.", "##.....", "##.....", "##.....", "##.....", ".##.##.", "..###.."],
    'A': ["...#...", "..###..", ".##.##.", "##...##", "##...##", "#######", "##...##", "##...##"],
    'T': ["#######", "#..##.#", "...##..", "...##..", "...##..", "...##..", "...##..", "..####."],
    'O': ["..###..", ".##.##.", "##...##", "##...##", "##...##", "##...##", ".##.##.", "..###.."],
    'M': ["##...##", "###.###", "#######", "##.#.##", "##...##", "##...##", "##...##", "##...##"],
    'B': ["#####..", "##..##.", "##..##.", "#####..", "##..##.", "##...##", "##...##", "######."],
    'S': ["..####.", ".##..##", "##.....", ".####..", "...###.", ".....##", "##..##.", ".####.."],
}


def draw_frame(p):
    # Lintel, y 1..7: a cornice in 16-pixel blocks -- lit top edge, body, a
    # groove, a lower moulding and a row of dentils hanging under it.
    block = [
        ".##.###..###.##.",
        "#.#.#.#.#.#.#.#.",
        ".#.#.#.#.#.#.#..",
        "................",
        ".##.###..###.##.",
        "###.###.###.###.",
        ".#...#...#...#..",
    ]
    for x in range(0, W, 16):
        p.put(x, 1, block)
    # Sill, y 28..31.
    sill = [
        ".##.###..###.##.",
        "#.#.#.#.#.#.#.#.",
        ".#.#.#.#.#.#.#..",
        "...#.......#....",
    ]
    for x in range(0, W, 16):
        p.put(x, 28, sill)
    p.colour(0, 0, COLS - 1, 0, GREY2)
    p.colour(0, 3, COLS - 1, 3, GREY2)

    for c in PILLARS:
        x = c * 8
        p.put(x, 1, SKULL_CAP)
        p.colour(c, 0, c, 0, GREY3)
        for y in range(8, 24):                      # fluted shaft, lit from the left
            p.put(x, y, ["##.##.#."])
        p.put(x, 8, ["########", "#.#.#.#."])        # necking under the skull
        p.put(x, 15, ["########", "........"])       # a joint
        p.colour(c, 1, c, 2, GREY2, GREY1)
        for y in range(24, 28):                     # plinth, on the sill
            p.put(x, y, ["##.##.#."])
        p.put(x, 26, ["########", "#.#.#.#."])


def draw_static(p):
    # Treasure chest.
    p.put(CHEST_COL * 8, 8, CHEST)
    p.colour(CHEST_COL, 1, CHEST_COL + 1, 1, YELLOW)
    p.colour(CHEST_COL, 2, CHEST_COL + 1, 2, ORANGE)
    # Crest: winged skull over the game's name.
    cx = CREST_COL * 8 + 36
    p.put(cx - 7, 8, SKULL)
    p.put(cx - 9 - 22, 8, BONE)
    p.put(cx + 9, 8, BONE)
    p.colour(CREST_COL, 1, CREST_COL + 8, 1, GREY3)
    x = cx - 36
    for ch in "CATACOMBS":
        p.put(x, 16, LETTERS[ch])
        x += 8
    p.colour(CREST_COL, 2, CREST_COL + 8, 2, YELLOW)


def set_dynamic_colours(p):
    """The colours of the cells Hud.cs copies tiles into."""
    p.colour(HEART_COL, 1, HEART_COL + 9, 1, LRED)
    p.colour(HEART_COL, 2, HEART_COL + 9, 2, RED)
    p.colour(COUNT_COL, 1, COUNT_COL + 2, 1, WHITE)
    p.colour(COUNT_COL, 2, COUNT_COL + 2, 2, GREY3)
    p.colour(WEAPON_COL, 1, WEAPON_COL + 1, 1, CYAN)
    p.colour(WEAPON_COL, 2, WEAPON_COL + 1, 2, LBLUE)
    p.colour(POWER_COL, 1, POWER_COL + 1, 1, WHITE)
    p.colour(POWER_COL, 2, POWER_COL + 1, 2, GREY3)
    # Minimap: explored rooms are lit pixels. Its pixels come from hud_Map.
    p.colour(MAP_COL, 1, MAP_COL + 5, 3, MAP_FG, MAP_BG)
    for y in range(8, 32):
        for x in range(MAP_COL * 8, MAP_COL * 8 + 48):
            p.bit[y][x] = 0


def build_panel():
    p = Panel()
    draw_frame(p)
    draw_static(p)
    set_dynamic_colours(p)
    return p


# ---------------------------------------------------------------------------
# Tile data for Hud.cs. A tile set is a list of 16-row pictures; each is
# stored as its top cell row, then its bottom cell row (Hud.Put copies the
# two halves into cell rows 22 and 23).
# ---------------------------------------------------------------------------
def strip_bytes(rows):
    """rows: 16 strings of width 8*n. Returns top n cells' bytes then bottom n cells' bytes."""
    n = len(rows[0]) // 8
    out = []
    for half in (0, 8):
        for c in range(n):
            for y in range(8):
                line = rows[half + y][c * 8:c * 8 + 8]
                out.append(sum((1 if ch == '#' else 0) << (7 - i) for i, ch in enumerate(line)))
    return out


MAP_FG, MAP_BG = YELLOW, BLACK


def map_frame():
    """The minimap's 48x24 pixels before anything is explored: a frame in
    the maze's solid outer ring (2 pixel rows top and bottom, 4 columns left
    and right). 0/1 rows."""
    g = [[0] * 48 for _ in range(24)]
    for x in range(1, 47):
        g[0][x] = g[23][x] = 1
    for y in range(0, 24):
        g[y][1] = g[y][46] = 1
    for cx, cy in ((1, 0), (45, 0), (1, 22), (45, 22)):      # corner studs
        for dy in range(2):
            for dx in range(2):
                g[cy + dy][cx + dx] = 1
    return g


def map_bytes(g):
    out = []
    for r in range(3):
        for c in range(6):
            for y in range(8):
                out.append(sum(g[r * 8 + y][c * 8 + x] << (7 - x) for x in range(8)))
    return out


def tile_sets():
    hearts = []
    for art in (HEART_FULL, HEART_EMPTY):            # full left, full right, empty left, empty right
        hearts += strip_bytes([line[:8] for line in art])
        hearts += strip_bytes([line[8:] for line in art])
    digits = []
    for ch in DIGIT_ORDER:
        digits += strip_bytes(digit_tile(ch))
    weapons = []
    for art in WEAPONS:
        weapons += strip_bytes(art)
    # hud_MapRow[y]: where maze row y's first byte is in hud_Map's first cell column.
    rows = [(y // 4) * 48 + (y % 4) * 2 for y in range(12)]
    return hearts + digits + weapons, map_bytes(map_frame()), rows


def write_cs(path, sets):
    names = ("hud_Tiles", "hud_Map", "hud_MapRow")
    lines = ["// Generated by art/hud_art.py -- do not edit. The status panel's tiles:",
             "// 8 bytes a cell, top cell row then bottom cell row of each picture.",
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
            if name == "hud_Tiles" and i % 16 == 0:
                lines[-1] += "   ; tile %d" % (i // 8)
    lines.append("\")]")
    with open(path, "w", newline="\n") as f:
        f.write("\n".join(lines) + "\n")
    return total


# ---------------------------------------------------------------------------
# Mock-ups: the panel as Hud.cs would draw it in a given state.
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


def mock(panel, hp, treasure, weapon, power, seen, here, blink_on=True):
    p = panel.copy()
    halves = (hp + 9) // 10
    for h in range(10):
        art = HEART_FULL if h < halves else HEART_EMPTY
        half = [line[8:] if h & 1 else line[:8] for line in art]
        p.put((HEART_COL + h) * 8, 8, half)
    for i, ch in enumerate("%d/3" % treasure):
        p.put((COUNT_COL + i) * 8, 8, digit_tile(ch))
    p.put(WEAPON_COL * 8, 8, WEAPONS[weapon])
    for i, ch in enumerate("%02d" % power):
        p.put((POWER_COL + i) * 8, 8, digit_tile(ch))
    p.tile(MAP_COL * 8, 8, map_frame())
    for my in range(1, 11):
        for mx in range(1, 11):
            on = (mx, my) in seen and MAZE[my][mx] == '1'
            if (mx, my) == here and not blink_on:
                on = False
            for dy in range(2):
                for dx in range(4):
                    p.bit[8 + my * 2 + dy][MAP_COL * 8 + mx * 4 + dx] = 1 if on else 0
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

    image = c64img.Image(c64img.MODE_HIRES, COLS, ROWS, 0)
    for r in range(ROWS):
        for c in range(COLS):
            fg, bg = panel.col[r][c]
            image.cells[r][c] = (panel.cell_bytes(c, r), (fg << 4) | bg, 0)
    payload, sizes = c64img.encode_planes(image, c64img.PLANE_BITMAP | c64img.PLANE_MATRIX)
    c64img.verify(image, payload)
    c64img.write_file(os.path.join(OUT, "hud_panel.img"), image, payload)

    sets = tile_sets()
    tiles = write_cs(os.path.join(HERE, "..", "HudArt.cs"), sets)
    print("hud_panel.img: bitmap %d + matrix %d = %d bytes; tiles %d bytes; assets %d bytes"
          % (sizes[c64img.PLANE_BITMAP], sizes[c64img.PLANE_MATRIX], len(payload), tiles, len(payload) + tiles))

    backdrop().save(os.path.join(SRC, "hud_backdrop.png"))
    c64img.convert(os.path.join(SRC, "hud_backdrop.png"), os.path.join(OUT, "hud_backdrop.img"), quiet=True)

    states = {
        "full": mock(panel, 100, 0, 0, 6, {(1, 1)}, (1, 1)),
        "mid": mock(panel, 55, 2, 1, 12, flood((1, 1), 30), (5, 5)),
        "low": mock(panel, 15, 3, 2, 20, flood((1, 1), 52), (9, 9)),
    }
    sheet = Image.new("RGB", (W * 4, (H * 4 + 16) * len(states)), (40, 40, 60))
    for i, (name, p) in enumerate(states.items()):
        sheet.paste(p.image(4), (0, i * (H * 4 + 16)))
    sheet.save(os.path.join(SRC, "hud_mock.png"))


if __name__ == "__main__":
    main()
