#!/usr/bin/env python3
"""
Every hardware sprite of the game: the adventurer, the room guardians, the
ambient vermin and the pickups (the "Characters" part of AAA-BRIEF.md).

    python Catacombs/art/characters_art.py            # sheets + .spr + previews
    python Catacombs/art/characters_art.py --no-preview

Writes
    art/src/chars_mc.png, art/src/chars_hi.png        the two sprite sheets
    art/build/chars_mc.spr, art/build/chars_hi.spr    what the .csproj embeds
    art/src/standin_room.png, art/build/standin_room.img
                                                     the dark stone backdrop the
                                                     SCENE_ builds stand on (not
                                                     in the normal build)
    .debug-tmp/chars_*.png                            enlarged previews (scratch)

The frames are drawn pixel by pixel as text grids (characters_frames.py): one
character per sprite pixel, so every pixel is a decision. Multicolour frames
are 12 double-wide pixels by 21; hi-res frames are 24 by 21.

Colours. Every multicolour sprite on screen shares two colours:
    K  black   ($D025, CommonColor1): outlines, eye sockets, the ground shadow
    O  orange  ($D026, CommonColor2): torch-lit skin, leather, wood, rust, embers
and has one colour of its own (the third letter used in the frame).
"""
import os
import sys

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.join(HERE, "..", "..")
sys.path.insert(0, os.path.join(ROOT, "tools"))
sys.path.insert(0, HERE)
import c64img   # noqa: E402
import c64spr   # noqa: E402
import characters_frames as F   # noqa: E402
import standin_room             # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
TMP = os.path.join(ROOT, ".debug-tmp")
P = c64img.PALETTE

# Letter -> C64 colour.
COL = {"K": 0, "W": 1, "R": 2, "C": 3, "P": 4, "G": 5, "U": 6, "Y": 7, "O": 8, "N": 9,
       "S": 10, "D": 11, "M": 12, "g": 13, "B": 14, "L": 15}
MC1, MC2 = 0, 8


def check(name, rows, width):
    if len(rows) != 21:
        raise ValueError("%s: %d rows, want 21" % (name, len(rows)))
    for i, r in enumerate(rows):
        if len(r) != width:
            raise ValueError("%s row %d: %d wide, want %d: %r" % (name, i, len(r), width, r))
        for ch in r:
            if ch != "." and ch not in COL:
                raise ValueError("%s row %d: unknown colour %r" % (name, i, ch))


def frame_image(rows, width):
    """A frame as an RGBA image, one PNG pixel per sprite pixel."""
    img = Image.new("RGBA", (width, 21), (0, 0, 0, 0))
    px = img.load()
    for y, r in enumerate(rows):
        for x, ch in enumerate(r):
            if ch != ".":
                px[x, y] = P[COL[ch]] + (255,)
    return img


def own_colour(rows):
    own = {COL[ch] for r in rows for ch in r if ch != "."} - {MC1, MC2}
    return sorted(own)


def sheet(names, table, width):
    img = Image.new("RGBA", (width * len(names), 21), (0, 0, 0, 0))
    for i, n in enumerate(names):
        check(n, table[n], width)
        img.paste(frame_image(table[n], width), (i * width, 0))
    return img


def screen_image(name):
    """A frame as it appears on screen: 24x21, multicolour pixels doubled."""
    if name in F.MC:
        return frame_image(F.MC[name], 12).resize((24, 21), Image.NEAREST)
    return frame_image(F.HI[name], 24)


def compose(canvas, parts, x, y):
    """Pastes sprites back to front. parts: (frame name, dx, dy), front first, like
    the hardware (sprite 0 in front)."""
    for name, dx, dy in reversed(parts):
        im = screen_image(name)
        canvas.paste(im, (x + dx, y + dy), im)


def preview(path, groups, scale=5, cols=8, cell=(34, 50), backdrop=None):
    """groups: list of (label, parts). Each is drawn in a cell on stone."""
    rows = (len(groups) + cols - 1) // cols
    w, h = cols * cell[0], rows * cell[1]
    room = backdrop or standin_room.floor_patch(w, h)
    canvas = room.convert("RGBA")
    for i, (label, parts) in enumerate(groups):
        cx, cy = (i % cols) * cell[0] + 5, (i // cols) * cell[1] + 4
        compose(canvas, parts, cx, cy)
    canvas = canvas.resize((w * scale, h * scale), Image.NEAREST)
    canvas.convert("RGB").save(path)


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    os.makedirs(TMP, exist_ok=True)

    mc = sheet(F.MC_ORDER, F.MC, 12)
    hi = sheet(F.HI_ORDER, F.HI, 24)
    mc.save(os.path.join(SRC, "chars_mc.png"))
    hi.save(os.path.join(SRC, "chars_hi.png"))
    for n in F.MC_ORDER:
        if len(own_colour(F.MC[n])) > 1:
            raise ValueError("%s: more than one own colour %s" % (n, own_colour(F.MC[n])))
    for n in F.HI_ORDER:
        if len(own_colour(F.HI[n]) + [c for c in (MC1, MC2) if any(COL.get(ch) == c for r in F.HI[n] for ch in r)]) > 1:
            raise ValueError("%s: a hi-res frame has one colour" % n)

    c64spr.convert(os.path.join(SRC, "chars_mc.png"), os.path.join(OUT, "chars_mc.spr"),
                   multicolor=True, px=1, mc1=MC1, mc2=MC2, quiet=True)
    c64spr.convert(os.path.join(SRC, "chars_hi.png"), os.path.join(OUT, "chars_hi.spr"), quiet=True)
    print("chars_mc: %d frames, chars_hi: %d frames, total %d of 48"
          % (len(F.MC_ORDER), len(F.HI_ORDER), len(F.MC_ORDER) + len(F.HI_ORDER)))
    for i, n in enumerate(F.MC_ORDER):
        print("  mc %2d %s" % (i, n))
    for i, n in enumerate(F.HI_ORDER):
        print("  hi %2d %s" % (i, n))

    room = standin_room.draw()
    room.save(os.path.join(SRC, "standin_room.png"))
    c64img.convert(os.path.join(SRC, "standin_room.png"), os.path.join(OUT, "standin_room.img"),
                   preview=os.path.join(SRC, "standin_room.c64.png"))

    if "--no-preview" in sys.argv:
        return
    for name, groups, kw in F.previews():
        preview(os.path.join(TMP, "chars_%s.png" % name), groups, **kw)


if __name__ == "__main__":
    main()
