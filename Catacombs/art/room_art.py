#!/usr/bin/env python3
"""
Generates the room art for Catacombs (see Catacombs/AAA-BRIEF.md, "Room
geometry"): the torch-lit stone room backdrop, the door / prop overlays, the
torch flame frames and the palette tables.

    python Catacombs/art/room_art.py            # everything
    python Catacombs/art/room_art.py --sheet    # also write preview sheets to art/src/preview_*.png

Writes PNGs to Catacombs/art/src/, .img files to Catacombs/art/build/ (the
files the .csproj embeds) and Catacombs/RoomArt.cs (the [RawImage]
declarations and the colour tables), all derived from this one script.

How the picture is made. The room is a small 3D model (a box seen in
one-point perspective, vanishing point (160, 56)); every multicolour pixel
is ray-cast to a surface and lands in a REGION (one stone block, one plank,
one flagstone...). Region borders become one-pixel black joints, the pixels
next to a joint become bevels (highlight on the side facing the nearest
torch, shadow on the other), the inside gets a little texture, and each
region is then lit as a whole from the two torches: its tones are positions
on a colour ramp that runs from black through the cold stone colours, with a
warm ramp near the flames. Only the wall right around a torch is shaded per
pixel (dithered). Small things (torches, bones, chains, webs, cracks) are
stamped on top as pixel maps.

Why per region: the picture has to fit in a few KB after LZ compression.
Dither patterns are aligned to the 4-pixel cell grid, so a lit stone and a
dark stone are the same bitmap bytes with different cell colours.
"""
import math
import os
import random
import sys

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import c64img   # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
P = c64img.PALETTE

W, H = 160, 168                 # multicolour pixels
F, CX, CY = 320.0, 160.0, 56.0  # focal length, vanishing point (screen px)
ZF = 320.0                      # far wall depth: 1 world unit = 1 screen px there
XW, YC, YF = 96.0, -32.0, 64.0  # half width, ceiling, floor
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]

TORCH_X = 48.0                  # torches at X = -48 / +48 on the far wall
TORCH_Y = 8.0
TORCHES = [(-TORCH_X, TORCH_Y, ZF - 10.0), (TORCH_X, TORCH_Y, ZF - 10.0)]
TORCH_SCREEN = [(CX - TORCH_X, CY + TORCH_Y), (CX + TORCH_X, CY + TORCH_Y)]
FLAME_CELLS = [(13, 7), (25, 7)]     # top-left cell of each 2x2-cell flame
LIGHT_R = 15.0                  # falloff length of the torch light
HOT_R = 34.0                    # far wall: per-pixel shading within this many px of a flame
ACCENT = 5                      # the theme accent (moss here; themes remap it)

STONE, WOOD, IRON, VOID, WATER = range(5)

# Colour ramps, dark to light. A lit surface is a position on its ramp: the
# face sits at the light level, its shadow one entry below, its highlight one
# above. (material, warmth 0 cold / 1 between / 2 torch-lit) -> ramp.
COLD = (0, 6, 11, 12, 15, 1)
MIDR = (0, 9, 11, 12, 15, 7)
WARM = (0, 9, 8, 10, 7, 1)
WOODR = (0, 9, 8, 10, 7)
IRONR = (0, 11, 12, 15, 1)
WATERR = (0, 6, ACCENT)
RAMPS = {
    (STONE, 0): COLD, (STONE, 1): MIDR, (STONE, 2): WARM,
    (WOOD, 0): WOODR, (WOOD, 1): WOODR, (WOOD, 2): WOODR,
    (IRON, 0): IRONR, (IRON, 1): IRONR, (IRON, 2): IRONR,
    (WATER, 0): WATERR, (WATER, 1): WATERR, (WATER, 2): WATERR,
}
LEVEL_SHIFT = {STONE: 0.0, WOOD: -1.25, IRON: -0.5, WATER: 0.0}


class Cfg:
    def __init__(self, north=0, west=False, east=False, south=False, props=(), flame=0):
        self.north = north      # 0 wall, 1 closed door, 2.. opening frames
        self.west = west
        self.east = east
        self.south = south
        self.props = tuple(props)
        self.flame = flame


