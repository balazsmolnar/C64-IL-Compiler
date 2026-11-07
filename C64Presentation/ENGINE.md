# C64Presentation: the engine

How slides are drawn: the display base, the fonts, the theme, the master
layouts, the elements, transitions and builds. Read `DECK-BRIEF.md` first,
and `art/ART.md` for the pictures.

## Decisions

- **Display: hi-res bitmap** (`ScreenMode.Bitmap`, 320x200, two colours per
  8x8 cell) in the "high" layout of `Catacombs/TECH.md`. **Multicolour
  pictures** go above a raster split with `Master.Picture(image, column, row,
  split, caption)`: split 21 for A1, 25 for no split. `image` is an address,
  so a picture loaded from the disk (`ArtDisk.Load`, `buf_Picture`) works the
  same as an embedded one.
- **Text: the deck's own fonts**, drawn by the generic
  `C64.Screen.DrawFontText` (x in pixels, glyphs OR-ed in). The ROM font and
  `DrawText` are not used.
  - **Body** (`art/font/body.txt`): 8 px, hand-drawn, proportional. Capitals
    are 7 px, the x-height 5 px; A is pointed, W is 7 px wide, zero is
    slashed. A 36-cell line holds about 48 characters.
  - **Heading** (`title.txt`): 16 px bold. It started from DejaVu Sans Bold
    and is now edited as pixel art.
  - **Big**: the heading glyphs drawn at 2x (32 px), set by a scale byte in
    the font header. Used for the cover title and section numbers.
  - **Code**: the body glyphs at a fixed 8 px pitch, with *leading* (a header
    flag): pixel line 0 is blank and the glyph's line 1 is dropped. Code lines
    sit on consecutive rows without touching, and no extra glyph data is
    needed.
  - Special glyphs in every face: `~` is an arrow, `\` a bullet dot, `^` a
    tick (`Theme.Arrow`, `Theme.Dot`, `Theme.Tick`).
- **Inline colour**: `` `x `` sets the colour (`Theme.Hi`, `Hi2`, `White`,
  `Off`, `Dim`, `Red`; for code `Kw`, `Ty`, `Str`, `Num`, `Com`, `Pl`).
  Interpolated constants fold at compile time: `$"{Theme.Kw}ldarg.0"`.
- **Palette** (ART.md):
  - black background;
  - Grey1 surface;
  - Grey2 rules and footer;
  - Grey3 body text;
  - white titles, and the **only** text colour on Grey1 (code comments in
    Grey3 excepted);
  - cyan accent, yellow second accent;
  - violet for C#/.NET only.
- **Grid** (`Theme`):
  - content columns 2-37;
  - title rows 1-2 and the accent rule on row 3;
  - body rows 5-20;
  - footer row 23 and the progress bar on row 24;
  - two columns: 2-18 and 21-37.
- **Transitions: fade through black.** The colours are saved at
  $0418-$07FF and stepped through `fade_Levels` with `RemapCellColorsFrom`.
  Every colour fades by brightness through the greys (cyan goes 15 → 12 →
  11 → 0, never through blue). The Grey1 surface is darker than any text on
  it at every level, so no card shows before its text. Slides are drawn
  blanked.
- **Builds:** `Deck.Step()` waits for the press, then the step is drawn
  hidden and faded in, and text is redrawn in its real colours at the end.
  - Arrows and bars drawn after a step grow.
  - A `CodeBlock` drawn over a shown one dims the old code out, then fades
    the new code in on the same card (`Deck.Build(onSurface: true)`).

## Writing a slide: idiomatic C#

Slides are declarative initializers. Elements have `init` properties, and
anything left at its default takes the theme's value. The compiler turns
auto-property accessors into plain field accesses, so this costs the same as
public fields. Custom `Present()` code is only for animated slides.

```csharp
static class PipelineSlide
{
    public static Slide Create() => new SimpleSlide
    {
        Elements =
        [
            new Title { Text = "The pipeline" },
            new Box { X = 2, Y = 6, Width = 8, Height = 3, Text = "C#", Color = Theme.Brand },
            new Arrow { FromCol = 10, FromRow = 7, ToCol = 14, ToRow = 7, Text = "dotnet build", Wait = true },
            new Box { X = 15, Y = 6, Width = 8, Height = 3, Text = "IL .dll", Color = Theme.Brand },
            new BulletList { Top = 12, Wait = true, Items = [$"Roslyn: {Theme.Hi}free", "Ours: IL ~ 6502"] },
        ]
    };
}
```

Add it to `Program.Slides`. Look at it in VICE with
`tools\presentation-shot.ps1` (`-Slide N`; `-Scene HOLD` shows every build
at once, `-Scene AUTO` presses by itself).

## Element API

