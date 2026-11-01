#!/usr/bin/env python3
"""
A full-bleed multicolour placeholder picture (engine-owned, for checking
Master.Picture: multicolour rows 0-20 over a hi-res caption band): a night
sky graded in bands, a horizon of hills and a moon. 40 x 21 cells, black
background, at most three colours per cell besides it. Art replaces it with
the real full-bleed pictures (DECK.md A1, A2).

    python C64Presentation/art/engine/make_backdrop.py
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "..", "tools"))
import c64img  # noqa: E402

P = c64img.PALETTE


def main():
    from PIL import Image, ImageDraw
    W, H = 160, 168                      # multicolour pixels
    img = Image.new("RGB", (W, H), P[0])
    d = ImageDraw.Draw(img)
    # Sky: blue at the top, dithered into purple, then to dark red.
    bands = [(0, 6), (40, 4), (88, 2), (128, 9)]
    for i, (y0, c) in enumerate(bands):
        y1 = bands[i + 1][0] if i + 1 < len(bands) else 168
        d.rectangle([0, y0, W - 1, y1 - 1], fill=P[c])
        if i + 1 < len(bands):          # an 8-line ordered dither into the next band
            nxt = bands[i + 1][1]
            for y in range(y1 - 8, y1):
                for x in range(W):
                    if (x + y) % 2 == 0 and (y - (y1 - 8)) >= ((x // 2) % 4) * 2:
                        img.putpixel((x, y), P[nxt])
    # Moon.
    d.ellipse([112, 18, 135, 41], fill=P[15])
    d.ellipse([118, 14, 141, 37], fill=P[6])
    # Hills: two black silhouettes.
    d.polygon([(0, 150), (30, 128), (62, 146), (96, 120), (130, 142), (159, 132), (159, 167), (0, 167)], fill=P[0])
    os.makedirs(os.path.join(HERE, "build"), exist_ok=True)
    png = os.path.join(HERE, "backdrop.png")
    img.save(png)
    c64img.convert(png, os.path.join(HERE, "build", "backdrop.img"), preview=os.path.join(HERE, "backdrop.c64.png"))


if __name__ == "__main__":
    main()
