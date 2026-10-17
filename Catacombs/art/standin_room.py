"""
A stand-in room for judging the sprites: dark, dithered grey/brown stone on
black, drawn to the fixed room geometry of AAA-BRIEF.md (far wall x 64..255,
y 24..119; floor trapezoid down to y 167; north door cell columns 17-22, rows
6-14). Used ONLY by the SCENE_ builds and the previews -- the real rooms are
the Rooms part's work and replace this entirely.
"""
import math

from PIL import Image

import c64img

P = c64img.PALETTE
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]
W, H = 160, 168                      # multicolour pixels
STONE = [0, 9, 11, 12]               # black, brown, dark grey, mid grey
WARM = [0, 9, 11, 8]                 # black, brown, dark grey, orange (dim torchlight)
TORCHES = [(50, 60), (109, 60)]      # multicolour x, y


def shade(ramp, level, centre, x, y):
    """Ordered dither from a ramp, kept within three neighbouring entries
    around the cell's own level so no cell needs more than three colours."""
    top = len(ramp) - 1
    low = max(0, min(top - 2, int(round(max(0.0, min(1.0, centre)) * top)) - 1))
    f = max(low, min(low + 1.999, max(0.0, min(0.999, level)) * top))
    i = int(f)
    return ramp[min(top, i + (1 if (f - i) * 16 > BAYER[y & 3][x & 3] else 0))]


def light(x, y):
    d = min(math.hypot((x - tx) * 2, y - ty) for tx, ty in TORCHES)
    return max(0.0, 1.0 - d / 120.0)


def surface(x, y):
    """(kind, brightness 0..1, is a joint line) of the surface at a pixel."""
    if 32 <= x <= 127 and 24 <= y <= 119:                    # far wall: blocks 12x8
        row = y // 8
        bx = (x + (6 if row & 1 else 0)) % 12
        joint = (y % 8 == 7) or bx == 11
        return "wall", 0.22 + 0.45 * light(x, y), joint
    if y >= 120:                                             # floor
        t = (y - 120) / 47.0
        left = 32 - 32 * t
        if x < left or x > 159 - left:
            return "void", 0, False
        u = (x - left) / max(1.0, (159 - 2 * left))          # 0..1 across the floor
        bands = (0, 5, 12, 21, 33)
        row = sum(1 for b in bands if (y - 120) >= b)
        joint = (y - 120) in bands or int(u * 7 + (0.5 if row & 1 else 0) + 100) != int(
            (x - 1 - left) / max(1.0, (159 - 2 * left)) * 7 + (0.5 if row & 1 else 0) + 100)
        return "floor", 0.26 + 0.22 * t + 0.20 * light(x, 119), joint
    if y < 24:                                               # ceiling
        t = (24 - y) / 24.0
        left = 32 - 32 * t
        if x < left or x > 159 - left:
            return "side", 0.16, (y % 6 == 5)
        return "ceil", 0.12 + 0.10 * (1 - t), ((x + y) % 14 == 0)
    # side walls
    return "side", 0.20 + 0.25 * light(x, y), (y % 12 == 11) or (x % 8 == 7)


def draw():
    img = Image.new("RGB", (W, H), P[0])
    px = img.load()
    for y in range(H):
        for x in range(W):
            cx, cy = (x // 4) * 4 + 2, (y // 8) * 8 + 4
            kind, level, joint = surface(x, y)
            if kind == "void":
                continue
            _, centre, _ = surface(cx, cy)
            if joint:
                level *= 0.45
            warm = kind == "wall" and min(math.hypot((cx - tx) * 2, cy - ty) for tx, ty in TORCHES) < 34
            px[x, y] = P[shade(WARM if warm else STONE, level, centre, x, y)]
    # the north door: a dark arch with planks (whole cells: columns 17-22, rows 6-14)
    for y in range(48, 120):
        for x in range(68, 92):
            edge = x in (68, 69, 90, 91) or y in (48, 49)
            plank = (x - 70) % 5 == 4
            px[x, y] = P[11 if edge else 0 if plank else 9]
    for tx, ty in TORCHES:                                    # torch: bracket and flame
        for y in range(ty - 6, ty + 10):
            for x in range(tx - 2, tx + 2):
                px[x, y] = P[0]
        for y in range(ty + 1, ty + 9):
            px[tx - 1, y] = px[tx, y] = P[9]
        for dy, half in ((-5, 0), (-4, 1), (-3, 1), (-2, 2), (-1, 2), (0, 1)):
            for x in range(tx - half, tx + half):
                px[x, ty + dy] = P[7 if abs(x - tx + 0.5) < 1 and dy > -4 else 8]
    return img


def floor_patch(w, h):
    """A w x h (screen pixels) piece of the floor and lower wall for previews."""
    room = draw().resize((320, 168), Image.NEAREST)
    out = Image.new("RGB", (w, h), P[0])
    tile = room.crop((70, 96, 250, 166))
    for y in range(0, h, tile.height):
        for x in range(0, w, tile.width):
            out.paste(tile, (x, y))
    return out
