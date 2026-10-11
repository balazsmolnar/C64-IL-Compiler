#!/usr/bin/env python3
"""
Art for the title / game over / victory screens (TitleScreen.cs, GameFlow.cs).

    python Catacombs/art/title_art.py

One multicolour picture (rows 0-20) serves all three screens: the ossuary
gate. It is not painted as an RGB picture and converted; it is built the way
a C64 artist economises:

  * every pixel is a ROLE (0 = black background, 1 = highlight, 2 = mid tone,
    3 = shadow) and every 8x8 cell has a three-colour PALETTE for those
    roles. The same few hand-drawn tiles (bone ends, skulls, stones) are
    reused all over the wall, and the torch light is in the cell palettes:
    yellow / light red / orange at the flames, falling through brown into
    cold grey, blue and black. Repeated tiles are what the LZ image format
    (tools/c64img.py) compresses best, so the picture costs about 2 KB.
  * the colours are chosen so the code can re-light the picture with 16-byte
    RemapCellColors tables (see TitleScreen.cs): a dim step and its exact
    inverse (fade in, torch flicker), a cold "the torches died" palette
    (game over) and a golden one (victory). For that the picture uses no red
    and never shows colour 0 through a role (black is always the background
    bit pair); the glow down the stairs is violet, a colour used nowhere
    else, so the remaps can turn it blood red or sunlit.

Writes the .img / .spr files to art/build and previews (the picture in each
palette, the sprite sheet, the font) to art/src.
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
# Colour tables shared with TitleScreen.cs (printed by main() as .byte lines)
# ---------------------------------------------------------------------------
# DIM: one step darker, and injective on the colours the picture uses, so
# BRIGHT (its inverse) restores the picture exactly.
DIM = {WHITE: GREY3, GREY3: GREY2, GREY2: GREY1, GREY1: BLUE, BLUE: BLACK,
       YELLOW: LRED, LRED: ORANGE, ORANGE: BROWN, BROWN: RED, VIOLET: VIOLET}
BRIGHT = {v: k for k, v in DIM.items()}
# Game over: the torches are out. Warm light turns to cold stone, the darkest
# warm tone and the glow in the gate turn blood red.
GAMEOVER = {YELLOW: GREY3, LRED: GREY2, ORANGE: GREY1, BROWN: RED, VIOLET: RED}
# Victory: daylight. Cold stone turns gold, the gate glows.
VICTORY = {BLUE: BROWN, GREY1: ORANGE, GREY2: LRED, GREY3: YELLOW, VIOLET: YELLOW}
# Fade to black, applied repeatedly; works on any screen (the game's too).
DARKEN = {WHITE: GREY3, GREY3: GREY2, GREY2: GREY1, GREY1: BLUE, BLUE: BLACK,
          YELLOW: LRED, LRED: ORANGE, ORANGE: BROWN, BROWN: BLACK, RED: BLACK,
          CYAN: LBLUE, LBLUE: BLUE, VIOLET: BLUE, LGREEN: GREEN, GREEN: GREY1}


def table(mapping):
    return [mapping.get(i, i) for i in range(16)]


def compose(first, then):
    a, b = table(first), table(then)
    return [b[a[i]] for i in range(16)]


# ---------------------------------------------------------------------------
# Canvas: roles + per-cell palettes
# ---------------------------------------------------------------------------
role = [[0] * W for _ in range(H)]
pal = [[(0, 0, 0)] * COLS for _ in range(ROWS)]
ROLE = {'.': 0, ' ': 0, 'a': 1, 'b': 2, 'c': 3}


def put(x, y, r):
    if 0 <= x < W and 0 <= y < H:
        role[y][x] = r


def stamp(x0, y0, rows, transparent=None):
    """Draws an ASCII tile ('.' black, a/b/c roles). `transparent`: a char
    that leaves the canvas alone."""
    for dy, line in enumerate(rows):
        for dx, ch in enumerate(line):
            if ch == transparent:
                continue
            put(x0 + dx, y0 + dy, ROLE[ch])


def set_pal(c0, r0, c1, r1, p):
    for r in range(r0, r1 + 1):
        for c in range(c0, c1 + 1):
            pal[r][c] = p


# ---------------------------------------------------------------------------
# Light: two wall torches. A cell's level picks its palette.
# ---------------------------------------------------------------------------
TORCH_X = (40, 120)        # mc x of the two flames' centres
TORCH_Y = 76
LEVELS = {
    6: (YELLOW, LRED, ORANGE),
    5: (LRED, ORANGE, BROWN),
    4: (ORANGE, BROWN, BLACK),
    3: (GREY1, BROWN, BLACK),
    2: (GREY1, BLUE, BLACK),
    1: (BLUE, BLACK, BLACK),
    0: (BLACK, BLACK, BLACK),
}


def light(x, y):
    """Light level (float, about 0..7) at mc pixel (x, y)."""
    total = 0.0
    for tx in TORCH_X:
        d = math.hypot((x - tx) * 2.0, (y - TORCH_Y) * 1.15)
        total += math.exp(-d / 30.0)
    return 7.3 + 1.9 * math.log(total + 1e-9)


def cell_light(c, r):
    return light(c * 4 + 2, r * 8 + 4)


def level_pal(level):
    return LEVELS[max(0, min(6, int(math.floor(level))))]


# ---------------------------------------------------------------------------
# Tiles
# ---------------------------------------------------------------------------
# A femur end seen head-on, 4x8 (8x8 hi-res pixels), lit from the top left.
# Three tonal variants: the fraction of the light level picks one, so the
# falloff between two palette steps is dithered at tile level.
KNOB = {
    2: [".aa.",
        "aaab",
        "aabb",
        "aabb",
        "abbc",
        "abbc",
        ".bc.",
        "...."],
    1: [".aa.",
        "aabb",
        "abbc",
        "abbc",
        "bbbc",
        "bbcc",
        ".cc.",
        "...."],
    0: [".ab.",
        "abbc",
        "bbcc",
        "bbcc",
        "bccc",
        "bccc",
        ".cc.",
        "...."],
}
# A snapped bone: the marrow hole.
HOLLOW = [".ab.",
          "ab.c",
          "b..c",
          "b..c",
          "bc.c",
          "bccc",
          ".cc.",
          "...."]
# A small skull of the wall, 8x16.
SKULL = ["..aaaa..",
         ".aaaabb.",
         "aaaabbbc",
         "aaabbbbc",
         "aabbbbbc",
         "aabbbbcc",
         "ab.bbb.c",
         "a...b..c",
         "a...b..c",
         "ab.bbb.c",
         ".bbb.bcc",
         ".bb..bc.",
         ".bbbbbc.",
         ".b.b.bc.",
         "..b.b.c.",
         "........"]

# The keystone skull, 16x40 (32x40 hi-res pixels).
BIGSKULL = [
    ".....aaaaaa.....",
    "...aaaaaaaabb...",
    "..aaaaaaaabbbb..",
    ".aaaaaaaabbbbbb.",
    ".aaaaaaabbbbbbc.",
    "aaaaaaabbbbbbbcc",
    "aaaaaabbbbbbbbcc",
    "aaaaabbbbbbbbbcc",
    "aaaabbbbbbbbbbcc",
    "aaaabbbbbbbbbccc",
    "aaabbbbbbbbbbccc",
    "aaabbbbbbbbbbccc",
    "aabbbbbbbbbbcccc",
    "aabcccbbbbcccccc",
    "ab.....cc.....cc",
    "ab......c......c",
    "a.......b......c",
    "a.......b......c",
    "a.......b......c",
    "ab......b......c",
    "ab.....bb.....cc",
    "abb...bbbb...bcc",
    "aabbbbb..bbbbbcc",
    ".abbbb....bbbcc.",
    ".abbbb....bbbcc.",
    "..abbb....bbcc..",
    "..abbbb..bbbcc..",
    "..aabbbbbbbbcc..",
    "..ab.b.b.b.bcc..",
    "..ab.b.b.b.bcc..",
    "...b.b.b.b.bc...",
    "................",
    "...b.b.b.b.bc...",
    "..ab.b.b.b.bcc..",
    "..abbbbbbbbbcc..",
    "..aabbbbbbbccc..",
    "...abbbbbbbcc...",
    "....abbbbbcc....",
    ".....bbbccc.....",
    "................",
]


def rnd(*key):
    """A repeatable pseudo-random float 0..1 for a key (so the layout is
    stable from run to run)."""
    return random.Random(hash(key) & 0xFFFFFFFF).random()


# ---------------------------------------------------------------------------
# The picture
# ---------------------------------------------------------------------------
WALL_TOP, WALL_BOTTOM = 5, 17          # cell rows of the bone wall
SKULL_ROWS = (8, 13)                   # top rows of the two courses of skulls
FLOOR_Y = 144
CX = 79.5                              # the gate's axis
SPRING_Y = 108                         # where the arch starts
R_IN, R_OUT = 32.0, 46.0               # arch radii, hi-res pixels
JAMB_L = (57, 63)                      # mc x range of the left jamb


def wall():
    for r in range(WALL_TOP, WALL_BOTTOM + 1):
        skull_row = next((s for s in SKULL_ROWS if r in (s, s + 1)), None)
        for c in range(COLS):
            lv = cell_light(c, r)
            # Each bone a touch lighter or darker than its neighbours.
            if rnd('j', c, r) < 0.16:
                lv += 1.0 if rnd('s', c, r) < 0.5 else -1.0
            pal[r][c] = level_pal(lv)
        if skull_row is not None:
            if r == skull_row:
                for c in range(0, COLS, 2):
                    stamp(c * 4, r * 8, SKULL)
            continue
        shift = 2 if (r & 1) else 0
        for c in range(-1, COLS):
            x = c * 4 + shift
            lv = light(x + 2, r * 8 + 4)
            frac = lv - math.floor(lv)
            variant = 2 if frac > 0.62 else (1 if frac > 0.30 else 0)
            tile = KNOB[variant]
            if rnd('h', c // 2, r) < 0.10 and (c & 1):
                tile = HOLLOW
            stamp(x, r * 8, tile)


def cornice():
    """Row 4: a carved stone band between the logo and the wall."""
    tile = ["aaaa", "bbbb", "bbbb", "cccc", "....", "abbc", "abbc", "...."]
    tile2 = ["aaaa", "bbbb", "bbbb", "cccc", "....", "....", "....", "...."]
    for c in range(COLS):
        stamp(c * 4, 32, tile if c & 1 else tile2)
        pal[4][c] = level_pal(cell_light(c, 5) - 0.6)


def floor():
    """Rows 18-20: three courses of flagstones, wider towards the viewer."""
    courses = [(144, 150, 12), (151, 158, 16), (159, 167, 24)]
    for r in range(18, ROWS):
        for c in range(COLS):
            pal[r][c] = level_pal(cell_light(c, r) - 0.3)
    for i, (y0, y1, width) in enumerate(courses):
        for y in range(y0, y1 + 1):
            for x in range(W):
                # joints fan out from the gate's axis
                spread = 1.0 + (y - 144) * 0.035
                u = (x - CX) / spread + 400 + (width // 2 if i & 1 else 0)
                k = int(u) % width
                if y == y0:
                    r_ = 1                        # lit top edge of the course
                elif y == y1:
                    r_ = 0                        # joint
                elif k == 0:
                    r_ = 0
                elif k == 1:
                    r_ = 1
                elif k == width - 1:
                    r_ = 3
                else:
                    r_ = 2 if ((x + y) & 1 or y < y0 + 3) else 3
                put(x, y, r_)


def gate():
    """The stone arch, its jambs and what is seen through it."""
    for y in range(40, FLOOR_Y):
        for x in range(W):
            dx = (x - CX) * 2.0
            dy = y - SPRING_Y
            mirror = x if x < 80 else 159 - x      # geometry is symmetric
            if y < SPRING_Y:
                u = math.hypot(dx, dy)
                inside = u < R_IN
                ring = R_IN <= u < R_OUT
            else:
                u = abs(dx)
                inside = u < R_IN
                ring = JAMB_L[0] <= mirror <= JAMB_L[1]
            if inside:
                put(x, y, void(x, y))
                continue
            if not ring:
                continue
            if y < SPRING_Y:
                ang = math.degrees(math.atan2(-dy, dx))          # 0..180
                seg = ang / 20.0
                joint = abs(seg - round(seg)) * 20.0 * u / 57.3 < 1.2 and 0.5 < round(seg) < 8.5
                edge_out = u >= R_OUT - 2.5
                edge_in = u < R_IN + 2.5
                if joint:
                    r_ = 0
                elif edge_out:
                    r_ = 1 if dx < 12 else 2
                elif edge_in:
                    r_ = 3
                else:
                    r_ = 2 if (x * 3 + y * 5) % 7 else 3
            else:
                k = (y - SPRING_Y) % 12
                if k == 11:
                    r_ = 0
                elif k == 0 or mirror == JAMB_L[0]:
                    r_ = 1 if x < 80 else 2
                elif mirror == JAMB_L[1] or k == 10:
                    r_ = 3
                else:
                    r_ = 2 if (x * 3 + y * 5) % 7 else 3
                if x >= 80 and mirror == JAMB_L[0]:
                    r_ = 3                                # far side of the right jamb is in shade
            put(x, y, r_)
    # the gate's own cells: the glow down the stairs is the palette's third colour
    for r in range(ROWS):
        for c in range(COLS):
            x0, y0 = c * 4, r * 8
            if all(is_void(x, y) for x in range(x0, x0 + 4) for y in range(y0, y0 + 8)):
                pal[r][c] = (GREY1, BLUE, VIOLET)


def is_void(x, y):
    dx = (x - CX) * 2.0
    if y >= FLOOR_Y or y < SPRING_Y - R_IN:
        return False
    if y < SPRING_Y:
        return math.hypot(dx, y - SPRING_Y) < R_IN
    return abs(dx) < R_IN


BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def void(x, y):
    """Through the gate: a vaulted stair going down, three receding arches and
    their steps, and a glow at the bottom of it."""
    dx = (x - CX) * 2.0
    for k, (radius, drop) in enumerate(((25.0, 5), (18.0, 11), (11.0, 18))):
        sy = SPRING_Y + drop
        if y < sy:
            u = math.hypot(dx, y - sy)
        else:
            u = abs(dx)
        if radius <= u < radius + (3.0 if k < 2 else 2.0):
            if k == 0:
                return 1
            return 2 if k == 1 or (x + y) & 1 else 0
        # treads between this arch and the next one out
    # steps: horizontal treads narrowing as they go down and away
    for k, (half, y0) in enumerate(((25.0, 140), (18.0, 134), (11.0, 129))):
        if y == y0 and abs(dx) < half:
            return 1 if k == 0 else 2
        if y == y0 + 1 and abs(dx) < half and k == 0:
            return 2
    # the glow: densest at the far end of the stair
    gx, gy = 0.0, SPRING_Y + 6
    d = math.hypot(dx * 0.9, (y - gy) * 0.8)
    if d < 11:
        level = 1.0 - d / 11.0
        return 3 if level * 16 > BAYER[y & 3][x & 3] else 0
    return 0


def keystone():
    x0, y0 = 72, 36
    # A black niche: the skull's own cells hold nothing else.
    for y in range(32, 80):
        for x in range(72, 88):
            put(x, y, 0)
    stamp(x0, y0, BIGSKULL)
    set_pal(18, 4, 21, 6, (WHITE, GREY3, GREY2))
    set_pal(18, 7, 21, 9, (GREY3, GREY2, GREY1))


def torches():
    """The iron baskets; the flames themselves are sprites."""
    basket = ["a......a",
              "ac....ac",
              "bcc..ccb",
              "bcbccbcb",
              ".cbccbc.",
              ".ccbbcc.",
              "..cccc..",
              "...cc...",
              "...bc...",
              "...bc...",
              "...bc...",
              "..abcc..",
              "..bccc..",
              "...cc...",
              "........",
              "........"]
    for tx in TORCH_X:
        x0 = tx - 4
        for y in range(80, 96):
            for x in range(x0, x0 + 8):
                put(x, y, 0)
        stamp(x0, 80, basket)
        c = x0 // 4
        set_pal(c, 10, c + 1, 11, (YELLOW, ORANGE, BROWN))


# ---------------------------------------------------------------------------
# Sprites: 8 multicolour frames. Shared colours yellow ($D025) and orange
# ($D026); 'x' is the sprite's own colour, set by the code per use.
# ---------------------------------------------------------------------------
SPRITE_FRAMES = [
    # 0-2: the torch flame
    ["............", ".....x......", ".....x......", "....xo......", "....xox.....", "....ooo.....",
     "...xooox....", "...oooyo....", "...ooyyox...", "..xoyyyoo...", "..ooyyyyo...", "..oyyyyyox..",
     ".xoyyyyyoo..", ".ooyyyyyyo..", ".ooyyyyyyo..", ".xoyyyyyyo..", "..oyyyyyoo..", "..ooyyyyox..",
     "...ooyyoo...", "....oooo....", "............"],
    ["............", "............", ".......x....", "......xo....", "......oox...", ".....xoo....",
     ".....ooox...", "....xoyoo...", "....ooyyo...", "...xoyyyox..", "...ooyyyoo..", "..xoyyyyyo..",
     "..ooyyyyyox.", "..oyyyyyyoo.", ".xoyyyyyyoo.", ".ooyyyyyyo..", "..oyyyyyyo..", "..xoyyyyoo..",
     "...ooyyoo...", "....oooo....", "............"],
    ["......x.....", "............", "....x.......", "....ox......", "...xoo......", "....oox.....",
     "...xooo.....", "...ooyox....", "..xoyyoo....", "..ooyyyo....", "..oyyyyox...", ".xoyyyyoo...",
     ".ooyyyyyox..", ".oyyyyyyoo..", ".ooyyyyyyo..", ".xoyyyyyyox.", "..oyyyyyoo..", "..ooyyyyo...",
     "...ooyyoo...", "....oooo....", "............"],
    # 3: a hooded figure with a lantern, seen from behind on the stair
    ["............", "....xxx.....", "...xxxxx....", "...xxxxx....", "...xxxxx....", "...xxxxx....",
     "..xxxxxxx...", "..xxxxxxx.y.", ".xxxxxxxxoyo", ".xxxxxxx.yyy", ".xxxxxxx.oyo", ".xxxxxxx....",
     ".xxxxxxx....", ".xxxxxxx....", ".xxxxxxxx...", ".xxxxxxxx...", "xxxxxxxxx...", "xxxxxxxxx...",
     "xxxxxxxxx...", "xxxxxxxxxx..", "............"],
    # 4: eyes for the keystone skull's sockets
    ["............", "............", "............", "............", "............", "............",
     "............", "............", ".xox....xox.", "xoyox..xoyox", "xoyox..xoyox", ".xox....xox.",
     "............", "............", "............", "............", "............", "............",
     "............", "............", "............"],
    # 5: a glint
    ["............", "............", "............", ".....x......", ".....x......", ".....x......",
     ".....x......", "....xyx.....", "..xxyyyxx...", "....xyx.....", ".....x......", ".....x......",
     ".....x......", ".....x......", "............", "............", "............", "............",
     "............", "............", "............"],
    # 6: embers / smoke (own colour only, so the code can recolour it)
    ["............", ".....x......", "............", "............", "........x...", "............",
     "............", "..x.........", "............", "............", "......x.....", "............",
     "............", "............", "...x........", "............", "............", ".......x....",
     "............", "............", "............"],
    # 7: a relic (a chalice)
    ["............", "............", "..xyyyyyyo..", "..xyyyyyyo..", "..xyyyyyoo..", "...xyyyyo...",
     "...xyyyoo...", "....xyoo....", ".....yo.....", ".....yo.....", ".....yo.....", "....xyoo....",
     "...xyyyoo...", "..xyyyyyoo..", "............", "............", "............", "............",
     "............", "............", "............"],
]
SPRITE_OWN = [RED, RED, RED, GREY1, RED, WHITE, LRED, WHITE]   # for the sheet PNG only


def sprite_sheet():
    img = PilImage.new("RGBA", (12 * len(SPRITE_FRAMES), 21), (0, 0, 0, 0))
    px = img.load()
    for f, rows in enumerate(SPRITE_FRAMES):
        assert len(rows) == 21 and all(len(r) == 12 for r in rows), "frame %d" % f
        for y, line in enumerate(rows):
            for x, ch in enumerate(line):
                if ch == '.':
                    continue
                col = {'y': YELLOW, 'o': ORANGE, 'x': SPRITE_OWN[f]}[ch]
                px[f * 12 + x, y] = P[col] + (255,)
    path = os.path.join(SRC, "title_sprites.png")
    img.save(path)
    c64spr.convert(path, os.path.join(OUT, "title.spr"), multicolor=True, px=1,
                   mc1=YELLOW, mc2=ORANGE, preview=os.path.join(SRC, "title_sprites.c64.png"))


def build():
    wall()
    cornice()
    floor()
    gate()
    keystone()
    torches()


# ---------------------------------------------------------------------------
# Roles -> C64 cells
# ---------------------------------------------------------------------------
def to_image():
    image = c64img.Image(c64img.MODE_MC, COLS, ROWS, 0)
    last = [0, 0, 0]
    for r in range(ROWS):
        for c in range(COLS):
            p = pal[r][c]
            used = set()
            bitmap = bytearray(8)
            for y in range(8):
                b = 0
                for x in range(4):
                    v = role[r * 8 + y][c * 4 + x]
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
    """Colour index per mc pixel as the C64 shows the image, optionally
    through a 16-entry colour table."""
    out = [[0] * W for _ in range(H)]
    for r in range(image.rows):
        for c in range(image.cols):
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


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    build()
    image = to_image()
    payload, sizes = c64img.encode_planes(image, 7)
    c64img.verify(image, payload)
    c64img.write_file(os.path.join(OUT, "title.img"), image, payload)
    print("title.img: bitmap %d, matrix %d, colour %d -> %d bytes"
          % (sizes[1], sizes[2], sizes[4], len(payload) + 11))

    used = sorted({v for row in pixels(image) for v in row})
    print("colours used:", " ".join(c64img.COLOR_NAMES[v] for v in used))
    assert RED not in used, "the picture must not use red (DIM turns brown into it)"

    save_png(pixels(image), os.path.join(SRC, "title.png"))
    save_png(pixels(image), os.path.join(SRC, "title_x3.png"), 3)
    save_png(pixels(image, table(DIM)), os.path.join(SRC, "title_dim_x3.png"), 3)
    save_png(pixels(image, table(GAMEOVER)), os.path.join(SRC, "title_gameover_x3.png"), 3)
    save_png(pixels(image, table(VICTORY)), os.path.join(SRC, "title_victory_x3.png"), 3)

    sprite_sheet()
    for name, t in (("pal_Dim", table(DIM)), ("pal_Bright", table(BRIGHT)),
                    ("pal_Dead", compose(BRIGHT, GAMEOVER)), ("pal_Dawn", compose(BRIGHT, VICTORY)),
                    ("pal_Darken", table(DARKEN))):
        print("%-11s .byte %s" % (name, ",".join(str(v) for v in t)))


if __name__ == "__main__":
    main()
