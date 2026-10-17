"""
The adventurer (see characters_frames.py for the helpers and the colours).

Three sprites: a hi-res yellow overlay (the torch flame's core and the
torchlight catching his edges), a multicolour top half and multicolour legs.
Every multicolour frame is black (outline, deepest shadow), brown (shadow,
leather, the face inside the hood) and red (his cloak, lit). The key light is
the room's, upper left: lit edges left and top, brown to the right and below.

The legs sprite carries the hem of the cloak, so the cloak swings with the
stride and there is no seam where the two sprites meet: the top half ends
in the middle of the cloak, the legs half carries on.
"""
from characters_frames import MC, HI, hi, mirror, shadow, done

# ------------------------------------------------------------------- tops
# Facing the viewer. Torch in his right hand (screen left), held at the
# shoulder; the flame red with a yellow hi-res core.
TOP_DOWN = [
    ".R..........",
    ".RR..KKK....",
    "RRR.KRRRK...",
    "RRRKRRRRNK..",
    ".R.KRRRRNK..",
    ".N.KRKKKNK..",
    ".N.KRNNNKK..",
    ".N.KRKNKNK..",
    ".N.KRNNNKK..",
    "KNK.KKNKK...",
    "KNNKRRKRRK..",
    "KNNRRRRRRNK.",
    ".KRRRRRRRRNK",
    "..KRRRRKRRNK",
    "..KRRRKNKRNK",
    "..KRRRKNKRNK",
    "..KRRKKKKRNK",
    "..KRRRKNKRNK",
    "..KRNRKNKNNK",
    "..KNRNKNKRNK",
    "..KRNRKNKNNK",
]
OV_DOWN = {
    0: "3", 1: "3-4", 2: "2-4 10", 3: "1-4 8", 4: "3 8",
    6: "10-11", 7: "12", 8: "10",
    10: "2 8-9", 11: "6-7", 12: "4-5",
    13: "6", 14: "6", 15: "6", 16: "12-13", 17: "6", 18: "6", 19: "6",
}

# Walking away: the torch is in the same hand, now on screen right.
TOP_UP = [
    "..........R.",
    "....KKK..RR.",
    "...KRRRK.RRR",
    "..KRRRRRKRRR",
    "..KRRRRNK.R.",
    "..KRRRRNK.N.",
    "..KRRRRNK.N.",
    "..KRRRNNK.N.",
    "..KRRRNNK.N.",
    "...KRRNK.KNK",
    "..KRRRRNKNNK",
    ".KRRRRRRNNNK",
    "KRRRRRRRNNK.",
    "KRRRRRRNNNK.",
    "KRRRKRRRNNK.",
    "KRRRKRRRNNK.",
    "KRRRRKRRNNK.",
    "KRRRRKRRNNK.",
    "KRNRRKRRRNK.",
    "KNRNRRKNRNK.",
    "KRNRNRKRNNK.",
]
OV_UP = {
    0: "21", 1: "20-21", 2: "19-22 8-11", 3: "19-22 6", 4: "21 6",
    5: "6", 6: "6", 7: "6", 8: "6",
    10: "6", 11: "4", 12: "2", 13: "2", 14: "2", 16: "2", 17: "2", 18: "2", 19: "2",
}

# Walking right: the torch forward and up in a bent arm, a short shaft.
TOP_RIGHT = [
    "............",
    "...KKKK...R.",
    "..KRRRRK.RRR",
    ".KRRRRRRKRRR",
    ".KRRRRKKKKR.",
    ".KRRRKNNNK.N",
    ".KRRRKNKNNKN",
    ".KRRRKNNNK.N",
    "..KRRRKNKKNN",
    "..KRRRRKKNNK",
    ".KRRRRRRNNK.",
    ".KRRRRRNNK..",
    ".KRRRRRNNK..",
    "KRRRRRKNK...",
    "KRRRRRKNK...",
    "KRRRRRKNK...",
    "KRRRRRKKK...",
    "KRRRRRKNK...",
    "KRNRRRRKNK..",
    "KNRNRNRKNK..",
    "KRNRNRNRKK..",
]
OV_RIGHT = {
    1: "21", 2: "20-22 8-11", 3: "19-22 4-5", 4: "21 4",
    5: "4 15", 6: "4", 7: "4 15", 8: "6",
    10: "4 12-15", 11: "4", 12: "4", 13: "2", 14: "2", 15: "2", 17: "2", 18: "2", 19: "2",
}

