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
    "bone":  ([0, 6, 11, 12, 15, 7, 1], [0, .08, .22, .42, .66, .98, 1.25]),
    "metal": ([0, 6, 11, 12, 15],       [0, .14, .36, .62, .92]),
    "horn":  ([0, 9, 8, 7],             [0, .18, .46, .92]),
}


def ramp_pos(mat, level):
    cols, stops = MAT[mat]
    for i in range(len(stops) - 1, -1, -1):
        if level >= stops[i]:
            if i == len(stops) - 1:
                return float(i)
            return i + (level - stops[i]) / (stops[i + 1] - stops[i])
    return 0.0


TORCH = (22.0, 44.0, 55.0)      # the left torch's flame; the right one is its mirror
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


SKULL_OUTLINE = [(0, 10), (4, 10), (8, 12), (12, 15), (16, 19), (19, 24), (22, 31), (24, 39), (25, 48),
                 (25, 57), (24, 63), (23, 68), (22.5, 76), (23.5, 83), (23.5, 89), (21, 93), (17, 96),
                 (15, 99), (14, 103), (14, 112), (0, 112)]
SOCKET = [(4.5, 67), (9, 64), (15, 62), (20.5, 63.5), (21.5, 70), (20, 77), (15, 82), (9, 82), (5.5, 78), (4, 72)]
NOSE_HOLE = [(0, 84), (1.5, 84), (3.5, 88), (4.5, 94), (3.5, 98), (1, 98), (0, 96)]


def paint_skull(cv):
    p = Paint()
    p.base(SKULL_OUTLINE, 3)
    # cranium
    p.poly([(0, 10), (4, 10), (8, 12), (12, 15), (10, 18), (5, 15), (0, 15)], 2)                    # falls away on top
    p.poly([(0, 14), (4, 14), (6, 22), (7, 40), (6, 54), (0, 58)], 2.5)                             # the unlit middle
    p.poly([(9, 15), (13, 17), (17, 22), (20, 28), (22, 36), (22.5, 50), (21, 55), (12, 54), (9, 40), (8, 24)], 4)
    p.poly([(14, 20), (16, 19), (19, 24), (22, 31), (24, 39), (25, 48), (25, 57), (23, 57), (23, 48),
            (22, 40), (20, 32), (17, 25)], 5)                                                       # rim light
    p.poly([(23, 38), (24.5, 40), (25, 50), (23.5, 50)], 6)
    # brow ridge, glabella
    p.poly([(0, 60), (3, 60), (8, 58), (15, 56), (21, 56), (24, 60), (23, 64), (21, 62), (15, 61), (9, 63),
            (4, 66), (0, 66)], 4)
    p.poly([(15, 56), (21, 56), (24, 60), (23, 61), (20, 58), (15, 57.5)], 5)
    p.poly([(0, 59), (3, 60), (4, 66), (3, 70), (0, 70)], 3)
    p.poly([(0, 55), (7, 55), (12, 54), (21, 55), (21, 56), (15, 56), (8, 58), (3, 60), (0, 60)], 2.5)   # furrow above it
    # outer rim of the socket
    p.poly([(20.5, 63), (23, 66), (22.5, 76), (23.5, 83), (21.5, 84), (20.5, 77), (21.5, 70)], 4)
    p.poly([(22, 66), (23, 66), (22.5, 76), (23.5, 83), (22.5, 83), (21.7, 76)], 5)
    # nose bridge
    p.poly([(1.5, 66), (3.5, 67), (4.5, 76), (4.5, 86), (2, 86), (1.5, 76)], 4)
    p.poly([(0, 66), (1.5, 66), (1.5, 86), (0, 86)], 3)
    # cheekbone and what hangs under it
    p.poly([(6, 84), (16, 86), (21, 82), (23.5, 83), (23.5, 89), (21, 93), (15, 93), (9, 91), (6, 89)], 4)
    p.poly([(21, 83), (23.5, 83), (23.5, 89), (22, 91), (21, 88)], 5)
    p.poly([(9, 91), (15, 93), (21, 93), (17, 96), (15, 99), (12, 96), (9, 94)], 2)
    # upper jaw
    p.poly([(5, 90), (9, 92), (12, 96), (14, 100), (14, 104), (11, 104), (9, 98), (6, 94)], 3.5)
    p.poly([(0, 98), (4, 98), (5, 104), (0, 104)], 2.5)
    # holes
    p.cut(SOCKET)
    p.cut(NOSE_HOLE)
    p.poly([(4.5, 70), (6, 69), (6.5, 76), (5.5, 78), (4, 74)], 1)                                  # far wall of the socket
    p.poly([(6, 81), (9, 83.5), (15, 83.5), (20, 79), (21.5, 79), (21, 82), (16, 86), (9, 86), (6, 84)], 4)   # lower rim
    p.soften()
    part = p.put(cv, "bone", 40.0)
    # the roots of the teeth show as ridges in the bone above them
    for y in range(98, 104):
        for c, tone in ((1, 4), (5, 4), (9, 4), (3, 2), (7, 2), (11, 2)):
            x = HW - 1 - c
            if cv.part[y, x] == part:
                cv.direct[y, x] = tone
    return part


