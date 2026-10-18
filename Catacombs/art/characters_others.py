"""
The guardians, the vermin and the pickups (see characters_frames.py for the
drawing helpers and characters_art.py for the colour letters).

Every multicolour frame shares black (outline, deepest shadow) and brown
(shadow, wood, leather, rust) and has one colour of its own; the key light
is upper left, as on the adventurer.
"""
from characters_frames import MC, HI, done, hi, shadow

# ------------------------------------------------------------------ guardians
# Two sprites stacked: top, then legs with the soles on row GFEET. Two legs
# frames each, the top stays.
GFEET = 17


def grounded(rows, half=5, cx=6):
    g = [list(r) for r in rows]
    g += [list("............") for _ in range(21 - len(g))]
    shadow(g, cx, GFEET, half)
    return done(g)


# Skeleton warrior: aged bone (light grey, brown in shadow), deep orbits with
# a dull ember in each, a broken blade gripped upright, a battered shield.
MC["skel_top"] = [
    "L...........",
    "LN...KKKK...",
    "LN..KLLLLNK.",
    "LN.KLLLLLLNK",
    "LN.KLKKLKKNK",
    "LN.KLKNLKNNK",
    "LN.KLLLKLNNK",
    "LN..KLKLKNK.",
    "LN..KKLKLKK.",
    "NNN..KKKKK..",
    "KLK...KLK...",
    "KLK.KLLLLLK.",
    "KLKKLKLKLKKK",
    "KLLKKLLLKNNN",
    ".KLKLKLKKNLN",
    "..KKKLLLKNNN",
    "....KKLKKKNK",
    ".....KLK..K.",
    "....KLLLNK..",
    "...KLKKKLNK.",
    "...KLK.KLNK.",
]
_skel = [
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "..KLLNKLLNK.",
    "...KNK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "...KLK.KNK..",
    "..KLLK.KLNK.",
    ".KLNNK.KLNNK",
    ".KKKK..KKKK.",
]
MC["skel_leg0"] = grounded(_skel)
MC["skel_leg1"] = grounded([
    "...KLK.KNK..",
    "...KLK..KNK.",
    "...KLK..KNK.",
    "..KLK...KNK.",
    "..KLLNK.KLLNK"[:12],
    "..KNK..KNK..",
    "..KLK..KNK..",
    "..KLK..KNK..",
    "..KLK..KNK..",
    "..KLK.KNK...",
    "..KLK.KNK...",
    "..KLK.KNK...",
    "..KLK.KLNK..",
    ".KLLK.KLNNK.",
    "KLNNK..KKKK.",
    "KKKK........",
])

# Ghoul: hunched, the head thrust forward under a humped back, green rotting
# flesh, eyes glowing in black sockets, arms bent and hanging to the knee,
# three long claws on each hand, knees bent.
MC["ghoul_top"] = [
    "............",
    "............",
    "............",
    "........KK..",
    "......KKGNK.",
    ".....KGGGNNK",
    "..KKKGGGGGNK",
    ".KGGGKGGGGNK",
    "KGGGGNKGGGNK",
    "KGKKGNKGGGNK",
    "KGKGKNKGGNNK",
    "KNKKKNKGGNNK",
    ".KGKGKKGGNNK",
    "..KKKGGGGNNK",
    ".KGGKGGGNNNK",
    "KGGNKGGNKNNK",
    "KGNKKGGGKNK.",
    "KGNK.KGGNNK.",
    "KGNK.KGGGNK.",
    "KNNK.KGGNNK.",
    ".KNK..KNNK..",
]
_ghoul = [
    ".KNK.KGGNKGK",
    "KGNK.KGNKGNK",
    "KGK.KGNNKGNK",
    "KGNK.KNKKGNK",
    "K.K.KGGNKKNK",
    "....KGNKNK.K",
    "...KGNKKGNK.",
    "...KGK..KGK.",
    "..KGNK..KGNK",
    "..KGK...KGNK",
    "..KGNK..KGK.",
    "..KNGK.KGNK.",
    ".KGGNK.KGNK.",
    "KGGNNK.KGGNK",
    "KKKKK..KKKKK",
]
MC["ghoul_leg0"] = grounded(_ghoul)
MC["ghoul_leg1"] = grounded([
    ".KNK.KGGNKGK",
    "KGNK.KGNKGNK",
    "KGNK.KGNNKGK",
    "KGK.KGNKKGNK",
    "KGNKKGNKKGNK",
    "K.K.KGNKK.K.",
    "...KGNKKGK..",
    "...KGK.KGNK.",
    "..KGNK.KGNK.",
    "..KGK...KGNK",
    "..KGNK..KGNK",
    "..KNGK..KNGK",
    ".KGGNK.KGGNK",
    "KGGNNK.KKKKK",
    "KKKKK.......",
])