MC["p_top_down"] = TOP_DOWN
MC["p_top_up"] = TOP_UP
MC["p_top_left"] = mirror(TOP_RIGHT)
MC["p_top_right"] = TOP_RIGHT
HI["p_ov_down"] = hi(OV_DOWN)
HI["p_ov_up"] = hi(OV_UP)
HI["p_ov_left"] = mirror(hi(OV_RIGHT))
HI["p_ov_right"] = hi(OV_RIGHT)

# Striking: the sword arm up (wind-up), then the slash down across him. The
# blade is the overlay's: a long bright edge.
TOP_RAISE = list(TOP_DOWN)
TOP_RAISE[3] = "RRRKRRRRNK.."
TOP_RAISE[8] = ".N.KRNNNKK.N"
TOP_RAISE[9] = "KNK.KKNKKKNK"
TOP_RAISE[10] = "KNNKRRKRRKNK"
TOP_RAISE[11] = "KNNRRRRRRRK."
TOP_RAISE[14] = "..KRRKNNKRK."
TOP_RAISE[15] = "..KRRKNNKRK."
MC["p_top_raise"] = TOP_RAISE
HI["p_ov_raise"] = hi({
    0: "3 21", 1: "3-4 21", 2: "2-4 10 21", 3: "1-4 8 21", 4: "3 8 21", 5: "21", 6: "21", 7: "19-23",
    8: "22", 6 + 0: "10-11 21", 10: "2 8-9", 11: "6-7", 12: "4-5",
    13: "6", 14: "6", 15: "6", 16: "13-14", 17: "6", 18: "6", 19: "6",
})
TOP_SLASH = list(TOP_DOWN)
TOP_SLASH[11] = "KNNRRRRRRRRK"
TOP_SLASH[12] = ".KRRRRRRRRNN"
TOP_SLASH[13] = "..KRRRKNRRKN"
TOP_SLASH[14] = "..KRRKNNKRK."
MC["p_top_slash"] = TOP_SLASH
HI["p_ov_slash"] = hi({
    0: "3", 1: "3-4", 2: "2-4 10", 3: "1-4 8", 4: "3 8",
    6: "10-11", 7: "12", 8: "10", 10: "2 8-9", 11: "6-7", 12: "4-5 23",
    13: "6 22 23", 14: "6 21-22", 15: "6 20-21", 16: "13-14 19-20", 17: "6 18-19", 18: "6 17-18", 19: "6 16-17", 20: "15-16",
})

# ------------------------------------------------------------------- legs
# The legs sprite: the cloak hem (red, brown in shadow), leggings (brown,
# black on the shadow side), boots (brown, black soles). Soles on row FEET.
FEET = 14
HEM_FRONT = [
    "..KNNNKNKNNK",
    "..KNNNKNKNNK",
    "..KNKNKNKNKK",
    "...K.K.K.K..",
]


def front(left, right):
    """A front-view legs frame. `left` / `right`: the row of that leg's sole
    (FEET - 1 = planted; one lower = stepping towards us; higher = lifted,
    and a lifted leg's shin and boot swing out by a pixel)."""
    g = [list(r) for r in HEM_FRONT] + [list("............") for _ in range(17)]
    for x0, sole, out in ((4, left, -1), (7, right, 1)):
        lifted = sole < FEET - 1
        for y in range(4, sole + 1):
            x = x0 + (out if lifted and y > sole - 5 else 0)
            boot = y > sole - 4
            if y == sole:
                cells = "KKKK" if out < 0 else "KKKK"
                g[y][x - 1:x + 3] = list(cells)
            else:
                g[y][x - 1] = "K"
                g[y][x] = "N" if boot else "M"
                g[y][x + 1] = "K" if boot and y == sole - 1 and False else ("N" if boot else "M")
                g[y][x + 2] = "K"
        g[sole - 1][x0 + (out if lifted else 0) + (-1 if out < 0 else 2)] = "K"
    for y in range(3, 21):
        for x in range(12):
            pass
    shadow(g, 6, FEET, 5)
    return done(g)


