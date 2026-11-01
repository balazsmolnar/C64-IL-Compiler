#!/usr/bin/env python3
"""
All the deck's pictures (see ART.md for the catalogue, palette and how to
place each one).

    python C64Presentation/art/presentation_art.py

Draws every asset as a PNG in exactly the 16 C64 colours (art/src/), converts
it with tools/c64img.py into art/build/*.img (the files C64Presentation.csproj
embeds, declared in Assets.cs), writes a preview of what the C64 shows next to
each source (*.c64.png) and prints a size table. The Catacombs screenshots are
converted from the pixel-exact VICE frames in art/src/shots/ (captured by
catacombs_shots.py, which needs VICE and is run by hand).

Needs Python 3 + Pillow and the Segoe UI fonts of Windows (C:/Windows/Fonts)
for the large lettering (drawn 4x larger, scaled down and thresholded to
two colours); small lettering uses the pixel font defined below.
"""
import math
import os
import sys

from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "tools"))
import c64img   # noqa: E402

SRC = os.path.join(HERE, "src")
OUT = os.path.join(HERE, "build")
SHOTS = os.path.join(SRC, "shots")
FONTS = r"C:\Windows\Fonts"

# --- The deck palette (C64 colour numbers = C64Lib.Colors) -------------------
BLACK, WHITE, RED, CYAN, VIOLET, GREEN, BLUE, YELLOW = 0, 1, 2, 3, 4, 5, 6, 7
ORANGE, BROWN, LRED, GREY1, GREY2, LGREEN, LBLUE, GREY3 = 8, 9, 10, 11, 12, 13, 14, 15

BG = BLACK          # slide background
SURFACE = GREY1     # cards, panels
RULE = GREY2        # hairlines, muted text, inactive
TEXT2 = GREY3       # body / secondary text
TEXT = WHITE        # titles, primary text
ACCENT = CYAN       # the accent: arrows, highlights, the active thing
ACCENT2 = YELLOW    # second accent, sparingly: numbers, "new"
BRAND = VIOLET      # C# / .NET marks only

T = 16              # "transparent" pseudo colour while drawing


# --- Drawing helpers ----------------------------------------------------------