# Wraith: a dark shape that the light barely finds -- a robe of black and
# brown, an asymmetric hood around nothing, two cold eyes and bony hands in
# its own pale blue, the hem dissolving into dither above the floor.
MC["wraith_top"] = [
    "......K.....",
    ".....KNK....",
    "....KNNKK...",
    "...KNBNNNK..",
    "..KNBNKKNNK.",
    "..KBNKKKKNK.",
    ".KNBKBKKBKNK",
    ".KNNKKKKKKNK",
    ".KNBNKKKKNNK",
    "KNBNNNKKNNNK",
    "KBNNNNNNNKNK",
    "KBNKNNNNKNNK",
    "BKBKNNNNKKNK",
    "BKBK.KNNK.KB",
    "B.B..KNNK.BK",
    ".....KNNNK.B",
    "....KNKNNK..",
    "....KNNKNK..",
    "...KNKNNNNK.",
    "...KNNKNKNK.",
    "..KNKNNKNNK.",
]
MC["wraith_leg0"] = grounded([
    "..KNNKNKNNK.",
    "..KNKNNNKNK.",
    ".KNNKNKNNNK.",
    ".KNKNNKNKNK.",
    ".KNK.NKN.KNK",
    ".K.N.K.N..K.",
    "..K...K.N...",
    "......K.....",
], half=4)
MC["wraith_leg1"] = grounded([
    "..KNNKNKNNK.",
    "..KNKNNKNNK.",
    ".KNNNKNKNKK.",
    ".KNKNKNNKNK.",
    "KNK.KNNK.KNK",
    ".K..N.KN.K..",
    "....K..N....",
    ".......K....",
], half=4)

# Giant spider: wide and low, 48 pixels (two sprites side by side). A big
# abdomen at the back with a gloss on its upper left, a black head with an
# eye cluster and fangs in front, four bent legs a side splayed out, in the
# spider's violet on black (brown only shades the abdomen). The legs swap in
# pairs between the two frames.
def _line(g, a, b, letter):
    (x0, y0), (x1, y1) = a, b
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    for i in range(n + 1):
        x = int(round(x0 + (x1 - x0) * i / n))
        y = int(round(y0 + (y1 - y0) * i / n))
        if 0 <= x < len(g[0]) and 0 <= y < 21:
            g[y][x] = letter


def _blob(g, cx, cy, rx, ry, letter):
    for y in range(21):
        for x in range(len(g[0])):
            if ((x + 0.5 - cx) / rx) ** 2 + ((y + 0.5 - cy) / ry) ** 2 <= 1.0:
                g[y][x] = letter


def big_spider(step):
    g = [["."] * 24 for _ in range(21)]
    # (hip, knee, foot) of the left legs, front to back; mirrored for the right
    legs = [((10, 12), (6, 10), (4, 17)), ((10, 11), (4, 7), (1, 14)),
            ((10, 10), (4, 4), (0, 9)), ((10, 9), (6, 2), (3, 5))]
    for i, (h, k, f) in enumerate(legs):
        for side in (0, 1):
            up = ((i + side + step) % 2) == 1
            k2 = (k[0], k[1] - (1 if up else 0))
            f2 = (f[0] + (1 if up else 0), f[1] - (2 if up else 0))
            m = (lambda p: p) if side == 0 else (lambda p: (23 - p[0], p[1]))
            _line(g, m(h), m(k2), "P")
            _line(g, m(k2), m(f2), "P")
    snap = [r[:] for r in g]
    for y in range(21):
        for x in range(24):
            if snap[y][x] == "P":
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < 24 and 0 <= ny < 21 and g[ny][nx] == ".":
                        g[ny][nx] = "K"
    _blob(g, 12, 6.0, 5.6, 4.6, "K")
    _blob(g, 12, 6.0, 4.6, 3.6, "N")
    _blob(g, 11, 5.0, 3.0, 2.2, "P")
    for x, y in ((12, 7), (13, 8), (11, 8)):
        g[y][x] = "K"
    _blob(g, 12, 12.0, 3.4, 2.8, "K")
    for x, y in ((10, 11), (13, 11), (11, 12), (12, 12), (10, 13), (13, 13)):
        g[y][x] = "P"
    for x, y in ((11, 15), (12, 15)):
        g[y][x] = "P"
    for dy in range(3):
        for x in range(4, 20):
            if g[17 + dy][x] == "." and (x + dy) % 2 == 0:
                g[17 + dy][x] = "K"
    rows = done(g)
    return [r[:12] for r in rows], [r[12:] for r in rows]


