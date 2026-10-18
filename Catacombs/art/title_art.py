#!/usr/bin/env python3
"""
Art for the title / game over / victory screens (TitleScreen.cs, GameFlow.cs).

    python Catacombs/art/title_art.py

One multicolour picture (rows 0-20) serves all three screens: the ossuary
gate. It is not painted as an RGB picture and converted; it is built the way
a C64 artist economises:

  * every pixel is a ROLE (0 = black background, 1 = light, 2 = mid tone,
    3 = dark) and every 8x8 cell has a three-colour PALETTE for those roles.
    A handful of hand-drawn tiles (femur ends, skulls) make the wall; each
    tile is lit as a whole (one palette for all its cells), so the torch
    light falls off bone by bone and palette steps only ever happen in the
    black gaps between bones. Repeated tiles are what the LZ image format
    (tools/c64img.py) compresses best.
  * the logo, the keystone skull and the sconces are painted in colours and
    their cells get hand-chosen palettes.
  * the colours are chosen so the code can re-light the picture with 16-byte
    RemapCellColors tables: a dim step and its exact inverse (fade in, torch
    flicker), a cold "the torches died" palette (game over), a golden one
    (victory) and a glint for the logo. For that the picture uses no red and
    never shows colour 0 through a role (black is always the background bit
    pair); the glow down the stairs is violet, a colour used nowhere else.

Writes art/build/title.img (the picture), title_logo.img (the logo's colours
only, to undo the glint), title.spr (8 sprite frames), title_tables.asm (the
colour tables), and previews to art/src.
"""
import math
import os
import random
import sys

from PIL import Image as PilImage

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import c64img   # noqa: E402
import c64spr   # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
P = c64img.PALETTE

W, H = 160, 168            # multicolour pixels (each two hi-res pixels wide)
COLS, ROWS = 40, 21

BLACK, WHITE, RED, CYAN, VIOLET, GREEN, BLUE, YELLOW = range(8)
ORANGE, BROWN, LRED, GREY1, GREY2, LGREEN, LBLUE, GREY3 = range(8, 16)

# ---------------------------------------------------------------------------
# Colour tables (written to art/build/title_tables.asm)
# ---------------------------------------------------------------------------
# DIM: one step darker, injective on the picture's colours; BRIGHT undoes it.
DIM = {WHITE: GREY3, GREY3: GREY2, GREY2: GREY1, GREY1: BLUE, BLUE: BLACK,
       YELLOW: LRED, LRED: ORANGE, ORANGE: BROWN, BROWN: RED, VIOLET: VIOLET}
BRIGHT = {v: k for k, v in DIM.items()}
# Game over: the torches are out. Warm light becomes cold stone, shadows go
# blue; only the glow in the gate turns red.
GAMEOVER = {YELLOW: GREY3, LRED: GREY2, ORANGE: GREY1, BROWN: BLUE, VIOLET: RED}
# ...and its own dim step (cold chain, red -> brown) and inverse.
GO_DIM = {WHITE: GREY3, GREY3: GREY2, GREY2: GREY1, GREY1: BLUE, BLUE: BLACK, RED: BROWN}
GO_BRIGHT = {v: k for k, v in GO_DIM.items()}
# Victory: the gate (and only the gate's columns) fills with daylight.
VICTORY = {BLUE: BROWN, GREY1: ORANGE, GREY2: LRED, GREY3: YELLOW, VIOLET: WHITE}
# Fade to black, applied repeatedly; works on any screen (the game's too).
DARKEN = {WHITE: GREY3, GREY3: GREY2, GREY2: GREY1, GREY1: BLUE, BLUE: BLACK,
          YELLOW: LRED, LRED: ORANGE, ORANGE: BROWN, BROWN: BLACK, RED: BLACK,
          CYAN: LBLUE, LBLUE: BLUE, VIOLET: BLUE, LGREEN: GREEN, GREEN: GREY1}
# The logo's glint: a column of the gold one step brighter.
GLINT = {YELLOW: WHITE, ORANGE: YELLOW, BROWN: ORANGE}
# The prompt's colour cycle (text colour << 4).
PROMPT = [WHITE, YELLOW, ORANGE, BROWN, ORANGE, YELLOW, WHITE, WHITE]


def table(mapping):
    return [mapping.get(i, i) for i in range(16)]


def compose(first, then):
    a, b = table(first), table(then)
    return [b[a[i]] for i in range(16)]


