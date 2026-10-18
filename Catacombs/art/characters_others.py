"""
The guardians, the vermin and the pickups (see characters_frames.py, which
holds the adventurer and the drawing helpers, and characters_art.py for the
colour letters).
"""
from characters_frames import MC, HI, blank, done, hi, outline, shadow

# ------------------------------------------------------------------ guardians
# Two sprites tall (or, the spider, two wide); soles on row GFEET of the bottom.
GFEET = 17


def variant(rows, edits):
    out = list(rows)
    for y, r in edits.items():
        out[y] = r
    return out


def grounded(rows, half=5, cx=6, y=GFEET):
    g = [list(r) for r in rows]
    shadow(g, cx, y, half)
    return done(g)


# Skeleton warrior: bone white, a rusted blade raised, a round shield.
MC["skel_top"] = [
    "O...........",
    "O...KKKK....",
    "O..KWWWWK...",
    "O.KWWWWWWK..",
    "O.KWKWWKWK..",
    "O.KWOWWOWK..",
    "O.KWWKKWWK..",
    "O..KWWWWK...",
    "O..KWKWKK...",
    "O...KKKK....",
    "OKKKKWWKKKK.",
    "WWKWWWWWWKWK",
    "KKKKWKKWKKWK",
    "...WWWWWKOOK",
    "...KWKKKOOOO",
    "...WWWWKOKKO",
    "...KWKKKOKKO",
    "....WWWKOOOO",
    "....KWWKKOOK",
    "...KWWWWKKK.",
    "...KWKKWK...",
]
_skel_legs = [
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "..KWWKKWWK..",
    "..KWWKKWWK..",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "...KWKKWK...",
    "..KWWKKWWK..",
    ".KWWWKKWWWK.",
    ".KKKKKKKKKK.",
    "............",
    "............",
    "............",
]
MC["skel_leg0"] = grounded(_skel_legs)
MC["skel_leg1"] = grounded(variant(_skel_legs, {
    2: "..KWK.KWK...", 3: "..KWK.KWK...", 4: ".KWWK.KWWK..", 5: ".KWWK.KWWK..",
    6: "..KWK..KWK..", 7: "..KWK..KWK..", 8: "..KWK..KWK..", 9: "..KWK..KWK..",
    10: "..KWK..KWK..", 11: "..KWK..KWK..", 12: "..KWK..KWK..", 13: ".KWWK..KWK..",
    14: "KWWWK..KWK..", 15: "KKKKK.KWWK..", 16: ".....KWWWK..", 17: ".....KKKKK..",
}))

# Ghoul: hunched, arms to its knees, ember eyes.
MC["ghoul_top"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "....KKKK....",
    "...KGGGGK...",
    ".KKKGGGGKKK.",
    "KGGKKGGKKGGK",
    "KGGGOGGOGGGK",
    "KGGGGGGGGGGK",
    "KGGGKKKKGGGK",
    "KGGGOKOKGGGK",
    "KGGKGGGGKGGK",
    "KGGKGGGGKGGK",
    "KGGKGKKGKGGK",
    "KGGKGGGGKGGK",
    "KGGKGKKGKGGK",
    "KGGKGGGGKGGK",
    "KGGKGGGGKGGK",
    "KGGKOOOOKGGK",
]
_ghoul_legs = [
    "KGGKOOOOKGGK",
    "KGGKOOOOKGGK",
    "KGGKOOKOKGGK",
    "KGGKGOKGKGGK",
    "KGGKGKKGKGGK",
    "KGGKGKKGKGGK",
    "KGKKGKKGKKGK",
    "OKOKGKKGKOKO",
    "O.OKGKKGKO.O",
    "...KGKKGK...",
    "...KGKKGK...",
    "...KGKKGK...",
    "...KGKKGK...",
    "...KGKKGK...",
    "...KGKKGK...",
    "..KGGKKGGK..",
    ".KGGGKKGGGK.",
    ".KKKKKKKKKK.",
    "............",
    "............",
    "............",
]
MC["ghoul_leg0"] = grounded(_ghoul_legs)
MC["ghoul_leg1"] = grounded(variant(_ghoul_legs, {
    6: "KGGKGKKGKGGK", 7: "KGKKGKKGKOKO", 8: "OKOKGKKGKO.O", 9: "O.OKGKKGK...",
    13: "...KGKKGGK..", 14: "...KGKKGGGK.", 15: "..KGGKKKKKK.", 16: ".KGGGK......", 17: ".KKKKK......",
}))