for _i in range(2):
    MC["bspider_l%d" % _i], MC["bspider_r%d" % _i] = big_spider(_i)

# ------------------------------------------------------------------- pickups
# Lying on the floor, at an angle; soles of the shadow on row 17.
MC["treasure"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "......KK....",
    ".....KYNK...",
    "....KYYNK...",
    ".....KNK....",
    "....KYYK.K..",
    "..KKYYNNKYK.",
    ".KYNYYNYNYNK",
    "KYYNYNYYNNYK",
    "KYNYYYNYNYNK",
    ".KNYNYNNYNK.",
    "..KKNKKNKKYK",
    "....K.K.KKK.",
    "...K.K.K.K..",
    "............",
    "............",
]
MC["sword"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "..........KK",
    "........KKLK",
    "......KKLLNK",
    ".KK.KKLLNNK.",
    "KNNKLKLNKK..",
    "KNKLKKKK....",
    ".KNLK.......",
    "..KK.K.K.K..",
    "...K.K.K.K..",
    "............",
    "............",
]
MC["axe"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "........KK..",
    ".......KLLK.",
    "......KLLLNK",
    "......KLLNNK",
    "..KK..KLNNNK",
    ".KNNKKKNNNK.",
    "KNNKKNKKKK..",
    "KNKKNNK.....",
    ".KKKNK......",
    "...KK.K.K...",
    "..K.K.K.K.K.",
    "...K.K.K.K..",
    "............",
    "............",
]

# Bat (multicolour): black body and ears, brown wing membranes on black
# finger bones, red eyes. Wings up, level, down, glide.
_bat_up = [
    "............",
    "............",
    "............",
    "KK........KK",
    "KNK......KNK",
    ".KNK....KNK.",
    ".KNNK..KNNK.",
    "..KNNKKNNK..",
    "..KNNKRKNK..",
    "...KNKKKNK..",
    "....KKKKK...",
    ".....K.K....",
]
_bat_level = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "....K..K....",
    "KK..KKKK..KK",
    "KNKKKRKRKKNK",
    ".KNNNKKKNNK.",
    "..KKNNKNKK..",
    "....K.KK....",
]
_bat_down = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "....K..K....",
    "...KKKKKK...",
    "..KNKRKRKNK.",
    ".KNNKKKKKNNK",
    "KNNK.KKK.KNK",
    "KNK.......KN",
    "KK.........K",
]
_bat_glide = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "....K..K....",
    ".KK.KKKK.KK.",
    "KNNKKRKRKNNK",
    "KNNNKKKKNNNK",
    ".KKNKKKKNKK.",
    "...K..KK....",
]
_bat_fix = lambda rows: [r.replace("N", "M").replace("R", "N") for r in rows]
for _n, _rows in (("bat0", _bat_up), ("bat1", _bat_level), ("bat2", _bat_down)):
    MC[_n] = done([list(r) for r in _bat_fix(_rows)] + [list("............") for _ in range(21 - len(_rows))])


# -------------------------------------------------------------- hi-res frames
# Small spider (black): a round body, eight bent legs with gaps between them,
# swapping stances while it runs, spread wide when it stops.
def _spider(legs):
    rows = {14: "10-13", 15: "9-14", 16: "9-14", 17: "10-13"}
    for y, xs in legs.items():
        rows[y] = (rows.get(y, "") + " " + xs).strip()
    return hi(rows, "K")


