#!/usr/bin/env python3
"""
Slide 1's typewriter sprites (Slides/Part1/TitleSlide.cs): writes a PNG
sheet and converts it with tools/c64spr.py into art/build/caret.spr.

Frames (frm_Caret + n):
    0 mask    24 x 8 solid: drawn black and x-expanded, it hides the rest of
              a line of text until the cursor has "typed" it
    1 cursor  5 x 7 solid, one code-face glyph (6 px pitch, the top pixel
              line left blank like the face's own leading)

    python C64Presentation/art/slides/make_caret.py
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.join(HERE, "..", "..", "..")
sys.path.insert(0, os.path.join(ROOT, "tools"))
import c64spr  # noqa: E402

WHITE = (255, 255, 255, 255)


def main():
    from PIL import Image
    sheet = Image.new("RGBA", (48, 21), (0, 0, 0, 0))
    for y in range(8):
        for x in range(24):
            sheet.putpixel((x, y), WHITE)
    for y in range(1, 8):
        for x in range(5):
            sheet.putpixel((24 + x, y), WHITE)
    png = os.path.join(HERE, "caret.png")
    sheet.save(png)
    c64spr.convert(png, os.path.join(HERE, "..", "build", "caret.spr"))


if __name__ == "__main__":
    main()