# Wraith: an empty hood, bone hands, a robe that ends in rags above the floor.
MC["wraith_top"] = [
    ".....KK.....",
    "....KBBK....",
    "...KBBBBK...",
    "..KBBBBBBK..",
    "..KBKKKKBK..",
    ".KBKKKKKKBK.",
    ".KBKOKKOKBK.",
    ".KBKKKKKKBK.",
    ".KBBKKKKBBK.",
    "KBBBBKKBBBBK",
    "KBBBBBBBBBBK",
    "KBBKBBBBKBBK",
    "KBKKBBBBKKBK",
    "KBK.KBBK.KBK",
    "OKO.KBBK.OKO",
    "O.O.KBBK.O.O",
    "....KBBBK...",
    "...KBBBBK...",
    "...KBBBBK...",
    "...KBBBBBK..",
    "..KBBBBBBK..",
]
_empty = ["............"] * 11
MC["wraith_leg0"] = grounded([
    "..KBBBBBBK..",
    "..KBBBBBBK..",
    "..KBBBBBBBK.",
    ".KBBBBBBBBK.",
    ".KBBBKBBBBK.",
    ".KBBKKBBKBK.",
    ".KBK.KBBKKBK",
    ".KK..KBK..K.",
    ".....KBK....",
    "......K.....",
] + _empty, half=4)
MC["wraith_leg1"] = grounded([
    "..KBBBBBBK..",
    "..KBBBBBBK..",
    ".KBBBBBBBK..",
    ".KBBBBBBBBK.",
    ".KBBBBKBBBK.",
    ".KBKBBKKBBK.",
    "KBKKBBK.KBK.",
    ".K..KBK..KK.",
    "....KBK.....",
    ".....K......",
] + _empty, half=4)


# Giant spider: 48 pixels wide, two sprites side by side.
def _line(g, a, b, letter):
    (x0, y0), (x1, y1) = a, b
    n = max(abs(x1 - x0), abs(y1 - y0), 1)
    for i in range(n + 1):
        x = int(round(x0 + (x1 - x0) * i / n))
        y = int(round(y0 + (y1 - y0) * i / n))
        if 0 <= x < len(g[0]) and 0 <= y < 21:
            g[y][x] = letter


def _ellipse(g, cx, cy, rx, ry, letter):
    for y in range(21):
        for x in range(len(g[0])):
            if ((x + 0.5 - cx) / rx) ** 2 + ((y + 0.5 - cy) / ry) ** 2 <= 1.0:
                g[y][x] = letter


def big_spider(step):
    g = blank(24)
    floor = GFEET
    # legs: (shoulder, knee, foot) for the left side; mirrored for the right
    legs_ = [((9, 11), (4, 3), (1, floor)), ((9, 10), (6, 5), (4, floor)),
             ((9, 12), (5, 8), (6, floor - 1)), ((9, 9), (2, 6), (0, floor - 4))]
    for i, (s, k, f) in enumerate(legs_):
        lift = 1 if (i + step) % 2 else 0
        for side in (0, 1):
            m = (lambda p: p) if side == 0 else (lambda p: (23 - p[0], p[1]))
            up = lift if side == 0 else 1 - lift
            k2 = (k[0], k[1] - up)
            f2 = (f[0] + (up if i < 2 else 0), f[1] - 2 * up)
            _line(g, m(s), m(k2), "R")
            _line(g, m(k2), m(f2), "R")
            x, y = m(k2)
            g[y][x] = "O"
    outline(g, "RO")
    _ellipse(g, 12, 7.5, 5.2, 5.5, "R")          # abdomen
    _ellipse(g, 12, 14, 3.2, 3.2, "R")           # head
    outline(g, "R")
    for x, y in ((11, 6), (12, 6), (11, 8), (12, 8), (10, 7), (13, 7)):   # the mark on its back
        g[y][x] = "O"
    for x, y in ((11, 5), (12, 5), (11, 9), (12, 9), (9, 4), (14, 4), (8, 8), (15, 8), (10, 11), (13, 11)):
        g[y][x] = "K"
    for x, y in ((10, 13), (13, 13), (11, 14), (12, 14)):                  # eyes
        g[y][x] = "O"
    for x, y in ((10, 16), (13, 16), (10, 17), (13, 17)):                  # fangs
        g[y][x] = "O"
    g[12][11] = g[12][12] = "K"
    for dy in range(3):
        for x in range(3, 21):
            if g[floor + dy][x] == "." and (x + dy) % 2 == 0:
                g[floor + dy][x] = "K"
    rows = done(g)
    return [r[:12] for r in rows], [r[12:] for r in rows]


for _i in range(2):
    MC["bspider_l%d" % _i], MC["bspider_r%d" % _i] = big_spider(_i)