| Element | Properties (defaults) | Notes |
|---|---|---|
| `Title` | `Text` | Content master: title, rule, footer. |
| `SectionElement` | `Number`, `Title`, `Subtitle`, `Icon` (an `img_Sec*` address; 0 = none) | On black: a big cyan number, rule, heading, 6x6 icon on the right. |
| `TextElement` | `X`, `Y`, `Text`, `Face` (`Body` / `Heading` / `Big`), `Color` (Body, or white for headings), `Strike`, `Wait` | One line. `Strike` draws a 1 px rule through the text, for an old value. |
| `BulletList` | `Items`, `Left` (2), `Top` (5), `Pitch` (2), `Level` (0 = dot, 1 = dash), `Color`, `Wait` | 5 px between the mark and the text. |
| `CodeBlock` | `X`, `Y`, `Width` (36), `Lines`, `Caption`, `Wait` | Grey1 card with a 1-cell padding. Caption on row 1, code from row 3, at most `Width` - 2 columns. Height = lines + 2 (+2 with a caption). |
| `Table` | `X` (2), `Y`, `RowNum`, `CollNum`, `ColumnWidths` or `CellWidth` (9), `Texts`, `WaitAfterRow` | Header row: white on Grey1. Then rows with hairlines, two rows each. |
| `Box` | `X`, `Y`, `Width`, `Height`, `Text`, `Caption`, `Color` (accent bar, cyan), `Hollow`, `ConnectToLeft`, `Wait`, `WaitAfter` | A card. With a caption: text on row 1, caption two rows lower, so `Height` ≥ 4. |
| `Arrow` | `FromCol`, `FromRow`, `ToCol`, `ToRow`, `VerticalFirst`, `Text`, `Color`, `Wait` | Any direction, straight or L-shaped; grows when drawn after a press. The label goes above a horizontal leg, or beside a vertical one, clamped to the content columns. |
| `BarChart` | `Labels`, `Values`, `ValueTexts`, `Max`, `BarColors`, `Legend`, `Outline`, `TickEvery`, `X`, `Y`, `Width`, `LabelWidth` (12), `ValueWidth` (7), `Pitch` (2), `Wait` | Layout: label, bar, value. `Outline` adds a full-scale frame, a dashed rest and ticks. The 38-column budget is `X = 1, Width = 38`. |
| `MemoryMap` | `X`, `Y`, `Width` (8), `Names`, `Addresses`, `Heights`, `Colors`, `Note`, `Wait` | Stacked segments, high addresses first, addresses in Grey2 on the left. |
| `ImageElement` | `Column`, `Row`, `Width`, `Height`, `Image`, `Caption`, `Wait` | A hi-res image. |
| `Callout` | `Lines`, `Attribution`, `Y`, `X` (2), `Width` (36), `Accent` | The key message in the 16 px face, 16 px between lines, with an accent bar. |

Lower level, for `Present()`:

- `Master.Content`, `Master.Cover`, `Master.Section`, `Master.Picture`,
  `Master.Footer`;
- `Text.Draw`, `Heading`, `Big`, `Code`, `Center`, `Right`, `Width`;
- `Deck.Paint`, `Step`, `Show`, `Reveal`, `Build`, `Dim`, `Live`;
- `Screen.Fill`, `Line`, `ClearRect`, `X`;
- `Box.DrawCard`.

Live slides (`IntroSlide`, `DemoSlide`) call `Deck.Reveal()`, then loop on
`KeyBoard.AdvancePressed()`.

## Rules of thumb (hi-res)

- Each cell has one foreground colour. Keep differently coloured things in
  different cells.
- Lines of the 8 px body face sit 16 px apart; code uses the code face,
  8 px apart.
- Multicolour pictures only through `Master.Picture` (one split). Their
  colour-RAM colour cuts in and out instead of fading.
- In strings, `"` and `@` can't be used, and `~`, `\` and `^` are glyphs.
  Pass "" rather than `null` for "no text" where a method takes a string.

## Modern C# in this compiler

| Construct | Compiles | Cost |
|---|---|---|
| File-scoped namespaces, expression-bodied members, raw string literals, interpolation of constants, `static` classes, named constants | yes | 0 |
| Collection expressions `[a, b]`, target-typed `new()` | yes | byte for byte the same as `new T[] { }` (measured: 283 B both ways) |
| `{ get; init; }` / `{ get; set; }` auto-properties | yes | the same as fields (compiler change: accessor calls become field accesses, and the accessors are not compiled; −2.7 KB on this deck) |
| `switch` expressions, `is` patterns (`n is > 3 and < 9`), `?:`, `??` | yes | ordinary branches |
| Primary constructors, field initializers, constructors with a body | **no** (C64004) | The compiler never calls a constructor body. Supporting that is a larger compiler change, not done. |
| Tuples, deconstruction | no (generic `ValueTuple`) | |
| Overloads of one name | no: labels don't tell them apart | Use distinct names (`Theme.Or` / `Theme.OrCells`). |

## Runtime and compiler additions (generic; nothing for programs that don't use them)

- `C64.Screen.DrawFontText` / `FontTextWidth`: header flags for scale 2 and
  leading (`tools/c64font.py --big`, `--leading`).
- `C64.Screen.RemapCellColorsFrom(source, table)`.
- Compiler fixes and features:
  - A discarded ulong/string/float result is popped whole.
  - A peephole window never contains a branch target (`x = c ? x + 1 : x - 1`
    used to increment in both arms).
  - Auto-properties compile to fields.

## Memory (with the art of storyboard round 5)

| Item | Bytes |
|---|---|
| Program end | $C034 (normal build) |
| Free | about 2.9 KB ($CC00 − end − 56 − 107 bytes of static frames), before the heap |
| Engine C# | about 15.5 KB (incl. the GC slide's `Stack`, 0.5 KB) |
| Fonts | 3,300 |
| `buf_Picture` | 3,755 (the disk shots load into it) |
| Title picture | 1,715 |
| A5 + A6 | about 780 |
| Typical slide | Title + 5-item `BulletList` = 420 B of code + about 80 B of strings. A table slide is about 1 KB. |

## Assets owned by the engine

- `art/font/make_fonts.py` → `art/font/build/fonts.asm`.
- `art/engine/make_logos.py` → `logos.spr`: the C# and Commodore sprites.
