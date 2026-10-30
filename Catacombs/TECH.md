# Catacombs: graphics tech reference

What exists for drawing, and how to use it. Everything here is generic
C64Lib / compiler / runtime capability (usable by any program) and is paid
for only when called. Read `AAA-BRIEF.md` first for the art direction and
the fixed screen layout.

A worked example of every item below is the tech demo scene:
`TechDemo.cs` + `art/techdemo_art.py`, shown with

    & "<root>\tools\catacombs-shot.ps1" -Root "<root>" -Scene TECHDEMO -OutPrefix "<root>\.debug-tmp\tech" -Shots 3 -IntervalMs 500

## Coordinates

- A **cell** is one 8x8 character position: column 0-39, row 0-24. The cell
  APIs (`DrawImage`, `SetCellColors`, `RemapCellColors`, `DrawText`,
  `SetSplit`) take cells, as `uint`. Nothing is clipped: stay on screen.
- The pixel APIs (`SetPixel`, `DrawLine`, ...) take pixels 0-319 / 0-199 as
  `ulong`. In multicolour a pixel is two wide (x and x+1 are the same one).
- Sprites: `X = bitmapX + 24`, `Y = bitmapY + 50` puts a sprite's top-left
  corner on bitmap pixel (bitmapX, bitmapY).
- Per cell colours in multicolour: bit pair 00 = screen background ($D021,
  black for this game), 01 = matrix high nibble, 10 = matrix low nibble,
  11 = colour RAM. Hi-res: bit 1 = matrix high nibble, bit 0 = low nibble.

## Asset pipeline

Art is drawn by a checked-in Python script (Pillow) that saves a PNG and
converts it; the converted file goes in `Catacombs/art/build/`, which the
.csproj embeds (`*.img`, `*.spr`) under the bare file name. Sources (scripts,
PNGs) live in `Catacombs/art/` and `Catacombs/art/src/`. Commit all three.

Draw with exactly the 16 C64 colours and the conversion is exact:

    import sys; sys.path.insert(0, "tools")      # or a path relative to the script
    import c64img
    P = c64img.PALETTE          # P[0] black ... P[15] light grey, (r, g, b); index = C64Lib.Colors value

### Pictures: `tools/c64img.py`

    python tools/c64img.py convert room.png art/build/room.img --preview room.c64.png
    python tools/c64img.py convert panel.png art/build/panel.img --mode hires
    python tools/c64img.py convert door.png  art/build/door.img  --base room.png
    python tools/c64img.py convert room.png  art/build/room_colours.img --planes mc
    python tools/c64img.py convert sheet.png art/build/prop.img --crop 8,4,6,5
    python tools/c64img.py info art/build/room.img

From Python: `c64img.convert(src, dst, mode=c64img.MODE_MC, px=0, bg=0,
crop=None, base=None, planes="bmc", preview=None, strict=False, quiet=False)`.

- The PNG's width and height must be whole cells. Multicolour (default): a
  160-wide PNG is taken as one PNG pixel per C64 pixel, a wider one as pixel
  pairs (`--px 1|2` to force). Hi-res (`--mode hires`): 320-wide, 1:1.
