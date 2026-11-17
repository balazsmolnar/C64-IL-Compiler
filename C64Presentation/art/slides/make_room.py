#!/usr/bin/env python3
"""
Slide 8's living Catacombs room (Slides/Part1/BuiltWithThisSlide.cs), made
with Catacombs' own art code, which stays read-only (nothing is written
under Catacombs/):

    art/build/disk/room.bin        the room the guardian holds, (5,3) in
                                   Catacombs/Maze.cs: moss theme, doors north,
                                   west and south, a puddle; no characters.
                                   40x21 multicolour, on the deck's disk
                                   (disk.txt "room"), loaded into buf_Picture
    art/build/room_flame0..3.img   the torch flame frames (2x2 cells; drawn
                                   at FLAME_CELLS, the same for both torches),
                                   all the same size: the slide steps from one
                                   to the next by it (FlameBytes)
    art/build/cast.spr             the characters, frames copied from
                                   Catacombs/art/build/chars_mc.spr (made by
                                   tools/c64spr.py from characters_art.py)
    art/build/room.png             preview of the room picture

    python C64Presentation/art/slides/make_room.py
"""

import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", "..", ".."))
CATACOMBS_ART = os.path.join(ROOT, "Catacombs", "art")
sys.path.insert(0, os.path.join(ROOT, "tools"))
sys.path.insert(0, CATACOMBS_ART)
import c64img       # noqa: E402
import room_art     # noqa: E402

OUT = os.path.join(HERE, "..", "build")
DISK = os.path.join(OUT, "disk")

# chars_mc.spr frame numbers (Catacombs/Sprites.cs, class Frame), in the
# order the slide's sheet keeps them (BuiltWithThisSlide.cs, class Cast).
CAST = [
    3,                  # 0  the adventurer's top, facing right
    14, 15, 16, 17,     # 1  his legs walking right
    19, 20, 21,         # 5  the skeleton guardian: top, then two leg frames
    35, 36, 37, 36,     # 8  the bat: wings up, level, down, level
]
FRAME = 64


def convert(grid, name, crop=None):
    png = os.path.join(OUT, name + ".png")
    room_art.save_png(grid, png)
    out = os.path.join(OUT, name + ".img")
    image, payload, report = c64img.convert(png, out, crop=crop, quiet=True)
    if report.compromised:
        raise SystemExit("%s: %d compromised cells" % (name, len(report.compromised)))
    print("%-12s %5d bytes" % (name, len(payload)))
    return payload


def main():
    os.makedirs(DISK, exist_ok=True)
    room = room_art.render(room_art.Cfg(north=1, west=1, south=True, props=("puddle",)))
    payload = convert(room, "room")
    os.remove(os.path.join(OUT, "room.img"))        # only the disk file is used
    with open(os.path.join(DISK, "room.bin"), "wb") as f:
        f.write(payload)

    cx, cy = room_art.FLAME_CELLS[0]
    sizes = set()
    for f in range(room_art.FLAME_FRAMES):
        grid = room if f == 0 else room_art.render(room_art.Cfg(flame=f))
        convert(grid, "room_flame%d" % f, crop=(cx, cy, 2, 2))
        os.remove(os.path.join(OUT, "room_flame%d.png" % f))
        sizes.add(os.path.getsize(os.path.join(OUT, "room_flame%d.img" % f)))
    if len(sizes) != 1:
        # BuiltWithThisSlide.cs steps from one flame to the next by a fixed size
        raise SystemExit("the flame frames differ in size: %s" % sorted(sizes))

    with open(os.path.join(CATACOMBS_ART, "build", "chars_mc.spr"), "rb") as f:
        sheet = f.read()
    with open(os.path.join(OUT, "cast.spr"), "wb") as f:
        for n in CAST:
            f.write(sheet[n * FRAME:(n + 1) * FRAME])
    print("cast.spr     %5d bytes (%d frames, under I/O)" % (len(CAST) * FRAME, len(CAST)))


if __name__ == "__main__":
    main()