# ---------------------------------------------------------------------------
# Canvas: roles (or explicit colours) + per-cell palettes
# ---------------------------------------------------------------------------
role = [[0] * W for _ in range(H)]
colour = [[None] * W for _ in range(H)]     # an explicit colour overrides the role
pal = [[(0, 0, 0)] * COLS for _ in range(ROWS)]
ROLE = {'.': 0, 'a': 1, 'b': 2, 'c': 3}


def put(x, y, r):
    if 0 <= x < W and 0 <= y < H:
        role[y][x] = r
        colour[y][x] = None


def paint(x, y, c):
    if 0 <= x < W and 0 <= y < H:
        colour[y][x] = c
        role[y][x] = 0


def stamp(x0, y0, rows, transparent=' '):
    """An ASCII tile: '.' black, a/b/c roles, `transparent` leaves the canvas."""
    for dy, line in enumerate(rows):
        for dx, ch in enumerate(line):
            if ch != transparent:
                put(x0 + dx, y0 + dy, ROLE[ch])


def set_pal(c0, r0, c1, r1, p):
    for r in range(r0, r1 + 1):
        for c in range(c0, c1 + 1):
            if 0 <= r < ROWS and 0 <= c < COLS:
                pal[r][c] = p


def rnd(*key):
    """A repeatable pseudo-random float 0..1 for a key."""
    return random.Random(repr(key)).random()


# ---------------------------------------------------------------------------
# Light: two wall torches
# ---------------------------------------------------------------------------
TORCH_X = (40, 120)        # mc x of the two flames' centres
TORCH_Y = 76
LEVELS = {
    6: (YELLOW, LRED, ORANGE),
    5: (LRED, ORANGE, BROWN),
    4: (ORANGE, BROWN, BLUE),         # shadows go cold away from the flames
    3: (GREY1, BROWN, BLUE),
    2: (GREY1, BLUE, BLUE),
    1: (BLUE, BLUE, BLUE),            # silhouettes
    0: (BLACK, BLACK, BLACK),
}


def light(x, y):
    total = 0.0
    for tx in TORCH_X:
        d = math.hypot((x - tx) * 2.0, (y - TORCH_Y) * 1.15)
        total += math.exp(-d / 40.0)
    return 7.9 + 2.6 * math.log(total + 1e-9)


def level_pal(level):
    return LEVELS[max(0, min(6, int(math.floor(level))))]


BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]

# ---------------------------------------------------------------------------
# Wall tiles, 8x16 (16x16 hi-res pixels), lit from the top left
# ---------------------------------------------------------------------------
FEMUR = ["..aaab..",
         ".aaaabb.",
         "aaaabbbc",
         "aaabbbbc",
         "aabbbbbc",
         "aabbbbcc",
         "abbbbbcc",
         "abbbbbcc",
         "abbbbccc",
         "abbbbccc",
         ".bbbccc.",
         ".bbbccc.",
         "..bccc..",
         "........"]
CONDYLE = [".aab.ab.",
           "aaabaabc",
           "aaabbabc",
           "aabbbbbc",
           "aabbbbbc",
           "abbbbbcc",
           "abbbbbcc",
           "abbbbbcc",
           "abbbbccc",
           ".bbbcccc",
           ".bbbccc.",
           "..bccc..",
           "........"]
HUMERUS = ["..aab...",
           ".aaabb..",
           "aaabbbc.",
           "aabbbbc.",
           "abbbbcc.",
           "abbbccc.",
           ".bbccc..",
           "..cc....",
           "........"]
RUBBLE = ["........",
          ".ab..ab.",
          "abbcabbc",
          ".cc..cc.",
          "........"]
SKULL = ["..aaab..",
         ".aaaabb.",
         "aaaabbbc",
         "aaabbbbc",
         "aabbbbbc",
         "abcbbccc",
         "a..bb..c",
         "a...b..c",
         "ab..b..c",
         "abbb.bbc",
         ".bb...c.",
         ".bbb.bc.",
         ".b.b.b..",
         "..c.c.c.",
         "........"]
SKULL34 = ["..aaab..",
           ".aaabbb.",
           "aaabbbbc",
           "aabbbbbc",
           "abbbbbcc",
           "abbcbccc",
           "ab..b..c",
           "ab..b..c",
           "abb.bb.c",
           ".abbb.cc",
           "..bb..c.",
           "..bbb.c.",
           "..b.b.c.",
           "...c.c..",
           "........"]
RUBBLE = ["........",
          ".ab..ab.",
          "abbcabbc",
          ".cc..cc.",
          "........"]