- Multicolour: per cell the background colour (`--bg`, default 0 = black;
  it must be the program's `SetBackgroundColor`) plus any three colours.
  Hi-res: any two colours per cell.
- Too many colours in a cell: the best subset is kept, the rest remapped to
  the nearest kept colour, and the cell is listed as `COMPROMISED` with its
  position. `--strict` makes that a failure. Aim for "no colour compromises".
- `--preview` writes exactly what the C64 will display. Look at it.
- **Transparency is per cell.** A cell whose PNG pixels are all transparent
  (alpha < 128) is not stored and `DrawImage` leaves it untouched. A partly
  transparent cell is stored whole, its transparent pixels as background
  (reported). For a prop that must blend with a backdrop inside a cell,
  composite it onto the backdrop in the script and convert with
  `--base backdrop.png`: only the cells that differ are stored, and the
  result is drawn at the backdrop's own position.
- `--planes`: `b` bitmap, `m` matrix, `c` colour RAM. `--planes mc` stores a
  picture's colours without its pixels (a recolouring of what is on screen).
- The report gives the stored size. Measured: the tech demo's dithered
  40x21 backdrop is 8400 -> 2118 bytes; a flat hi-res panel frame 1440 -> 93.
  Random-looking dither compresses worst; repeated patterns and empty areas
  cost almost nothing.

### Sprites: `tools/c64spr.py`

    python tools/c64spr.py convert coin.png art/build/coin.spr --preview coin.c64.png
    python tools/c64spr.py convert hero.png art/build/hero.spr --mc --px 1 --mc1 0 --mc2 6
    python tools/c64spr.py info art/build/hero.spr

From Python: `c64spr.convert(src, dst, multicolor=False, px=0, mc1=None,
mc2=None, transparent=None, count=None, preview=None, quiet=False)`.

- The PNG is a grid of frames, left to right, top to bottom. Hi-res: 24x21
  per frame, one colour. Multicolour (`--mc`): 12x21 double-wide pixels per
  frame, drawn 12 wide (`--px 1`) or 24 wide with doubled pixels (`--px 2`;
  say which). Transparent = alpha < 128.
- Multicolour frames may use the frame's own colour plus the two colours
  shared by every multicolour sprite on screen: `--mc1` ($D025,
  `C64.Sprites.CommonColor1`) and `--mc2` ($D026, `CommonColor2`). Agree on
  those two for the whole game. The tool prints each frame's own colour (set
  it with `Sprite.Color`); it is also in byte 63 of the frame.
- Extra colours in a frame are remapped and reported as `COMPROMISED`.

## Using the assets

    // Sprites.cs (assembly attributes; Resource = file name in art/build)
    [assembly: RawImage(Resource = "room.img", Label = "Room")]
    [assembly: RawSprites(Resource = "hero.spr", Label = "Hero")]

### Images

    C64.Screen.DrawImage(uint column, uint row, ulong image)

    C64.Screen.BeginUpdate();                                     // blank the screen
    C64.Screen.DrawImage(0, 0, C64Address.FromLabel("img_Room"));
    C64.Screen.DrawImage(17, 15, C64Address.FromLabel("img_Chest"));
    C64.Screen.EndUpdate();

- Draws pixels and colours; cells the image does not contain are untouched.
  The screen must be in the image's mode (or split, see below).
- A full 40x21 picture takes roughly a quarter of a second (estimated from
  cycle counts, not measured), pixels first, colours after: blank the screen
  with `BeginUpdate`/`EndUpdate` around a full redraw. Small overlays are
  instant.
- The vector calls (`DrawLine`, `DrawRectangle`, `DrawBitmap`...) still work
  on top of an image; they change pixels, not cell colours.

