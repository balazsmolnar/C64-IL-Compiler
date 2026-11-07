#!/usr/bin/env python3
"""
c64font.py -- turns a text-drawn bitmap font into 64tass data for
C64Lib's C64.Screen.DrawFontText / FontTextWidth.

    python tools/c64font.py build body.txt fonts.asm --label Body [--mono 8 [--leading]] [--big]
    python tools/c64font.py preview body.txt out.png "Some text" ["More text" ...]

Source format (plain text, one file per font):

    lines 8            # 8 or 16: pixel lines per glyph (one or two cell rows)
    spacing 1          # pixels added after every glyph (proportional fonts)
    =A                 # a glyph: '=' and the character it is for
    .###.              # `lines` rows, all the same width: '#' ink, '.' none
    #...#
    ...
    =space             # names for characters that are awkward to write
    ...

Names: space, backslash, backquote, hash (lines starting with '#' are
comments). Any other glyph header is the single character itself.

Characters are stored under the screen code that 64tass's .enc "screen"
(the compiler's string encoding) gives them: A-Z = 1-26, a-z = $41-$5A,
' '-'?' = $20-$3F, [ \\ ] ^ _ = $1B-$1F, ` { | } ~ = $40, $5B-$5E. '@' is code
0, which ends a string, and ` is DrawFontText's colour escape: neither can
be drawn. A font may give any of the others a glyph that is not the
character itself (e.g. '~' as an arrow).

Output: font_<Label> (proportional) and, with --mono N, font_<Label>Mono
(fixed pitch N, glyphs centred; --leading drops each glyph's pixel line 1
and puts a blank line on top, so lines can be set solid), and with --big
font_<Label>Big (every pixel drawn 2x2), all sharing the same glyph data:

    font_L    .byte lines, advance, spacing, first, count
              .word widths, lowBytes, highBytes
              .byte scale (+$80 = leading)
    widths    count bytes
    lowBytes  count bytes: low bytes of the glyph addresses
    highBytes count bytes
    glyphs    per glyph ceil(width/8) strips of `lines` bytes, left strip
              first; bit 7 = the leftmost pixel. Identical glyphs are stored
              once.
"""

import argparse
import sys

NAMES = {"space": " ", "backslash": "\\", "backquote": "`", "hash": "#"}


def screen_code(ch):
    o = ord(ch)
    if "A" <= ch <= "Z":
        return o - 0x40
    if "a" <= ch <= "z":
        return o - 0x20
    if ch == "@":
        return 0
    special = {"[": 0x1B, "\\": 0x1C, "]": 0x1D, "^": 0x1E, "_": 0x1F,
               "`": 0x40, "{": 0x5B, "|": 0x5C, "}": 0x5D, "~": 0x5E}
    if ch in special:
        return special[ch]
    if 0x20 <= o <= 0x3F:
        return o
    raise ValueError(f"no screen code for {ch!r}")


class Font:
    def __init__(self):
        self.lines = 8
        self.spacing = 1
        self.glyphs = {}   # char -> list of row strings

    def width(self, ch):
        g = self.glyphs.get(ch)
        return len(g[0]) if g else 0

    def advance(self, ch, mono=0):
        if mono:
            return mono
        return self.width(ch) + self.spacing


def load(path):
    font = Font()
    cur = None
    rows = None
    with open(path, encoding="utf-8") as f:
        for n, raw in enumerate(f, 1):
            line = raw.rstrip("\n").rstrip("\r")
            if cur is None or len(rows) >= font.lines:
                s = line.strip()
                if not s or s.startswith("#"):
                    continue
            if line.startswith("="):
                name = line[1:].strip() if len(line) > 2 else line[1:]
                ch = NAMES.get(name, name)
                if len(ch) != 1:
                    sys.exit(f"{path}:{n}: unknown glyph name {name!r}")
                if ch in font.glyphs:
                    sys.exit(f"{path}:{n}: {ch!r} defined twice")
                cur = ch
                rows = []
                font.glyphs[ch] = rows
                continue
            if cur is None:
                key, _, val = line.strip().partition(" ")
                if key == "lines":
                    font.lines = int(val)
                elif key == "spacing":
                    font.spacing = int(val)
                else:
                    sys.exit(f"{path}:{n}: unexpected {line!r}")
                continue
            if len(rows) < font.lines:
                r = line.strip()
                if set(r) - set(".#"):
                    sys.exit(f"{path}:{n}: glyph rows use only '.' and '#': {r!r}")
                if rows and len(r) != len(rows[0]):
                    sys.exit(f"{path}:{n}: {cur!r}: rows of different widths")
                rows.append(r)
                continue
            sys.exit(f"{path}:{n}: unexpected {line!r}")
    for ch, rows in font.glyphs.items():
        if len(rows) != font.lines:
            sys.exit(f"{path}: {ch!r} has {len(rows)} rows, not {font.lines}")
        if len(rows[0]) > 16:
            sys.exit(f"{path}: {ch!r} is wider than 16 pixels")
    if font.lines not in (8, 16):
        sys.exit(f"{path}: lines must be 8 or 16")
    return font


