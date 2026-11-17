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
  - **Code**: the body glyphs at a fixed 6 px pitch, with *leading* (a header
    flag): pixel line 0 is blank and the glyph's line 1 is dropped. Code lines
    sit on consecutive rows without touching, and no extra glyph data is
    needed. `i` and `j` have code-face glyphs of their own (a shorter,
    serifed stem): dropping line 1 put their dots on the stem.
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
  - Arrows and bars drawn after a step grow; what follows an arrow in
    the same step fades in after it (`Arrow` ends with `Deck.Build(false)`).
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
| `SectionElement` | `Number`, `Title`, `Subtitle`, `Icon` (an `img_Sec*` address; 0 = none) | On black: the part number big in Grey2, the title 2x in white, the subtitle, the icon top right. No number: a plain title slide (`DemoSlide`: the LIVE badge sits in the footer). |
| `TextElement` | `X`, `Y`, `Text`, `Face` (`Body` / `Heading` / `Big`), `Color` (Body, or white for headings), `Strike`, `Wait` | One line. `Strike` draws a 1 px rule through the text, for an old value. |
| `BulletList` | `Items`, `Left` (2), `Top` (5), `Pitch` (2), `Level` (0 = dot, 1 = dash), `Color`, `Wait` | 5 px between the mark and the text. |
| `CodeBlock` | `X`, `Y`, `Width` (36), `Rows`, `Lines`, `Caption`, `Replace`, `Wait` | Black panel, Grey1 1 px frame. Caption on row 1, code from row 3 in the code face: 6 px pitch, with leading (underscores clear the next line); each colour change starts on the next cell edge, so pad with spaces to line up a column after one. `Replace`: the old code is swapped row by row, never an empty card. |
| `Table` | `X` (2), `Y`, `Widths`, `Header`, `Rows` (`string[][]`), `WaitAfterRow`, `WaitAfterHeader` (all rows in one step) | Header labels in white on black over a 2 px cyan rule; rows with Grey2 hairlines, two cell rows each. First column left aligned, the rest right aligned. |
| `Box` | `X`, `Y`, `Width`, `Height`, `Text`, `Caption`, `Color` (accent bar, cyan), `Hollow`, `ConnectToLeft`, `Wait`, `WaitAfter` | A card. With a caption: text on row 1, caption two rows lower, so `Height` ≥ 4. |
| `Arrow` | `FromCol`, `FromRow`, `ToCol`, `ToRow`, `VerticalFirst`, `Text`, `Color`, `Wait` | Any direction, straight or L-shaped; grows when drawn after a press. The label goes above a horizontal leg, or beside a vertical one, clamped to the content columns. |
| `BarChart` | `Bars` (`Bar`: `Label`, `Value`, `Text`, `Color`), `Gap` (`BarGap`: `From`, `To`, `Text`), `Max`, `Outline`, `TickEvery`, `X`, `Y`, `Width`, `LabelWidth` (12), `ValueWidth` (7), `Pitch` (2), `Wait` | Label, bar, value. `Gap`: a dashed box level with the shorter bar, up to the longer bar's end, its text under it, and the shorter bar's value after the box. `Outline` adds a full-scale frame with ticks. C2 and C4 use `X = 2, Width = 36`. |
| `ButterflyChart` | `Rows` (`ButterflyRow`: `Label`, `LeftValue`, `LeftText`, `RightValue`, `RightText`), `Left`, `Right` (series names), `LeftColor` (cyan), `RightColor` (yellow), `Max`, `LegendRow`, `Y`, `LabelWidth` (12), `Pitch` (2), `BarCells` (8) | Two series item by item: labels in a centred column, the left series growing left, the right one growing right, each value at its bar's outer end in the bar's colour; 0 = no bar, no number. The legend right aligned on `LegendRow`. C3 (DECK.md's C3 spec changes from 8 x 2 bars on consecutive rows to this). |
| `MemoryMap` | `X`, `Y`, `Width` (8), `Segments` (`Segment`: `Name`, `Address`, `Detail`, `Height`, `Color`), `Note`, `Wait` | Stacked segments, high addresses first, a 1 px black line along each segment's bottom; each start address right aligned in Grey2 on its segment's bottom row, with a tick to the boundary. Give every segment 2 rows or more; a `Detail` (Grey3, its own row) needs 3. |
| `ImageElement` | `Column`, `Row`, `Width`, `Height`, `Image`, `Caption`, `Wait` | A hi-res image. |
| `Callout` | `Lines`, `Attribution`, `Y`, `X` (2), `Width` (36), `Accent` | The key message in the 16 px face, 24 px between lines, the attribution in white, an accent bar. 3 rows a line + 1 (+2 with an attribution). |

Lower level, for `Present()`:

- `Master.Content`, `Master.Cover`, `Master.Section`, `Master.Picture`,
  `Master.Footer`;
- `Text.Draw`, `Heading`, `Big`, `Code`, `Center`, `Right`, `Width`;
- `Deck.Paint`, `Step`, `Show`, `Reveal`, `Build`, `Dim`, `Live`;
- `Screen.Fill`, `Line`, `Frame`, `ClearRect`, `Px`;
- `Box.Card`.

`PictureSlide` (EngineGallery.cs) and `Master.Picture(image, column, row,
split, caption)`: a multicolour picture above a raster split (rows 15 and
21 are shot) with a caption under it.

The gallery: `dotnet build C64Presentation -p:Scene=ENGINE` builds
`Slides/EngineGallery.cs` (D7, C2, C3, C4, D8, two split pictures, a
callout, two columns, a section) instead of the deck, and holds each slide
like `-Scene HOLD`.

Live slides (`TitleSlide`, `DemoSlide`) call `Deck.Reveal()`, then loop on
`KeyBoard.AdvancePressed()`.

## Navigation (keys)

| Key | Does |
|---|---|
| SPACE / fire | the next build step, then the next slide |
| N | the next slide at once, from any point of a slide |
| B | the previous slide at once, from any point of a slide |

N and B are edge-triggered (one press, one slide). A press sets
`KeyBoard.Pending` (`Navigation.Next` / `Back`). The next engine call that
notices it (`Deck.Step`, `Deck.Wait`, `Deck.Show`, `Deck.Reveal`, any
`Deck.Paint` / `Text` drawing; `Deck.Leaving()`) fades what is on screen
out at once (what a build step had hidden stays hidden) and blanks the
screen; from then on those calls return at once and `Deck.Live` is false,
so the rest of `Present()` runs through unseen and fast. The Presenter then
clears up and goes to slide +1 / -1 (B on the first slide redraws it).

**The one rule for slides: a slide waits only through `KeyBoard` / `Deck`.**
A build step is `Deck.Step()`; a timed pause is `Deck.Wait(frames)`; an
animation loop runs `while (!KeyBoard.AdvancePressed()) { ...; Deck.Wait(1); }`
(or `C64.Screen.WaitForVBlank()` inside it). `AdvancePressed` is also true
once a jump is pending, so the loop ends by itself. Never wait on
`C64.IsKeyPressed`, a bare `WaitForVBlank` loop or a `Delay` loop of your
own: N and B would not be seen until it ends.

Scene builds: HOLD and AUTO as before (AUTO is SPACE); `-Scene NAVTEST` is
AUTO plus a faked N during the first slide's second build step and a faked
B soon after the next slide comes up.

## Rules of thumb (hi-res)

- Content slides keep row 21 (and 22) empty: `Master.Content` sets it,
  and a scene build turns the border red when anything draws there
  (`Deck.CheckRows`, `[Conditional("SCENE")]`: not in the real deck). The
  same check turns it red when a slide draws before its fade-in while the
  display is on (`` bit 4).

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
| Object initializers written out in the element list vs. a static helper that builds them (`Item(col, n, text)`) | yes | Write them out: a literal initializer compiles to a table (`#newObjInit`), a helper costs a call per use plus its body. Slides 4-7 shrank by 1.5 KB this way (slide 4 alone 713 B). |
| Spread `..` in a collection expression | no (a `List<T>`, C64002/C64003) | |
| Lambdas `() => ...` in more than one class | no: every lambda class is named `<>c` and the labels collide | Method groups (`Foo.Create`). |
| `{ get; init; }` / `{ get; set; }` auto-properties | yes | the same as fields (compiler change: accessor calls become field accesses, and the accessors are not compiled; −2.7 KB on this deck) |
| `switch` expressions, `is` patterns (`n is > 3 and < 9`), `?:`, `??` | yes | ordinary branches |
| Primary constructors, field initializers, constructors with a body | yes since the constructor support merge (a1f6cdc) | Not used here: a two-field `LegendItem(string name, Colors color)` cost 189 B more than `new() { Name = .., Color = .. }` (a constructor routine and its calls instead of inline stores). |
| `record` / `record struct`, `struct`, `Nullable<T>` | no (C64004 / C64003) | |
| 3-part string concatenation (`$"{a}{b}{c}"`) | no (`String.Concat` with 3 arguments, C64002) | Two 2-part steps. |
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
  - Static constructors run in dependency order (a static initializer that
    reads another type's static field runs after that type's), so
    `static readonly Func<Slide>[] Slides = EngineGallery.Slides;` works.

## Memory (round 4, plain build, with the art)

| Item | Bytes |
|---|---|
| Program end | $B4F7; static frames to $B5C8 |
| Free | about 5.7 KB before $CC00, before the heap |
| Elements no slide uses | cost nothing: dead-code elimination drops unused classes (byte-identical to the old csproj `Compile Remove` list, which is gone) |
| Engine C# in the deck build | about 11 KB (Arrow 2,122, Text 1,393, Deck 1,199, Table 1,130, Box, Master, CodeBlock, BulletList, ...) |
| Fonts | 3,630 (the code face's own width/address tables, 4 narrow capitals and its i/j: +330) |
| `buf_Picture` | 3,755 (the disk shots load into it) |
| Typical slide | Agenda (title + 5 bullets) 420 B of code; a table slide (What is it) 1,100 B; Features (two columns) 1,107 B; plus strings |
| GC slide | 4,550 B |

The gallery build (`-p:Scene=ENGINE`) ends at $C46B.

## Assets owned by the engine

- `art/font/make_fonts.py` → `art/font/build/fonts.asm`.
- `art/engine/make_logos.py` → `logos.spr`: the C# and Commodore sprites.
