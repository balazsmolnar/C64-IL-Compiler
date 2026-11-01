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

Needs Python 3 + Pillow and the Segoe UI / Bahnschrift fonts of Windows
(C:\\Windows\\Fonts) for the lettering, rendered without anti-aliasing.
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

    def text(self, x, y, s, font, c, anchor="la"):
        self.d.text((x, y), s, font=font, fill=c, anchor=anchor)

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
LIGHT = "segoeuisl.ttf"
DIN = "bahnschrift.ttf"


# --- Logos --------------------------------------------------------------------

def c64_logo():
    """The Commodore "chicken lips": a ring C and two flags, 5x4 cells. The
    C stays in columns 0-2, the flags in columns 3-4, the light-red flag in
    its own cell row, so no cell has more than two colours."""
    p = Pic(40, 32)
    cx, cy, R, r = 15.5, 15.5, 16.0, 8.5
    for y in range(32):
        for x in range(22):                    # the ring, cut off vertically at x = 22
            d = math.hypot(x - cx, y - cy)
            if r <= d <= R:
                p.px(x, y, LBLUE)
    # upper flag (blue), lower flag (red): right ends cut on the diagonal
    for y in range(8, 16):
        p.line([(24, y), (39 - (y - 8), y)], LBLUE)
    for y in range(17, 25):
        p.line([(24, y), (39 - (24 - y), y)], LRED)
    return p


def hexagon(p, cx, cy, R, c):
    pts = [(cx + R * math.cos(math.radians(a)), cy + R * math.sin(math.radians(a)))
           for a in (30, 90, 150, 210, 270, 330)]
    p.poly(pts, c)


def csharp_logo():
    """A violet hexagon with a white "C#" (generic, not the trademark art), 5x5 cells."""
    p = Pic(40, 40)
    hexagon(p, 19.5, 19.5, 20.5, BRAND)
    f = font(BOLD, 19)
    p.text(20, 21, "C#", f, WHITE, anchor="mm")
    return p


def dotnet_logo():
    """A violet rounded square with a white ".NET", 5x5 cells."""
    p = Pic(40, 40)
    p.rrect(0, 0, 39, 39, 6, BRAND)
    f = font(BOLD, 13)
    p.text(20, 21, ".NET", f, WHITE, anchor="mm")
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


# --- Title / hero -------------------------------------------------------------

def monitor(p, x0, y0):
    """A 1702-style monitor, 16x12 cells at (x0, y0) (multiples of 8),
    showing syntax-coloured C# as bars on the C64's blue screen."""
    p.rrect(x0, y0, x0 + 127, y0 + 95, 6, GREY3)
    p.rect(x0 + 8, y0 + 8, x0 + 119, y0 + 79, LBLUE)     # the C64's border
    p.rect(x0 + 16, y0 + 16, x0 + 111, y0 + 71, BLUE)    # its screen: 12x7 cells
    # code: one line per cell row; (indent cells, [(width px, colour), ...]);
    # every token starts on a cell so each cell has one ink colour.
    lines = [
        (0, [(20, YELLOW), (38, WHITE)]),
        (0, [(4, LBLUE)]),
        (1, [(20, YELLOW), (13, CYAN), (30, WHITE)]),
        (1, [(4, LBLUE)]),
        (2, [(22, LBLUE), (30, WHITE), (10, LGREEN)]),
        (1, [(4, LBLUE)]),
        (0, [(4, LBLUE), (5, LBLUE)]),
    ]
    for row, (indent, toks) in enumerate(lines):
        x = x0 + 18 + indent * 8
        y = y0 + 16 + row * 8 + 2
        for i, (w, c) in enumerate(toks):
            if row == 6 and i == 1:                    # the cursor
                p.rect(x0 + 16 + 3 * 8, y - 2, x0 + 16 + 3 * 8 + 6, y + 4, LBLUE)
                continue
            cx = (x - x0) // 8 * 8 + x0
            p.rect(x, y, x + w - 1, y + 2, c)
            x = cx + ((x - cx) + w + 7) // 8 * 8 + 2
    p.rect(x0 + 108, y0 + 85, x0 + 111, y0 + 86, RED)    # power LED
    p.rect(x0 + 52, y0 + 96, x0 + 75, y0 + 103, GREY2)   # neck