# ------------------------------------------------------------------- pickups
MC["treasure"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "....KKKK....",
    "...KYYYYK...",
    "...KOYYOK...",
    "....KOOK....",
    ".....KK.....",
    "....KYYK....",
    "..KKKOOKKK..",
    ".KYYKKKKYYK.",
    "KYOYYKKYYOYK",
    "KOYYOYYOYYOK",
    "KYYOYYYYOYYK",
    ".KKKKKKKKKK.",
    "K.K.K.K.K.K.",
    ".K.K.K.K.K.K",
    "............",
]
MC["sword"] = [
    "............",
    ".....KK.....",
    "....KOOK....",
    "....KOOK....",
    ".....OO.....",
    ".....OO.....",
    "..KKKOOKKK..",
    ".KOOOOOOOOK.",
    "..KKKLLKKK..",
    "....KLLK....",
    "....KLLK....",
    "....KLLK....",
    "....KLLK....",
    "....KLLK....",
    "....KLLK....",
    "....KLLK....",
    "....KLLK....",
    "...KKLLKK...",
    "..K.KKKK.K..",
    "...K.K.K.K..",
    "............",
]
MC["axe"] = [
    "............",
    "..KKK.......",
    ".KLLLKKKK...",
    "KLLLLLOOK...",
    "KLLLLLOOK...",
    "KLLLLLOOK...",
    ".KLLLKOOK...",
    "..KKKKOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    ".....KOOK...",
    "....KKOOKK..",
    "...K.KKKK.K.",
    "....K.K.K.K.",
    "............",
]


# -------------------------------------------------------------- hi-res frames
# Small spider: eight one-pixel legs that swap between stances.
def small_spider(step):
    g = blank(24)
    cx, cy = 12, 15
    _ellipse(g, cx, cy - 0.5, 3.0, 2.2, "M")
    _ellipse(g, cx, cy + 1.5, 2.0, 1.6, "M")
    reach = [(7, -4, 9, 3), (6, -1, 8, 4), (5, 1, 6, 4), (4, 2, 4, 4)]
    for i, (kx, ky, fx, fy) in enumerate(reach):
        lift = (0, 1, 0)[step] if i % 2 == 0 else (1, 0, 0)[step]
        spread = (0, 0, 1)[step]
        for side in (-1, 1):
            k = (cx + side * (kx + spread), cy + ky - lift)
            f = (cx + side * (fx + spread), cy + fy - 2 * lift)
            s = (cx + side * 2, cy + (i - 1))
            _line(g, s, k, "M")
            _line(g, k, f, "M")
    return done(g)


for _i in range(3):
    HI["spider%d" % _i] = small_spider(_i)

HI["bat0"] = hi({
    4: "2-3 20-21", 5: "2-5 18-21", 6: "3-8 15-20 11 12", 7: "4-10 13-19", 8: "5-18", 9: "7-16",
    10: "9-14", 11: "10-13", 12: "11-12",
}, "M")
HI["bat1"] = hi({
    7: "11 12", 8: "8-15", 9: "5-18", 10: "3-20", 11: "2-8 10-13 15-21", 12: "2-5 11-12 18-21",
    13: "2-3 20-21", 14: "2 21",
}, "M")
HI["bat2"] = hi({
    7: "11 12", 8: "2-4 9-14 19-21", 9: "2-21", 10: "4-19", 11: "7-8 10-13 15-16", 12: "11-12",
}, "M")
HI["spark0"] = hi({9: "12", 10: "11-13", 11: "12"}, "W")
HI["spark1"] = hi({6: "12", 7: "12", 8: "12", 9: "11-13", 10: "8-16", 11: "11-13", 12: "12", 13: "12", 14: "12"}, "W")

MC_NAMES = ["skel_top", "skel_leg0", "skel_leg1",
            "ghoul_top", "ghoul_leg0", "ghoul_leg1",
            "wraith_top", "wraith_leg0", "wraith_leg1",
            "bspider_l0", "bspider_l1", "bspider_r0", "bspider_r1",
            "treasure", "sword", "axe"]
HI_NAMES = ["spider0", "spider1", "spider2", "bat0", "bat2", "bat1", "spark0", "spark1"]


def stack(n, step):
    return [(n + "_top", 0, 0), ("%s_leg%d" % (n, step), 0, 21)]


def preview_groups():
    out = []
    for n in ("skel", "ghoul", "wraith"):
        out += [(n, stack(n, 0)), (n, stack(n, 1))]
    for i in range(2):
        out.append(("bspider", [("bspider_l%d" % i, 0, 12), ("bspider_r%d" % i, 24, 12)]))
        out.append(("", []))
    out += [("treasure", [("spark1", 4, -2), ("treasure", 0, 12)]),
            ("sword", [("sword", 0, 12)]), ("axe", [("axe", 0, 12)])]
    out += [(n, [(n, 0, 12)]) for n in HI_NAMES]
    return out