def paint_mandible(cv, jaw):
    p = Paint()
    d = jaw
    p.base([(0, 118), (14, 118), (15.5, 108), (17, 99), (20.5, 97), (21.5, 105), (20.5, 116), (18.5, 125),
            (14, 132), (8, 137), (0, 138)], 3, d)
    p.poly([(15.5, 108), (17, 99), (20.5, 97), (21.5, 105), (20, 112), (16, 114)], 2, d)           # under the cheekbone
    p.poly([(20, 99), (21.5, 105), (20.5, 116), (18.5, 125), (17, 125), (19, 116), (20, 106)], 4.5, d)   # lit edge
    p.poly([(0, 118), (14, 118), (14, 121), (0, 121)], 2.5, d)
    p.poly([(0, 123), (5, 123), (8, 126), (7, 131), (4, 133), (0, 133)], 4, d)                      # the chin
    p.poly([(0, 134), (8, 133), (14, 128), (18.5, 125), (14, 132), (8, 137), (0, 138)], 2, d)       # its underside
    p.soften()
    return p.put(cv, "bone", 30.0)


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
    "mmdmmmdmmmdmmm",
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
    "lm.lm.lm.lm.lm",
    "lm.lm.lm.lm.lm",
    "lmdlmdlmdlmdlm",
    "mmdmmdmmdmmdmm",
]


def stones(cv):
    """The wall by the torch: blocks shaded flat, one tone per block."""
    for y in range(H):
        for x in range(16):
            course = y // 12
            off = 0 if course % 2 == 0 else 4
            bx = (x + off) // 8
            in_x, in_y = (x + off) % 8, y % 12
            cxm = bx * 8 - off + 3.5
            cym = course * 12 + 5.5
            d = math.hypot((cxm * 2 + 1) - TORCH[0], (cym - (TORCH[1] + 6)) * 0.9)
            tone = 3 if d < 22 else 2 if d < 40 else 1 if d < 58 else 0
            if in_x == 7 or in_y == 11:
                c = BLACK
            elif tone == 3:
                c = YELLOW if in_y == 0 else ORANGE
            elif tone == 2:
                c = ORANGE if in_y == 0 else (ORANGE if (x + y) & 1 else BROWN)
            elif tone == 1:
                c = BROWN if in_y == 0 else (BROWN if (x + y) & 1 else BLACK)
            else:
                c = BLACK
            cv.px(x, y, c)