def breadbox(p, x0, y0):
    """The C64 itself seen from the front, top tilted towards us: 19x5 cells at (x0, y0)."""
    w = 152
    top = [(x0 + 8, y0), (x0 + w - 9, y0), (x0 + w - 1, y0 + 23), (x0, y0 + 23)]
    p.poly(top, GREY2)
    p.rrect(x0, y0 + 24, x0 + w - 1, y0 + 39, 3, GREY1)
    p.rect(x0, y0 + 24, x0 + w - 1, y0 + 28, GREY1)
    # keys: three rows inside cells 2..16, dark grey on mid grey
    for k, (y, inset) in enumerate([(y0 + 3, 0), (y0 + 10, 2), (y0 + 17, 4)]):
        x = x0 + 16 + inset
        while x + 6 <= x0 + 16 + 14 * 8:
            p.rect(x, y, x + 5, y + 4, GREY1)
            x += 8
    p.rect(x0 + 48, y0 + 17, x0 + 95, y0 + 21, GREY2)     # space bar gap ...
    p.rect(x0 + 48, y0 + 17, x0 + 95, y0 + 21, GREY1)     # ... and the bar
    for i in range(3):                                     # function keys
        p.rect(x0 + 136, y0 + 3 + i * 7, x0 + 141, y0 + 7 + i * 7, GREY3)
    p.rect(x0 + 18, y0 + 31, x0 + 21, y0 + 32, LRED)     # power LED on the front


def title():
    """The full-screen title picture, hi-res 40x25: lettering left, the
    machine right. Rows 22-24 are left empty for the speaker's name."""
    p = Pic(320, 200, BG)
    x = 16
    f1 = font(SEMI, 20)
    size = 30
    while text_width("Commodore", font(BOLD, size)) > 160:   # clear of the monitor at x 184
        size -= 1
    f2 = font(BOLD, size)
    p.text(x, 54, "C# on the", f1, ACCENT, anchor="ls")
    p.text(x - 1, 88, "Commodore", f2, TEXT, anchor="ls")
    p.text(x - 1, 120, "64", f2, TEXT, anchor="ls")
    p.rect(x, 136, x + 23, 137, ACCENT)
    f3 = font(REG, 12)
    p.text(x, 156, ".NET IL compiled to", f3, TEXT2, anchor="ls")
    p.text(x, 172, "6502 machine code", f3, TEXT2, anchor="ls")
    monitor(p, 184, 24)
    breadbox(p, 168, 128)
    return p


# --- Pipeline diagram ---------------------------------------------------------
# Cards are 8 cells wide and 6 high: row 0 and 5 are the rounded ends, rows
# 1-3 the icon, row 4 is left plain for the label (DrawText with matrix
# 0x1B: white on the card's dark grey). The compiler card is 18 wide.

