# C64Presentation: the engine

How slides are drawn: the display base, the fonts, the theme, the master
layouts, the elements, transitions and builds. Read `DECK-BRIEF.md` first;
`art/ART.md` for the pictures.

## Decisions

- **Display: hi-res bitmap** (`ScreenMode.Bitmap`, 320x200, two colours per
  8x8 cell), the "high" layout of `Catacombs/TECH.md` (bitmap under the
  KERNAL ROM, matrix at $CC00). Crisp text and 1-pixel lines everywhere.
  **Multicolour pictures** go above a raster split (`Master.Picture`), hi-res
  text below it: split at 21 (A1, a caption band), 15 (A3), or none (A2,
  whole screen multicolour).
- **Text: our own fonts, drawn by the runtime** (`C64.Screen.DrawFontText`,
  new, generic): proportional, x in pixels, glyphs OR-ed into the bitmap.
  The ROM font and `DrawText` are not used at all (which also leaves
  $0800-$0BFF to the runtime's tables).
  - **Body**: 8 px, hand drawn (`art/font/body.txt`): capitals 7 px,
    x-height 5, one-pixel descenders, a slashed zero; about 6 px a letter,
    so a 36-cell line holds ~48 characters.
  - **Title**: 16 px bold (`art/font/title.txt`), bootstrapped from DejaVu
    Sans Bold (free licence) at 14 px and kept as editable pixel art; about
    9 px a letter, ~30 characters across the content width.
  - **Code**: the body glyphs at a fixed 8 px pitch (`font_BodyMono`), one
    glyph per cell, so syntax colours can change at every character.
  - Special glyphs in both fonts: `~` arrow, `\` bullet dot, `^` tick
    (`Theme.Arrow`, `Theme.Dot`, `Theme.Tick`). `@` cannot be drawn (it ends
    a string), `"` cannot be in a string literal, `` ` `` is the colour escape.
- **Inline colour**: `` `x `` in any string sets the text colour to x's low
  nibble (`Theme.Hi`, `Hi2`, `White`, `Off`, `Dim`, `Red`, `Green`; for code
  `Theme.Kw`, `Ty`, `Str`, `Num`, `Com`, `Pl`). Constant strings fold, so
  `"a " + Theme.Hi + "b"` costs nothing. In proportional text a colour
  change moves on to the next cell (two colours never share a cell, a hi-res
  limit); at the code pitch it costs nothing.
- **Palette** (`art/ART.md`): black background, Grey1 surface (cards, code,
  table header), Grey2 rules / muted text, Grey3 body, white titles, cyan
  accent, yellow second accent (sparingly), violet for C#/.NET only.
- **Grid** (`Theme`): content columns 2-37 (16 px margins); title rows 1-2,
  accent rule row 3, body rows 5-20, footer row 23 (deck title left,
  "n / total" right), progress bar row 24. Two columns: 2-18 and 21-37.
- **Transitions: fade through black.** The screen's colours are saved
  ($0418-$07FF, the unused text screen) and shown through four fade tables
  (`fade_Levels` in Theme.cs) with `C64.Screen.RemapCellColorsFrom` (new,
  generic: no pixel is redrawn, about 2.5 frames a step). Each slide is
  drawn while the screen is blanked, so nothing half-drawn is ever seen.
- **Builds: fade in.** `Deck.Step()` waits for the press; what follows is
  drawn with its cells hidden (background on background) and faded in
  through the same levels at the next step; text is redrawn in its own
  colours at the last level, so inline colours survive. Arrows and bars
  drawn after a step are drawn live and grow; code blocks are typed in.

## Writing a slide

A slide is a `SimpleSlide` of elements (or a `Slide` subclass with
`Present()` for anything animated). Fields left at 0 / `Colors.Black` /
null take the theme default (the compiler has no field initializers).
`Wait = true` makes an element a build step (`WaitAfter` on Box: step after).

```csharp
static Slide Pipeline() => new SimpleSlide
{
    Elements = new SlideElement[]
    {
        new Title { Text = "The pipeline" },
        new Box { X = 2, Y = 6, Width = 8, Height = 3, Text = "C#" },
        new RightArrow { X = 10, Y = 7, Width = 5, Text1 = "dotnet build", Wait = true },
        new Box { X = 15, Y = 6, Width = 8, Height = 3, Text = "IL .dll", Color = Theme.Brand },
        new BulletList { Top = 12, Wait = true, Items = new[] { "Roslyn: " + Theme.Hi + "free", "Ours: IL ~ 6502" } },
    }
};
```

and add it to `Program.cs`'s list. The deck runs in VICE with
`tools\presentation-shot.ps1` (`-Slide N`, `-Scene HOLD` shows every build
at once, `-Scene AUTO` presses by itself).

## Element API

| Element | Fields (defaults) | Notes |
|---|---|---|
| `Title` | `Text` | content master: title, accent rule, footer |
| `SectionElement` | `Part`, `Title`, `Subtitle` | full-width Grey1 band, accent bar, footer |
| `TextElement` | `X`, `Y` cells, `Text`, `Color` (Body), `Heading` (16 px face) | one line; inline colours |
| `BulletList` | `Items`, `Left` (2), `Top` (5), `Pitch` (2), `Level` (0 dot / 1 dash), `Color` (Body), `Wait` | one item per step |
| `CodeBlock` | `X`, `Y`, `Width`, `Height`, `Lines`, `Caption` (tab row), `Color` (Body), `Wait` | Grey1 card, fixed pitch, `Theme.Kw`... colours; drawing again replaces the code |
| `Table` | `X` (2), `Y`, `RowNum`, `CollNum`, `ColumnWidths` or `CellWidth` (9), `Texts` (header first), `WaitAfterRow` | header on a card; column 0 left, others right aligned; hairline per row (2 rows each) |
| `Box` | `X`, `Y`, `Width`, `Height`, `Text`, `Caption`, `TextColor` (white), `Color`/`BorderColor` (accent bar, cyan), `Hollow`, `ConnectToLeft`, `Wait`, `WaitAfter` | a card: diagram node, memory block, band |
| `RightArrow` | `X`, `Y`, `Width`, `Text1` above, `Text2` below, `Color` (cyan), `Wait` | grows when drawn after a step |
| `DownArrow` | `X`, `Y`, `Height`, `Text` (right of it), `Color`, `Wait` | same, downwards |
| `BarChart` | `Labels`, `Values` (ulong), `ValueTexts`, `Max`, `BarColors` (cyan), `X` (2), `Y` (5), `Width` (36), `LabelWidth` (12), `ValueWidth` (7), `Pitch` (2), `Wait` | label / bar / value; bars grow; 38-column budget: `X = 1, Width = 38` |
| `ImageElement` | `Column`, `Row`, `Width`, `Height`, `Image` (`C64Address.FromLabel("img_X")`), `Caption`, `Wait` | hi-res images (`c64img --mode hires`) anywhere on the grid |
| `Callout` | `Lines`, `Attribution`, `Y`, `X` (2), `Width` (36), `Height` (lines + 2/4), `Accent` (cyan), `Wait` | quote / key message card with an accent bar |

Lower level, for `Present()` code: `Master.Content(title)`, `Master.Cover(title, subtitle)`,
`Master.Section(part, title, subtitle)`, `Master.Picture(image, column, row, split, caption)`
(no chrome), `Master.Footer()`; `Text.Draw / Heading / Code / Center / CenterHeading / Right /
Width`; `Deck.Paint(column, row, w, h, fg, bg)` (cell colours, build-aware), `Deck.Step()`,
`Deck.Show()`, `Deck.Reveal()` (call before a live animation loop), `Deck.Live`;
`Helper.Screen.Fill / Line / ClearRect` (pixels); `Box.DrawCard`, `Arrow.Draw`, `BarChart.Bar`.
Live slides (`IntroSlide`, `DemoSlide`) loop on `KeyBoard.AdvancePressed()` after `Deck.Reveal()`.

## Rules of thumb (hi-res)

- One foreground colour per cell: keep differently coloured things in
  different cells (bullet dot and text, label and bar, value and bar).
- A Paint sets whole cells; pixels come from Text / Fill / Line / images.
- Charts: two bars cannot share a cell row in different colours; use a row
  each (`Pitch = 1`).
- Pictures: hi-res images anywhere; multicolour only via `Master.Picture`
  (one split). A picture's colour-RAM colour cuts in/out rather than fading.
- Strings: `"`, `@` impossible; `~ \ ^` are glyphs; no `null` string
  arguments (use "").

## Runtime additions (generic, flag-gated: nothing for programs that don't call them)

- `C64.Screen.DrawFontText(x, row, text, font, matrix) -> x`, `FontTextWidth(text, font)`
  (asm/C64Graphics.asm, ~470 bytes with both); fonts from `tools/c64font.py`.
- `C64.Screen.RemapCellColorsFrom(source, table)` (~80 bytes).
- Compiler fix: a discarded ulong/string/float result (`Pop`) now takes all
  its bytes off the stack (it took one; the deck crashed into BRK).

## Assets owned by the engine

- `art/font/make_fonts.py` -> `art/font/build/fonts.asm` (3,285 bytes: body
  1,040 incl. the mono header, title 2,245).
- `art/engine/make_logos.py` -> `logos.spr` (the C# and Commodore logo
  sprites, from `sprites.asm`; five frames under the I/O area, 0 bytes of
  program memory).
- `art/engine/make_chip.py`, `make_backdrop.py`: placeholders (297 and 513
  bytes) used only by `Slides/EngineShowcaseSlides.cs`; drop them with it.