# Courses of the wall: (top cell row, kind). Each course is 2 cell rows.
COURSES = [(5, 'k'), (7, 's'), (9, 'k'), (11, 'k'), (13, 's'), (15, 'k')]
FLOOR_Y = 144
CX = 79.5                              # the gate's axis
SPRING_Y = 108                         # where the arch starts
R_IN, R_OUT = 32.0, 46.0               # arch radii, hi-res pixels
JAMB_L = (57, 63)                      # mc x range of the left jamb


# Each course repeats a short hand-ordered sequence of tiles from a different
# starting point: irregular to the eye, but repeats that LZ compresses. The
# tiles carry their own offsets inside their 8x16 frames, so bones do not sit
# on an exact grid either.
COURSE_TILES = {
    'k': ['FEMUR', 'CONDYLE', 'FEMUR_LOW', 'HUMERUS', 'FEMUR', 'CONDYLE_LOW', 'FEMUR', 'HUMERUS_LOW', 'CONDYLE', 'GAP'],
    's': ['SKULL', 'SKULL34', 'SKULL', 'FEMUR', 'SKULL', 'SKULL34', 'CONDYLE'],
}


def shifted(tile, dx, dy):
    """The tile moved right by dx and down by dy inside its 8x16 frame."""
    rows = ["." * 8] * dy + ["." * dx + r[:8 - dx] for r in tile]
    return (rows + ["." * 8] * 16)[:16]


TILES = {
    'FEMUR': shifted(FEMUR, 0, 1), 'FEMUR_LOW': shifted(FEMUR, 0, 2),
    'CONDYLE': shifted(CONDYLE, 0, 1), 'CONDYLE_LOW': shifted(CONDYLE, 0, 3),
    'HUMERUS': shifted(HUMERUS, 1, 3), 'HUMERUS_LOW': shifted(HUMERUS, 0, 5), 'GAP': None,
    'SKULL': shifted(SKULL, 0, 0), 'SKULL34': shifted(SKULL34, 0, 1),
}