def card(wc, icon):
    p = Pic(wc * 8, 48)
    p.rrect(0, 0, wc * 8 - 1, 47, 5, SURFACE)
    p.paste(icon, (wc * 8 - icon.w) // 2 // 8 * 8, 8)
    return p


def icon_page(label, ink=WHITE):
    """A document with a folded corner and big letters, 32x24."""
    p = Pic(32, 24)
    x0, x1 = 6, 25
    p.poly([(x0, 0), (x1 - 6, 0), (x1, 6), (x1, 23), (x0, 23)], ink)
    p.poly([(x0 + 1, 1), (x1 - 7, 1), (x1 - 7, 6), (x1 - 1, 6), (x1 - 1, 22), (x0 + 1, 22)], SURFACE)
    p.poly([(x1 - 6, 1), (x1 - 1, 6), (x1 - 6, 6)], ink)
    f = font(BOLD, 10)
    p.text(15 - text_width(label, f) // 2 + 1, 19, label, f, ink, anchor="ls")
    return p


def icon_gear(size=24, teeth=8, ink=ACCENT, hole=None):
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


def icon_terminal():
    """asm listing: a window with a title bar and indented code lines, 32x24."""
    p = Pic(32, 24)
    p.rrect(1, 0, 30, 23, 2, WHITE)
    p.rect(2, 4, 29, 22, SURFACE)
    for i, (ind, w) in enumerate([(0, 14), (6, 16), (6, 10), (0, 18)]):
        p.rect(5 + ind, 7 + i * 4, 5 + ind + w - 1, 8 + i * 4, WHITE)
    return p


def icon_hammer():
    """64tass, the assembler: a hammer, 32x24."""
    p = Pic(32, 24)
    p.line([(9, 22), (19, 9)], WHITE, 3)                         # handle, tilted
    p.poly([(13, 2), (18, 0), (27, 11), (22, 14)], WHITE)        # head
    p.poly([(13, 2), (9, 1), (6, 3), (11, 5)], WHITE)            # claw
    return p


def icon_floppy():
    """.prg: a 5.25" floppy disk, 32x24."""
    p = Pic(32, 24)
    p.rect(5, 0, 26, 23, WHITE)
    p.ellipse(12, 7, 19, 14, SURFACE)
    p.ellipse(14, 9, 17, 12, WHITE)
    p.rrect(14, 16, 17, 22, 1, SURFACE)
    p.rect(7, 2, 9, 3, SURFACE)                 # write-protect notch
    return p


def icon_c64():
    """The machine: a tiny breadbox, 32x24."""
    p = Pic(32, 24)
    p.poly([(4, 6), (27, 6), (31, 17), (0, 17)], WHITE)
    p.rect(0, 18, 31, 21, WHITE)
    for ry, (a, b) in enumerate([(7, 24), (8, 25), (9, 26)]):
        for x in range(a, b, 3):
            p.rect(x, 8 + ry * 3, x + 1, 9 + ry * 3, SURFACE)
    p.rect(10, 16, 21, 16, SURFACE)              # space bar
    return p


def compiler_card():
    """The compiler: a gear and a column of pass bars, 18x6 cells."""
    p = Pic(144, 48)
    p.rrect(0, 0, 143, 47, 5, SURFACE)
    p.paste(icon_gear(24, ink=ACCENT), 16, 8)
    # passes: IL model -> optimizers -> emit, as three rows of short bars
    for row, (n, c) in enumerate([(3, WHITE), (6, ACCENT), (2, WHITE)]):
        y = 9 + row * 8
        for i in range(n):
            x = 56 + i * 12
            p.rect(x, y, x + 9, y + 4, c)
    return p


def arrow_right(cells=2, ink=ACCENT):
    p = Pic(cells * 8, 8)
    w = cells * 8
    p.rect(2, 3, w - 4, 4, ink)
    p.poly([(w - 6, 0), (w - 2, 3.5), (w - 6, 7)], ink)
    return p


def pipeline():
    """Returns {name: Pic} and the layout {name: (col, row)} relative to the
    diagram's top-left cell. The diagram is 40x14 cells."""
    parts, at = {}, {}
    parts["pipe_cs"], at["pipe_cs"] = card(8, icon_page("C#")), (1, 0)
    parts["pipe_il"], at["pipe_il"] = card(8, icon_page("IL")), (11, 0)
    parts["pipe_compiler"], at["pipe_compiler"] = compiler_card(), (21, 0)
    parts["pipe_asm"], at["pipe_asm"] = card(8, icon_terminal()), (1, 8)
    parts["pipe_tass"], at["pipe_tass"] = card(8, icon_hammer()), (11, 8)
    parts["pipe_prg"], at["pipe_prg"] = card(8, icon_floppy()), (21, 8)
    parts["pipe_c64"], at["pipe_c64"] = card(8, icon_c64()), (31, 8)
    parts["pipe_arrow"] = arrow_right()          # drawn at (9,2), (19,2), (9,10), (19,10), (29,10)
    # the elbow from the compiler down and back to the asm card: rows 6-7
    e = Pic(40 * 8 - 8 * 4, 16)
    xr = (21 + 9) * 8 - 4 * 8 + 3                # under the compiler card's middle
    xl = 4                                       # over the asm card's middle (col 4.5)
    e.rect(xr, 0, xr + 1, 4, ACCENT)
    e.rect(xl, 3, xr + 1, 4, ACCENT)
    e.rect(xl, 3, xl + 1, 11, ACCENT)
    e.poly([(xl - 3, 11), (xl + 4, 11), (xl + 0.5, 15)], ACCENT)
    parts["pipe_elbow"], at["pipe_elbow"] = e, (4, 6)
    return parts, at


def pipeline_preview(parts, at):
    p = Pic(320, 112, BG)
    for name, (c, r) in at.items():
        p.paste(parts[name], c * 8, r * 8)
    for c, r in [(9, 2), (19, 2), (9, 10), (19, 10), (29, 10)]:
        p.paste(parts["pipe_arrow"], c * 8, r * 8)
    return p


# --- Section icons (48x48, one accent colour on transparent) ----------------

def sec_gear():
    p = Pic(48, 48)
    p.paste(icon_gear(46, teeth=9, ink=ACCENT, hole=9), 1, 1)
    return p


def sec_chip():
    p = Pic(48, 48)
    p.rrect(10, 10, 37, 37, 3, ACCENT)
    p.rrect(14, 14, 33, 33, 1, T)
    p.rect(18, 18, 29, 29, ACCENT)
    for i in range(5):
        v = 12 + i * 5
        p.rect(v, 3, v + 1, 8, ACCENT)
        p.rect(v, 39, v + 1, 44, ACCENT)
        p.rect(3, v, 8, v + 1, ACCENT)
        p.rect(39, v, 44, v + 1, ACCENT)
    return p


def sec_memory():
    """A RAM bar: a grid of bytes, a few of them "allocated"."""
    p = Pic(48, 48)
    p.rrect(1, 8, 46, 39, 3, ACCENT)
    p.rect(3, 10, 44, 37, T)
    used = {(0, 0), (1, 0), (2, 0), (0, 1), (3, 1), (4, 1), (1, 2), (2, 2), (5, 2), (0, 3), (4, 3)}
    for r in range(4):
        for c in range(6):
            x, y = 5 + c * 7, 12 + r * 6
            if (c, r) in used:
                p.rect(x, y, x + 4, y + 3, ACCENT)
            else:
                p.rect(x, y, x + 4, y, ACCENT)
                p.rect(x, y + 3, x + 4, y + 3, ACCENT)
                p.rect(x, y, x, y + 3, ACCENT)
                p.rect(x + 4, y, x + 4, y + 3, ACCENT)
    for i in range(7):                                  # contacts
        p.rect(5 + i * 6, 41, 7 + i * 6, 45, ACCENT)
    return p


def sec_joystick():
    """The C64's game controller: a joystick (base, stick, ball, fire button)."""
    p = Pic(48, 48)
    p.rrect(4, 32, 43, 45, 4, ACCENT)
    p.rect(6, 34, 41, 37, T)
    p.rect(21, 12, 26, 33, ACCENT)
    p.ellipse(16, 1, 31, 16, ACCENT)
    p.ellipse(20, 4, 24, 8, T)
    p.rect(32, 28, 39, 31, ACCENT)
    return p


def sec_chart():
    """Optimizations: falling bars (bytes saved) and a trend arrow."""
    p = Pic(48, 48)
    p.rect(2, 45, 45, 46, ACCENT)
    for i, h in enumerate([34, 28, 20, 15, 11]):
        x = 4 + i * 9
        p.rect(x, 44 - h, x + 5, 43, ACCENT)
    return p


# --- Catacombs screenshots ----------------------------------------------------

# name -> (VICE frame in art/src/shots, crop in cells: col, row, width, height)
SHOTS_USED = {
    # A1: a room with the skeleton guardian, torches lit (room 5,3 -- the
    # MONSTERROOM room; this frame is the CHAR_SKELETON scene build)
    "shot_room": ("CHAR_SKELETON_11.png", (0, 0, 40, 21)),
    # A2: the title screen without its "PRESS FIRE" lines
    "shot_title": ("FLOW_6.png", (0, 0, 40, 18)),
    # A3: the Bone Warden, rows 0-14 (horns, skull, torch pillars; its name
    # banner is at row 18 of this frame, outside the crop)
    "shot_boss": ("COMBAT_10.png", (0, 0, 40, 15)),
}


def shot(name):
    """A crop of the VICE frame (border cut off: the screen starts at 32,35)."""
    src, (c, r, w, h) = SHOTS_USED[name]
    im = Image.open(os.path.join(SHOTS, src)).convert("RGB")
    return im.crop((32 + c * 8, 35 + r * 8, 32 + (c + w) * 8, 35 + (r + h) * 8))


# --- A5 (hi-res version, the one the storyboard uses): 18x8 cells ---------

def breadbox_hires():
    """The C64 from the front, its top tilted towards us, 144x64 hi-res: a
    mid-grey top with dark-grey keys (cell columns 2-14, function keys in
    light grey in column 15), the slanted ends in columns 0-1 and 16-17,
    a dark-grey front with a three-colour badge and a red power light. Two
    colours per cell everywhere (checked: no compromises)."""
    p = Pic(144, 64)
    p.poly([(12, 4), (131, 4), (143, 47), (0, 47)], GREY2)          # top
    p.rrect(0, 48, 143, 61, 3, GREY1)                               # front
    p.rect(0, 48, 143, 52, GREY1)
    for r in range(5):                                              # keys
        y = 10 + r * 7
        shift = (0, 2, 4, 3, 0)[r]
        for c in range(13):
            x = 16 + c * 8 + shift
            if r == 4 and 3 <= c <= 9:
                continue
            if x + 5 <= 16 + 13 * 8 - 1:
                p.rect(x, y, x + 5, y + 4, GREY1)
    p.rect(16 + 3 * 8, 38, 16 + 10 * 8 - 3, 42, GREY1)              # space bar
    for r in range(4):                                              # function keys
        p.rect(121, 10 + r * 7, 126, 14 + r * 7, GREY3)
    for i, c in enumerate([LRED, YELLOW, LBLUE]):                   # badge
        p.rect(17 + i * 8, 53, 22 + i * 8, 55, c)
    p.rect(115, 53, 118, 54, LRED)                                  # power light
    return p


# --- A6: six 3x3-cell icons, pixels only (the cells keep their colours) ----

def ic_int8():
    p = Pic(24, 24)
    p.rect(4, 4, 19, 19, WHITE)
    p.rect(6, 6, 17, 17, T)
    for i in range(3):
        v = 7 + i * 4
        p.rect(v, 1, v + 1, 3, WHITE)
        p.rect(v, 20, v + 1, 22, WHITE)
        p.rect(1, v, 3, v + 1, WHITE)
        p.rect(20, v, 22, v + 1, WHITE)
    p.text(12, 12, "8", font(BOLD, 12), WHITE, anchor="mm")
    return p


def ic_stack():
    p = Pic(24, 24)
    for i in range(4):
        y = 4 + i * 5
        p.rrect(2 + (i == 0) * 3, y, 21 - (i == 0) * 3, y + 3, 1, WHITE)
    p.poly([(9, 0), (14, 0), (11.5, 2)], WHITE)                 # push
    return p


def ic_object():
    p = Pic(24, 24)
    p.line([(12, 2), (21, 7), (21, 17), (12, 22), (3, 17), (3, 7), (12, 2)], WHITE, 2)
    p.line([(3, 7), (12, 12), (21, 7)], WHITE, 2)
    p.line([(12, 12), (12, 22)], WHITE, 2)
    return p


def ic_gc():
    p = Pic(24, 24)
    p.d.arc([2, 2, 21, 21], 200, 340, fill=WHITE, width=3)
    p.d.arc([2, 2, 21, 21], 20, 160, fill=WHITE, width=3)
    p.poly([(18, 2), (22, 8), (15, 8)], WHITE)                 # arrow heads
    p.poly([(5, 21), (1, 15), (8, 15)], WHITE)
    return p


def ic_string():
    p = Pic(24, 24)
    for x0 in (3, 13):
        p.ellipse(x0, 5, x0 + 7, 12, WHITE)
        p.poly([(x0 + 7, 9), (x0 + 6, 13), (x0 + 1, 19), (x0 + 2, 12)], WHITE)
    return p


def ic_bitmap():
    p = Pic(24, 24)
    p.rect(1, 3, 22, 20, WHITE)
    p.rect(3, 5, 20, 18, T)
    p.poly([(4, 17), (10, 9), (14, 14), (16, 12), (19, 17)], WHITE)
    p.ellipse(14, 6, 17, 9, WHITE)
    return p


ICONS = [("icon_int8", ic_int8), ("icon_stack", ic_stack), ("icon_object", ic_object),
         ("icon_gc", ic_gc), ("icon_string", ic_string), ("icon_bitmap", ic_bitmap)]


# --- Build --------------------------------------------------------------------

def convert(name, pic_or_png, mode, results, px=0, planes="bmc"):
    png = os.path.join(SRC, name + ".png")
    if isinstance(pic_or_png, Pic):
        pic_or_png.save(png)
    else:
        pic_or_png.save(png)
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
    for name in SHOTS_USED:
        convert(name, shot(name), c64img.MODE_MC, res, px=2)
    convert("breadbox", breadbox_hires(), HI, res)
    for name, fn in ICONS:
        convert(name, fn(), HI, res, planes="b")

    total = 0
    print("%-16s %6s %-6s %6s %s" % ("asset", "cells", "mode", "bytes", "compromised"))
    for name, c, r, mode, size, comp in res:
        total += size
        print("%-16s %6s %-6s %6d %s" % (name, "%dx%d" % (c, r), mode, size, comp or ""))
    print("%-16s %6s %-6s %6d" % ("TOTAL", "", "", total))


if __name__ == "__main__":
    main()