# Towards the viewer: left foot forward (lower), passing (left planted,
# right lifted), right foot forward, passing the other way.
MC["p_leg_down0"] = front(FEET, FEET - 2)
MC["p_leg_down1"] = front(FEET - 1, FEET - 3)
MC["p_leg_down2"] = front(FEET - 2, FEET)
MC["p_leg_down3"] = front(FEET - 3, FEET - 1)


def side(rows):
    g = [list(r) for r in rows]
    g += [list("............") for _ in range(21 - len(g))]
    shadow(g, 5, FEET, 5)
    return done(g)


# Walking right: contact (heel down, back toe off), down (weight on the
# front leg, body dips), passing (back leg swings past, knee up), up (pushes
# off). The near leg is drawn over the far one; the hem trails behind.
MC["p_leg_right0"] = side([
    "KRRRRRRKNK..",
    "KRRRRRRKK...",
    ".KRRRRRRK...",
    "KRRKKRKKK...",
    ".KKKNNKNK...",
    "...KNKKNNK..",
    "..KNK..KNNK.",
    "..KNK...KNK.",
    ".KNK....KNNK",
    ".KNK....KNNK",
    "KNNK....KKNK",
    "KNK.....KNNN",
    "KK......KKKK",
    "............",
])
MC["p_leg_right1"] = side([
    "KRRRRRRKNK..",
    "KRRRRRRKK...",
    "KRRRRRRK....",
    ".KRKKRKK....",
    "..KNNKNK....",
    "..KNKKNNK...",
    "..KNNKKNNK..",
    "...KNK.KNK..",
    "...KNK.KNK..",
    "..KNNK.KNK..",
    "..KKNK.KNNK.",
    "...KK..KNNNK",
    "......KKKKKK",
    "............",
])
MC["p_leg_right2"] = side([
    "KRRRRRRKNK..",
    "KRRRRRRKK...",
    ".KRRRRRK....",
    "..KRKKRK....",
    "..KKNNNK....",
    "...KNKNNK...",
    "...KNKKNNK..",
    "...KNK.KNNK.",
    "..KNNK..KNK.",
    "..KNK...KNK.",
    "..KNNK..KKK.",
    "..KNNNK.....",
    "..KKKKK.....",
    "............",
])
MC["p_leg_right3"] = side([
    "KRRRRRRKNK..",
    "KRRRRRRKK...",
    "KRRRRRK.....",
    ".KKRKRK.....",
    "..KNNNK.....",
    "..KNKNNK....",
    "..KNKKNNK...",
    "..KNK.KNK...",
    "..KNK.KNK...",
    ".KNNK..KNK..",
    ".KNKK..KNNK.",
    "KNNK...KKKK.",
    "KKK.........",
    "............",
])


def _recolour(rows, hem, boots):
    """Hem rows: the cloak in shadow (brown); then the leggings in mid grey;
    the boots (from row `boots`) stay brown."""
    out = []
    for i, r in enumerate(rows):
        if i < hem:
            r = r.replace("R", "N")
        elif i < boots:
            r = r.replace("N", "M")
        out.append(r)
    return out


for _i in range(4):
    MC["p_leg_right%d" % _i] = _recolour(MC["p_leg_right%d" % _i], 4, 9)
    MC["p_leg_left%d" % _i] = mirror(MC["p_leg_right%d" % _i])

# Slain: on his back, hood to the left, a boot up, the torch dropped by his
# head (its ember is a hi-res spark in the overlay's slot, cooling off).
MC["p_fallen"] = [
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    "............",
    ".........KK.",
    "........KNNK",
    ".KKK...KRNK.",
    "KRRRKKKRRRK.",
    "KRKNKRRRRRNK",
    "KRNNKRRRNNNK",
    "KKNKRRRNNKKK",
    ".KKNNKKKNK..",
    "KNNNKKK.KK..",
    "K.K.K.K.K.K.",
    ".K.K.K.K.K.K",
    "............",
    "............",
    "............",
]

PLAYER_MC = (["p_top_down", "p_top_up", "p_top_left", "p_top_right", "p_top_raise", "p_top_slash"]
             + ["p_leg_%s%d" % (d, i) for d in ("down", "left", "right") for i in range(4)]
             + ["p_fallen"])
PLAYER_HI = ["p_ov_down", "p_ov_up", "p_ov_left", "p_ov_right", "p_ov_raise", "p_ov_slash"]
