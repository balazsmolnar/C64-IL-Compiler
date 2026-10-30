#!/usr/bin/env python3
"""
Generates the art for the SCENE_TECHDEMO build (see Catacombs/TECH.md): a
worked example of the whole asset pipeline, not game art.

    python Catacombs/art/techdemo_art.py

writes PNGs to Catacombs/art/src/ and runs them through tools/c64img.py and
tools/c64spr.py into Catacombs/art/build/ (the files the .csproj embeds).
"""
import math
import os
import sys

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import c64img   # noqa: E402
import c64spr   # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
P = c64img.PALETTE
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]


def shade(ramp, level, centre, x, y):
    """Ordered-dither pick from a colour ramp; level 0..1. `centre` is the
    level at the middle of the pixel's cell: the result is kept within three
    neighbouring ramp entries around it, so no cell needs more than three
    colours (multicolour's limit) however steep the gradient is."""
    top = len(ramp) - 1
    low = max(0, min(top - 2, int(round(max(0.0, min(1.0, centre)) * top)) - 1))
    f = max(low, min(low + 1.999, max(0.0, min(0.999, level)) * top))
    i = int(f)
    return ramp[min(top, i + (1 if (f - i) * 16 > BAYER[y & 3][x & 3] else 0))]


def backdrop():
    """160x168 (multicolour pixels): a torch-lit brick wall and a flagstone floor."""
    w, h = 160, 168
    img = Image.new("RGB", (w, h), P[0])
    px = img.load()
    torches = [(40, 56), (120, 56)]
    warm = [0, 9, 8, 10, 7]       # black, brown, orange, light red, yellow
    cold = [0, 6, 11, 12, 15]     # black, blue, dark grey, mid grey, light grey

    def dist(x, y):
        return min(math.hypot((x - tx) * 2, y - ty) for tx, ty in torches)

    for y in range(h):
        for x in range(w):
            cx, cy = (x // 4) * 4 + 2, (y // 8) * 8 + 4          # the cell's centre
            d, dc = dist(x, y), dist(cx, cy)
            light, light_c = max(0.0, 1.0 - d / 110.0), max(0.0, 1.0 - dc / 110.0)
            if y < 112:                                   # wall: bricks 16x8, staggered
                row = y // 8
                bx = (x + (8 if row & 1 else 0)) % 16
                mortar = (y % 8 == 7) or bx == 15
                ramp = warm if dc < 46 else cold          # one ramp per cell
                px[x, y] = P[shade(ramp, light * (0.35 if mortar else 0.95), light_c * 0.95, x, y)]
            else:                                         # floor: perspective flagstones
                depth = (y - 112) / 56.0
                depth_c = (cy - 112) / 56.0
                fx = (x - 80) / (0.35 + depth)
                line = (int(fx) % 28 == 0) or ((y - 112) in (0, 7, 17, 30, 46))
                level = (0.25 + 0.5 * depth) * (0.4 if line else 1.0) * (0.55 + 0.45 * light)
                centre = (0.25 + 0.5 * depth_c) * (0.55 + 0.45 * light_c)
                px[x, y] = P[shade(cold, level, centre, x, y)]
    d = ImageDraw.Draw(img)
    for tx, ty in torches:                                # a black niche (whole cells), bracket, flame
        d.rectangle([tx - 4, ty - 8, tx + 3, ty + 15], fill=P[0])
        d.rectangle([tx - 1, ty + 2, tx, ty + 12], fill=P[9])
        d.polygon([(tx - 3, ty + 2), (tx + 2, ty + 2), (tx, ty - 7)], fill=P[8])
        d.polygon([(tx - 2, ty + 1), (tx + 1, ty + 1), (tx, ty - 4)], fill=P[7])
    d.rectangle([64, 48, 95, 111], fill=P[0])             # a doorway (whole cells)
    d.rectangle([66, 50, 93, 111], fill=P[9])
    for x in range(68, 92, 6):
        d.line([x, 50, x, 111], fill=P[8])
    d.rectangle([86, 80, 88, 83], fill=P[7])
    return img


def overlay():
    """A 48x40 treasure chest with a transparent surround (an overlay image)."""
    img = Image.new("RGBA", (24, 40), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rectangle([2, 14, 21, 37], fill=P[9] + (255,))
    d.rectangle([2, 14, 21, 20], fill=P[8] + (255,))
    d.rectangle([2, 21, 21, 22], fill=P[0] + (255,))
    for x in (2, 11, 21):
        d.line([x, 14, x, 37], fill=P[7] + (255,))
    d.rectangle([10, 23, 12, 26], fill=P[7] + (255,))
    d.rectangle([4, 9, 19, 13], fill=P[7] + (255,))       # gold heaped on top
    d.point([(6, 8), (11, 7), (15, 8), (17, 10)], fill=P[1] + (255,))
    return img


def panel():
    """320x32 hi-res status panel frame (rows 21-24): one colour on black."""
    img = Image.new("RGB", (320, 32), P[0])
    d = ImageDraw.Draw(img)
    d.rectangle([0, 1, 319, 31], outline=P[12])
    d.rectangle([2, 3, 317, 29], outline=P[12])
    for x in range(12, 312, 8):
        d.point([(x, 2), (x, 30)], fill=P[12])
    return img


def sprite_sheets():
    """A 16-frame hi-res sheet (a spinning coin, numbered) and an 8-frame multicolour one."""
    hi = Image.new("RGBA", (24 * 16, 21), (0, 0, 0, 0))
    d = ImageDraw.Draw(hi)
    for f in range(16):
        ox = f * 24
        half = abs(math.cos(f * math.pi / 8)) * 9 + 1
        d.ellipse([ox + 12 - half, 2, ox + 11 + half, 18], fill=P[7] + (255,))
        if half > 4:
            d.rectangle([ox + 11, 6, ox + 12, 14], fill=(0, 0, 0, 0))
        for b in range(4):                                # frame number in binary, bottom row
            if f >> b & 1:
                d.rectangle([ox + 18 - b * 5, 19, ox + 21 - b * 5, 20], fill=P[7] + (255,))
    mc = Image.new("RGBA", (12 * 8, 21), (0, 0, 0, 0))
    d = ImageDraw.Draw(mc)
    for f in range(8):
        ox = f * 12
        bob = (0, 1, 2, 1, 0, 1, 2, 1)[f]
        d.rectangle([ox + 3, 1 + bob, ox + 8, 6 + bob], fill=P[10] + (255,))      # head (sprite colour)
        d.point([(ox + 4, 3 + bob), (ox + 7, 3 + bob)], fill=P[0] + (255,))       # eyes  (shared 1)
        d.rectangle([ox + 2, 7 + bob, ox + 9, 14 + bob], fill=P[6] + (255,))      # tunic (shared 2)
        d.rectangle([ox + 2, 10 + bob, ox + 9, 10 + bob], fill=P[0] + (255,))     # belt
        swing = (f % 4) - 1
        d.rectangle([ox + 3 - swing, 15 + bob, ox + 4 - swing, 18], fill=P[10] + (255,))
        d.rectangle([ox + 7 + swing, 15 + bob, ox + 8 + swing, 18], fill=P[10] + (255,))
    return hi, mc


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    backdrop().save(os.path.join(SRC, "techdemo_room.png"))
    overlay().save(os.path.join(SRC, "techdemo_chest.png"))
    panel().save(os.path.join(SRC, "techdemo_panel.png"))
    hi, mc = sprite_sheets()
    hi.save(os.path.join(SRC, "techdemo_coin.png"))
    mc.save(os.path.join(SRC, "techdemo_walker.png"))

    c64img.convert(os.path.join(SRC, "techdemo_room.png"), os.path.join(OUT, "techdemo_room.img"),
                   preview=os.path.join(SRC, "techdemo_room.c64.png"))
    c64img.convert(os.path.join(SRC, "techdemo_chest.png"), os.path.join(OUT, "techdemo_chest.img"))
    c64img.convert(os.path.join(SRC, "techdemo_panel.png"), os.path.join(OUT, "techdemo_panel.img"),
                   mode=c64img.MODE_HIRES)
    c64spr.convert(os.path.join(SRC, "techdemo_coin.png"), os.path.join(OUT, "techdemo_coin.spr"))
    c64spr.convert(os.path.join(SRC, "techdemo_walker.png"), os.path.join(OUT, "techdemo_walker.spr"),
                   multicolor=True, px=1, mc1=0, mc2=6)


if __name__ == "__main__":
    main()
