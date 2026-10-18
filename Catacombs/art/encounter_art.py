#!/usr/bin/env python3
"""
Art for the combat encounter screen (Encounter.cs): the guardian's portrait,
its poses and variants, the name plates, and the sprites for sprites 6-7.

    python Catacombs/art/encounter_art.py            # everything
    python Catacombs/art/encounter_art.py preview    # only the previews in .debug-tmp/

The picture is left/right symmetric and only its LEFT HALF (20x21 cells) is
stored; the game mirrors it with Screen.MirrorCells. Everything is drawn at
the C64's own resolution (multicolour pixels, 2:1 wide): a sculpted height
field lit by the two wall torches gives the big forms, and teeth, cracks,
eyes, flames, masonry, lettering and the health bar are placed pixel by pixel.

Writes PNGs to art/src/ and the converted .img/.spr files to art/build/.
"""
import itertools
import math
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import c64img   # noqa: E402
import c64spr   # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
P = c64img.PALETTE

HW, H = 80, 168                 # the left half, in multicolour pixels
BLACK, WHITE, RED, CYAN, VIOLET, GREEN, BLUE, YELLOW = 0, 1, 2, 3, 4, 5, 6, 7
ORANGE, BROWN, LRED, DGREY, MGREY, LGREEN, LBLUE, LGREY = 8, 9, 10, 11, 12, 13, 14, 15

# Pixel grid. u, v: square ("hi-res") units; a: distance from the centre line.
XX, YY = np.meshgrid(np.arange(HW), np.arange(H))
U = XX * 2.0 + 1.0
V = YY + 0.5
A = 160.0 - U

# Materials: the colours from dark to bright, and the light level at which
# each one starts (so the bright end can be kept for thin highlights).
MAT = {
    "bone":  ([0, 9, 8, 12, 15, 7, 1], [0, .08, .22, .42, .66, .98, 1.25]),
    "metal": ([0, 6, 11, 14, 1],       [0, .16, .48, 1.0, 1.4]),
    "horn":  ([0, 9, 8, 7],            [0, .18, .46, .92]),
}


def ramp_pos(mat, level):
    cols, stops = MAT[mat]
    for i in range(len(stops) - 1, -1, -1):
        if level >= stops[i]:
            if i == len(stops) - 1:
                return float(i)
            return i + (level - stops[i]) / (stops[i + 1] - stops[i])
    return 0.0


TORCH = (16.0, 32.0, 55.0)      # the left torch's flame; the right one is its mirror
FILL = 0.17


# ---------------------------------------------------------------------------
# Height-field parts
# ---------------------------------------------------------------------------

def ell(a, v, a0, v0, ra, rv, base, h):
    r2 = ((a - a0) / ra) ** 2 + ((v - v0) / rv) ** 2
    with np.errstate(invalid="ignore"):
        return np.where(r2 < 1.0, base + h * np.sqrt(np.maximum(0.0, 1.0 - r2)), np.nan)


def smooth_union(zs, k=0.22):
    acc = np.zeros_like(zs[0])
    any_valid = np.zeros(zs[0].shape, bool)
    for z in zs:
        ok = ~np.isnan(z)
        acc += np.where(ok, np.exp(k * np.where(ok, z, 0.0)), 0.0)
        any_valid |= ok
    with np.errstate(divide="ignore", invalid="ignore"):
        return np.where(any_valid, np.log(acc) / k, np.nan)


def hard_union(zs):
    out = np.full(zs[0].shape, np.nan)
    for z in zs:
        out = np.where(np.isnan(out), z, np.where(np.isnan(z), out, np.maximum(out, np.where(np.isnan(z), -1e9, z))))
    return out


def bezier(p, t):
    p = np.asarray(p, float)
    while len(p) > 1:
        p = p[:-1] * (1 - t) + p[1:] * t
    return p[0]


def tube(a, v, pts, r0, r1, z0, z1, power=0.8, n=70):
    """A round tapering tube along a Bezier curve. Returns (z, t along it)."""
    best = np.full(a.shape, 1e9)
    bt = np.zeros(a.shape)
    for i in range(n + 1):
        t = i / n
        ca, cv = bezier(pts, t)
        d = (a - ca) ** 2 + (v - cv) ** 2
        m = d < best
        best = np.where(m, d, best)
        bt = np.where(m, t, bt)
    rad = r1 + (r0 - r1) * (1 - bt) ** power
    inside = best < rad ** 2
    with np.errstate(invalid="ignore"):
        z = np.where(inside, z0 + (z1 - z0) * bt + np.sqrt(np.maximum(0, rad ** 2 - best)), np.nan)
    return z, bt


def skull(a, v):
    a = np.abs(a)
    z = smooth_union([
        ell(a, v, 0, 52, 50, 42, 0, 50),        # cranium
        ell(a, v, 21, 63, 25, 9, 30, 22),       # brow ridge
        ell(a, v, 40, 78, 5, 12, 16, 16),       # outer rim of the eye socket
        ell(a, v, 32, 89, 14, 8, 16, 22),       # cheekbone
        ell(a, v, 0, 99, 27, 15, 16, 30),       # upper jaw
        ell(a, v, 0, 80, 8, 14, 24, 24),        # nose bridge
    ], k=0.15)
    return z


def socket_r(a, v):
    """Normalised radius inside the (slanted) eye socket; < 1 is inside."""
    a = np.abs(a)
    da, dv = a - 23.0, v - 76.0
    c, s = math.cos(0.38), math.sin(0.38)
    p = da * c - dv * s            # along the slant: the outer corner is higher
    q = da * s + dv * c
    return np.sqrt((p / 16.0) ** 2 + (q / 10.5) ** 2)


def nose_w(v):
    t = (v - 86.0) / 16.0
    with np.errstate(invalid="ignore"):
        w = 8.5 * np.sin(np.pi * np.clip(t, 0, 1)) ** 0.6 * (0.3 + 0.7 * t)
    return np.where((t > 0) & (t < 1), w, 0.0)


def mandible(a, v):
    a = np.abs(a)
    return smooth_union([
        ell(a, v, 0, 122, 30, 12, 8, 24),
        ell(a, v, 0, 131, 18, 9, 12, 22),
        ell(a, v, 28, 109, 7, 16, 6, 16),       # the ramus, up toward the cheek
    ], k=0.2)


HORN_BULL = [(34, 44), (66, 58), (104, 40), (92, 4)]


def horn(a, v, pts=HORN_BULL, r0=14.0):
    return tube(a, v, pts, r0, 1.2, 14, 30)