HI["spider0"] = _spider({12: "6 17", 13: "5 7 16 18", 14: "4 8 15 19", 15: "3 8 15 20",
                         16: "4 7 16 19", 17: "6 17", 18: "5 7 16 18", 19: "4 8 15 19"})
HI["spider1"] = _spider({12: "5 18", 13: "6 8 15 17", 14: "7 16 3 20", 15: "4 7 16 19",
                         16: "3 8 15 20", 17: "5 18", 18: "6 8 15 17", 19: "7 16"})
HI["spider2"] = _spider({11: "4 19", 12: "5 18", 13: "6 8 15 17", 14: "2 7 16 21", 15: "3 8 15 20",
                         16: "2 8 15 21", 17: "3 7 16 20", 18: "2 5 18 21", 19: "1 4 19 22"})
HI["spark"] = hi({7: "12", 8: "12", 9: "11-13", 10: "8-10 12 14-16", 11: "11-13", 12: "12", 13: "12"}, "W")

def _rim(rows, letter, first=0, last=20, width=1):
    """The light catching the left (lit) edge: the first `width` brown pixels
    after the outline on each row become the creature's own colour."""
    out = []
    for i, r in enumerate(rows):
        if first <= i <= last:
            r = list(r)
            k = next((x for x, c in enumerate(r) if c == "K"), None)
            n = 0
            if k is not None:
                for x in range(k + 1, len(r)):
                    if r[x] == "N" and n < width:
                        r[x] = letter
                        n += 1
                    elif r[x] not in "N" + letter:
                        break
            r = "".join(r)
        out.append(r)
    return out


def _robe(rows, eyes=()):
    """The wraith's robe in its own dark grey: every brown or pale pixel
    becomes grey, except the last one of each run (the shadow side, brown);
    the eyes are dull embers (brown) in the void."""
    out = []
    for y, r in enumerate(rows):
        r = list(r.replace("B", "N"))
        for x in range(len(r)):
            if r[x] == "N" and x + 1 < len(r) and r[x + 1] == "N":
                r[x] = "D"
        for ex, ey in eyes:
            if ey == y:
                r[ex] = "N"
        out.append("".join(r))
    return out


MC["wraith_top"] = _robe(MC["wraith_top"], ((5, 6), (8, 6)))
MC["wraith_leg0"] = _robe(MC["wraith_leg0"])
MC["wraith_leg1"] = _robe(MC["wraith_leg1"])
def _body(rows, letter, first, last):
    """Lights a body: in each run of brown, all but its last (shadow-side)
    pixel become the creature's own colour."""
    out = []
    for i, r in enumerate(rows):
        if first <= i <= last:
            r = list(r)
            for x in range(len(r)):
                if r[x] == "N" and x + 1 < len(r) and r[x + 1] in "N" + letter:
                    r[x] = letter
            r = "".join(r)
        out.append(r)
    return out




MC_NAMES = ["skel_top", "skel_leg0", "skel_leg1",
            "ghoul_top", "ghoul_leg0", "ghoul_leg1",
            "wraith_top", "wraith_leg0", "wraith_leg1",
            "bspider_l0", "bspider_l1", "bspider_r0", "bspider_r1",
            "treasure", "sword", "axe",
            "bat0", "bat1", "bat2"]
HI_NAMES = ["spider0", "spider1", "spider2", "spark"]


def stack(n, step, bob=0):
    return [(n + "_top", 0, bob), ("%s_leg%d" % (n, step), 0, 21)]


def preview_groups():
    out = []
    for n in ("skel", "ghoul", "wraith"):
        out += [(n, stack(n, 0)), (n, stack(n, 1))]
    for i in range(2):
        out.append(("bspider", [("bspider_l%d" % i, 0, 12), ("bspider_r%d" % i, 24, 12)]))
        out.append(("", []))
    out += [("treasure", [("spark", 2, 8), ("treasure", 0, 12)]),
            ("sword", [("spark", 6, 12), ("sword", 0, 12)]), ("axe", [("axe", 0, 12)])]
    out += [(n, [(n, 0, 12)]) for n in ("bat0", "bat1", "bat2", "spider0", "spider1", "spider2", "spark")]
    return out