class Pic:
    """A picture in C64 colour numbers (16 = transparent), on a Pillow
    palette image so ImageDraw works with colour numbers directly."""

    def __init__(self, w, h, fill=T):
        self.img = Image.new("P", (w, h), fill)
        pal = []
        for c in c64img.PALETTE:
            pal += list(c)
        pal += [255, 0, 255] * (256 - 16)
        self.img.putpalette(pal)
        self.d = ImageDraw.Draw(self.img)
        self.d.fontmode = "1"
        self.w, self.h = w, h

    def rect(self, x0, y0, x1, y1, c):
        self.d.rectangle([x0, y0, x1, y1], fill=c)

    def rrect(self, x0, y0, x1, y1, r, c, outline=None, width=1):
        self.d.rounded_rectangle([x0, y0, x1, y1], radius=r, fill=c, outline=outline, width=width)

    def line(self, pts, c, w=1):
        self.d.line(pts, fill=c, width=w)

    def poly(self, pts, c):
        self.d.polygon(pts, fill=c)

    def ellipse(self, x0, y0, x1, y1, c, outline=None, width=1):
        self.d.ellipse([x0, y0, x1, y1], fill=c, outline=outline, width=width)

    def px(self, x, y, c):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.img.putpixel((x, y), c)

    def get(self, x, y):
        return self.img.getpixel((x, y))

    def text(self, x, y, s, font, c, anchor="la", ss=4, threshold=0.5):
        """TrueType lettering without the stair-stepped bowls of a hinted
        1-bit render: drawn `ss` times larger with anti-aliasing, scaled
        down, and every pixel more than `threshold` covered is set."""
        big = ImageFont.truetype(font.path, font.size * ss)
        mask = Image.new("L", (self.w * ss, self.h * ss), 0)
        ImageDraw.Draw(mask).text((x * ss, y * ss), s, font=big, fill=255, anchor=anchor)
        mask = mask.resize((self.w, self.h), Image.BOX)
        ink = mask.point(lambda v: 255 if v >= threshold * 255 else 0).convert("1")
        self.img.paste(c, (0, 0, self.w, self.h), ink)

    def smooth(self, x0, y0, x1, y1, ink, bg):
        """Hand-cleaning for large lettering: a pixel on an edge that sticks
        out (or in) for a single row or column, while the pixels on both
        sides of it along the edge agree, takes their value."""
        g = [[self.get(x, y) for x in range(self.w)] for y in range(self.h)]
        out = [row[:] for row in g]
        for y in range(max(1, y0), min(self.h - 1, y1)):
            for x in range(max(1, x0), min(self.w - 1, x1)):
                v = g[y][x]
                if v not in (ink, bg):
                    continue
                up, dn, lf, rt = g[y - 1][x], g[y + 1][x], g[y][x - 1], g[y][x + 1]
                if up == dn != v and lf != rt:          # a one-row bump/notch on a vertical edge
                    out[y][x] = up
                elif lf == rt != v and up != dn:        # a one-column one on a horizontal edge
                    out[y][x] = lf
        for y in range(y0, y1):
            for x in range(x0, x1):
                if out[y][x] != g[y][x]:
                    self.px(x, y, out[y][x])

    def glyphs(self, x, y, s, c, bold=True):
        """The deck's own 8x8 pixel font (PIXEL_FONT): one character per
        8 pixels, so a cell-aligned string gets one cell per character."""
        for i, ch in enumerate(s):
            rows = PIXEL_FONT[ch]
            for gy, row in enumerate(rows):
                for gx, v in enumerate(row):
                    if v == "#":
                        self.px(x + i * 8 + 1 + gx, y + gy, c)
                        if bold:
                            self.px(x + i * 8 + 2 + gx, y + gy, c)

    def bits(self, x, y, rows, c):
        """Pixels from a list of strings ('#' = set)."""
        for gy, row in enumerate(rows):
            for gx, v in enumerate(row):
                if v == "#":
                    self.px(x + gx, y + gy, c)

    def paste(self, other, x, y):
        """Copies another Pic's opaque pixels."""
        for yy in range(other.h):
            for xx in range(other.w):
                v = other.get(xx, yy)
                if v != T:
                    self.px(x + xx, y + yy, v)

    def save(self, path):
        rgba = Image.new("RGBA", (self.w, self.h))
        src = self.img.load()
        dst = rgba.load()
        for y in range(self.h):
            for x in range(self.w):
                v = src[x, y]
                dst[x, y] = (0, 0, 0, 0) if v == T else c64img.PALETTE[v] + (255,)
        rgba.save(path)


def font(name, size):
    return ImageFont.truetype(os.path.join(FONTS, name), size)


def text_width(s, f):
    return int(math.ceil(f.getlength(s)))


BOLD = "segoeuib.ttf"
SEMI = "seguisb.ttf"
REG = "segoeui.ttf"


# --- The pixel font (5x7 glyphs in 8x8 cells; drawn bold = 6 wide) --------