def gorget(a, v):
    a = np.abs(a)
    inside = (v > 122 + a * 0.10) & (v < 150) & (a < 34 + (v - 122) * 0.5)
    z = 14 - a * 0.25 + 3.0 * np.cos(np.clip(a / 34.0, 0, 1) * np.pi / 2) + 2.5 * np.sin((v - 122) / 4.5)
    return np.where(inside, z, np.nan)


def pauldron(a, v):
    a = np.abs(a)
    return smooth_union([
        ell(a, v, 78, 139, 36, 19, 4, 28),
        ell(a, v, 52, 143, 22, 13, 2, 20),
    ], k=0.2)


def spike(a, v):
    return tube(a, v, [(86, 126), (96, 110), (106, 92)], 7.0, 0.8, 22, 26, power=1.0, n=40)[0]


# ---------------------------------------------------------------------------
# Lighting
# ---------------------------------------------------------------------------

def normals(fn, a, v):
    z = fn(a, v)
    e = 0.75
    zl, zr = fn(a + e, v), fn(a - e, v)          # a + e is to the LEFT (smaller u)
    zu, zd = fn(a, v - e), fn(a, v + e)
    zz = np.where(np.isnan(z), 0, z)

    def diff(lo, hi):
        lo_ok, hi_ok = ~np.isnan(lo), ~np.isnan(hi)
        l0, h0 = np.where(lo_ok, lo, 0), np.where(hi_ok, hi, 0)
        d = np.where(lo_ok & hi_ok, (h0 - l0) / (2 * e), 0.0)
        d = np.where(lo_ok & ~hi_ok, (zz - l0) / e, d)
        d = np.where(~lo_ok & hi_ok, (h0 - zz) / e, d)
        return d
    dzdu = diff(zl, zr)
    dzdv = diff(zu, zd)
    n = np.stack([-dzdu, -dzdv, np.ones_like(dzdu)], -1)
    n /= np.linalg.norm(n, axis=-1, keepdims=True)
    return z, n


def light(z, n, a, v, gain=1.0, reach=190.0):
    """(warm, cold) light levels at each pixel."""
    u = 160.0 - a
    zz = np.where(np.isnan(z), 0, z)
    warm = np.zeros(z.shape)
    for tx in (TORCH[0], 320.0 - TORCH[0]):
        d = np.stack([tx - u, TORCH[1] - v, TORCH[2] - zz], -1)
        dist = np.linalg.norm(d, axis=-1)
        l = d / dist[..., None]
        lam = np.maximum(0.0, (n * l).sum(-1))
        warm += lam * gain * 1.12 / (1.0 + (dist / reach) ** 2)
    warm += FILL * n[..., 2] ** 2             # what the room throws back, from the front
    cold = n[..., 2]
    return warm, cold


# ---------------------------------------------------------------------------
# The canvas: per pixel a material + light level, or a fixed colour
# ---------------------------------------------------------------------------

# Dither: between two neighbouring ramp colours only flat / checkerboard /
# flat -- the one pattern that stays clean on 2:1 pixels, and it repeats
# every byte (it compresses well).
def dither(frac, x, y):
    if frac < 0.34:
        return False
    if frac > 0.66:
        return True
    return bool((x + y) & 1)


