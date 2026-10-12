#!/usr/bin/env python3
"""
The prototype's three sprite frames (a spider and the two-frame player walk
cycle borrowed from Hunchback), as raw sprite rows. Writes
Catacombs/art/build/legacy.spr, embedded by Sprites.cs as the "Legacy"
sheet: frame 0 = spider, 1 = player step A, 2 = player step B.

    python Catacombs/art/legacy_sprites.py

Placeholder art: replace this sheet (and this script) with real sprites
drawn as PNGs and converted by tools/c64spr.py.
"""
import os

HERE = os.path.dirname(os.path.abspath(__file__))

SPIDER = [                      # hi-res
    0x000000, 0x000000, 0x000000, 0x400002, 0x301C0C, 0x0C3E10, 0x037F60,
    0x003E00, 0xFFFFFF, 0x01FFC0, 0x01FFC0, 0x01FFC0, 0x1FFFFC, 0xE03E03,
    0x000000, 0x030060, 0x0C0018, 0x300004, 0x400002, 0x000000, 0x000000,
]
PLAYER_A = [                    # multicolour; shared colours blue ($D025) / yellow ($D026)
    0x001000, 0x005400, 0x005C00, 0x007C00, 0x027C00, 0x02B000, 0x01A000,
    0x01A800, 0x01A800, 0x01A800, 0x01A800, 0x03A800, 0x035400, 0x02A800,
    0x02A800, 0x00A000, 0x00A000, 0x008000, 0x008000, 0x00C000, 0x00A000,
]
PLAYER_B = [
    0x001000, 0x005400, 0x005C00, 0x007C00, 0x027C00, 0x02B000, 0x01A000,
    0x01A800, 0x01A900, 0x01A900, 0x01A9C0, 0x01E8C0, 0x02D400, 0x02A800,
    0x00A800, 0x02A000, 0x0A2000, 0x0C2000, 0x082000, 0x083000, 0x002800,
]


def frame(rows, colour_byte):
    data = bytearray()
    for row in rows:
        data += bytes([(row >> 16) & 0xFF, (row >> 8) & 0xFF, row & 0xFF])
    data.append(colour_byte)
    return data


def main():
    out = os.path.join(HERE, "build")
    os.makedirs(out, exist_ok=True)
    data = frame(SPIDER, 0x00) + frame(PLAYER_A, 0x80) + frame(PLAYER_B, 0x80)
    with open(os.path.join(out, "legacy.spr"), "wb") as f:
        f.write(data)
    print("legacy.spr: %d frames" % (len(data) // 64))


if __name__ == "__main__":
    main()