def strips(rows):
    w = len(rows[0])
    out = []
    for s in range(0, max(w, 1), 8):
        for r in rows:
            bits = r[s:s + 8].ljust(8, ".")
            out.append(int(bits.replace("#", "1").replace(".", "0"), 2))
    return out


def build(font, label, mono, big=False, leading=False):
    codes = {screen_code(ch): ch for ch in font.glyphs}
    first = min(codes)
    count = max(codes) - first + 1
    datas = []        # unique glyph byte lists
    index = {}        # code -> data index
    for code, ch in codes.items():
        d = strips(font.glyphs[ch]) if font.width(ch) else []
        if d not in datas:
            datas.append(d)
        index[code] = datas.index(d)
    out = [f"; generated by tools/c64font.py -- do not edit; edit the .txt and rebuild",
           f"font_{label}",
           f"    .byte {font.lines}, 0, {font.spacing}, {first}, {count}",
           f"    .word font_{label}_w, font_{label}_lo, font_{label}_hi",
           f"    .byte 1"]
    if mono:
        out += [f"font_{label}Mono",
                f"    .byte {font.lines}, {mono}, 0, {first}, {count}",
                f"    .word font_{label}_w, font_{label}_lo, font_{label}_hi",
                f"    .byte {0x81 if leading else 1}"]
    if big:
        out += [f"font_{label}Big",
                f"    .byte {font.lines}, 0, {font.spacing}, {first}, {count}",
                f"    .word font_{label}_w, font_{label}_lo, font_{label}_hi",
                f"    .byte 2"]
    widths, refs = [], []
    for c in range(first, first + count):
        ch = codes.get(c)
        widths.append(font.width(ch) if ch else 0)
        refs.append(f"font_{label}_g{index[c]}" if c in index else f"font_{label}_g0")
    out.append(f"font_{label}_w .byte " + ", ".join(map(str, widths)))
    out.append(f"font_{label}_lo .byte " + ", ".join(f"<{r}" for r in refs))
    out.append(f"font_{label}_hi .byte " + ", ".join(f">{r}" for r in refs))
    for i, d in enumerate(datas):
        out.append(f"font_{label}_g{i}" + (" .byte " + ", ".join(f"${b:02x}" for b in d) if d else ""))
    size = 12 * (1 + bool(mono) + bool(big)) + 3 * count + sum(len(d) for d in datas)
    return "\n".join(out) + "\n", size


def render(font, text, mono=0):
    """Pixel rows of a string as DrawFontText draws it (no colour escapes)."""
    x = 0
    pix = set()
    for ch in text:
        g = font.glyphs.get(ch)
        w = font.width(ch)
        dx = x + ((mono - w) // 2 if mono else 0)
        if g:
            for y, r in enumerate(g):
                for i, c in enumerate(r):
                    if c == "#":
                        pix.add((dx + i, y))
        if g or mono:
            x += font.advance(ch, mono)
    return x, pix


def preview(font, texts, path, mono=0, scale=3):
    from PIL import Image
    width = max(render(font, t, mono)[0] for t in texts) + 8
    h = font.lines + 4
    img = Image.new("RGB", (width, h * len(texts) + 4), (0, 0, 0))
    for i, t in enumerate(texts):
        _, pix = render(font, t, mono)
        for (x, y) in pix:
            img.putpixel((x + 4, y + 4 + i * h), (255, 255, 255))
    img = img.resize((img.width * scale, img.height * scale), Image.NEAREST)
    img.save(path)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = ap.add_subparsers(dest="cmd", required=True)
    b = sub.add_parser("build")
    b.add_argument("src")
    b.add_argument("dst")
    b.add_argument("--label", required=True)
    b.add_argument("--mono", type=int, default=0)
    b.add_argument("--big", action="store_true")
    b.add_argument("--leading", action="store_true")
    p = sub.add_parser("preview")
    p.add_argument("src")
    p.add_argument("dst")
    p.add_argument("texts", nargs="+")
    p.add_argument("--mono", type=int, default=0)
    a = ap.parse_args()
    font = load(a.src)
    if a.cmd == "build":
        asm, size = build(font, a.label, a.mono, a.big, a.leading)
        with open(a.dst, "w", encoding="utf-8", newline="\n") as f:
            f.write(asm)
        print(f"{a.src}: {len(font.glyphs)} glyphs, {size} bytes -> {a.dst}")
    else:
        preview(font, a.texts, a.dst, a.mono)


if __name__ == "__main__":
    main()