class Canvas:
    def __init__(self):
        self.z = np.full((H, HW), -1e9)
        self.part = np.zeros((H, HW), int)
        self.mat = np.full((H, HW), "", dtype=object)
        self.warm = np.zeros((H, HW))
        self.cold = np.zeros((H, HW))
        self.fixed = np.full((H, HW), -1)
        self.direct = np.full((H, HW), np.nan)      # painted pixels: the ramp position itself
        self.nparts = 0

    def add(self, fn, mat, zoff=0.0, gain=1.0, shade=None, dv=0.0, reach=190.0):
        """Renders a height-field part. `shade`(a, v, z) may return a
        multiplier (grooves, fades). dv shifts the part down."""
        self.nparts += 1
        z, n = normals(fn, A, V - dv)
        ok = ~np.isnan(z)
        warm, cold = light(z, n, A, V, gain, reach)
        if shade is not None:
            m = shade(A, V - dv, z)
            warm, cold = warm * m, cold * m
        zz = np.where(ok, z + zoff, -1e9)
        win = ok & (zz > self.z)
        self.z = np.where(win, zz, self.z)
        self.part = np.where(win, self.nparts, self.part)
        self.mat[win] = mat
        self.warm = np.where(win, warm, self.warm)
        self.cold = np.where(win, cold, self.cold)
        self.fixed = np.where(win, -1, self.fixed)
        self.direct = np.where(win, np.nan, self.direct)
        return win

    def scale(self, mask, k):
        self.warm = np.where(mask, self.warm * k, self.warm)
        self.cold = np.where(mask, self.cold * k, self.cold)

    def black(self, mask):
        self.fixed = np.where(mask, BLACK, self.fixed)
        self.part = np.where(mask, 0, self.part)

    def px(self, x, y, c):
        if 0 <= x < HW and 0 <= y < H:
            self.fixed[y, x] = c

    def occlude(self, mask, radius=4, k=0.07, lo=0.35, hi=1.25):
        """Darkens hollows and lifts ridges: the light level is scaled by how
        far the surface lies below/above the average of its surroundings."""
        ok = (self.part != 0) & mask
        zf = np.where(ok, self.z, 0.0)
        acc = np.zeros_like(zf)
        cnt = np.zeros_like(zf)
        for dy in range(-radius * 2, radius * 2 + 1, 2):
            for dx in range(-radius, radius + 1):
                acc += np.roll(zf, (dy, dx), (0, 1))
                cnt += np.roll(ok, (dy, dx), (0, 1))
        n = np.maximum(cnt, 1)          # only the surface itself counts: no dark halo along its edge
        m = np.clip(1.0 + k * (zf - acc / n), lo, hi)
        self.warm = np.where(mask, self.warm * m, self.warm)

    def outline(self, strength=0.3, step=3.0):
        """Darkens the far side of every depth step between two parts."""
        dark = np.zeros((H, HW), bool)
        for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
            nz = np.roll(self.z, (dy, dx), (0, 1))
            npart = np.roll(self.part, (dy, dx), (0, 1))
            dark |= (npart != self.part) & (npart != 0) & (nz > self.z + step) & (self.part != 0)
        self.scale(dark, strength)
        self.direct = np.where(dark, self.direct - 1.3, self.direct)

    def quantise(self):
        ramps = np.empty((H, HW), dtype=object)
        pos = np.zeros((H, HW))
        for y in range(H):
            for x in range(HW):
                ramps[y, x] = None
                if self.fixed[y, x] >= 0 or self.part[y, x] == 0:
                    continue
                ramps[y, x] = MAT[self.mat[y, x]][0]
                pos[y, x] = ramp_pos(self.mat[y, x], self.warm[y, x]) if np.isnan(self.direct[y, x]) \
                    else max(0.0, self.direct[y, x])
        out = np.zeros((H, HW), int)

        def pick(ramp, f, x, y, allowed=None):
            idx = list(range(len(ramp))) if allowed is None else \
                [i for i in range(len(ramp)) if i == 0 or ramp[i] in allowed]
            lo = max(i for i in idx if i <= f + 1e-9)
            his = [i for i in idx if i > f + 1e-9]
            if not his:
                return ramp[lo]
            hi = his[0]
            frac = (f - lo) / (hi - lo)
            return ramp[hi] if dither(frac, x, y) else ramp[lo]

        for cy in range(H // 8):
            for cx in range(HW // 4):
                cell = [(x, y) for y in range(cy * 8, cy * 8 + 8) for x in range(cx * 4, cx * 4 + 4)]
                cols = {}
                for x, y in cell:
                    if self.fixed[y, x] >= 0:
                        c = self.fixed[y, x]
                    elif ramps[y, x] is None:
                        c = BLACK
                    else:
                        c = pick(ramps[y, x], pos[y, x], x, y)
                    out[y, x] = c
                    if c != BLACK:
                        cols[c] = cols.get(c, 0) + 1
                if len(cols) <= 3:
                    continue
                # Too many colours. Fixed (hand-placed) colours keep their
                # slots; the shaded pixels get a window of consecutive ramp
                # colours and are clamped into it (clean bands, no plaid).
                fixed_cols = {}
                ramp_px = []
                for x, y in cell:
                    if self.fixed[y, x] > 0:
                        fixed_cols[self.fixed[y, x]] = fixed_cols.get(self.fixed[y, x], 0) + 1
                    elif ramps[y, x] is not None:
                        ramp_px.append((x, y))
                keep = sorted(fixed_cols, key=lambda c: -fixed_cols[c])[:3]
                ramp = ramps[ramp_px[0][1], ramp_px[0][0]] if ramp_px else None
                best = None
                if ramp is not None:
                    cands = [list(range(st, st + ln)) for ln in (3, 2, 1, 0)
                             for st in range(1, len(ramp) - ln + 1)]
                    for win in cands:
                        if len(set(keep) | {ramp[i] for i in win}) > 3:
                            continue
                        err = 0.0
                        for x, y in ramp_px:
                            r2 = ramps[y, x]
                            f = pos[y, x] if r2 is ramp else 0
                            if not win:
                                err += f
                            elif f < win[0]:
                                err += min(f, win[0] - f)
                            elif f > win[-1]:
                                err += (f - win[-1]) * 1.5
                        if best is None or err < best[0]:
                            best = (err, win)
                win = best[1] if best else []
                allowed = set(keep) | {ramp[i] for i in win} if ramp is not None else set(keep)
                avail = list(allowed) + [BLACK]
                for x, y in cell:
                    if self.fixed[y, x] >= 0:
                        c = self.fixed[y, x]
                        if c != BLACK and c not in allowed:
                            c = min(avail, key=lambda k: c64img.COLOR_DIST[c][k])
                    elif ramps[y, x] is None:
                        c = BLACK
                    elif ramps[y, x] is not ramp or not win:
                        want = pick(ramps[y, x], pos[y, x], x, y)
                        c = BLACK if want == BLACK else min(avail, key=lambda k: c64img.COLOR_DIST[want][k])
                    else:
                        f = pos[y, x]
                        if f < win[0]:
                            c = BLACK if f < win[0] * 0.5 else ramp[win[0]]
                        else:
                            f = min(f, float(win[-1]))
                            lo = int(f)
                            c = ramp[lo] if lo == win[-1] else (ramp[lo + 1] if dither(f - lo, x, y) else ramp[lo])
                    out[y, x] = c
        return out


# ---------------------------------------------------------------------------
# Painted parts: flat planes of tone, drawn as polygons
# ---------------------------------------------------------------------------
# Coordinates are (c, y): c = multicolour pixels LEFT of the centre line.
# A tone is a position on the material's ramp (bone: 1 blue, 2 dark grey,
# 3 mid grey, 4 light grey, 5 yellow, 6 white); x.5 is a checkerboard of the
# two neighbours. Planes meet in a one-pixel checker seam (`soften`).

class Paint:
    SS = 4

    def __init__(self):
        self.tone = np.full((H, HW), np.nan)
        self.hole = np.zeros((H, HW), bool)

    def mask(self, pts, dy=0.0):
        ss = self.SS
        img = Image.new("L", (HW * ss, H * ss), 0)
        ImageDraw.Draw(img).polygon([((HW - c) * ss, (y + dy) * ss) for c, y in pts], fill=255)
        a = np.asarray(img, float).reshape(H, ss, HW, ss).mean(axis=(1, 3))
        return a >= 127.5

    def poly(self, pts, tone, dy=0.0, clip=True):
        m = self.mask(pts, dy)
        if clip:
            m &= ~np.isnan(self.tone) | self.hole
        self.tone = np.where(m, tone, self.tone)
        self.hole &= ~m

    def base(self, pts, tone, dy=0.0):
        self.poly(pts, tone, dy, clip=False)

    def cut(self, pts, dy=0.0):
        m = self.mask(pts, dy)
        self.hole |= m
        self.tone = np.where(m, np.nan, self.tone)

    def soften(self):
        ok = ~np.isnan(self.tone) & ~self.hole
        t = np.where(ok, self.tone, 0.0)
        acc = t * 2.0
        cnt = ok * 2.0
        for dy, dx in ((0, 1), (0, -1), (1, 0), (-1, 0)):
            acc += np.roll(t, (dy, dx), (0, 1))
            cnt += np.roll(ok, (dy, dx), (0, 1))
        self.tone = np.where(ok, acc / np.maximum(cnt, 1), self.tone)

    def put(self, cv, mat, z):
        cv.nparts += 1
        ok = ~np.isnan(self.tone) & ~self.hole
        cv.z = np.where(ok | self.hole, z, cv.z)
        cv.part = np.where(ok, cv.nparts, np.where(self.hole, 0, cv.part))
        cv.mat[ok] = mat
        cv.direct = np.where(ok, self.tone, cv.direct)
        cv.fixed = np.where(ok, -1, np.where(self.hole, BLACK, cv.fixed))
        return cv.nparts


# ---------------------------------------------------------------------------
# The guardian
# ---------------------------------------------------------------------------

KEY = {".": BLACK, "b": BLUE, "d": DGREY, "m": MGREY, "l": LGREY, "y": YELLOW, "w": WHITE,
       "o": ORANGE, "n": BROWN, "r": RED, "p": LRED}


def stamp(cv, rows, right, top):
    """Places hand-drawn pixels. `right` is the column of each row's LAST
    character; a space leaves what is there."""
    for j, row in enumerate(rows):
        for i, ch in enumerate(row):
            if ch != " ":
                cv.px(right - (len(row) - 1) + i, top + j, KEY[ch])


# The mouth, drawn by hand; each row ends at the centre line. Upper teeth:
# premolar, canine (the fang), two incisors. They are lit from the left.
UPPER_TEETH = [
    "nnnnnnnnnnnnnn",
    "wldwlldwlldwll",
    "wldwlldwlldwll",
    "wldwlldwlldwll",
    "wldwlldwlldwll",
    "wl.wlldwll.wll",
    ".l.wll.wll.wll",
    "...wll.wll.wll",
    "...wll..l...l.",
    "...wll        ",
    "...wll        ",
    "....wl        ",
    "....w         ",
]
LOWER_TEETH = [
    ".l..l..l...l..",
    "lm.lm.lm.lm.lm",
    "lm.lm.lm.lm.lm",
    "lm.lm.lm.lm.lm",
    "mmnmmnmmnmmnmm",
]


def lerp_tone(c, y, pts):
    """Tone from the nearest of a few (c, y, tone) anchors, blended by inverse distance."""
    w = np.zeros_like(c)
    t = np.zeros_like(c)
    for pc, py, pt in pts:
        d = 1.0 / (((c - pc) ** 2 + ((y - py) * 0.5) ** 2) + 4.0) ** 1.5
        w += d
        t += d * pt
    return t / w


CC = HW - 0.5 - XX          # each pixel's c (multicolour pixels left of the centre line)
YC = YY + 0.5

SKULL_OUTLINE = [(0, 10), (5, 10), (10, 12), (14, 15), (18, 20), (21, 26), (23, 33), (24.5, 42), (25, 50),
                 (24.5, 58), (23.5, 63), (22, 67), (21.5, 74), (23, 80), (24.5, 85), (24, 89), (21, 91.5),
                 (17, 94), (15, 97), (14, 101), (13.5, 104), (0, 104)]
SOCKET = [(3.5, 69), (6, 65.5), (11, 64), (16, 64), (20, 66), (21, 70), (20, 76), (16.5, 80.5), (11, 82),
          (6.5, 80.5), (4, 76)]
NOSE_HOLE = [(0, 86), (1.5, 86), (3.5, 90), (4.5, 95), (3.5, 99.5), (1.5, 99), (0, 97.5)]
EYE_C, EYE_Y = 11.0, 75.0      # the burning point in each socket (c, y)


def paint_skull(cv):
    p = Paint()
    p.base(SKULL_OUTLINE, 3)
    # the dome: brightest on its upper outer plane (toward the torch), mid in
    # the middle, falling off toward the top and the lower sides
    dome = lerp_tone(CC, YC, [(18, 28, 4.35), (11, 20, 3.8), (4, 15, 3.0), (0, 30, 3.1), (6, 44, 3.3),
                              (16, 46, 3.8), (24, 40, 3.6), (24, 56, 2.0), (12, 56, 3.0), (0, 54, 3.0)])
    m = p.mask(SKULL_OUTLINE) & (YC < 60)
    p.tone = np.where(m, dome, p.tone)
    # the temple, hollow under the dome's edge
    p.poly([(24.5, 58), (23.5, 63), (22, 67), (21.5, 74), (20, 70), (20, 62), (22, 56)], 1.6)
    # brow ridges: a heavy shelf, lit on top, black under its lip
    p.poly([(0, 61), (4, 59), (9, 57), (15, 56.5), (20, 57.5), (23, 60), (23, 63.5), (20, 63), (15, 62.5),
            (9, 63.5), (4, 66), (0, 66)], 4)
    p.poly([(7, 57.5), (14, 56.5), (20, 57.5), (22.5, 60), (21, 60.5), (15, 58.5), (9, 59)], 5)
    p.poly([(0, 58), (3, 59), (3.5, 66), (2.5, 72), (0, 72)], 3.2)                                   # glabella
    # outer rim of the socket, down into the cheekbone
    p.poly([(20, 63), (22.5, 64), (22, 70), (21.5, 76), (22.5, 80), (20.5, 81), (20, 76), (20.5, 69)], 3.6)
    # nasal bones
    p.poly([(1.2, 70), (3.5, 69), (4.5, 76), (4.8, 86), (2.5, 87), (1.2, 78)], 4)
    p.poly([(0, 70), (1.2, 70), (1.2, 87), (0, 87)], 3)
    # cheekbone: a flat shelf flaring out, lit along its top edge
    p.poly([(5, 83), (12, 85), (18, 82), (22.5, 79.5), (24.5, 85), (24, 89), (21, 91), (14, 91), (8, 90), (5, 88)], 3.4)
    p.poly([(12, 84.5), (18, 81.5), (22.5, 79.5), (24.5, 84.5), (23.5, 85.5), (21, 82.5), (17, 84)], 4.6)
    p.poly([(7.5, 89.5), (14, 91), (21, 91.5), (17, 94.5), (14.5, 98.5), (11, 95), (8, 92.5)], 1.4)  # under it
    # upper jaw, the bone the teeth sit in
    p.poly([(4.5, 91), (8, 92), (11, 95), (13.5, 100), (13.5, 104), (0, 104), (0, 99.5), (3.5, 99.5)], 3)
    p.poly([(8, 92), (11, 95), (13.5, 100), (13.5, 102), (11.5, 99), (9, 95)], 3.8)
    p.poly([(0, 102.5), (13.5, 102.5), (13.5, 104), (0, 104)], 1.5)                                  # gum line
    # holes
    p.cut(SOCKET)
    p.cut(NOSE_HOLE)
    p.poly([(6, 81), (11, 83), (16.5, 81.5), (20, 77.5), (20.5, 79.5), (17, 83.5), (11, 85), (6, 83.5)], 4.3)   # lower rim
    p.soften()
    part = p.put(cv, "bone", 40.0)
    # the fire inside the sockets: red, then orange toward the burning point
    for y in range(66, 82):
        for x in range(HW - 22, HW - 3):
            if cv.fixed[y, x] != BLACK or cv.part[y, x] != 0:
                continue
            c = HW - 0.5 - x
            r = math.hypot((c - EYE_C) / 4.2, (y + 0.5 - EYE_Y) / 3.6)
            if r < 0.55:
                cv.px(x, y, YELLOW)
            elif r < 0.95:
                cv.px(x, y, ORANGE if r < 0.8 or (x + y) & 1 else RED)
            elif r < 1.45 and ((x + y) & 1 or r < 1.2):
                cv.px(x, y, RED)
    return part


def paint_mandible(cv, jaw):
    p = Paint()
    d = jaw
    outline = [(0, 117), (13, 117), (14.5, 110), (16, 101), (17, 96), (19, 96), (19.5, 102), (18.5, 112),
               (17, 121), (14, 127.5), (9.5, 132), (5, 134.5), (0, 135)]
    p.base(outline, 3.4, d)
    p.poly([(17, 96), (19, 96), (19.5, 102), (18.5, 112), (17, 121), (16, 121), (17.5, 111), (18, 102)], 4.2, d)    # ramus edge
    p.poly([(14.5, 110), (16, 101), (17, 96), (17.3, 101), (16, 110), (14.6, 116)], 2.0, d)                   # 1-px shadow inside the ramus
    p.poly([(0, 118.5), (13.5, 118.5), (13.5, 119.6), (0, 119.6)], 2.6, d)                                   # under the teeth
    p.poly([(0, 123), (4, 122.5), (8, 125), (8.5, 129), (5, 132), (0, 132.5)], 3.8, d)                         # the chin
    p.poly([(4, 122.5), (8, 125), (8.5, 129), (7, 130), (6.5, 126)], 4.4, d)
    p.poly([(0, 133.5), (5, 133.2), (9.5, 131), (14, 127.5), (9.5, 132), (5, 134.5), (0, 135)], 2.4, d)        # underside
    p.soften()
    return p.put(cv, "bone", 30.0)


def paint_spine(cv):
    """Neck vertebrae under the jaw."""
    p = Paint()
    for i in range(4):
        y = 134 + i * 5
        p.base([(0, y), (4.5, y + 0.5), (5.5, y + 2), (4.5, y + 3.5), (0, y + 4)], 3.2 - i * 0.45)
        p.poly([(3, y + 0.5), (4.5, y + 0.5), (5.5, y + 2), (4.5, y + 3.5), (3, y + 3.5)], 4.0 - i * 0.5)
    p.soften()
    return p.put(cv, "bone", 20.0)


def mail(cv):
    """A collar of mail between the neck and the pauldrons, fading into the dark."""
    for y in range(132, 152):
        for x in range(HW - 40, HW - 4):
            c = HW - 0.5 - x
            top = 131 + c * 0.12
            if y < top or cv.part[y, x] != 0 or cv.fixed[y, x] >= 0:
                continue
            fade = (151 - y) / 18.0
            ring = (x + (y // 2) % 2 * 2) % 4
            row = y % 2
            if fade < 0.15:
                continue
            if row == 0 and ring in (1, 2):
                cv.px(x, y, LBLUE if fade > 0.7 and ring == 1 and c > 18 else DGREY)
            elif row == 1 and ring == 0:
                cv.px(x, y, BLUE if fade > 0.35 else BLACK)


ARCH_C, ARCH_Y = 0.0, 84.0
ARCH = False


def arch(cv):
    """The floor the guardian stands on. (A dark stone arch behind it was
    tried, ARCH=True: it cost some 200 bytes the game does not have.)"""
    for y in range(0, 124 if ARCH else 0):
        for x in range(16, HW):
            c = HW - 0.5 - x
            yy = y + 0.5
            if yy < ARCH_Y:
                ri = math.hypot(c / 44.0, (yy - ARCH_Y) / 72.0)
                ro = math.hypot(c / 52.0, (yy - ARCH_Y) / 80.0)
                inside = ri >= 1.0 and ro < 1.0
                ang = math.atan2(ARCH_Y - yy, c * 1.6)
                joint = (ang * 180 / math.pi) % 15 < 1.6 and ri > 1.02
                edge = ri < 1.035
            else:
                inside = 44 <= c < 52
                joint = int(yy - ARCH_Y) % 11 == 10
                edge = c < 45
            if not inside or cv.fixed[y, x] >= 0 or cv.part[y, x] != 0:
                continue
            u = 2 * x + 1
            d = math.hypot(u - TORCH[0], (yy - TORCH[1]) * 0.8)
            warm = d < 40
            if joint:
                col = BLACK
            elif edge:
                col = BROWN if warm else DGREY
            elif warm:
                col = BROWN if (x + y) & 1 else BLACK
            else:
                col = BLACK
            cv.px(x, y, col)
    # the floor: flagstones in perspective, lit where the torch is near
    for y in range(120, 144):
        for x in range(0, HW):
            if cv.fixed[y, x] >= 0 or cv.part[y, x] != 0:
                continue
            yy = y + 0.5
            depth = (yy - 112) / 32.0
            c = HW - 0.5 - x
            bed = y in (120, 124, 130, 138)
            radial = int((c / depth) / 9.0 + 0.5) != int((c + 1) / depth / 9.0 + 0.5)
            u = 2 * x + 1
            d = math.hypot(u - TORCH[0], (yy - 118) * 2.0)
            lvl = max(0.0, 1.0 - d / 70.0) * (1.2 - (yy - 120) / 40.0)
            if bed or radial:
                col = BLACK
            elif lvl > 0.6:
                col = ORANGE if (x + y) & 1 else BROWN
            elif lvl > 0.35:
                col = BROWN
            elif lvl > 0.12:
                col = BROWN if (x + y) & 1 else BLACK
            elif lvl > 0.0 or (x + y) % 4 == 0:
                col = BLUE if (x + y) & 1 and c > 30 else BLACK
            else:
                col = BLACK
            cv.px(x, y, col)


def pillar(cv):
    """The torch pillar: dressed stone, lit by the flame and falling off into the dark."""
    for y in range(0, 128):
        for x in range(0, 16):
            yy = y + 0.5
            course = y // 10
            off = 0 if course % 2 == 0 else 4
            in_x, in_y = (x + off) % 8, y % 10
            u = 2 * x + 1
            d = math.hypot(u - TORCH[0], (yy - TORCH[1]) * 0.75)
            lvl = max(0.16, 1.0 - d / 46.0)
            if y >= 104:                     # the plinth: one row, lit along its top
                if y >= 111:
                    col = BLACK
                elif y == 104:
                    col = YELLOW if x % 8 != 7 else ORANGE
                elif y == 105:
                    col = ORANGE
                else:
                    col = BROWN if (x + y) & 1 or y < 108 else BLACK
                cv.px(x, y, col)
                continue
            if in_x == 7 or in_y == 9:
                col = BLACK
            elif in_y == 0 and lvl > 0.55:
                col = YELLOW
            elif lvl > 0.62:
                col = ORANGE
            elif lvl > 0.45:
                col = ORANGE if (x + y) & 1 else BROWN
            elif lvl > 0.28:
                col = BROWN
            elif lvl > 0.13:
                col = BROWN if (x + y) & 1 else BLACK
            else:
                col = BLACK
            cv.px(x, y, col)
    # the niche the torch burns in: its own two cells, so its flicker
    # (a colour cycle on just those cells) touches nothing else
    for y in range(24, 40):
        for x in range(4, 12):
            cv.px(x, y, BLACK)
    key = {"y": YELLOW, "o": ORANGE, "w": WHITE, ".": None}
    for j, row in enumerate(FLAME):
        for i, ch in enumerate(row):
            if key[ch] is not None:
                cv.px(5 + i, 25 + j, key[ch])
    for j, row in enumerate(SCONCE):
        for i, ch in enumerate(row):
            if ch != ".":
                cv.px(4 + i, 40 + j, {"n": BROWN, "o": ORANGE, "d": DGREY}[ch])


FLAME = [  # 6 x 15, inside the niche
    "...y..",
    "..y...",
    "..yy..",
    ".yy...",
    ".oyy..",
    ".oyyo.",
    "ooywyo",
    "oywwyo",
    "oywwyo",
    "oywwyo",
    "oyywyo",
    ".oyyo.",
    ".oyyo.",
    "..oo..",
    "..o...",
]
SCONCE = [
    "nooooonn",
    ".nooonn.",
    "..nonn..",
    "...nn...",
    "...nd...",
    "...nd...",
    "..nnnd..",
    ".......",
]


def guardian(jaw=0.0, body=True):
    cv = Canvas()
    pillar(cv)

    if body:
        def low_fade(a, v, z):
            return np.clip((150.0 - v) / 16.0, 0, 1)

        def paul_shade(a, v, z):
            rim = np.abs(np.hypot((a - 78) / 36.0, (v - 139) / 19.0) - 0.84) < 0.05
            return np.where(rim, 0.4, 1.0) * np.clip((150.0 - v) / 14.0, 0, 1)
        cv.add(pauldron, "metal", zoff=-12, gain=0.85, shade=paul_shade)
        cv.add(spike, "metal", zoff=-8, gain=0.95)

    zh, th = horn(A, V)

    def horn_fn(a, v):
        return horn(a, v)[0]

    def horn_shade(a, v, z):
        ring = (np.mod(th * 12.0, 1.0) < 0.10) & (th > 0.05) & (th < 0.92)
        return np.where(ring, 0.55, 1.0)
    cv.add(horn_fn, "horn", zoff=-4, gain=1.05, shade=horn_shade)

    if body:
        paint_spine(cv)
    jaw_part = paint_mandible(cv, jaw)
    skull_part = paint_skull(cv)
    gum = 104
    cv.outline()

    low0 = int(gum + 9 + jaw)
    for y in range(gum, low0):                                  # the dark of the mouth
        for x in range(HW - 15, HW):
            cv.px(x, y, BLACK)
    stamp(cv, LOWER_TEETH, HW - 1, low0)
    stamp(cv, UPPER_TEETH, HW - 1, gum)
    if body:
        mail(cv)
    # arch(cv): the floor and an arch behind the guardian, ~300 bytes the game does not have
    return cv


def line(cv, pts, c, wide=False):
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        n = max(abs(x1 - x0), abs(y1 - y0), 1)
        for i in range(n + 1):
            x = round(x0 + (x1 - x0) * i / n)
            y = round(y0 + (y1 - y0) * i / n)
            cv.px(x, y, c)
            if wide:
                cv.px(x + 1, y, c)


# ---------------------------------------------------------------------------
# Readouts: the name plate ornaments and the health bar (rows 18-20)
# ---------------------------------------------------------------------------

BAR_Y = 154           # top line of the bar's frame
BAR_HALF = 30         # half the bar's inside width in multicolour pixels (60 px = 30 hit points)


def readouts(cv):
    cx = HW                                   # the centre line is the right edge of the half
    top, bot = BAR_Y, BAR_Y + 11
    inner_l = cx - BAR_HALF
    for y in range(BAR_Y - 10, H):
        for x in range(16, HW):
            cv.px(x, y, BLACK)
    # frame: orange with a yellow inner edge
    for x in range(inner_l - 1, HW):
        cv.px(x, top, ORANGE)
        cv.px(x, top + 1, YELLOW)
        cv.px(x, bot, ORANGE)
        cv.px(x, bot - 1, BROWN)
    for y in range(top + 1, bot):
        cv.px(inner_l - 1, y, YELLOW)
        cv.px(inner_l - 2, y, ORANGE)
    # the fill: red, a light band on top, a dark foot, a notch every 5 points
    for x in range(inner_l, HW):
        point = (x - inner_l) // 2                      # 0-14: 2 pixels a point
        notch = (x - inner_l) % 10 == 0 and point > 0 or x == HW - 1
        for y in range(top + 2, bot - 1):
            c = RED
            if y == top + 2:
                c = LRED
            if y >= bot - 3:
                c = BROWN if (x + y) % 2 == 0 else RED
            if notch:
                c = BLACK if y > top + 2 else BROWN
            cv.px(x, y, c)
    # end cap: a pointed finial with a stud
    cap = [
        "......o.",
        "....ooy.",
        "..ooyyy.",
        "ooyywyy.",
        "ooyywyy.",
        "..ooyyy.",
        "....ooy.",
        "......o.",
    ]
    key = {"o": ORANGE, "y": YELLOW, "w": WHITE}
    for j, row in enumerate(cap):
        for i, ch in enumerate(row):
            if ch != ".":
                cv.px(inner_l - 10 + i, top + 2 + j, key[ch])
    # filigree either side of the name (row 18)
    ny = 147
    for x in range(inner_l - 8, inner_l + 6):
        cv.px(x, ny, ORANGE if x % 2 else BROWN)
    for dx, dy in ((0, 0), (1, -1), (1, 1), (2, 0), (1, 0)):
        cv.px(inner_l + 6 + dx, ny + dy, YELLOW if (dx, dy) == (1, 0) else ORANGE)


# ---------------------------------------------------------------------------
# Lettering
# ---------------------------------------------------------------------------

FONT = {
    "A": [".#.", "#.#", "#.#", "###", "#.#", "#.#", "#.#"],
    "B": ["##.", "#.#", "#.#", "##.", "#.#", "#.#", "##."],
    "C": [".##", "#..", "#..", "#..", "#..", "#..", ".##"],
    "D": ["##.", "#.#", "#.#", "#.#", "#.#", "#.#", "##."],
    "E": ["###", "#..", "#..", "##.", "#..", "#..", "###"],
    "F": ["###", "#..", "#..", "##.", "#..", "#..", "#.."],
    "G": [".##", "#..", "#..", "#.#", "#.#", "#.#", ".##"],
    "H": ["#.#", "#.#", "#.#", "###", "#.#", "#.#", "#.#"],
    "I": ["#", "#", "#", "#", "#", "#", "#"],
    "K": ["#..#", "#.#.", "##..", "##..", "#.#.", "#..#", "#..#"],
    "L": ["#..", "#..", "#..", "#..", "#..", "#..", "###"],
    "N": ["#..#", "##.#", "##.#", "#.##", "#.##", "#..#", "#..#"],
    "O": ["###", "#.#", "#.#", "#.#", "#.#", "#.#", "###"],
    "P": ["##.", "#.#", "#.#", "##.", "#..", "#..", "#.."],
    "R": ["##.", "#.#", "#.#", "##.", "#.#", "#.#", "#.#"],
    "S": [".##", "#..", "#..", ".#.", "..#", "..#", "##."],
    "T": ["###", ".#.", ".#.", ".#.", ".#.", ".#.", ".#."],
    "U": ["#.#", "#.#", "#.#", "#.#", "#.#", "#.#", ".#."],
    "W": ["#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#"],
    "Y": ["#.#", "#.#", "#.#", ".#.", ".#.", ".#.", ".#."],
    " ": ["..", "..", "..", "..", "..", "..", ".."],
}


def name_plate(text, cells=12, hi=YELLOW, lo=ORANGE):
    """A name, centred in `cells` cells of one row: RGBA image, 4*cells x 8."""
    width = sum(len(FONT[ch][0]) + 1 for ch in text) - 1
    img = Image.new("RGBA", (cells * 4, 8), P[0] + (255,))
    x = (cells * 4 - width) // 2
    for ch in text:
        g = FONT[ch]
        for j, row in enumerate(g):
            for i, bit in enumerate(row):
                if bit == "#":
                    img.putpixel((x + i, j), P[hi if j < 4 else lo] + (255,))
        x += len(g[0]) + 1
    return img


# ---------------------------------------------------------------------------
# Sprites 6-7: eight hi-res frames, 24x21, one colour each
# ---------------------------------------------------------------------------
# 0 eye: a hot core with sparks around it  1 eye, narrowed to a blazing slit
# 2 3 the player's blade stroke, left and right half (shown double size)
# 4 5 the guardian's claws, left and right half (shown double size)
# 6 "MISS" (sprite 7 shows it again in black, as its shadow)
# 7 a spark burst

EYE = [
    "........................",
    "........................",
    "........................",
    "........................",
    "............#...........",
    "........................",
    ".......#.........#......",
    "..........####..........",
    ".........######.........",
    "....#...########...#....",
    "........##.##.##........",
    "....#...########...#....",
    ".........######.........",
    "..........####..........",
    ".......#.........#......",
    "........................",
    "............#...........",
    "........................",
    "........................",
    "........................",
    "........................",
]
EYE_NARROW = [
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "..#..................#..",
    "......#..........#......",
    "...#.###.######.###.#...",
    ".##################.....",
    "...#.###.######.###.#...",
    "......#..........#......",
    "..#..................#..",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
]
MISS = [
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "##...##.##..####...####.",
    "###.###.##.##.....##....",
    "#######.##.##.....##....",
    "##.#.##.##..###....###..",
    "##...##.##....##.....##.",
    "##...##.##....##.....##.",
    "##...##.##.####...####..",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
    "........................",
]


def sprite_sheet():
    sheet = Image.new("RGBA", (24 * 8, 21), (0, 0, 0, 0))
    on = P[1] + (255,)

    def put(frame, rows):
        for y, row in enumerate(rows):
            for x, ch in enumerate(row):
                if ch == "#":
                    sheet.putpixel((frame * 24 + x, y), on)
    put(0, EYE)
    put(1, EYE_NARROW)
    put(6, MISS)

    # blade stroke: a crescent across both frames (48x21), thick in the middle
    for x in range(48):
        t = x / 47.0
        yc = 19.0 - 17.0 * t - 5.0 * math.sin(math.pi * t)          # bottom-left to top-right, bowed
        half = 0.4 + 2.6 * math.sin(math.pi * t) ** 1.5
        for y in range(21):
            d = abs(y - yc)
            if d <= half or (d <= half + 1.5 and (x + y) % 2 == 0 and 0.15 < t < 0.85):
                sheet.putpixel((2 * 24 + x, y), on)
    # claws: three long tapering rakes, top-left to bottom-right across both frames
    for k in range(3):
        for x in range(48):
            t = x / 47.0
            yc = -6.0 + k * 6.0 + 24.0 * t
            half = 0.35 + 1.6 * math.sin(math.pi * t) ** 0.8
            if t < 0.06 + 0.05 * k or t > 0.97 - 0.04 * (2 - k):
                continue
            for y in range(21):
                if abs(y - yc) <= half:
                    sheet.putpixel((4 * 24 + x, y), on)
    # spark burst
    for i in range(12):
        ang = i * math.pi / 6
        ln = 10 if i % 2 == 0 else 6
        for r in range(2, ln):
            x = int(round(11.5 + math.cos(ang) * r * 1.15))
            y = int(round(10 + math.sin(ang) * r))
            if 0 <= x < 24 and 0 <= y < 21:
                sheet.putpixel((7 * 24 + x, y), on)
    return sheet


# ---------------------------------------------------------------------------
# The four guardians: a colour table each (Screen.RemapCellColors, applied
# to the left half before it is mirrored) and a one-sided overlay
# ---------------------------------------------------------------------------
# Each table maps its results to themselves, so it can be applied again
# over cells it already changed (the jaw is redrawn and re-tinted).

KINDS = [
    ("BONE WARDEN", {}),
    ("CRYPT GHOUL", {8: 9, 7: 13, 2: 5}),                        # grey-green bone, green fire
    ("BLOOD FIEND", {6: 9, 11: 2, 14: 10}),                       # bone kept, iron and stone run red
    ("LICH KING", {9: 6, 8: 4, 7: 15, 2: 4}),                     # cold pale bone, violet fire
]
EYE_RAMPS = [  # the eye sprites' colours, dim to hot, per guardian
    (RED, ORANGE, YELLOW, WHITE),
    (GREEN, LGREEN, YELLOW, WHITE),
    (RED, LRED, YELLOW, WHITE),
    (VIOLET, LBLUE, CYAN, WHITE),
]
TINT_COLS = (4, 20)    # the tint covers columns 4-19 (not the torch pillar) and rows 0-17
TINT_ROWS = 18
JAW = (14, 12, 6, 6)   # the jaw's cells in the left half: column, row, width, height


def table(kind):
    t = list(range(16))
    for a, b in KINDS[kind][1].items():
        t[a] = b
    for a in range(16):
        assert t[t[a]] == t[a], "table %d is not idempotent at %d" % (kind, a)
    return t


def tinted(half, kind):
    t = table(kind)
    out = half.copy()
    region = out[:TINT_ROWS * 8, TINT_COLS[0] * 4:TINT_COLS[1] * 4]
    out[:TINT_ROWS * 8, TINT_COLS[0] * 4:TINT_COLS[1] * 4] = np.vectorize(lambda c: t[c])(region)
    return out


def fpx(img, x, y, c):
    if 0 <= x < img.shape[1] and 0 <= y < img.shape[0]:
        img[y, x] = c


def fline(img, pts, c):
    for (x0, y0), (x1, y1) in zip(pts, pts[1:]):
        n = max(abs(x1 - x0), abs(y1 - y0), 1)
        for i in range(n + 1):
            fpx(img, round(x0 + (x1 - x0) * i / n), round(y0 + (y1 - y0) * i / n), c)


def overlay(kind, f):
    """Draws guardian `kind`'s own marks on the full (mirrored, tinted)
    picture f, in place. One-sided, to break the mirror."""
    if kind == 0:      # the Warden is the plain one
        pass
    # kind 1, the Crypt Ghoul: its colours and name only (a broken horn cost 63 bytes)
    elif kind == 2:    # a third eye, and blood running from the right socket
        for dy in range(-4, 5):
            w = 3 - abs(dy) // 2
            for dx in range(-w + 1, w):
                c = RED if abs(dy) > 2 or abs(dx) == w - 1 else (YELLOW if dx == 0 and dy == 0 else LRED)
                fpx(f, 79 + dx + (1 if dx >= 0 else 0), 43 + dy, c)
        for y in range(82, 96):
            x = 92 + (1 if y > 88 else 0)
            fpx(f, x, y, RED)
            if y in (86, 87, 93, 94, 95):
                fpx(f, x + 1, y, RED)
    # kind 3, the Lich King: its colours and name only (a crown cost 120 bytes)


# ---------------------------------------------------------------------------
# Output
# ---------------------------------------------------------------------------

def to_png(idx, alpha=None):
    img = Image.new("RGBA", (idx.shape[1], idx.shape[0]))
    for y in range(idx.shape[0]):
        for x in range(idx.shape[1]):
            a = 255 if alpha is None or alpha[y, x] else 0
            img.putpixel((x, y), P[int(idx[y, x])] + (a,))
    return img


def full(idx):
    return np.concatenate([idx, idx[:, ::-1]], axis=1)


def view(idx, path, scale=3):
    """A look at the result with the C64's 2:1 pixels."""
    img = to_png(idx).convert("RGB")
    img = img.resize((idx.shape[1] * 2 * scale, idx.shape[0] * scale), Image.NEAREST)
    img.save(path)


def diff_cells(a, b):
    """Mask of the pixels in cells where a and b differ."""
    m = np.zeros(a.shape, bool)
    for cy in range(a.shape[0] // 8):
        for cx in range(a.shape[1] // 4):
            s = (slice(cy * 8, cy * 8 + 8), slice(cx * 4, cx * 4 + 4))
            if (a[s] != b[s]).any():
                m[s] = True
    return m


def save_img(name, idx, alpha=None, planes="bmc"):
    png = os.path.join(SRC, name + ".png")
    to_png(idx, alpha).save(png)
    return c64img.convert(png, os.path.join(OUT, name + ".img"), planes=planes)


def render(**kw):
    cv = guardian(**kw)
    readouts(cv)
    return cv.quantise()




def cells_mask(shape, col, row, w, h):
    m = np.zeros(shape, bool)
    m[row * 8:(row + h) * 8, col * 4:(col + w) * 4] = True
    return m


def save_full(name, idx, alpha=None, planes="bmc"):
    """Converts a full-width (160-pixel) picture."""
    png = os.path.join(SRC, name + ".png")
    to_png(idx, alpha).save(png)
    return c64img.convert(png, os.path.join(OUT, name + ".img"), planes=planes)


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    preview = os.path.join(HERE, "..", "..", ".debug-tmp")
    os.makedirs(preview, exist_ok=True)
    only_preview = len(sys.argv) > 1 and sys.argv[1] == "preview"

    base = render()
    view(full(base), os.path.join(preview, "enc_base.png"))
    if only_preview:
        png = os.path.join(preview, "enc_half.png")
        to_png(base).save(png)
        c64img.convert(png, None)
        return
    # pixels and colours apart, so the pixels can go up unseen
    save_img("enc_base_px", base, planes="b")
    save_img("enc_base_col", base, planes="mc")
    jaw = cells_mask(base.shape, *JAW)

    # the four guardians: tables and overlays
    tables = []
    sheet = []
    for kind in range(4):
        tables.append(table(kind))
        f = full(tinted(base, kind))
        g = f.copy()
        overlay(kind, g)
        plate = name_plate(KINDS[kind][0]).convert("RGB")
        for y in range(8):
            for x in range(48):
                g[144 + y, 56 + x] = c64img.nearest_color(plate.getpixel((x, y)))
        m = diff_cells(f, g)
        fj = np.zeros(f.shape, bool)
        fj[:, :80] = jaw
        fj[:, 80:] = jaw[:, ::-1]
        assert not (m & fj).any(), "overlay %d touches the jaw" % kind
        save_full("enc_kind%d" % kind, g, m)
        sheet.append(g)
    view(np.concatenate([np.concatenate(sheet[:2], 1), np.concatenate(sheet[2:], 1)], 0),
         os.path.join(preview, "enc_kinds.png"), scale=2)
    with open(os.path.join(SRC, "enc_tables.txt"), "w") as fh:
        for kind, t in enumerate(tables):
            fh.write("kind %d: %s  eyes %s\n" % (kind, ",".join(map(str, t)), ",".join(map(str, EYE_RAMPS[kind]))))

    sheet_img = sprite_sheet()
    png = os.path.join(SRC, "enc_sprites.png")
    sheet_img.save(png)
    c64spr.convert(png, os.path.join(OUT, "enc_sprites.spr"), preview=os.path.join(preview, "enc_sprites.png"))



if __name__ == "__main__":
    main()