### Per-cell colours

    C64.Screen.SetCellColors(uint column, uint row, uint width, uint height, uint matrix, Colors colorRam)
    C64.Screen.RemapCellColors(uint column, uint row, uint width, uint height, ulong table)

    [assembly: RawAssembly(Asm = @"
    pal_Moss    .byte 0,1,2,3,4,5,9,7,8,9,10,5,13,13,14,7     ; table[colour] = new colour
    pal_Flicker .byte 0,1,2,3,4,5,6,8,10,9,7,11,12,13,14,15   ; yellow -> orange -> light red -> yellow
    ")]
    C64.Screen.RemapCellColors(0, 0, 40, 21, C64Address.FromLabel("pal_Moss"));     // room theme
    C64.Screen.RemapCellColors(4, 1, 12, 12, C64Address.FromLabel("pal_Flicker"));  // every few frames

- `SetCellColors` fills a rectangle: `matrix` = (high-nibble colour << 4) |
  low-nibble colour, `colorRam` = the third colour.
- `RemapCellColors` replaces every colour in the rectangle through a
  16-byte table. It changes the cells in place: a table that permutes
  colours can be applied repeatedly (cycling) or undone by its inverse; a
  table that merges colours cannot be undone except by redrawing the
  colours (`DrawImage` of a `--planes mc` image, or of the picture).
- Cost: about 80 cycles per cell. The whole viewport is ~3.5 frames (do it
  blanked or accept a visible sweep); for a per-frame effect keep it to
  roughly 100 cells and call it first thing after `WaitForVBlank`.
- A whole-screen flash is cheaper as `SetBackgroundColor`/`SetBorderColor`.

### Sprites

    uint hero = (uint)C64Address.FromLabel("frm_Hero");   // block number of the sheet's first frame
    C64.Sprites.Sprite0.Frame = hero + n;                 // frame n of the sheet

- `Sprite.Frame` (set-only, `uint`) is the sprite pointer; a sheet's frames
  are consecutive. `C64Address.FromLabel("spt_Hero")` is the first frame's
  address (for `Sprite.DataBlock`, which also works). No addresses to pick,
  and no pointer pokes: the old `Graphics_ColorMatrix + $3F8` poke still
  works but is not needed.
- Capacity: 64 frames stored under the I/O area at no cost in program
  memory, then up to 48 more that each take 64 bytes off the top of free
  memory. A sheet is never split between the two groups; sheets are placed
  in `Order`. Over the limit is a compile error.
- `C64.Sprites.MultiColorMask` (bit n = sprite n is multicolour) and
  `C64.Sprites.BehindBackgroundMask` (bit n = sprite n goes behind the
  picture's 10 and 11 pixels) set all eight sprites at once.
- **Gotcha:** `Sprite.MultiColor = false` does nothing (it can only switch
  multicolour on). Use `MultiColorMask`. The prototype's spiders are drawn
  multicolour by accident because of this.
- Eight hardware sprites, no multiplexer. Sprite 0 is in front of sprite 1,
  and so on (see `SpriteStage.cs`).
- `[RawSprite]` (SpritePad import) does not work in a bitmap program; the
  build says so.

### Text

    C64.Screen.DrawText(uint column, uint row, string text, uint matrix)

    C64.Screen.DrawText(2, 22, "HEALTH", 0x10);              // white on black
    C64.Screen.DrawText(9, 22, hp.ToString(), 0x70);

- Writes 8x8 hi-res glyphs into the bitmap, one per cell, and sets those
  cells' colours to `matrix` (text colour << 4 | background). Use it in a
  hi-res area: the status panel below the split. In a multicolour area the
  glyphs come out as coloured blocks; put lettering there into a picture.
- It overwrites whole cells and does not erase beyond the string: pad
  numbers with spaces when they can get shorter.
- Upper case, digits, punctuation (the ROM's upper-case set, screen codes
  0-127). `@` cannot be drawn. The font is in RAM at
  `C64Address.FromLabel("Graphics_Font")`, 8 bytes per character: copy your
  own glyphs over it for a custom font.

### Tiles

    C64.Screen.DrawTiles(uint column, uint row, uint width, uint height, ulong tiles, uint index)

    [assembly: RawAssembly(Asm = "tiles_Hud .byte $00,$1c,$3e,...")]      // 8 bytes a tile
    C64.Screen.DrawTiles(4, 22, 2, 2, C64Address.FromLabel("tiles_Hud"), heart * 4);

- Copies raw 8-byte cells into the bitmap: a `width` x `height` block of
  cells, filled row by row from consecutive tiles starting at tile `index`
  of the set at `tiles` (8 bytes a tile, top pixel row first).
- Pixels only. The cells keep their colours: set them once (a picture,
  `SetCellColors`) and afterwards redraw just the tiles. Nothing is erased
  first and nothing is read back, so swapping a tile cannot flicker; a
  couple of cells cost a few hundred cycles. This is how the status panel
  (`Hud.cs`) updates hearts, digits, the weapon icon and the minimap.
- The bits mean what the cells' mode makes of them: hi-res tiles below a
  split, multicolour tiles (bit pairs) elsewhere.
- `width` is at most 31. Nothing is clipped. A tile set can be RAM the
  program changes (the minimap is one: change bytes, draw it again).

### Raster split (hi-res status panel)

    C64.Screen.SetScreenMode(ScreenMode.MultiColor);
    C64.Screen.SetSplit(21, ScreenMode.Bitmap);     // rows 21-24 hi-res, rows 0-20 multicolour
    C64.Screen.ClearSplit();

- Same bitmap, same cell colours; below the split the bits are simply read
  as hi-res (320 wide, two colours per cell). Draw the panel with a
  `--mode hires` image and `DrawText`.
- Call `SetSplit` after `SetScreenMode`, and `ClearSplit` before changing
  the mode. `SetBackgroundColor` is not split (hi-res ignores $D021).
- Keep the first pixel line of the lower part (y = row * 8) empty with a
  black cell background. Measured clean in VICE even with all eight sprites
  crossing the line, but that line is where a late switch would show.
- While a split is on, the system interrupt runs once per video frame (50
  Hz) from the raster interrupt, in the bottom border, instead of 60 Hz from
  the CIA timer. That includes a `C64.Interrupt` subscriber (music): it is
  called 50 times a second, whichever of `SetSplit` and `+=` came first.
- Float maths masks interrupts and can make the split late for a frame.
- Do not write $D011 yourself while a split is on (bit 7 must stay 0); the
  library's own calls take care.

## Memory map (programs that use bitmap graphics)

| Range | Use |
|---|---|
| $0800-$0BFF | `DrawText` font if `DrawText` is used; otherwise object tables |
| $0C00-$0FFF | with `DrawText`: the locals stack and three object tables (address low/high, root count), page-aligned |
| $1000-... | runtime, compiled code, assets: one contiguous block |
| ...-$CBFF | with `DrawText`: the other four object tables (1024 bytes), then 56 bytes of string buffers, then the heap; without `DrawText` all tables and the locals stack are below $1000 and only the 56 bytes come first |
| below $CC00 | sprite frames 65-112, if any (64 bytes each) |
| $CC00-$CFE7 | video matrix (`Graphics_ColorMatrix`); sprite pointers at $CFF8 |
| $D000-$DFFF | I/O; the RAM underneath holds sprite frames 1-64 |
| $D800-$DBE7 | colour RAM |
| $E000-$FF3F | bitmap (`Graphics_Bitmap`), RAM under the KERNAL ROM |

- The VIC is switched to bank 3 by `SetScreenMode(Bitmap/MultiColor)` and
  back to bank 0 by `SetScreenMode(Character)`.
- **Free memory.** The build prints the segment map; the single `Data:` line
  from $1000 ends where free memory starts (the build may print a few
  short `Data:` lines for aligned runtime pieces; take the last one's end).
  Free = $CC00 - that end - 1080 (object tables, locals stack and string
  buffers left above the code when `DrawText` is used; 56 without it) -
  what the heap needs - 64 per sprite frame beyond 64. The runtime owns
  $0800-$0FFF: nothing else may use it.
  After the round-4 integration (all six parts merged) the block ends
  at $BBBE, of which the last 4096 bytes ($ABBF-) are the 64 sprite frames
  that are moved at startup: the tables take $ABBF-$AFF6 and the heap
  starts at $AFF7, so **7,177 bytes ($AFF7-$CBFF) are left for more code,
  assets and the heap** (the prototype had 25,827).
- Sprite frames 1-64 are free: they load at the end of the program and are
  copied under the I/O area at startup; the object tables and heap then
  reuse that space. (The loaded file must still end below $CC00, which the
  build checks.)
- The bitmap is not in the .prg and is not readable from C#
  (`C64.GetMemory` sees the ROM there); writes work. The drawing routines
  bank the ROM out themselves, with interrupts left on.
- Programs that use `SetDrawBuffer`/`SwapBuffers` get the older low layout
  (bitmap $2000, matrix $0C00, second buffer in bank 1) with none of the
  sprite storage; Catacombs must not call them.

## Cost in bytes (only when used)

| Feature | Bytes |
|---|---|
| High layout itself (interrupt trampoline, vector setup, sprite copy, ROM banking in each drawing call) | about 110; frees 9.4 KB |
| `DrawImage` (155 of it the decompressor) | 299 |
| `SetCellColors` | 63 + 85 shared with `RemapCellColors` |
| `RemapCellColors` | 109 + 85 shared |
| `DrawText` (and the font copy) | 173 + 34 shared with the above |
| `DrawTiles` | 129 + 34 shared with the above |
| `SetSplit` + `ClearSplit` | about 260 |
| `Sprite.Frame`, `MultiColorMask`, `BehindBackgroundMask` | about 15 each |

## Limits and gotchas

- uint is 8 bits, ulong 16: `C64Address.FromLabel` returns `ulong`; cast the
  `frm_` labels to `uint`.
- An image must be drawn in the mode it was converted for; a multicolour
  image's `--bg` must equal the screen background.
- `DrawImage` cannot flip, scale or clip, and has no per-pixel transparency.
- No fades: a fade is a series of `RemapCellColors` tables, and fading in
  needs the colours redrawn first (visible unless blanked).
- Bitmap text is hi-res only; there is no multicolour font.
- No sprite multiplexing, no scrolling, no double buffering in this layout.
- Returning to BASIC is not a supported path for a program using these
  (the font overwrites the BASIC start line).
- The image and sprite tools need Python 3 with Pillow.