def hash2(*k):
    h = 2166136261
    for v in k:
        for ch in str(v):
            h = ((h ^ ord(ch)) * 16777619) & 0xFFFFFFFF
    h ^= h >> 15
    h = (h * 0x2C1B3C6D) & 0xFFFFFFFF
    h ^= h >> 12
    return h


# ---------------------------------------------------------------------------
# Doors (in door-local units: 48 wide, 72 high, origin top-left)
# ---------------------------------------------------------------------------

OPEN_W = {1: 32.0, 2: 22.0, 3: 12.0, 4: 4.0}     # visible leaf width per north-door state
FRAME_BIAS = 0.9


def door(du, dv, state, tag, planks=4):
    """Region of a door at door coordinates, or None for wall."""
    cx = 24.0
    if du < 0 or du >= 48 or dv < 0 or dv >= 72:
        return None
    if dv < 24.0:
        r = math.hypot(du - cx, dv - 24.0)
        if r >= 24.0:
            return None
        if r >= 16.0:
            ang = math.atan2(24.0 - dv, du - cx)          # 0..pi
            k = int(ang / (math.pi / 5.0))
            return ((tag, 'v', k), STONE, du, dv, FRAME_BIAS + (0.5 if k == 2 else 0.0))
    elif du < 8.0 or du >= 40.0:
        k = int((dv - 24.0) // 12.0)
        return ((tag, 'j', du < 8.0, k), STONE, du, dv, FRAME_BIAS)
    if dv >= 68.0:
        return ((tag, 'step'), STONE, du, dv, 0.6)
    lw = OPEN_W[state]
    x = du - 8.0
    if x >= lw:
        # the passage beyond: black, with a few steps leading up
        if dv >= 52.0:
            k = int((dv - 52.0) // 4.0)
            inset = 2.5 * (3 - k)
            if lw + 1.0 + inset * 0.4 <= x < 32.0 - inset * 0.4:
                return ((tag, 'stair', k), STONE, du, dv, -1.9 + 0.4 * k)
        return ((tag, 'void'), VOID, du, dv, 0)
    t = x / lw                                            # 0 hinge .. 1 free edge
    squeeze = (32.0 - lw) / 32.0                          # 0 closed .. ~1 open
    if state == 1:
        top = 24.0 - math.sqrt(max(0.0, 256.0 - (du - cx) ** 2)) if dv < 24.0 else 0.0
        if dv < top:
            return ((tag, 'void'), VOID, du, dv, 0)
        ly = dv - 8.0
    else:
        top = 8.0 + 8.0 * squeeze * t
        if dv < 24.0:
            top = max(top, 24.0 - math.sqrt(max(0.0, 256.0 - (du - cx) ** 2)))
        bot = 68.0 - 5.0 * squeeze * t
        if dv < top or dv >= bot:
            return ((tag, 'void'), VOID, du, dv, 0)
        ly = (dv - (8.0 + 8.0 * squeeze * t)) / (bot - (8.0 + 8.0 * squeeze * t)) * 60.0
    lx = t * 32.0
    dim = -0.8 * squeeze
    for n, by in enumerate((17.0, 43.0)):
        if by <= ly < by + 6.0:
            stud = int(lx // 8.0)
            if abs(lx - (stud * 8.0 + 4.0)) < 2.0 and by + 2.0 <= ly < by + 4.0 and lw > 12:
                return ((tag, 'stud', n, stud), IRON, lx, ly, 1.0 + dim)
            return ((tag, 'band', n), IRON, lx, ly, dim)
    if state == 1 and planks == 4:
        rr = math.hypot((lx - 25.0) * 0.8, ly - 32.0)
        if 1.8 <= rr < 4.2:
            return ((tag, 'ring'), IRON, lx, ly, 1.0)
    k = int(lx // (32.0 / planks))
    return ((tag, 'plank', k), WOOD, ly, lx, dim + (0.35 if (k * 7 + 3) % 3 == 0 else 0.0))


# ---------------------------------------------------------------------------
# The ray caster: screen position -> (region id, material, texel a, texel b, level bias, X, Y, Z)
# ---------------------------------------------------------------------------

BEAMS = [(198.0, 12.0), (246.0, 11.0), (292.0, 10.0)]
BEAM_Y = -26.5
KIND = {'so': 'fl', 'pu': 'fl', 'gr': 'fl'}


def sample(sx, sy, cfg):
    dx = (sx - CX) / F
    dy = (sy - CY) / F
    best = (ZF, 'far')
    if dx < 0 and -XW / dx < best[0]:
        best = (-XW / dx, 'left')
    if dx > 0 and XW / dx < best[0]:
        best = (XW / dx, 'right')
    if dy > 0 and YF / dy < best[0]:
        best = (YF / dy, 'floor')
    if dy < 0:
        if YC / dy < best[0]:
            best = (YC / dy, 'ceil')
        for i, (zb, depth) in enumerate(BEAMS):
            y = dy * zb
            if YC <= y <= BEAM_Y and zb < best[0]:
                best = (zb, ('beamF', i))
            z = BEAM_Y / dy
            if zb <= z <= zb + depth and z < best[0]:
                best = (z, ('beamB', i))
    z, s = best
    x, y = dx * z, dy * z
    pos = (x, y, z)
    if s == 'far':
        u, v = x + XW, y - YC
        if cfg.north:
            d = door(u - 72.0, v - 24.0, cfg.north, 'dn')
            if d:
                return d + pos
        course = int(v // 16.0)
        off = 16.0 if course & 1 else 0.0
        idx = int((u + off) // 32.0)
        bias = -0.5 if course == 5 else 0.0
        return (('fw', course, idx), STONE, (u + off) % 32.0 / 2.0, v % 16.0, bias) + pos
    if s in ('left', 'right'):
        w, v = ZF - z, y - YC
        if (cfg.west and s == 'left') or (cfg.east and s == 'right'):
            d = door(w - 44.0, v - 24.0, 1, 'dw' if s == 'left' else 'de', planks=2)
            if d:
                return d + pos
        course = int(v // 16.0)
        off = 0.0 if course & 1 else 16.0
        idx = int((w + off) // 32.0)
        bias = -0.5 if course == 5 else 0.0
        return ((s, course, idx), STONE, (w + off) % 32.0 / 2.0, v % 16.0, bias) + pos
    if s == 'floor':
        u, w = x + XW, ZF - z
        if cfg.south and abs(x) < 27.0 and w >= 105.0:
            if abs(x) < 21.0 and w >= 110.0:
                k = int((w - 110.0) // 8.0)
                return (('so', 'st', k), STONE, 8.0, 8.0, -0.4 - 0.75 * k) + pos
            side = 0 if w < 110.0 else (1 if x < 0 else 2)
            return (('so', 'curb', side, int(w // 14.0) if side else 0), STONE, 8.0, 8.0, 0.8) + pos
        if 'puddle' in cfg.props:
            ang = math.atan2((w - 60.0) / 14.0, (x - 46.0) / 30.0)
            e = ((x - 46.0) / 30.0) ** 2 + ((w - 60.0) / 14.0) ** 2
            e *= 1.0 + 0.22 * math.sin(ang * 3.0 + 1.0) + 0.1 * math.sin(ang * 7.0)
            if e < 1.0:
                return (('pu', 0 if e < 0.42 else 1), WATER, x, w, 0.0) + pos
        if 'grate' in cfg.props and -78.0 <= x < -42.0 and 74.0 <= w < 106.0:
            gx, gw = x + 78.0, w - 74.0
            if gx < 3.0 or gx >= 33.0 or gw < 3.0 or gw >= 29.0:
                return (('gr', 'frame'), IRON, gx, gw, 0.6) + pos
            n = int((gx - 3.0) // 6.0)
            if (gx - 3.0) % 6.0 >= 3.6:
                return (('gr', 'bar', n), IRON, gx, gw, 0.6) + pos
            return (('gr', 'hole', n), VOID, gx, gw, 0.0) + pos
        row = int(w // 32.0)
        off = 24.0 if row & 1 else 0.0
        idx = int(math.floor((u + off) / 48.0))
        return (('fl', row, idx), STONE, (u + off) % 48.0 / 3.0, w % 32.0 / 2.0, 0.0) + pos
    if s == 'ceil':
        u = x + XW
        return (('ce', int(u // 24.0)), WOOD, ZF - z, u, 0.0) + pos
    kind, i = s
    u = x + XW
    if kind == 'beamF':
        return (('bf', i), WOOD, u, (y - YC) * 2.0, 0.0) + pos
    return (('bb', i), WOOD, u, (z - BEAMS[i][0]), 0.0) + pos


# ---------------------------------------------------------------------------
# Texture
# ---------------------------------------------------------------------------

def make_tiles():
    """16x16 texels of pits and specks for a stone face, a few variants."""
    tiles = []
    for variant in range(6):
        rnd = random.Random(1000 + variant)
        t = [[0.0] * 17 for _ in range(17)]
        for _ in range(1 + variant % 3):
            a, b = rnd.randrange(3, 12), rnd.randrange(3, 11)
            t[b][a] = -1.0
            if rnd.random() < 0.6:
                t[b][a + 1] = -1.0
            t[b + 1][a] = 1.0
        for _ in range(1 + variant % 2):
            a, b = rnd.randrange(2, 13), rnd.randrange(2, 13)
            t[b][a] = 1.0
        tiles.append(t)
    return tiles


TILES = make_tiles()


def wood_tex(a, b, rid):
    """a runs along the grain."""
    h = hash2(rid, int(b), int(a) // 6)
    if h % 7 == 0:
        return -1.0
    if h % 11 == 0:
        return 1.0
    return 0.0


# ---------------------------------------------------------------------------
# Lighting
# ---------------------------------------------------------------------------

NORMALS = {'fw': (0, 0, -1), 'dn': (0, 0, -1), 'left': (1, 0, 0), 'dw': (1, 0, 0), 'right': (-1, 0, 0),
           'de': (-1, 0, 0), 'fl': (0, -1, 0), 'ce': (0, 1, 0), 'bf': (0, 0, -1.0001), 'bb': (0, 1, 0)}
AMBIENT = {'fw': 0.125, 'dn': 0.125, 'left': 0.14, 'right': 0.14, 'dw': 0.14, 'de': 0.14,
           'fl': 0.165, 'ce': 0.13, 'bf': 0.17, 'bb': 0.10}


def light(pos, normal):
    """Warm energy from the torches at a 3D point."""
    e = 0.0
    for tx, ty, tz in TORCHES:
        lx, ly, lz = tx - pos[0], ty - pos[1], tz - pos[2]
        d = math.sqrt(lx * lx + ly * ly + lz * lz) + 1e-6
        ndl = (lx * normal[0] + ly * normal[1] + lz * normal[2]) / d
        facing = 1.0 if normal == (0, 0, -1) else max(0.0, 0.25 + 0.75 * ndl)
        e += math.exp(-d / LIGHT_R) * facing
    return e


def hot_light(sx, sy):
    """Per-pixel torch light on the far wall, or None outside the hot zones."""
    best = None
    for tx, ty in TORCH_SCREEN:
        d = math.hypot(sx - tx, (sy - ty) * 1.1)
        if d < HOT_R:
            e = math.exp(-d / LIGHT_R)
            below = sy - (ty + 9.0)
            if below > 0:                                 # the bracket's shadow
                half = 5.0 + below * 0.5
                k = abs(sx - tx) / half
                if k < 1.0:
                    e *= 0.25 + 0.75 * k * k
            best = e if best is None else max(best, e)
    return best


def level_of(e):
    return max(0.0, min(4.0, math.log2(max(e, 1e-6) / 0.035)))


def edge_dist(S, mx, my, dx, dy, rid, cap=4):
    """Pixels from (mx, my) to the region's edge going (dx, dy): 1 = the next one is outside."""
    for k in range(1, cap + 1):
        x, y = mx + dx * k, my + dy * k
        if x < 0 or y < 0 or x > W or y > H:
            return cap + 1
        if S[y][x][0] != rid:
            return k
    return cap + 1


# ---------------------------------------------------------------------------
# Stamps: small pixel maps drawn over the lit room
# ---------------------------------------------------------------------------

def flame_rows(frame):
    """8x16 multicolour pixels: the flame, the lit wall behind it, the cup's rim."""
    height = (11.5, 9.5, 12.5)[frame]
    lean = (0.0, -1.5, 1.2)[frame]
    rows = []
    for fy in range(16):
        row = ""
        for fx in range(8):
            if fy == 15:
                ch = 'K'
            elif fy == 14:
                ch = 'K' if fx in (0, 7) else 'y'
            else:
                t = (13.6 - fy) / height
                ch = 'y' if fy == 0 else 'r'
                if 0.0 <= t <= 1.0:
                    cxf = 4.0 + lean * t * t + (0.5 if frame == 1 and t > 0.5 else 0.0)
                    wdt = 3.3 * (1.0 - t) ** 0.65 * min(1.0, 0.6 + t * 2.5)
                    dist = abs(fx + 0.5 - cxf)
                    if dist < wdt:
                        ch = 'W' if (fy >= 8 and dist < wdt * 0.5 - 0.3 and t < 0.5) else 'y'
                        if fy < 8 and dist > wdt - 1.0 and wdt > 1.2:
                            ch = 'o'
                    elif fy < 8 and dist < wdt + 1.0 and wdt > 0.4:
                        ch = 'o'
                if frame == 2 and (fx, fy) == (2, 2):
                    ch = 'o'
                if frame == 1 and (fx, fy) == (6, 3):
                    ch = 'o'
            row += ch
        rows.append(row)
    return rows


FLAME_LEGEND = {'r': 10, 'y': 7, 'W': 1, 'o': 8}

BRACKET = [
    "KhhhhhhK",
    "KKmmmmKK",
    ".KKssKK.",
    "..KKKK..",
    "..KmsK..",
    "..KmsK..",
    ".KKKKKK.",
    ".KhhhmK.",
    ".KKKKKK.",
    "..KmsK..",
    "..KmsK..",
    "..KmsK..",
    "...KsK..",
    "...KK...",
]
BRACKET_LEGEND = {'h': 'H', 'm': 'M', 's': 'S'}

SKULL = [
    "..KKKKK...........",
    ".KwwwwwK..........",
    "KwwwwwwgK.....KK..",
    "KwKKwKKgK...KKwwK.",
    "KwKKwKKgK.KKwwgKK.",
    "KwwwKwwgKKwwgKK...",
    ".KwwwwgKwwgKK.....",
    ".KwKwKgKwKK.KKKK..",
    "..KKKKKKKKKKwwwgK.",
    "..KwwwwwwwwwgKKKK.",
    ".KwgKKKKKKKKK.....",
    ".KKK..............",
]
SKULL_LEGEND = {'w': 15, 'g': 12}

CHAIN = [
    ".KKKK..........KKKK.",
    "KhKKhK........KhKKhK",
    "KhKKhK........KhKKhK",
    ".KhhK..........KhhK.",
    "..KK............KK..",
    "..Kh............hK..",
    "..Kh............hK..",
    "..KK............KK..",
    ".KhK............KhK.",
    ".KhK............KhK.",
    "..KK............KK..",
    "..Kh............hK..",
    "..Kh............hK..",
    "..KK............KK..",
    ".KhK............KhK.",
    ".KhK............KhK.",
    "..KK............KK..",
    "..Kh............hK..",
    "..Kh............hK..",
    "..KK............KK..",
    ".KhhK..........KhhK.",
    "KhKKhK........KhKKhK",
    "KhKKhK........KhKKhK",
    "KhKKhK........KhKKhK",
    ".KhhK..........KhhK.",
    "..KK............KK..",
]
CHAIN_LEGEND = {'h': 'H'}


def banner_rows():
    """8 wide x 44 high: a tattered cloth banner with a pale device."""
    rows = []
    for y in range(44):
        row = ""
        for x in range(8):
            if y < 2:
                ch = 'K' if y == 1 else 'p'
            else:
                tatter = 44 - (3, 9, 5, 1, 4, 11, 6, 2)[x]
                if y >= tatter:
                    ch = '.'
                elif y == tatter - 1 or x == 7:
                    ch = 'K'
                else:
                    fold = (x + (y // 9)) % 4 == 3
                    ch = 'd' if fold else 'c'
                    if 10 <= y < 24:
                        dxx, dyy = x - 3.5, y - 17.0
                        if abs(dxx) + abs(dyy) / 2.4 < 3.0 and not (abs(dxx) < 1.0 and abs(dyy) < 3.0):
                            ch = 'e'
            row += ch
        rows.append(row)
    return rows


BANNER_LEGEND = {'c': 2, 'd': 9, 'e': 7, 'p': 12}


def line_px(x0, y0, x1, y1):
    n = int(max(abs(x1 - x0), abs(y1 - y0))) + 1
    return [(int(round(x0 + (x1 - x0) * i / n)), int(round(y0 + (y1 - y0) * i / n))) for i in range(n + 1)]


def cobweb(cx, cy, sign):
    """Pixels of a corner web; (cx, cy) the corner, sign +1 opening right, -1 left."""
    ends = [(15, 2), (13, 9), (9, 16), (5, 21), (1, 24)]
    px = set()
    for ex, ey in ends:
        px.update(line_px(cx, cy, cx + sign * ex, cy + ey))
    for frac, sag in ((0.45, 1.0), (0.78, 1.6)):
        for i in range(len(ends) - 1):
            ax, ay = cx + sign * ends[i][0] * frac, cy + ends[i][1] * frac
            bx, by = cx + sign * ends[i + 1][0] * frac, cy + ends[i + 1][1] * frac
            mxm, mym = (ax + bx) / 2 - sign * sag, (ay + by) / 2 - sag
            px.update(line_px(ax, ay, mxm, mym))
            px.update(line_px(mxm, mym, bx, by))
    return px


def crack_px(x, y, n, seed):
    rnd = random.Random(seed)
    out = []
    for _ in range(n):
        out.append((x, y))
        y += 1
        r = rnd.random()
        if r < 0.3:
            x -= 1
        elif r > 0.72:
            x += 1
    return out


CRACKS = [(40, 42, 11, 3), (87, 26, 10, 5), (118, 90, 12, 8), (61, 92, 9, 2), (106, 44, 9, 12),
          (20, 62, 16, 4), (143, 84, 14, 6), (70, 133, 9, 7), (100, 150, 11, 9), (36, 144, 8, 1)]
MOSS = [(37, 115, 6, 6), (58, 118, 9, 3), (122, 116, 8, 5), (99, 118, 5, 3), (27, 122, 5, 9),
        (133, 121, 4, 8), (33, 30, 3, 8), (126, 28, 3, 6), (52, 124, 8, 2), (110, 124, 9, 2),
        (14, 138, 3, 10), (146, 132, 3, 9)]


def in_moss(mx, my):
    for bx, by, rx, ry in MOSS:
        e = ((mx - bx) / rx) ** 2 + ((my - by) / ry) ** 2
        if e < 1.0:
            if e < 0.45 or hash2('moss', mx, my) % 3:
                return True
    return False


# ---------------------------------------------------------------------------
# Rendering
# ---------------------------------------------------------------------------

def render(cfg):
    """Returns a 160x168 grid of C64 colour indices."""
    S = [[sample(2 * mx + 1.0, my + 0.5, cfg) for mx in range(W + 1)] for my in range(H + 1)]

    acc = {}
    for my in range(H):
        for mx in range(W):
            s = S[my][mx]
            a = acc.setdefault(s[0], [0, 0.0, 0.0, 0.0, 0.0, 0.0])
            a[0] += 1
            a[1] += s[5]
            a[2] += s[6]
            a[3] += s[7]
            a[4] += mx
            a[5] += my
    info = {}
    for rid, a in acc.items():
        n = a[0]
        pos = (a[1] / n, a[2] / n, a[3] / n)
        kind = KIND.get(rid[0], rid[0])
        ew = light(pos, NORMALS[kind])
        ec = AMBIENT[kind]
        if kind in ('left', 'right', 'fl', 'dw', 'de'):
            ec *= 0.5 + 0.5 * max(0.0, (pos[2] - 190.0) / 130.0)       # darker toward the viewer
        if kind in ('ce', 'bf', 'bb'):
            ec *= 0.45 + 0.55 * max(0.0, (pos[2] - 190.0) / 130.0)
        scx, scy = 2 * a[4] / n + 1, a[5] / n + 0.5
        tsx, tsy = min(TORCH_SCREEN, key=lambda t: (t[0] - scx) ** 2 + (t[1] - scy) ** 2)
        info[rid] = (ew, ec, tsx >= scx, tsy >= scy, hash2('v', rid))

    want = [[None] * W for _ in range(H)]
    lit = [[None] * W for _ in range(H)]
    for my in range(H):
        for mx in range(W):
            s = S[my][mx]
            rid, mat, ta, tb, bias = s[0], s[1], s[2], s[3], s[4]
            if mat == VOID:
                continue
            joint = S[my][mx + 1][0] != rid or S[my + 1][mx][0] != rid
            ew, ec, lit_right, lit_down, hv = info[rid]
            du = edge_dist(S, mx, my, 0, -1, rid)
            dd = edge_dist(S, mx, my + 1, 0, 1, rid)
            dl = edge_dist(S, mx, my, -1, 0, rid)
            dr = edge_dist(S, mx + 1, my, 1, 0, rid)
            lv_, fv_ = (dd, du) if lit_down else (du, dd)
            lh_, fh_ = (dr, dl) if lit_right else (dl, dr)
            tone = 2.0
            if lv_ == 1 or lh_ == 1:
                tone = 3.0
            elif fv_ == 1 or fh_ == 1:
                tone = 1.0
            elif mat == STONE:
                if lv_ == 2:
                    tone += 0.5
                elif lv_ == 3:
                    tone += 0.25
                elif lh_ == 2:
                    tone += 0.25
                if fv_ == 2:
                    tone -= 0.5
                elif fv_ == 3:
                    tone -= 0.25
                elif fh_ == 2:
                    tone -= 0.5
                d_ = TILES[hv % len(TILES)][int(tb) & 15][int(ta) & 15]
                if d_ and min(lv_, lh_, fv_, fh_) > 2:
                    tone = 2.0 + d_
            elif mat == WOOD:
                tone += wood_tex(ta, tb, rid)
            elif mat == WATER:
                tone = 2.0 + (0.5 if (int(tb * 0.7) + rid[1]) % 3 == 0 else 0.0)
                if lv_ == 1:
                    tone = 3.0

            sx, sy = 2 * mx + 1.0, my + 0.5
            jitter = ((hv >> 8) % 100) / 100.0 * 0.7 - 0.35
            hot = hot_light(sx, sy) if rid[0] == 'fw' else None
            if hot is not None:
                wr = hot / (hot + ec)
                warm = 2 if wr > 0.60 else (1 if wr > 0.30 else 0)
                ramp = RAMPS[(mat, warm)]
                top = len(ramp) - 1
                lv = max(0.0, min(3.45, level_of(hot + ec) + bias + jitter * 0.4))
                ps, pm, ph = max(0.0, lv - 1.0), lv, min(top, lv + 1.0)
            else:
                wr = ew / (ew + ec)
                warm = 2 if wr > 0.60 else (1 if wr > 0.30 else 0)
                ramp = RAMPS[(mat, warm)]
                top = len(ramp) - 1
                lv = level_of(ew + ec) + bias + jitter + LEVEL_SHIFT[mat]
                lv = max(0.0, min(top - 1.0, math.floor(lv * 2.0 + 0.5) / 2.0))
                lo_, hi_ = math.floor(lv), math.ceil(lv)
                ps = max(0.0, lo_ - 1.0)
                if lo_ >= 2 and hi_ > lo_:
                    ps = lo_
                pm, ph = lv, min(top, hi_ + 1.0)
                if mat == IRON:
                    ps, pm, ph = 0.0, 0.0, max(1.0, min(top, math.floor(lv + 1.0)))
                elif mat == WATER:
                    ps, pm, ph = 0.0, 0.5, 2.0
            lit[my][mx] = (ramp, ps, pm, ph)
            if joint:
                continue
            pos = ps + (pm - ps) * (tone - 1.0) if tone <= 2.0 else pm + (ph - pm) * (tone - 2.0)
            want[my][mx] = (ramp, pos)
            if mat == STONE and rid[0] in ('fw', 'left', 'right', 'fl') and pm >= 1.5 and in_moss(mx, my) \
                    and (mx + my) & 1:
                want[my][mx] = ((0, ACCENT), 1.0)

    def tone_at(mx, my, t):
        l_ = lit[my][mx]
        if l_ is None:
            return None
        return (l_[0], {'S': l_[1], 'M': l_[2], 'H': l_[3]}[t])

    def stamp(x0, y0, rows, legend):
        for j, row in enumerate(rows):
            for i, ch in enumerate(row):
                mx, my = x0 + i, y0 + j
                if ch == '.' or not (0 <= mx < W and 0 <= my < H):
                    continue
                if ch == 'K':
                    want[my][mx] = None
                    continue
                v = legend[ch]
                want[my][mx] = tone_at(mx, my, v) if isinstance(v, str) else ((0, v), 1.0)

    # cracks: a black line with a lit edge
    for x, y, n, seed in CRACKS:
        for cxp, cyp in crack_px(x, y, n, seed):
            if 0 <= cxp < W - 1 and 0 <= cyp < H and want[cyp][cxp] is not None:
                want[cyp][cxp] = None
                if want[cyp][cxp + 1] is not None and lit[cyp][cxp + 1]:
                    want[cyp][cxp + 1] = tone_at(cxp + 1, cyp, 'H')
    # cobwebs in the far wall's top corners
    for cxw, cyw, sign in ((32, 24, 1), (127, 24, -1)):
        for wx, wy in cobweb(cxw, cyw, sign):
            if 0 <= wx < W and 0 <= wy < H and lit[wy][wx] and S[wy][wx][0][0] == 'fw':
                want[wy][wx] = tone_at(wx, wy, 'H')
    # props
    if 'chains' in cfg.props:
        stamp(36, 40, CHAIN, CHAIN_LEGEND)
    if 'banner' in cfg.props:
        stamp(116, 34, banner_rows(), BANNER_LEGEND)
    if 'bones' in cfg.props:
        stamp(38, 124, SKULL, SKULL_LEGEND)
    # torches
    for cxc, cyc in FLAME_CELLS:
        stamp(cxc * 4, cyc * 8, flame_rows(cfg.flame), FLAME_LEGEND)
        stamp(cxc * 4, cyc * 8 + 16, BRACKET, BRACKET_LEGEND)
    return quantise(want)


def dither(ramp, pos, mx, my):
    lo = int(math.floor(pos))
    if lo >= len(ramp) - 1:
        return ramp[-1]
    return ramp[lo + (1 if (pos - lo) * 16 > BAYER[my & 3][mx & 3] else 0)]


def quantise(want):
    """(ramp, position) per pixel -> colour indices, at most black + 3 colours per cell."""
    out = [[0] * W for _ in range(H)]
    for cy in range(H // 8):
        for cx in range(W // 4):
            cells = [(mx, my) for my in range(cy * 8, cy * 8 + 8) for mx in range(cx * 4, cx * 4 + 4)]
            cols = {}
            for mx, my in cells:
                w = want[my][mx]
                c = dither(w[0], w[1], mx, my) if w else 0
                out[my][mx] = c
                if c:
                    cols[c] = cols.get(c, 0) + 1
            if len(cols) <= 3:
                continue
            ramps = {want[my][mx][0] for mx, my in cells if want[my][mx]}
            if len(ramps) == 1:
                ramp = ramps.pop()
                best = None
                for w0 in range(1, len(ramp) - 2):
                    err = 0.0
                    for mx, my in cells:
                        if want[my][mx]:
                            p = want[my][mx][1]
                            if p > w0 + 2:
                                err += p - (w0 + 2)
                            elif w0 > 1 and 0.5 * w0 <= p < w0:
                                err += w0 - p
                            elif w0 > 1 and p < 0.5 * w0:
                                err += p
                    if best is None or err < best[0]:
                        best = (err, w0)
                w0 = best[1]
                for mx, my in cells:
                    if not want[my][mx]:
                        continue
                    p = min(want[my][mx][1], w0 + 2)
                    if w0 > 1 and p < w0:
                        out[my][mx] = 0 if p < 0.5 * w0 else ramp[w0]
                    else:
                        out[my][mx] = dither(ramp, p, mx, my)
            else:
                kept, mapping = c64img._best_subset(cols, [0], 3)
                for mx, my in cells:
                    out[my][mx] = mapping.get(out[my][mx], out[my][mx])
    return out


def save_png(grid, path):
    img = Image.new("RGB", (W, H))
    px = img.load()
    for y in range(H):
        for x in range(W):
            px[x, y] = P[grid[y][x]]
    img.save(path)


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    base = render(Cfg())
    save_png(base, os.path.join(SRC, "room.png"))
    c64img.convert(os.path.join(SRC, "room.png"), os.path.join(OUT, "room.img"),
                   preview=os.path.join(SRC, "room.c64.png"))
    full = render(Cfg(north=1, west=True, east=True, south=True, props=('chains', 'banner', 'bones', 'puddle', 'grate')))
    save_png(full, os.path.join(SRC, "room_doors.png"))
    c64img.convert(os.path.join(SRC, "room_doors.png"), None,
                   preview=os.path.join(SRC, "room_doors.c64.png"))


if __name__ == "__main__":
    main()