def wall():
    for n, (r, kind) in enumerate(COURSES):
        shift = 1 if n & 1 else 0       # courses are staggered by a cell
        seq = COURSE_TILES[kind]
        for t in range(-1, COLS // 2 + 1):
            c = t * 2 + shift           # the tile's first cell
            name = seq[(t + n * 3) % len(seq)]
            lv = light(c * 4 + 4, r * 8 + 8) + (rnd('l', t, n) - 0.5) * 0.8 - n * 0.12
            set_pal(c, r, c + 1, r + 1, level_pal(lv))
            if TILES[name] is not None:
                stamp(c * 4, r * 8, TILES[name])
    # the foot of the wall: rubble in the shadow
    for t in range(COLS // 2):
        lv = light(t * 8 + 4, 140) - 1.2
        set_pal(t * 2, 17, t * 2 + 1, 17, level_pal(max(1.0, lv)))
        stamp(t * 8, 137 if t % 3 else 138, RUBBLE)


def cornice():
    """Row 4: a carved band under the logo."""
    tile = ["....", "aaaa", "bbbb", "cccc", "....", "abbc", "abbc", "...."]
    for c in range(COLS):
        stamp(c * 4, 32, tile)
        pal[4][c] = level_pal(max(2.0, light(c * 4 + 2, 48) - 0.5))


def floor():
    """Rows 18-20: flagstones in perspective, joints converging on the gate,
    warm pools under the sconces, black towards the viewer."""
    vx, vy = CX, 96.0
    rows_y = (144, 149, 156, 168)
    for r in range(18, ROWS):
        for c in range(COLS):
            x, y = c * 4 + 2, r * 8 + 4
            pool = max(math.exp(-((x - tx) * 2.0) ** 2 / 3000.0) for tx in TORCH_X)
            gate = math.exp(-((x - CX) * 2.0) ** 2 / 2500.0)
            lv = 1.6 + 3.4 * pool + 1.2 * gate - (r - 18) * 1.0
            pal[r][c] = level_pal(max(1.0 if r < 20 else 0.0, lv))
    for y in range(FLOOR_Y, H):
        for x in range(W):
            band = next(i for i in range(3) if y < rows_y[i + 1])
            if y == rows_y[band]:
                v = 1 if y != 144 else 3                 # lit front edge of each course
            else:
                # radial joints: the stone's position along its course
                u = (x + 0.5 - vx) * (168 - vy) / (y - vy)
                u = u / 26.0 + (0.5 if band & 1 else 0.0)
                f = u - math.floor(u)
                spread = 0.6 / (y - vy) * 4
                if f < spread:
                    v = 0
                elif f < spread * 2.2:
                    v = 1
                else:
                    v = 2
                if y == rows_y[band + 1] - 1:
                    v = 0                                 # joint under each course
            put(x, y, v)
    # the threshold in front of the gate
    for y in (144, 145):
        for x in range(60, 100):
            put(x, y, 1 if y == 144 else 3)


def is_stone(x, y):
    if not 40 <= y < FLOOR_Y:
        return False
    dx = (x - CX) * 2.0
    mirror = x if x < 80 else 159 - x
    if y < SPRING_Y:
        return R_IN <= math.hypot(dx, y - SPRING_Y) < R_OUT or (abs(dx) < 18 and 56 <= y < 80)
    return JAMB_L[0] <= mirror <= JAMB_L[1]


def is_void(x, y):
    dx = (x - CX) * 2.0
    if y >= FLOOR_Y or y < SPRING_Y - R_IN:
        return False
    if y < SPRING_Y:
        return math.hypot(dx, y - SPRING_Y) < R_IN
    return abs(dx) < R_IN


def void(x, y):
    """Through the gate: mostly black; a stair going down, its treads' lit
    edges narrowing, a vault edge, and a glow in bands at the bottom."""
    dx = (x - CX) * 2.0
    u = math.hypot(dx, y - (SPRING_Y + 6)) if y < SPRING_Y + 6 else abs(dx)
    if 24.0 <= u < 25.5 and y < 132:
        return 2                                          # the vault's first rib
    for half, ty in ((28.0, 140), (22.0, 135), (17.0, 131), (13.0, 128), (9.0, 126)):
        if y == ty and abs(dx) < half:
            return 3                                      # a tread's lit edge
    d = math.hypot(dx * 0.8, (y - 117) * 0.7)
    if d < 4:
        return 3
    if d < 8:
        return 3 if (x + y) & 1 else 0
    if d < 12:
        return 3 if (x & 1) and (y & 1) else 0
    return 0


def gate():
    """The stone arch, keystone, jambs, and what is seen through it."""
    for y in range(40, FLOOR_Y):
        for x in range(W):
            if is_void(x, y):
                put(x, y, void(x, y))
                continue
            if not is_stone(x, y):
                continue
            dx = (x - CX) * 2.0
            dy = y - SPRING_Y
            mirror = x if x < 80 else 159 - x
            if y < SPRING_Y:
                u = math.hypot(dx, dy)
                if abs(dx) < 18 and 56 <= y < 80:           # the keystone block
                    if y == 56 or abs(dx) >= 16:
                        r_ = 0 if abs(dx) >= 16 else 1
                    elif y >= 77:
                        r_ = 3
                    else:
                        r_ = 2 if dx < 10 else 3
                else:
                    ang = math.degrees(math.atan2(-dy, dx))
                    seg = ang / 20.0
                    joint = abs(seg - round(seg)) * 20.0 * u / 57.3 < 1.2 and 0.5 < round(seg) < 8.5
                    if joint:
                        r_ = 0
                    elif u >= R_OUT - 2.5:
                        r_ = 1 if dx < 12 else 2
                    elif u < R_IN + 2.5:
                        r_ = 3
                    else:
                        r_ = 2
            else:
                k = (y - SPRING_Y) % 12
                if k == 11:
                    r_ = 0
                elif k == 0 or (mirror == JAMB_L[0] and x < 80):
                    r_ = 1
                elif mirror == JAMB_L[1] or k == 10 or (x >= 80 and mirror == JAMB_L[0]):
                    r_ = 3
                else:
                    r_ = 2
            put(x, y, r_)
    for r in range(ROWS):
        for c in range(COLS):
            x0, y0 = c * 4, r * 8
            cells = [(x, y) for x in range(x0, x0 + 4) for y in range(y0, y0 + 8)]
            if all(is_void(x, y) for x, y in cells):
                pal[r][c] = (GREY1, BLUE, VIOLET)
            elif any(is_stone(x, y) for x, y in cells):
                pal[r][c] = LEVELS[5]


# ---------------------------------------------------------------------------
# The keystone skull: lit warm from both torches, painted in colours
# ---------------------------------------------------------------------------
# W white, Y yellow, L light red, O orange, B brown, U blue, . black
BIGSKULL = [
    "....YYYYYY....",
    "..WYYYYYYYYL..",
    ".WYYYYYYYYYLL.",
    ".YYYYYYYYYYLL.",
    "LYYYYYYYYYYLLL",
    "LYYYYYYYYLLLLL",
    "LLYYYYYLLLLLLL",
    "LLLLLLLLLLLLLO",
    "LLLLLLLLLLLLOO",
    "LLLLLLLLLLLLOO",
    "LLOOOLLLLOOOLO",
    "LOOOOOLLOOOOOO",
    "YYYYYLLLLYYYYO",
    "LO....OO....OO",
    "L.....OO.....O",
    "L.....LO.....O",
    "L.....LO.....B",
    "LO....LO....OB",
    "LOO..OLLO..OOB",
    ".LOOOOBBOOOOB.",
    ".LOOOB..BOOOB.",
    "..LOB....BOB..",
    "..LOOB..BOOB..",
    "..LOOOOOOOOB..",
    "..LO.L.L.L.B..",
    "..LO.L.L.L.B..",
    "...B.O.O.O.B..",
    "....BBBBBBB...",
    "..............",
]
SKULL_X, SKULL_Y = 73, 33
SKULL_PALS = {4: (WHITE, YELLOW, LRED), 5: (YELLOW, LRED, ORANGE),
              6: (LRED, ORANGE, BROWN), 7: (LRED, ORANGE, BROWN)}
LETTER = {'W': WHITE, 'Y': YELLOW, 'L': LRED, 'O': ORANGE, 'B': BROWN, 'U': BLUE}


def keystone():
    for dy, line in enumerate(BIGSKULL):
        for dx, ch in enumerate(line):
            x, y = SKULL_X + dx, SKULL_Y + dy
            if ch == '.':
                # black only inside the skull's outline (its sockets, mouth,
                # and a one-pixel outline); outside it the arch shows
                inside = 0 < dx < len(line) - 1 and 0 < dy < len(BIGSKULL) - 1
                if inside and any(c != '.' for c in line[:dx]) and any(c != '.' for c in line[dx + 1:]):
                    paint(x, y, BLACK)
                continue
            paint(x, y, LETTER[ch])
    for r, p in SKULL_PALS.items():
        set_pal(18, r, 21, r, p)
    # a black outline round the skull, so it stands off the stone
    for dy in range(-1, len(BIGSKULL) + 1):
        for dx in range(-1, 15):
            x, y = SKULL_X + dx, SKULL_Y + dy
            inside = 0 <= dy < len(BIGSKULL) and 0 <= dx < 14 and BIGSKULL[dy][dx] != '.'
            if inside:
                continue
            near = any(0 <= dy + j < len(BIGSKULL) and 0 <= dx + i < 14 and BIGSKULL[dy + j][dx + i] != '.'
                       for i in (-1, 0, 1) for j in (-1, 0, 1))
            if near:
                paint(x, y, BLACK)


# ---------------------------------------------------------------------------
# Sconces (the flames are sprites)
# ---------------------------------------------------------------------------
SCONCE = ["  .    .  ",
          " .a.  .a. ",
          " .ba..ab. ",
          ".cbbaabbc.",
          ".cbbbbbbc.",
          " .cbbbbc. ",
          " ..cbbc.. ",
          "  .cccc.  ",
          "   .bc.   ",
          "   .bc.   ",
          " ...bc... ",
          ".abbbcccc.",
          " ...bc... ",
          "   .bc.   ",
          "   .cc.   ",
          "    ..    "]


def torches():
    for tx in TORCH_X:
        x0 = tx - 5
        stamp(x0, 80, SCONCE)
        c = (tx - 4) // 4
        set_pal(c - 1, 10, c + 2, 11, (YELLOW, ORANGE, BROWN))
        set_pal(c, 12, c + 1, 12, (YELLOW, ORANGE, BROWN))


# ---------------------------------------------------------------------------
# The logo: hand-drawn letters, 12x22 core, with a brown outline, a bevel, a
# gold ramp and a drop shadow, over a bone.
# ---------------------------------------------------------------------------
def rep(line, n):
    return [line] * n


GLYPHS = {
    'C': ["....######..", "..#########.", ".####...###.", ".###.....##.", "####......#.", "####........"]
         + rep("###.........", 10)
         + ["####........", "####......#.", ".###.....##.", ".####...###.", "..#########.", "....######.."],
    'A': [".....##.....", ".....###....", "....####....", "....#####...", "...##.###...", "...##..###..",
          "..###..###..", "..##....###.", "..##....###.", ".###....###.", ".##########.", ".##########.",
          ".##......###", "###......###", "##.......###", "##.......###", "##.......###", "##.......###",
          "##.......###", "##.......###", "###.....####", "####...#####"],
    'T': ["############", "############", "##...###..##", "#....###...#"]
         + rep(".....###....", 16)
         + ["....#####...", "...#######.."],
    'O': ["....####....", "..########..", ".###....###.", ".##......##.", "###......###"]
         + rep("##........##", 12)
         + ["###......###", ".##......##.", ".###....###.", "..########..", "....####...."],
    'M': ["##........##", "###......###", "####....####", "#####..#####", "##.######.##", "##..####..##",
          "##...##...##"]
         + rep("##........##", 13)
         + ["###......###", "####....####"],
    'B': ["#########...", "##########..", ".###...####.", ".###....###.", ".###....###.", ".###....###.",
          ".###...###..", ".#########..", ".##########.", ".###....####"]
         + rep(".###.....###", 7)
         + [".###....####", ".###...####.", "###########.", "##########..", "#########..."],
    'S': ["...#######..", ".#########..", "###.....##..", "###......#..", "####........", "#####.......",
          ".#######....", "..########..", "....#######.", ".......#####", ".........###", "..........##",
          "..........##", "..........##", "#.........##", "##.......###", "###.....###.", "####...####.",
          ".#########..", "..#######..."] + ["............"] * 2,
}
LOGO_X0, LOGO_Y0 = 9, 4
LOGO_PALS = [(WHITE, YELLOW, BROWN), (YELLOW, ORANGE, BROWN), (YELLOW, ORANGE, BROWN), (YELLOW, ORANGE, BROWN)]
FEMUR_ORNAMENT = [
    " .OO.                                 .OO. ",
    ".OYYO.................................OYYO.",
    ".OYYYOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOYYYO.",
    ".OYYOBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBOYYO.",
    " .OO.................................. .OO. ",
]


def logo():
    for i, ch in enumerate("CATACOMBS"):
        g = GLYPHS[ch]
        assert len(g) == 22 and all(len(r) == 12 for r in g), ch
        x0, y0 = LOGO_X0 + i * 16, LOGO_Y0

        def on(x, y):
            return 0 <= x < 12 and 0 <= y < 22 and g[y][x] == '#'
        # drop shadow and outline first, the letter over them
        for y in range(-1, 24):
            for x in range(-1, 14):
                if on(x, y):
                    continue
                if on(x - 1, y - 1) or on(x - 1, y) or on(x, y - 1):
                    paint(x0 + x, y0 + y, BROWN)          # shadow, down-right
                elif any(on(x + i, y + j) for i in (-1, 0, 1) for j in (-1, 0, 1)):
                    paint(x0 + x, y0 + y, BROWN)          # outline
        for y in range(22):
            for x in range(12):
                if not on(x, y):
                    continue
                py = y0 + y
                if not on(x, y - 1) or not on(x - 1, y):
                    c = WHITE if py < 8 else YELLOW        # lit bevel
                elif not on(x, y + 1) or not on(x + 1, y):
                    c = BROWN                              # shaded bevel
                elif py < 10:
                    c = YELLOW
                elif py < 16:
                    c = ORANGE if (py - 10) * 2 + 1 > BAYER[py & 3][x & 3] * 12 // 16 else YELLOW
                elif py < 22:
                    c = ORANGE
                else:
                    c = BROWN if BAYER[py & 3][x & 3] < 8 else ORANGE
                paint(x0 + x, py, c)
    for r in range(4):
        set_pal(0, r, COLS - 1, r, LOGO_PALS[r])
    # the bone under the middle letters
    ox = 80 - len(FEMUR_ORNAMENT[0]) // 2
    for dy, line in enumerate(FEMUR_ORNAMENT):
        for dx, ch in enumerate(line):
            if ch == ' ':
                continue
            if ch == '.':
                paint(ox + dx, 27 + dy, BLACK)
            else:
                paint(ox + dx, 27 + dy, LETTER[ch])


def build():
    wall()
    cornice()
    floor()
    gate()
    torches()
    keystone()
    logo()


# ---------------------------------------------------------------------------
# Sprites: 8 multicolour frames. Shared colours yellow ($D025) and orange
# ($D026); 'x' is the sprite's own colour, set by the code per use.
# ---------------------------------------------------------------------------
SPRITE_FRAMES = [
    # 0-2: the torch flame, three heights and leans
    ["............", "............", "............", "......x.....", "......xx....", ".....xox....",
     ".....xoox...", "..x..xoox...", "..xx.xooox..", "..xoxooyox..", "..xooooyoox.", ".xooyyyyyox.",
     ".xooyyyyyox.", ".xoyyyyyyox.", "xooyyyyyyoox", "xoyyyyyyyyox", "xooyyyyyyoox", ".xoyyyyyyox.",
     "..xooyyoox..", "...xoooox...", "............"],
    ["...x........", "...x........", "...xx.......", "...xox......", "....xox.....", "....xoox....",
     "....xoox..x.", "...xoooox.x.", "...xooyoxxx.", "..xooyyooox.", "..xoyyyyoox.", ".xooyyyyyox.",
     ".xoyyyyyyox.", "xooyyyyyyox.", "xoyyyyyyyoox", "xoyyyyyyyyox", "xooyyyyyyoox", ".xoyyyyyyox.",
     "..xooyyoox..", "...xoooox...", "............"],
    ["............", "............", "............", "............", "............", ".......x....",
     "......xx....", ".x....xox...", ".xx..xoox...", "..xx.xooox..", "..xoxooyox..", ".xooooyyoox.",
     ".xooyyyyyox.", "xooyyyyyyox.", "xoyyyyyyyoox", "xoyyyyyyyyox", "xooyyyyyyoox", ".xoyyyyyyox.",
     "..xooyyoox..", "...xoooox...", "............"],
    # 3: the hooded figure, a black silhouette rimmed by the glow, lantern out
    ["............", ".....o......", "....oxx.....", "...oxxxx....", "...oxxxx....", "...oxx.x....",
     "..oxxxxxx...", ".oxxxxxxxx..", ".oxxxxxxxxx.", ".oxxxxxxx.x.", ".oxxxxxxx.o.", ".oxxxxxxxoyo",
     ".oxxxxxxx.y.", ".oxxxxxxx...", ".oxxxxxxxx..", "oxxxxxxxxx..", "oxxxxxxxxx..", "oxxxxxxxxxx.",
     "oxxxxxxxxxx.", "oxxxxxxxxxxx", "............"],
    # 4: eyes for the keystone skull's sockets
    ["............", "............", "............", "............", "............", "............",
     "............", "............", ".xox....xox.", "xoyox..xoyox", "xoyox..xoyox", ".xox....xox.",
     "............", "............", "............", "............", "............", "............",
     "............", "............", "............"],
    # 5: the figure again, the lantern swung
    ["............", ".....o......", "....oxx.....", "...oxxxx....", "...oxxxx....", "...oxx.x....",
     "..oxxxxxx...", ".oxxxxxxxx..", ".oxxxxxxxxx.", ".oxxxxxxx.x.", ".oxxxxxxx..x", ".oxxxxxxx..o",
     ".oxxxxxxx.oyo", ".oxxxxxxx..y", ".oxxxxxxxx..", "oxxxxxxxxx..", "oxxxxxxxxx..", "oxxxxxxxxxx.",
     "oxxxxxxxxxx.", "oxxxxxxxxxxx", "............"],
    # 6: embers (own colour only: the code cools them as they rise)
    ["............", ".....x......", "............", "............", "........x...", "............",
     "............", "..x.........", "............", "............", "......x.....", "............",
     "............", "............", "...x........", "............", "............", ".......x....",
     "............", "............", "............"],
    # 7: a relic: a chalice, outlined in its own colour (black)
    ["............", "..xxxxxxxx..", ".xyyyyyyyox.", ".xyyyyyyyox.", ".xyyyyyyoox.", "..xyyyyyox..",
     "..xyyyyoox..", "...xyyyox...", "....xyox....", ".....yo.....", ".....yo.....", "....xyox....",
     "...xyyyox...", "..xyyyyyoox.", "..xxxxxxxx..", "............", "............", "............",
     "............", "............", "............"],
]
SPRITE_OWN = [RED, RED, RED, BLACK, RED, BLACK, YELLOW, BLACK]   # for the sheet preview only


def sprite_sheet():
    img = PilImage.new("RGBA", (12 * len(SPRITE_FRAMES), 21), (0, 0, 0, 0))
    px = img.load()
    for f, rows in enumerate(SPRITE_FRAMES):
        rows = [r[:12] for r in rows]
        assert len(rows) == 21 and all(len(r) == 12 for r in rows), "frame %d" % f
        for y, line in enumerate(rows):
            for x, ch in enumerate(line):
                if ch == '.':
                    continue
                c = {'y': YELLOW, 'o': ORANGE, 'x': SPRITE_OWN[f]}[ch]
                px[f * 12 + x, y] = P[c] + (255,)
    path = os.path.join(SRC, "title_sprites.png")
    img.save(path)
    c64spr.convert(path, os.path.join(OUT, "title.spr"), multicolor=True, px=1,
                   mc1=YELLOW, mc2=ORANGE, preview=os.path.join(SRC, "title_sprites.c64.png"), quiet=True)


# ---------------------------------------------------------------------------
# Roles / colours -> C64 cells
# ---------------------------------------------------------------------------
def to_image(rows=range(ROWS)):
    image = c64img.Image(c64img.MODE_MC, COLS, max(rows) + 1, 0)
    last = [0, 0, 0]
    for r in rows:
        for c in range(COLS):
            p = pal[r][c]
            used = set()
            bitmap = bytearray(8)
            for y in range(8):
                b = 0
                for x in range(4):
                    px, py = c * 4 + x, r * 8 + y
                    if colour[py][px] is not None:
                        k = colour[py][px]
                        if k == BLACK:
                            v = 0
                        else:
                            if k not in p:
                                raise ValueError("cell (%d,%d) palette %s lacks colour %d" % (c, r, p, k))
                            v = p.index(k) + 1
                    else:
                        v = role[py][px]
                        if v and p[v - 1] == BLACK:
                            v = 0                   # black is always the background bit pair
                    if v:
                        used.add(v)
                    b = (b << 2) | v
                bitmap[y] = b
            slots = [p[i] if (i + 1) in used else last[i] for i in range(3)]
            last = slots
            image.cells[r][c] = (bytes(bitmap), (slots[0] << 4) | slots[1], slots[2])
    return image


def pixels(image, remap=None):
    out = [[0] * W for _ in range(image.rows * 8)]
    for r in range(image.rows):
        for c in range(image.cols):
            if image.cells[r][c] is None:
                continue
            cols = c64img.cell_colours(image, image.cells[r][c])
            for y in range(8):
                for x in range(4):
                    v = cols[y * 4 + x]
                    out[r * 8 + y][c * 4 + x] = remap[v] if remap else v
    return out


def save_png(pix, path, scale=1):
    h = len(pix)
    img = PilImage.new("RGB", (W * 2, h))
    px = img.load()
    for y in range(h):
        for x in range(W):
            px[x * 2, y] = px[x * 2 + 1, y] = P[pix[y][x]]
    if scale != 1:
        img = img.resize((W * 2 * scale, h * scale), PilImage.NEAREST)
    img.save(path)


def write_img(image, planes, name):
    payload, sizes = c64img.encode_planes(image, planes)
    c64img.verify(image, payload)
    c64img.write_file(os.path.join(OUT, name), image, payload)
    return len(payload) + 11


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    build()
    image = to_image()
    size = write_img(image, 7, "title.img")
    logo_colours = to_image(range(4))
    logo_size = write_img(logo_colours, c64img.PLANE_MATRIX | c64img.PLANE_COLOR, "title_logo.img")
    print("title.img %d bytes, title_logo.img %d bytes" % (size, logo_size))

    used = sorted({v for row in pixels(image) for v in row})
    assert RED not in used, "the picture must not use red (DIM turns brown into it)"

    base = pixels(image)
    save_png(base, os.path.join(SRC, "title.png"))
    save_png(base, os.path.join(SRC, "title_x3.png"), 3)
    save_png(pixels(image, table(GAMEOVER)), os.path.join(SRC, "title_gameover_x3.png"), 3)
    vic = [[VICTORY.get(v, v) if 12 <= x // 4 < 28 else v for x, v in enumerate(row)] for row in base]
    save_png(vic, os.path.join(SRC, "title_victory_x3.png"), 3)
    sprite_sheet()

    tables = [
        ("pal_Dim", table(DIM)), ("pal_Bright", table(BRIGHT)),
        ("pal_GoDim", compose(GAMEOVER, GO_DIM)), ("pal_GoBright", table(GO_BRIGHT)),
        ("pal_Dawn", compose(BRIGHT, VICTORY)), ("pal_Darken", table(DARKEN)),
        ("pal_Glint", table(GLINT)), ("pal_Prompt", [c << 4 for c in PROMPT]),
    ]
    with open(os.path.join(OUT, "title_tables.asm"), "w") as f:
        f.write("; Written by Catacombs/art/title_art.py -- do not edit.\n")
        for name, t in tables:
            f.write("%-12s .byte %s\n" % (name, ",".join(str(v) for v in t)))


if __name__ == "__main__":
    main()