def guardian(jaw=0.0, horns="bull", eyes="open", crack=False, third_eye=False, body=True):
    cv = Canvas()
    stones(cv)

    if body:
        def low_fade(a, v, z):
            return np.clip((150.0 - v) / 16.0, 0, 1)
        cv.add(gorget, "metal", zoff=-30, gain=1.0, shade=low_fade)

        def paul_shade(a, v, z):
            rim = np.abs(np.hypot((a - 78) / 36.0, (v - 139) / 19.0) - 0.84) < 0.05
            return np.where(rim, 0.4, 1.0) * np.clip((150.0 - v) / 14.0, 0, 1)
        cv.add(pauldron, "metal", zoff=-12, gain=1.2, shade=paul_shade)
        cv.add(spike, "metal", zoff=-8, gain=1.3)

    if horns == "bull":
        zh, th = horn(A, V)

        def horn_fn(a, v):
            return horn(a, v)[0]

        def horn_shade(a, v, z):
            ring = (np.mod(th * 12.0, 1.0) < 0.10) & (th > 0.05) & (th < 0.92)
            return np.where(ring, 0.55, 1.0)
        cv.add(horn_fn, "horn", zoff=-4, gain=1.05, shade=horn_shade)

    jaw_part = paint_mandible(cv, jaw)
    skull_part = paint_skull(cv)
    gum = 104
    cv.outline()

    low0 = int(gum + 10 + jaw)
    for y in range(gum, low0):                                  # the dark of the mouth
        for x in range(HW - 14, HW):
            cv.px(x, y, BLACK)
    stamp(cv, LOWER_TEETH, HW - 1, low0)
    stamp(cv, UPPER_TEETH, HW - 1, gum)

    for pts in ([(69, 20), (70, 24), (69, 28), (71, 32), (70, 37), (72, 41), (71, 45)],
                [(71, 32), (73, 34), (75, 35)], [(69, 28), (66, 30), (64, 33)]):
        line(cv, pts, BLACK)
    if crack:
        for pts in ([(79, 16), (77, 22), (78, 28), (75, 36), (77, 44), (74, 52), (76, 60)],
                    [(75, 36), (71, 39), (68, 38)], [(77, 44), (79, 48)], [(74, 52), (70, 56), (67, 55)]):
            line(cv, pts, BLACK, wide=True)

    ex, ey = HW - 1 - 11, 77
    if eyes != "none":
        for dy in range(-2, 3):
            for dx in range(-2, 3):
                if abs(dx) + abs(dy) <= 2:
                    cv.px(ex + dx, ey + dy, RED)
        if eyes == "open":
            for dx, dy in ((0, 0), (-1, 0), (0, -1), (1, 0), (0, 1)):
                cv.px(ex + dx, ey + dy, ORANGE if (dx, dy) != (0, 0) else YELLOW)
    if third_eye:
        for dy in range(-4, 5):
            w = 2 - abs(dy) // 2
            for dx in range(w):
                cv.px(HW - 1 - dx, 44 + dy, RED if dx == w - 1 or abs(dy) > 2 else YELLOW)
        cv.px(HW - 1, 44, WHITE)

    torch(cv)
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


FLAME = [  # 6 columns, rows downward; . = nothing
    "..y...",
    "..y...",
    ".yy...",
    ".yyo..",
    ".oyy..",
    ".oyyo.",
    "ooyyo.",
    "oyyyo.",
    "oywyyo",
    "oywwyo",
    "oywwyo",
    "oyywyo",
    ".oyyo.",
    ".oooo.",
]


def torch(cv):
    x0, y0 = 8, 30
    key = {"y": YELLOW, "o": ORANGE, "w": WHITE, "r": LRED}
    for j, row in enumerate(FLAME):
        for i, ch in enumerate(row):
            if ch != ".":
                cv.px(x0 + i, y0 + j, key[ch])
    # iron cup and bracket
    for y, (xa, xb) in enumerate([(8, 13), (8, 13), (9, 12), (9, 12), (10, 11), (10, 11), (10, 11), (10, 11)]):
        for x in range(xa, xb + 1):
            cv.px(x, y0 + len(FLAME) + y, BROWN if (x == xb or y > 3) else ORANGE if x == xa else BROWN)
    for y in range(8):
        cv.px(10, y0 + len(FLAME) + 8 + y, BROWN)
        cv.px(11, y0 + len(FLAME) + 8 + y, BLACK)


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
        k = (cx - 1 - x)
        notch = (k % 10) == 9
        for y in range(top + 2, bot - 1):
            c = RED
            if y == top + 3:
                c = LRED
            if y >= bot - 3:
                c = BROWN if (x + y) % 2 == 0 else RED
            if notch:
                c = BLACK
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
    "K": ["#.#", "#.#", "##.", "#..", "##.", "#.#", "#.#"],
    "L": ["#..", "#..", "#..", "#..", "#..", "#..", "###"],
    "N": ["#..#", "##.#", "##.#", "#.##", "#.##", "#..#", "#..#"],
    "O": [".#.", "#.#", "#.#", "#.#", "#.#", "#.#", ".#."],
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
    save_img("enc_base_px", base, planes="b")
    save_img("enc_base_col", base, planes="mc")

    roar = render(jaw=11.0)
    view(full(roar), os.path.join(preview, "enc_roar.png"))
    m = diff_cells(base, roar)
    save_img("enc_roar", roar, m)
    save_img("enc_shut", base, m)

    for text, label in (("BONE WARDEN", "name0"), ("CRYPT GHOUL", "name1"),
                        ("BLOOD FIEND", "name2"), ("LICH KING", "name3")):
        img = name_plate(text)
        png = os.path.join(SRC, "enc_" + label + ".png")
        img.save(png)
        c64img.convert(png, os.path.join(OUT, "enc_" + label + ".img"))


if __name__ == "__main__":
    main()
