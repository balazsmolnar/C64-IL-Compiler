#!/usr/bin/env python3
"""
Builds the deck's fonts (C64Presentation/art/font/build/fonts.asm, embedded by
the .csproj) from the hand-editable glyph files next to this script:

    body.txt   8-line proportional sans, hand drawn -> font_Body
               and the code face, fixed 6 px pitch   -> font_BodyMono
    title.txt  16-line bold sans for titles          -> font_Title, font_TitleBig (2x)

The code face draws the body glyphs with leading: pixel line 1 left out
and a blank line on top (capitals 6 px), so code lines can sit on
consecutive cell rows without touching. No extra glyph data.

    python C64Presentation/art/font/make_fonts.py            # build + previews
    python C64Presentation/art/font/make_fonts.py --bootstrap-title

--bootstrap-title regenerates title.txt from DejaVu Sans Bold (Bitstream Vera
licence; the copy that ships with matplotlib), rendered unhinted-mono at 14
px, baseline on line 12. It was run once; title.txt has been touched up by
hand since, so running it again throws those edits away.
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "..", "tools"))
import c64font  # noqa: E402

CHARS = (" !#$%&'()*+,-./0123456789:;<=>?"
         "ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}")


def bootstrap_title():
    from PIL import Image, ImageDraw, ImageFont
    import matplotlib
    ttf = os.path.join(os.path.dirname(matplotlib.__file__), "mpl-data", "fonts", "ttf", "DejaVuSans-Bold.ttf")
    font = ImageFont.truetype(ttf, 14)
    out = ["# C64Presentation title font: 16 pixel lines, bold. Bootstrapped from",
           "# DejaVu Sans Bold 14 px by make_fonts.py --bootstrap-title, then",
           "# touched up by hand. Capitals lines 2-12, x-height 5-12, descenders 13-15.",
           "#   ~  right arrow      \\  bullet dot",
           "lines 16", "spacing 2", ""]
    for ch in CHARS:
        img = Image.new("1", (40, 16), 0)
        d = ImageDraw.Draw(img)
        d.fontmode = "1"
        d.text((8, 13), ch, font=font, fill=1, anchor="ls")
        px = img.load()
        cols = [x for x in range(40) if any(px[x, y] for y in range(16))]
        if ch == " ":
            rows = ["....."] * 16
        else:
            x0, x1 = min(cols), max(cols)
            rows = ["".join("#" if px[x, y] else "." for x in range(x0, x1 + 1)) for y in range(16)]
        name = {" ": "space", "#": "hash", "\\": "backslash"}.get(ch, ch)
        out.append("=" + name)
        out += rows
    # The two special glyphs.
    out += ["=~"] + ["." * 11] * 4 + ["......#....", ".......#...", "........##.",
                                       "###########", "###########", "........##.",
                                       ".......#...", "......#...."] + ["." * 11] * 4
    out += ["=backslash"] + ["......"] * 5 + [".####.", "######", "######", "######",
                                              "######", ".####."] + ["......"] * 5
    with open(os.path.join(HERE, "title.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out) + "\n")
    print("title.txt bootstrapped")


# The code face: a 6 px pitch (an escape that changes colour moves to the
# next cell edge, so syntax colours never share a cell). The body's 6-7 px
# capitals would touch their neighbours at that pitch: 5 px ones instead.
CODE_PITCH = 6
CODE_GLYPHS = {
    "M": ["#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#", "....."],
    "W": ["#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#", "....."],
    "O": [".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###.", "....."],
    "Q": [".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#", "....."],
}


def main():
    if "--bootstrap-title" in sys.argv:
        bootstrap_title()
        return
    build = os.path.join(HERE, "build")
    os.makedirs(build, exist_ok=True)
    parts, total = [], 0
    body = c64font.load(os.path.join(HERE, "body.txt"))
    for src, label, mono, big, font in (("body.txt", "Body", CODE_PITCH, False, body),
                                        ("title.txt", "Title", 0, True, c64font.load(os.path.join(HERE, "title.txt")))):
        asm, size = c64font.build(font, label, mono, big, leading=bool(mono),
                                  mono_glyphs=CODE_GLYPHS if mono else None)
        parts.append(asm)
        total += size
        print(f"{src}: {len(font.glyphs)} glyphs, {size} bytes")
        sample = ["The quick brown fox jumps over the lazy dog.",
                  "HOW DOES IT WORK? Garbage collector 0123456789",
                  "C# ~ IL ~ 6502: (a+b)*c = [x]; {y} 50% #1 & $FF \\ ok!"]
        c64font.preview(font, sample, os.path.join(build, f"{label.lower()}_preview.png"))
        if mono:
            c64font.preview(font, ["static int Add(int a, int b) => a + b;", "LDA $FF,X ; ok"],
                            os.path.join(build, f"{label.lower()}_mono_preview.png"), mono)
    with open(os.path.join(build, "fonts.asm"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(parts))
    print(f"fonts.asm: {total} bytes")


if __name__ == "__main__":
    main()
