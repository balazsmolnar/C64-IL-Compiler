#!/usr/bin/env python3
"""
A hi-res picture of a 6502-family chip (engine-owned placeholder art for the
ImageElement example slide): 16 x 8 cells, black background, the package in
dark grey with light grey pins and its label in white. Every cell keeps to
two colours (package edges on cell boundaries, pins in the rows above and
below it), so the conversion is exact.

    python C64Presentation/art/engine/make_chip.py
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "..", "tools"))
import c64img  # noqa: E402

P = c64img.PALETTE


def main():
    from PIL import Image, ImageDraw
    W, H = 16 * 8, 8 * 8
    img = Image.new("RGB", (W, H), P[0])
    d = ImageDraw.Draw(img)
    # Package: cells 1-14 x rows 2-5 (x 8..119, y 16..47).
    d.rectangle([8, 16, 119, 47], fill=P[11])
    # Notch: a half circle on the left edge, background coloured.
    d.ellipse([3, 27, 13, 36], fill=P[0])
    d.rectangle([3, 27, 7, 36], fill=P[0])
    # Pins: rows 1 and 6 (y 8..15 above, 48..55 below), 2 px wide every 6 px.
    for x in range(12, 116, 6):
        d.rectangle([x, 10, x + 2, 15], fill=P[15])
        d.rectangle([x, 48, x + 2, 53], fill=P[15])
    # Label, pixel font drawn here (no font file needed): "6510".
    glyphs = {
        "6": ["0111", "1000", "1110", "1001", "1001", "0110"],
        "5": ["1111", "1000", "1110", "0001", "1001", "0110"],
        "1": ["0010", "0110", "0010", "0010", "0010", "0111"],
        "0": ["0110", "1001", "1011", "1101", "1001", "0110"],
    }
    x0, y0, s = 36, 23, 3
    for i, ch in enumerate("6510"):
        for y, row in enumerate(glyphs[ch]):
            for x, b in enumerate(row):
                if b == "1":
                    d.rectangle([x0 + i * 15 + x * s, y0 + y * 3, x0 + i * 15 + x * s + s - 1, y0 + y * 3 + 2], fill=P[1])
    os.makedirs(os.path.join(HERE, "build"), exist_ok=True)
    png = os.path.join(HERE, "chip.png")
    img.save(png)
    c64img.convert(png, os.path.join(HERE, "build", "chip.img"), mode=c64img.MODE_HIRES,
                   preview=os.path.join(HERE, "chip.c64.png"), strict=True)


if __name__ == "__main__":
    main()
