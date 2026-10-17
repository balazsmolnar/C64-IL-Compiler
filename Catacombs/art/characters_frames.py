"""
The sprite frames themselves, as text grids (see characters_art.py for the
colour letters). MC: 12 double-wide pixels x 21. HI: 24 x 21, one colour.

A character taller than one sprite is two sprites stacked (top, bottom); the
adventurer's top half also has a hi-res overlay sprite in front of it carrying
the torch flame's core and the torchlight catching his edges.
"""

MC = {}
HI = {}


def mirror(rows):
    return [r[::-1] for r in rows]


def blank(width=12):
    return [["."] * width for _ in range(21)]


def done(grid):
    return ["".join(r) for r in grid]


def hi(spec, letter="Y"):
    """A hi-res frame from {row: "x x0-x1 ..."}."""
    g = blank(24)
    for y, runs in spec.items():
        for run in runs.split():
            a, _, b = run.partition("-")
            for x in range(int(a), int(b or a) + 1):
                g[y][x] = letter
    return done(g)


def shadow(g, cx, y, half, rows=3):
    """A dithered black ground shadow, drawn only where the frame is empty."""
    for dy in range(rows):
        h = half - (0 if dy == rows // 2 else 1)
        for x in range(cx - h, cx + h + 1):
            if 0 <= x < 12 and 0 <= y + dy < 21 and g[y + dy][x] == "." and (x + y + dy) % 2 == 0:
                g[y + dy][x] = "K"


def outline(g, letters, over=""):
    """Black around the pixels whose letter is in `letters`: into empty pixels,
    and (left and right only) into pixels whose letter is in `over`."""
    h, w = len(g), len(g[0])
    put = []
    for y in range(h):
        for x in range(w):
            if g[y][x] not in letters:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w and 0 <= ny < h:
                    if g[ny][nx] == "." or (dy == 0 and g[ny][nx] in over):
                        put.append((nx, ny))
    for x, y in put:
        g[y][x] = "K"


# ---------------------------------------------------------------- adventurer
# Hooded, cyan cloak over a tunic, steel-grey hose, leather boots and gloves,
# a torch held up in his right hand. Top half: black / orange / cyan.

TOP_DOWN = [
    "............",
    ".....KKK....",
    "....KCCCK...",
    ".O.KCCCCCK..",
    ".OOKCKKKCK..",
    "OOOKCOOOCK..",
    "OOOKCKOKCK..",
    "OOOKCOOOCK..",
    "KOK.KCOCK...",
    ".O.KKCCCKK..",
    ".OKCCCCCCCK.",
    "KOOCCCOCCCK.",
    "KOOCCKOKCCK.",
    ".KKCKOOOKCK.",
    "..KCKOOOKCOK",
    "..KCKKKKKCOK",
    "..KCKOOOKCK.",
    "..KCKOOOKCK.",
    "..KCKOOOKCK.",
    "..KCCKOKCCK.",
    "..KKKKKKKKK.",
]

TOP_UP = [
    "............",
    ".....KKK....",
    "....KCCCK...",
    "...KCCCCCK.O",
    "...KCCCCCKOO",
    "...KCCCCCKOO",
    "...KCCCCCKOO",
    "...KCCCCCKOO",
    ".O..KCCCK.KO",
    "..OKKCCCKK.O",
    "..KOCCCCCCKO",
    "..KCOCCCCCOO",
    "..KCCOCCCCOO",
    "..KCCCOCCCKK",
    ".KOKCCCOCCK.",
    ".KOKCCCCOCK.",
    "..KCCCCCCOK.",
    "..KCCCCCCCK.",
    "..KCCCKCCCK.",
    "..KCCKCKCCK.",
    "..KKKKKKKKK.",
]

TOP_RIGHT = [
    "..........O.",
    "...KKKK..OOO",
    "..KCCCCK.OOO",
    ".KCCCCCCK.O.",
    ".KCCCCKKKKO.",
    ".KCCCKOOOKO.",
    ".KCCCKOKOKO.",
    ".KCCCKOOOKO.",
    "..KCCCKOOKO.",
    "..KCCCCKKKOO",
    ".KCCCCCCCCOO",
    ".KCCCCKKKKK.",
    ".KCCCCCKOK..",
    ".KCCCCCKOK..",
    ".KCCCCCKOK..",
    "KCCCCCCKKK..",
    "KCCCCCCKOK..",
    "KCCCCCCKOK..",
    "KCCCCCCKOK..",
    "KCCCCCCKOK..",
    ".KKKKKKKK...",
]

MC["p_top_down"] = TOP_DOWN
MC["p_top_up"] = TOP_UP
MC["p_top_left"] = mirror(TOP_RIGHT)
MC["p_top_right"] = TOP_RIGHT

OV_RIGHT = hi({
    0: "21", 1: "20-21", 2: "19-22 11-14", 3: "20-21",
    5: "16-17", 7: "17",
    10: "14-19", 12: "17", 13: "17", 14: "17", 16: "17", 17: "17", 18: "17", 19: "17",
})
HI["p_ov_down"] = hi({
    3: "3 8", 4: "3-4 8", 5: "2-4 8 10-11", 6: "1-4 8", 7: "2-3 8 10",
    2: "10", 10: "6", 11: "2-3 12-13",
    13: "6", 14: "6", 15: "6 12-13", 16: "6", 17: "6", 18: "6", 19: "6",
})
HI["p_ov_up"] = hi({
    3: "22", 4: "21-22", 5: "21-22", 6: "20-23", 7: "21-22",
    2: "16-17", 3 + 0: "22 19", 4 + 0: "21-22 19", 5 + 0: "21-22 19", 6 + 0: "20-23 19", 7 + 0: "21-22 19",
    10: "20", 11: "22-23", 13: "21", 14: "20", 15: "20", 16: "20", 17: "20", 18: "20", 19: "20",
})
HI["p_ov_left"] = mirror(OV_RIGHT)
HI["p_ov_right"] = OV_RIGHT

# Striking: the sword arm up, the blade (hi-res, in the overlay) catching the light.
TOP_ATTACK = list(TOP_DOWN)
TOP_ATTACK[7] = "OOOKCOOOCKOK"
TOP_ATTACK[8] = "KOK.KCOCKKOK"
TOP_ATTACK[9] = ".O.KKCCCKCCK"
TOP_ATTACK[14] = "..KCKOOOKCK."
TOP_ATTACK[15] = "..KCKKKKKCK."
MC["p_top_attack"] = TOP_ATTACK
HI["p_ov_attack"] = hi({
    0: "22", 1: "22", 2: "21-22", 3: "3 8 21-22", 4: "3-4 8 21-22", 5: "2-4 8 10-11 21-22", 6: "1-4 8 19-23",
    7: "2-3 8 10", 10: "6", 11: "2-3 12-13",
    13: "6", 14: "6", 15: "6 12-13", 16: "6", 17: "6", 18: "6", 19: "6",
})

FEET = 14            # the row of the soles in a bottom frame: the floor line


def _leg(g, pts, boot_from, foot, tag):
    """A leg two pixels wide through the (x, y) points (hip, knee, ankle), x being
    its left pixel; orange from row boot_from down; foot = -1/0/1 extends the
    sole backward/not/forward. Pixels are tagged (upper case = near leg)."""
    cells = []
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        n = max(1, y1 - y0)
        for i in range(n + 1):
            y = y0 + i
            x = int(round(x0 + (x1 - x0) * i / n))
            cells += [(x, y), (x + 1, y)]
    ax, ay = pts[-1]
    if foot > 0:
        cells += [(ax + 2, ay)]
    if foot < 0:
        cells += [(ax - 1, ay)]
    for x, y in cells:
        if 0 <= x < 12 and 0 <= y < 21:
            g[y][x] = ("O" if y >= boot_from else "L") if tag else ("o" if y >= boot_from else "l")
            if y == boot_from - 1:
                g[y][x] = "K"


def legs(far, near, far_foot=1, near_foot=1, boot=4):
    """A bottom frame: the far leg, then the near leg in front of it."""
    g = blank()
    _leg(g, far, far[-1][1] - boot + 1, far_foot, False)
    outline(g, "lo")
    _leg(g, near, near[-1][1] - boot + 1, near_foot, True)
    outline(g, "LO", over="lo")
    for r in g:
        for i, ch in enumerate(r):
            r[i] = {"l": "L", "o": "O"}.get(ch, ch)
    for x in range(12):                       # soles sit on the floor line or above it
        for y in range(FEET + 1, 21):
            if g[y][x] == "K" and g[y - 1][x] != "K":
                pass
    shadow(g, 6, FEET, 5)
    return done(g)


# Facing the viewer / away: the stepping leg reaches lower (nearer) and the
# trailing one is lifted. (hip, ankle): left pixel of the leg, row.
def _front(left_len, right_len, toe):
    return legs([(7, 0), (7, right_len)], [(4, 0), (4, left_len)], far_foot=toe, near_foot=-toe)


MC["p_leg_down0"] = _front(FEET - 1, FEET - 4, 1)
MC["p_leg_down1"] = _front(FEET - 2, FEET - 2, 1)
MC["p_leg_down2"] = _front(FEET - 4, FEET - 1, 1)
MC["p_leg_down3"] = _front(FEET - 2, FEET - 2, 1)
# (Walking away uses the same four: from behind the legs read the same.)

# Walking right: contact, passing, contact, passing. Points are hip, knee, ankle.
_S = [
    legs([(4, 0), (2, 6), (1, FEET - 2)], [(4, 0), (6, 6), (7, FEET - 1)]),
    legs([(4, 0), (5, 5), (3, FEET - 4)], [(4, 0), (4, 7), (4, FEET - 1)]),
    legs([(4, 0), (6, 6), (7, FEET - 1)], [(4, 0), (3, 6), (1, FEET - 2)]),
    legs([(4, 0), (4, 7), (4, FEET - 1)], [(4, 0), (6, 5), (4, FEET - 4)]),
]
for i in range(4):
    MC["p_leg_right%d" % i] = _S[i]
    MC["p_leg_left%d" % i] = mirror(_S[i])

# Slain: a heap of cloak, the torch gone out beside it.
MC["p_fallen"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "....KKKK....",
    "..KKCCCCKK..",
    ".KCCCCCCCCK.",
    "KCCKCCCCCCCK",
    "KCOKCCCKCCCK",
    "KOOKCCKCCCCK",
    ".KKKKKKKKKK.",
    "K.K.K.K.K.K.",
    ".K.K.K.K.K.K",
    "............",
    "............",
    "............",
    "............",
]

import characters_others as _others   # noqa: E402  (adds its frames to MC / HI)

# The order of the frames in the two sheets = the frame numbers in Sprites.cs.
MC_ORDER = (
    ["p_top_down", "p_top_up", "p_top_left", "p_top_right", "p_top_attack"]
    + ["p_leg_%s%d" % (d, i) for d in ("down", "left", "right") for i in range(4)]
    + ["p_fallen"] + _others.MC_NAMES
)
HI_ORDER = ["p_ov_down", "p_ov_up", "p_ov_left", "p_ov_right", "p_ov_attack"] + _others.HI_NAMES


def player(d, step):
    legs_ = "down" if d in ("up", "attack") else d
    return [("p_ov_" + d, 0, 0), ("p_top_" + d, 0, 0), ("p_leg_%s%d" % (legs_, step), 0, 21)]


def previews():
    walk = [("%s%d" % (d, i), player(d, i)) for d in ("down", "up", "left", "right") for i in range(4)]
    others = _others.preview_groups() + [("fallen", [("p_fallen", 0, 21)]), ("scale", player("down", 1)),
                                         ("attack", player("attack", 0))]
    return [("player", walk, dict(cols=8, scale=6)), ("player_small", walk, dict(cols=16, scale=2)),
            ("others", others, dict(cols=10, scale=5, cell=(34, 50)))]
