#!/usr/bin/env python3
"""
The C64 and C# logo sprites (engine-owned placeholder art, used by the
title slide and IntroSlide): reads the original hand-made sprite bytes from
C64Presentation/sprites.asm, writes them as a PNG sheet and converts that
with tools/c64spr.py into art/engine/build/logos.spr ([RawSprites] needs a
.spr, and the bitmap layout's VIC bank cannot see sprites stored in line).

Frames, in sheet order (frm_Logo + n):
    0 C upper half   1 C lower half   2 Commodore flag upper
    3 Commodore flag lower            4 the '#' of C#

    python C64Presentation/art/engine/make_logos.py
"""

import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.join(HERE, "..", "..", "..")
sys.path.insert(0, os.path.join(ROOT, "tools"))
import c64spr  # noqa: E402

ORDER = ["spt_c_upper", "spt_c_lower", "spt_logo_upper", "spt_logo_lower", "spt_sharp"]


def main():
    from PIL import Image
    src = open(os.path.join(HERE, "..", "..", "sprites.asm"), encoding="utf-8-sig").read()
    sprites, cur = {}, None
    for line in src.splitlines():
        m = re.match(r"(spt_\w+):", line.strip())
        if m:
            cur = m.group(1)
            sprites[cur] = []
            continue
        if cur and ".byte" in line:
            sprites[cur] += [int(b.strip().lstrip("$"), 16) for b in line.split(".byte")[1].split(",")]
    sheet = Image.new("RGBA", (24 * len(ORDER), 21), (0, 0, 0, 0))
    for i, name in enumerate(ORDER):
        data = sprites[name][:63]
        for y in range(21):
            for x in range(24):
                if data[y * 3 + x // 8] & (0x80 >> (x % 8)):
                    sheet.putpixel((i * 24 + x, y), (255, 255, 255, 255))
    os.makedirs(os.path.join(HERE, "build"), exist_ok=True)
    png = os.path.join(HERE, "logos.png")
    sheet.save(png)
    c64spr.convert(png, os.path.join(HERE, "build", "logos.spr"))


if __name__ == "__main__":
    main()