PIXEL_FONT = {
    "C": [".###.", "#...#", "#....", "#....", "#....", "#...#", ".###."],
    "6": ["..##.", ".#...", "#....", "####.", "#...#", "#...#", ".###."],
    "4": ["...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#."],
    ".": [".....", ".....", ".....", ".....", ".....", ".##..", ".##.."],
    "S": [".####", "#....", "#....", ".###.", "....#", "....#", "####."],
    "c": [".....", ".....", ".###.", "#....", "#....", "#....", ".###."],
    "r": [".....", ".....", "#.##.", "##..#", "#....", "#....", "#...."],
    "e": [".....", ".....", ".###.", "#...#", "#####", "#....", ".###."],
    "n": [".....", ".....", "####.", "#...#", "#...#", "#...#", "#...#"],
    "l": [".##..", "..#..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "a": [".....", ".....", ".###.", "....#", ".####", "#...#", ".####"],
    "(": ["...#.", "..#..", ".#...", ".#...", ".#...", "..#..", "...#."],
    ")": [".#...", "..#..", "...#.", "...#.", "...#.", "..#..", ".#..."],
    ";": [".....", ".##..", ".##..", ".....", ".##..", ".##..", ".#..."],
    "H": ["#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    "u": [".....", ".....", "#...#", "#...#", "#...#", "#..##", ".##.#"],
    "d": ["....#", "....#", ".####", "#...#", "#...#", "#...#", ".####"],
    "t": [".#...", ".#...", "####.", ".#...", ".#...", ".#..#", "..##."],
    "h": ["#....", "#....", "####.", "#...#", "#...#", "#...#", "#...#"],
    "1": ["..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "0": [".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###."],
    "#": [".#.#.", ".#.#.", "#####", ".#.#.", "#####", ".#.#.", ".#.#."],
    "I": [".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###."],
    "L": ["#....", "#....", "#....", "#....", "#....", "#....", "#####"],
    "A": [".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    " ": ["....."] * 7,
}


# --- Logos --------------------------------------------------------------------

def c64_logo():
    """The Commodore "chicken lips", generic: a thin ring C, a 3-pixel gap,
    then a light-blue upper flag and a light-red lower flag, 5x4 cells. The
    C stays in cell columns 0-2, the flags in 3-4, each flag in its own cell
    row, so no cell has more than two colours."""
    p = Pic(40, 32)
    cx, cy, R, r = 15.5, 15.5, 16.0, 10.0
    for y in range(32):
        for x in range(20):                    # the ring, cut off vertically at x = 20
            d = math.hypot(x - cx, y - cy)
            if r <= d <= R:
                p.px(x, y, LBLUE)
    for y in range(9, 15):                     # upper flag, its right end cut "\"
        p.line([(23, y), (38 - (y - 9), y)], LBLUE)
    for y in range(17, 23):                    # lower flag, cut "/"
        p.line([(23, y), (38 - (22 - y), y)], LRED)
    return p


def hexagon(p, cx, cy, R, c):
    pts = [(cx + R * math.cos(math.radians(a)), cy + R * math.sin(math.radians(a)))
           for a in (30, 90, 150, 210, 270, 330)]
    p.poly(pts, c)


def csharp_logo():
    """A violet hexagon with a white "C#" (generic, not the trademark art), 5x5 cells."""
    p = Pic(40, 40)
    hexagon(p, 19.5, 19.5, 20.5, BRAND)
    p.text(20, 20, "C#", font(BOLD, 19), WHITE, anchor="mm")
    return p


def dotnet_logo():
    """A violet rounded square with a white ".NET", 5x5 cells."""
    p = Pic(40, 40)
    p.rrect(0, 0, 39, 39, 6, BRAND)
    f = font(BOLD, 13)
    w = text_width("NET", f)
    x = 20 - (w + 3) // 2 + 3                            # "NET" plus a 2 px dot and a 1 px gap
    p.text(x, 25, "NET", f, WHITE, anchor="ls")
    p.rect(x - 3, 23, x - 2, 24, WHITE)                  # the dot, on the baseline
    return p


def chip_6502():
    """A 40-pin DIP seen from above, "6502" on the package, 8x4 cells: pins
    (light grey) in cell rows 0 and 3, the package (dark grey) in rows 1-2."""
    p = Pic(64, 32)
    p.rect(1, 8, 62, 23, SURFACE)
    p.ellipse(-3, 12, 3, 19, T)                    # the notch
    for i in range(15):
        x = 3 + i * 4
        p.rect(x, 3, x + 1, 7, GREY3)
        p.rect(x, 24, x + 1, 28, GREY3)
    p.text(33, 15, "6502", font(BOLD, 13), WHITE, anchor="mm")
    return p


# --- A5: the breadbox, hi-res 18x8 cells ----------------------------------------

def breadbox():
    """The Commodore 64 breadbox from the front, 144x64 hi-res, two colours
    per cell everywhere:

      row 0-1   the domed back (light grey, a white highlight, rounded
                corners) with the rainbow badge stripes in cells 11-15 of
                row 0 and a mid-grey crease at the bottom of row 1
      row 2-5   the deck: dark-brown keys in the C64's staggered rows (wide
                CTRL, RETURN, both SHIFTs, the space bar), the four
                function keys in their own column (16)
      row 6     the front lip (mid grey) with the red power light
      row 7     the shadowed base (dark grey), rounded corners"""
    p = Pic(144, 64)
    p.rrect(0, 0, 143, 55, 8, GREY3)                 # body, domed top corners
    p.rect(0, 40, 143, 47, GREY3)                    # (square below the deck)
    p.rect(9, 2, 85, 2, WHITE)                       # highlight along the dome
    p.rect(2, 15, 141, 15, GREY2)                    # crease: dome / deck
    stripes = [LRED, ORANGE, YELLOW, LGREEN, LBLUE]  # the badge
    for i, c in enumerate(stripes):
        x = (11 + i) * 8 + 2
        p.poly([(x + 2, 3), (x + 5, 3), (x + 3, 7), (x, 7)], c)
    p.rect(0, 48, 143, 55, GREY2)                    # front lip
    p.rrect(2, 56, 141, 61, 2, GREY1)                # base
    p.rect(0, 56, 143, 58, GREY1)
    p.rect(126, 51, 129, 52, LRED)                   # power light

    u = 7                                            # one key unit, pixels
    rows = [                                         # (x offset, [widths in units])
        (6, [1] * 16),
        (6, [1.5] + [1] * 14 + [1.5]),
        (6, [1, 1] + [1] * 12 + [2]),
        (6, [1, 1.5] + [1] * 10 + [1.5, 1, 1]),
    ]
    for r, (x, widths) in enumerate(rows):
        y = 18 + r * 6
        for w in widths:
            wpx = round(w * u)
            p.rect(x, y, x + wpx - 2, y + 3, BROWN)
            x += wpx
    p.rect(6 + round(3.5 * u), 42, 6 + round(12.5 * u) - 2, 45, BROWN)   # space bar
    for i in range(4):                                                    # F1-F8
        p.rect(130, 18 + i * 7, 137, 22 + i * 7, GREY2)
    return p


# --- Title / hero (extra) ----------------------------------------------------------

def code_card(p, col, row, lines):
    """A floating dark-grey card with real C# in the pixel font, syntax
    coloured per character cell. lines = [(text, colours)], colours one
    letter per character: c cyan, w white, g light grey, y yellow."""
    width = max(len(t) for t, _ in lines) + 2
    x0, y0 = col * 8, row * 8
    p.rrect(x0, y0, x0 + width * 8 - 1, y0 + (len(lines) + 2) * 8 - 1, 4, SURFACE)
    ink = {"c": ACCENT, "w": WHITE, "g": GREY3, "y": ACCENT2}
    for i, (t, cols) in enumerate(lines):
        for j, ch in enumerate(t):
            p.glyphs(x0 + 8 + j * 8, y0 + 8 + i * 8, ch, ink[cols[j]])
    return width


def title():
    """The full-screen title picture, hi-res 40x25: the title on one line,
    the breadbox bottom left, and above it to the right a card with two real
    lines of C# (C64Lib, and Catacombs' Hud), as if the machine were saying
    them."""
    p = Pic(320, 200, BG)
    p.text(16, 36, "C# on the", font(SEMI, 18), ACCENT, anchor="ls")
    size = 40
    while text_width("Commodore 64", font(BOLD, size)) > 288:
        size -= 1
    p.text(14, 72, "Commodore 64", font(BOLD, size), TEXT, anchor="ls")
    p.smooth(0, 40, 320, 76, TEXT, BG)
    for y in range(60, 64):                      # by hand: the first "o"'s right side straight
        p.px(60, y, BG)
        if y > 60:
            p.px(54, y, TEXT)
    p.rect(16, 81, 39, 82, ACCENT)
    p.text(16, 98, "Compiling .NET IL to 6502 machine code", font(REG, 12), TEXT2, anchor="ls")
    code_card(p, 18, 13, [
        ("C64.Screen.Clear();", "cccgwwwwwwgwwwwwggg"),
        ("Hud.SetHealth(100);", "cccgwwwwwwwwwgyyygg"),
    ])
    p.poly([(156, 136), (165, 136), (156, 141)], SURFACE)   # the card's tail, towards the machine
    p.paste(breadbox(), 8, 136)
    return p


# --- Icon grammar ------------------------------------------------------------------
# Every icon: solid fills and 2-pixel strokes in one ink, drawn inside its
# box with 2 pixels of padding (3x3-cell icons: 20x20 inside 24x24).

INK = ACCENT


def ic_int8(ink=INK):
    p = Pic(24, 24)
    p.rect(5, 5, 18, 18, ink)
    p.rect(7, 7, 16, 16, T)
    for v in (7, 11, 15):                       # pins
        p.rect(v, 2, v + 1, 4, ink)
        p.rect(v, 19, v + 1, 21, ink)
        p.rect(2, v, 4, v + 1, ink)
        p.rect(19, v, 21, v + 1, ink)
    p.bits(9, 8, [".####.", "##..##", "##..##", ".####.", "##..##", "##..##", "##..##", ".####."][:8], ink)
    return p


def ic_stack(ink=INK):
    p = Pic(24, 24)
    # rounded plates widening downwards; the top one (the last push) is
    # lifted and shifted 2 px to the right
    for x0, y, x1 in ((9, 2, 18), (5, 8, 18), (4, 13, 19), (2, 18, 21)):
        p.rrect(x0, y, x1, y + 3, 1, ink)
    return p


def ic_object(ink=INK):
    """A cube: the lit top face solid, the two side faces as 2-pixel
    outlines (slope 2:1, so every edge is a clean pixel staircase)."""
    p = Pic(24, 24)
    top = {2: (11, 12), 3: (9, 14), 4: (7, 16), 5: (5, 18), 6: (3, 20), 7: (2, 21),
           8: (4, 19), 9: (6, 17), 10: (8, 15), 11: (10, 13), 12: (11, 12)}
    for y, (a, b) in top.items():
        p.rect(a, y, b, y, ink)
    p.rect(2, 7, 3, 16, ink)                         # outer side edges
    p.rect(20, 7, 21, 16, ink)
    p.rect(11, 12, 12, 21, ink)                      # front edge
    for i, y in enumerate(range(17, 21)):            # bottom edges
        p.rect(4 + 2 * i, y, 5 + 2 * i, y, ink)
        p.rect(18 - 2 * i, y, 19 - 2 * i, y, ink)
    return p


def ic_gc(ink=INK):
    p = Pic(24, 24)
    p.d.arc([3, 3, 20, 20], 200, 330, fill=ink, width=2)
    p.d.arc([3, 3, 20, 20], 20, 150, fill=ink, width=2)
    p.poly([(16, 2), (21, 8), (14, 9)], ink)
    p.poly([(7, 21), (2, 15), (9, 14)], ink)
    return p


def ic_string(ink=INK):
    """"A": a string literal."""
    p = Pic(24, 24)
    for x in (2, 5, 17, 20):                    # the quote marks (A at x 8-15)
        p.rect(x, 4, x + 1, 9, ink)
    p.bits(8, 6, ["...##...", "..####..", ".##..##.", ".##..##.", "##....##", "##....##",
                  "########", "########", "##....##", "##....##", "##....##", "##....##"], ink)
    return p


def ic_bitmap(ink=INK):
    p = Pic(24, 24)
    p.rect(2, 4, 21, 19, ink)
    p.rect(4, 6, 19, 17, T)
    p.poly([(5, 16), (10, 9), (13, 13), (15, 11), (18, 16)], ink)
    p.rect(14, 7, 16, 9, ink)
    return p


ICONS = [("icon_int8", ic_int8), ("icon_stack", ic_stack), ("icon_object", ic_object),
         ("icon_gc", ic_gc), ("icon_string", ic_string), ("icon_bitmap", ic_bitmap)]


def icon_gear(size=24, teeth=8, ink=INK, hole=None):
    p = Pic(size, size)
    c = (size - 1) / 2
    ro, ri = size / 2 - 0.2, size / 2 - size / 7
    hole = hole if hole is not None else size / 6
    for y in range(size):
        for x in range(size):
            dx, dy = x - c, y - c
            d = math.hypot(dx, dy)
            a = math.atan2(dy, dx)
            tooth = (math.cos(a * teeth) > 0.1)
            if hole <= d <= (ro if tooth else ri):
                p.px(x, y, ink)
    return p


# --- Section icons (6x6 cells, extra) --------------------------------------------

def sec_gear():
    p = Pic(48, 48)
    p.paste(icon_gear(46, teeth=9, hole=9), 1, 1)
    return p


def sec_chip():
    p = Pic(48, 48)
    p.rrect(10, 10, 37, 37, 3, INK)
    p.rrect(14, 14, 33, 33, 1, T)
    p.rect(18, 18, 29, 29, INK)
    for i in range(5):
        v = 12 + i * 5
        p.rect(v, 3, v + 1, 8, INK)
        p.rect(v, 39, v + 1, 44, INK)
        p.rect(3, v, 8, v + 1, INK)
        p.rect(39, v, 44, v + 1, INK)
    return p


def sec_memory():
    """A RAM module: a board with four chips and its row of contacts."""
    p = Pic(48, 48)
    p.rrect(2, 10, 45, 35, 2, INK)
    p.rect(4, 12, 43, 33, T)
    for i in range(4):
        x = 6 + i * 10
        p.rect(x, 15, x + 6, 26, INK)
    p.rect(22, 33, 25, 35, T)                         # the key notch
    for i in range(10):
        x = 5 + i * 4
        if not 20 <= x <= 25:
            p.rect(x, 37, x + 1, 42, INK)
    return p


def sec_joystick():
    p = Pic(48, 48)
    p.rrect(4, 32, 43, 45, 4, INK)
    p.rect(6, 34, 41, 37, T)
    p.rect(21, 12, 26, 33, INK)
    p.ellipse(16, 1, 31, 16, INK)
    p.ellipse(20, 4, 24, 8, T)
    p.rect(32, 28, 39, 31, INK)
    return p


def sec_chart():
    p = Pic(48, 48)
    p.rect(2, 45, 45, 46, INK)
    for i, h in enumerate([34, 28, 20, 15, 11]):
        x = 4 + i * 9
        p.rect(x, 44 - h, x + 5, 43, INK)
    return p


# --- Pipeline diagram (extra) ------------------------------------------------------
# Cards are 8 cells wide and 6 high: rows 0 and 5 hold the rounded ends,
# rows 1-3 a cyan icon (up to 48x24), row 4 is plain card for the label
# (DrawText with matrix 0x1B: white on the card's dark grey).

def card(wc, icon):
    p = Pic(wc * 8, 48)
    p.rrect(0, 0, wc * 8 - 1, 47, 5, SURFACE)
    p.paste(icon, (wc * 8 - icon.w) // 2, 8)
    return p


def pc_page(label):
    """A page (2-pixel outline, folded corner) with letters in the pixel font, 24x24."""
    p = Pic(24, 24)
    p.poly([(1, 0), (16, 0), (22, 6), (22, 23), (1, 23)], INK)
    p.poly([(3, 2), (15, 2), (20, 7), (20, 21), (3, 21)], T)
    p.poly([(15, 0), (22, 7), (15, 7)], INK)
    p.glyphs(12 - len(label) * 4, 11, label, INK)
    return p


def pc_terminal():
    """asm: a listing window, 32x24."""
    p = Pic(32, 24)
    p.rect(0, 0, 31, 23, INK)
    p.rect(2, 5, 29, 21, T)
    for i, (ind, w) in enumerate([(0, 12), (6, 14), (6, 10)]):
        p.rect(5 + ind, 8 + i * 4, 5 + ind + w - 1, 9 + i * 4, INK)
    return p


def pc_assemble():
    """64tass: text lines -> arrow -> a solid binary block, 40x24."""
    p = Pic(40, 24)
    for i, w in enumerate([12, 9, 12, 7]):
        p.rect(0, 3 + i * 5, w - 1, 4 + i * 5, INK)
    p.rect(15, 11, 22, 12, INK)
    p.poly([(21, 7), (26, 11.5), (21, 16)], INK)
    p.rect(29, 2, 39, 21, INK)
    for i in range(3):
        p.rect(31, 5 + i * 5, 37, 6 + i * 5, T)   # bytes
    return p


def pc_floppy():
    """.prg: a 5.25-inch floppy, 24x24."""
    p = Pic(24, 24)
    p.rect(1, 0, 22, 23, INK)
    p.ellipse(8, 7, 15, 14, T)
    p.ellipse(10, 9, 13, 12, INK)
    p.rrect(10, 16, 13, 22, 1, T)
    p.rect(3, 2, 5, 3, T)
    return p


def pc_c64():
    """The machine, solid: a breadbox with its domed back, 40x24."""
    p = Pic(40, 24)
    p.rrect(0, 4, 39, 19, 5, INK)
    p.rect(0, 12, 39, 19, INK)
    for r in range(3):
        for k in range(10):
            x = 3 + r + k * 3
            p.rect(x, 9 + r * 3, x + 1, 9 + r * 3, T)
    p.rect(11, 18, 26, 18, T)
    return p


def compiler_card():
    """The compiler, 18x6 cells: a gear, then three pass boxes and arrows."""
    p = Pic(144, 48)
    p.rrect(0, 0, 143, 47, 5, SURFACE)
    p.paste(icon_gear(24), 10, 8)
    for i in range(3):
        x = 44 + i * 34
        p.rect(x, 12, x + 21, 27, INK)
        p.rect(x + 2, 14, x + 19, 25, SURFACE)
        p.rect(x + 6, 19, x + 15, 20, INK)
        if i < 2:
            p.rect(x + 24, 19, x + 29, 20, INK)
            p.poly([(x + 29, 16), (x + 32, 19.5), (x + 29, 23)], INK)
    return p


def arrow_right(cells=2):
    p = Pic(cells * 8, 8)
    w = cells * 8
    p.rect(2, 3, w - 4, 4, INK)
    p.poly([(w - 6, 0), (w - 2, 3.5), (w - 6, 7)], INK)
    return p


def pipeline():
    """{name: Pic} and the layout {name: (col, row)} relative to the
    diagram's top-left cell; the diagram is 40x14 cells."""
    parts, at = {}, {}
    parts["pipe_cs"], at["pipe_cs"] = card(8, pc_page("C#")), (1, 0)
    parts["pipe_il"], at["pipe_il"] = card(8, pc_page("IL")), (11, 0)
    parts["pipe_compiler"], at["pipe_compiler"] = compiler_card(), (21, 0)
    parts["pipe_asm"], at["pipe_asm"] = card(8, pc_terminal()), (1, 8)
    parts["pipe_tass"], at["pipe_tass"] = card(8, pc_assemble()), (11, 8)
    parts["pipe_prg"], at["pipe_prg"] = card(8, pc_floppy()), (21, 8)
    parts["pipe_c64"], at["pipe_c64"] = card(8, pc_c64()), (31, 8)
    parts["pipe_arrow"] = arrow_right()          # at (9,2), (19,2), (9,10), (19,10), (29,10)
    e = Pic(40 * 8 - 8 * 4, 16)                  # compiler -> asm, rows 6-7
    xr = (21 + 9) * 8 - 4 * 8 + 3
    xl = 4
    e.rect(xr, 0, xr + 1, 4, INK)
    e.rect(xl, 3, xr + 1, 4, INK)
    e.rect(xl, 3, xl + 1, 11, INK)
    e.poly([(xl - 3, 11), (xl + 4, 11), (xl + 0.5, 15)], INK)
    parts["pipe_elbow"], at["pipe_elbow"] = e, (4, 6)
    return parts, at


def pipeline_preview(parts, at):
    p = Pic(320, 112, BG)
    for name, (c, r) in at.items():
        p.paste(parts[name], c * 8, r * 8)
    for c, r in [(9, 2), (19, 2), (9, 10), (19, 10), (29, 10)]:
        p.paste(parts["pipe_arrow"], c * 8, r * 8)
    return p


# --- Catacombs screenshots ----------------------------------------------------

# name -> (VICE frame in art/src/shots, crop in cells: col, row, width, height)
SHOTS_USED = {
    # A1: MONSTERROOM, the frame where the skeleton (raised sword) stands
    # clearest above the player
    "shot_room": ("MONSTERROOM_12.png", (0, 0, 40, 21)),
    # A2: the title screen without "PRESS FIRE"; the outermost half skulls
    # (columns 0-1 and 38-39) are left out
    "shot_title": ("FLOW_6.png", (2, 0, 36, 18)),
    # A3: the Bone Warden, rows 0-14 (its name banner is at row 18 of the
    # frame; the slide draws the name in hi-res instead)
    "shot_boss": ("COMBAT_10.png", (0, 0, 40, 15)),
}


def shot(name):
    """A crop of the VICE frame (border cut off: the screen starts at 32,35)."""
    src, (c, r, w, h) = SHOTS_USED[name]
    im = Image.open(os.path.join(SHOTS, src)).convert("RGB")
    im = im.crop((32 + c * 8, 35 + r * 8, 32 + (c + w) * 8, 35 + (r + h) * 8))
    if name == "shot_boss":
        flatten_horns(im)
    return im


def flatten_horns(im):
    """The horn bases are orange with single dark-brown pixels scattered
    through them (a checker that reads as mud at slide size). Inside the two
    horn-base boxes, a brown pixel pair with orange on both sides, or above
    and below, becomes orange: the brown outline stays, the speckle goes."""
    px = im.load()
    orange, brown = c64img.PALETTE[ORANGE], c64img.PALETTE[BROWN]

    def at(x, y):
        return c64img.nearest_color(px[x, y])

    for x0, x1 in ((56, 128), (192, 264)):          # left horn base, right horn base (pixels)
        for y in range(24, 72):
            for x in range(x0, x1, 2):
                if at(x, y) != BROWN:
                    continue
                lr = at(x - 2, y) == ORANGE and at(x + 2, y) == ORANGE
                ud = at(x, y - 1) == ORANGE and at(x, y + 1) == ORANGE
                if lr or ud:
                    px[x, y] = px[x + 1, y] = orange
    return im


# --- Build --------------------------------------------------------------------

def convert(name, pic, mode, results, px=0, planes="bmc"):
    png = os.path.join(SRC, name + ".png")
    pic.save(png)
    image, payload, report = c64img.convert(
        png, os.path.join(OUT, name + ".img"), mode=mode, px=px, planes=planes,
        preview=os.path.join(SRC, name + ".c64.png"), quiet=True)
    results.append((name, image.cols, image.rows, "hires" if mode == c64img.MODE_HIRES else "mc",
                    len(payload), len(report.compromised)))


def main():
    os.makedirs(SRC, exist_ok=True)
    os.makedirs(OUT, exist_ok=True)
    HI = c64img.MODE_HIRES
    res = []
    # the storyboard's assets
    for name in SHOTS_USED:
        convert(name, shot(name), c64img.MODE_MC, res, px=2)
    convert("breadbox", breadbox(), HI, res)
    for name, fn in ICONS:
        convert(name, fn(), HI, res, planes="b")
    # extras
    convert("title", title(), HI, res)
    convert("logo_c64", c64_logo(), HI, res)
    convert("logo_csharp", csharp_logo(), HI, res)
    convert("logo_dotnet", dotnet_logo(), HI, res)
    convert("chip_6502", chip_6502(), HI, res)
    parts, at = pipeline()
    for name in ["pipe_cs", "pipe_il", "pipe_compiler", "pipe_asm", "pipe_tass", "pipe_prg",
                 "pipe_c64", "pipe_arrow", "pipe_elbow"]:
        convert(name, parts[name], HI, res)
    pipeline_preview(parts, at).save(os.path.join(SRC, "pipeline_layout.png"))
    for name, fn in [("sec_gear", sec_gear), ("sec_chip", sec_chip), ("sec_memory", sec_memory),
                     ("sec_joystick", sec_joystick), ("sec_chart", sec_chart)]:
        convert(name, fn(), HI, res)

    total = 0
    print("%-16s %6s %-6s %6s %s" % ("asset", "cells", "mode", "bytes", "compromised"))
    for name, c, r, mode, size, comp in res:
        total += size
        print("%-16s %6s %-6s %6d %s" % (name, "%dx%d" % (c, r), mode, size, comp or ""))
    print("%-16s %6s %-6s %6d" % ("TOTAL", "", "", total))


if __name__ == "__main__":
    main()
