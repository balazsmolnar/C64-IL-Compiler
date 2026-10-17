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

# Giant spider, rearing: a glossy abdomen held high behind, the front legs
# raised to strike, eight pinpoint eyes, fangs; the walking legs below.
# Black and brown with violet gloss and eyes.
MC["bspider_top"] = [
    "K..........K",
    "NK..KKKK..KN",
    "KN.KPPPNK.NK",
    ".KNKPKPPNKNK",
    ".KNKPPKPNKN.",
    "..KNKPPNKNK.",
    "..KNKPNNKNK.",
    "...KNKKKNK..",
    "...KNNNNNK..",
    "..KNPKNPNNK.",
    "..KNNPNNPNK.",
    "...KPNPPNK..",
    "...KNNNNNK..",
    "....KPKPK...",
    "....KPKPK...",
    "..KNK.K.KNK.",
    ".KNK.....KNK",
    ".KNK.....KNK",
    "KNK.......KN",
    "KNK.......KN",
    "KK.........K",
]
MC["bspider_leg0"] = grounded([
    "KNK.......KN",
    "KNK..KKK..KN",
    "KNK.KNNNK.KN",
    "KNKKNKKKNKKN",
    ".KNNK...KNN.",
    "..KNK...KNK.",
    ".KNK.....KNK",
    ".KNK.....KNK",
    "KNK.......KN",
    "KNK.......KN",
    "KK.........K",
], half=6)
MC["bspider_leg1"] = grounded([
    ".KNK.....KNK",
    ".KNK.KKK.KNK",
    "..KNKNNNKNK.",
    "..KNKKKKKNK.",
    ".KNNK...KNNK",
    "KNK.......KN",
    "KNK.......KN",
    ".KNK.....KNK",
    "..KNK...KNK.",
    "..KNK...KNK.",
    "..KK.....KK.",
], half=6)

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
    "............",
    "............",
    "............",
    "........KKK.",
    ".......KLLNK",
    "......KLLLNK",
    "..KK..KLLNNK",
    ".KNNKKKKLNK.",
    "KNNKKNNKKK..",
    "KNKKNNK.....",
    ".KK.KK.K.K..",
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
for _n, _rows in (("bat0", _bat_up), ("bat1", _bat_level), ("bat2", _bat_down)):
    MC[_n] = done([list(r) for r in _rows] + [list("............") for _ in range(21 - len(_rows))])


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


MC["wraith_top"] = _rim(MC["wraith_top"], "B", 7, 20)
MC["wraith_leg0"] = _rim(MC["wraith_leg0"], "B", 0, 4)
MC["wraith_leg1"] = _rim(MC["wraith_leg1"], "B", 0, 4)
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


MC["bspider_leg0"] = _rim(MC["bspider_leg0"], "P", 0, 4)
MC["bspider_leg1"] = _rim(MC["bspider_leg1"], "P", 0, 4)

MC_NAMES = ["skel_top", "skel_leg0", "skel_leg1",
            "ghoul_top", "ghoul_leg0", "ghoul_leg1",
            "wraith_top", "wraith_leg0", "wraith_leg1",
            "bspider_top", "bspider_leg0", "bspider_leg1",
            "treasure", "sword", "axe",
            "bat0", "bat1", "bat2"]
HI_NAMES = ["spider0", "spider1", "spider2", "spark"]


def stack(n, step, bob=0):
    return [(n + "_top", 0, bob), ("%s_leg%d" % (n, step), 0, 21)]


def preview_groups():
    out = []
    for n in ("skel", "ghoul", "wraith", "bspider"):
        out += [(n, stack(n, 0)), (n, stack(n, 1))]
    out += [("treasure", [("spark", 2, 8), ("treasure", 0, 12)]),
            ("sword", [("spark", 6, 12), ("sword", 0, 12)]), ("axe", [("axe", 0, 12)])]
    out += [(n, [(n, 0, 12)]) for n in ("bat0", "bat1", "bat2", "spider0", "spider1", "spider2", "spark")]
    return out
