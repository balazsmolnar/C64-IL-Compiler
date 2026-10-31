# C64Presentation: the storyboard

The talk, slide by slide: what is on screen, what appears on each press, what
to say, and where every fact comes from. The Slides phase builds from this
file; the Art phase builds the asset list at the end. Owner: Story (see
`DECK-BRIEF.md`).

**Talk:** "C# on the C64", 28 slides, about 22 minutes plus 3 minutes of live
demo. Audience: .NET developers and retro fans. One idea per slide.

## Conventions

- **Screen budget.** 40x25 cells. Body text: at most 36 characters per line,
  at most 11 lines (inside a 1-cell margin, below the title, above the
  footer). Titles: at most 18 characters (a 2x2-cell title font gives 19 in
  38 columns). Code: at most 34 columns, at most 8 lines. Every block marked
  `screen` or `code` below has been counted.
- **Text is exact.** What is in a `screen`/`code` block is what goes on the
  slide, line for line. `->` is an arrow: draw it as one if the font has an
  arrow glyph, otherwise leave the two characters.
- **Layouts:** title, section, bullets, two-column, diagram, chart, code,
  image, full-bleed. Diagrams and charts are drawn by the engine (boxes,
  lines, bars and text) unless the asset list says otherwise: that costs
  code, not image memory.
- **Builds.** "Press n" = what appears on the n-th SPACE/fire after the
  slide has drawn. "On entry" = drawn before the first press.
- **Sources.** Commit hashes are in this repo (`git log`); "measured" means
  measured for this storyboard at `5216d06` (method under "Measurements").
  Hardware facts about the C64 itself are from the Commodore 64
  Programmer's Reference Guide.

## Running order

| # | Section | Slide | Layout | Min |
|---|---|---|---|---|
| 1 | Hook | Catacombs (no title) | full-bleed | 0:45 |
| 2 | Hook | C# on the C64 | title | 0:30 |
| 3 | What it is | The machine | two-column | 0:45 |
| 4 | What it is | Just C# | code | 0:45 |
| 5 | How it works | The pipeline | diagram | 1:00 |
| 6 | How it works | 46 passes | diagram | 1:00 |
| 7 | How it works | A stack machine | code | 1:00 |
| 8 | How it works | Two stacks | diagram | 0:45 |
| 9 | Hard parts | The hard parts | section | 0:15 |
| 10 | Hard parts | int is 8 bits | two-column | 1:00 |
| 11 | Hard parts | 256 bytes of stack | diagram | 1:00 |
| 12 | Hard parts | One-byte objects | diagram | 1:00 |
| 13 | Hard parts | Garbage collector | diagram (animated) | 1:15 |
| 14 | Hard parts | Strings & floats | two-column | 0:45 |
| 15 | Hard parts | Bitmap graphics | diagram | 1:00 |
| 16 | Small and fast | Small and fast | section | 0:20 |
| 17 | Small and fast | 39 bytes to 12 | code | 1:00 |
| 18 | Small and fast | Six optimizations | bullets | 1:00 |
| 19 | Small and fast | Hunchback: -24% | chart | 0:45 |
| 20 | Small and fast | Where bytes went | chart | 0:45 |
| 21 | Small and fast | Frame budget | chart | 0:45 |
| 22 | Catacombs | Catacombs | full-bleed | 0:20 |
| 23 | Catacombs | By the numbers | two-column | 0:45 |
| 24 | Catacombs | C64 tricks, in C# | bullets | 0:45 |
| 25 | Testing | Tests on a 6502 | diagram | 1:00 |
| 26 | Live | Live | bullets | 3:00 |
| 27 | Close | Lessons | bullets | 1:00 |
| 28 | Close | Thank you | title | 0:20 |

---

## 1. Hook

### Slide 1 — (no title): Catacombs

- **Layout:** full-bleed image, caption band at the bottom.
- **Title:** none.
- **On screen:** the picture fills rows 0-20. Rows 21-24 are a black hi-res
  band (a raster split, as in Catacombs itself) holding the captions,
  centred, one per press:

```screen
A Commodore 64 game.
Every line of game logic: C#.
```

- **Visual:** asset **A1** (Catacombs room with the skeleton guardian,
  torches lit). No deck chrome: no title, no footer, no slide number.
- **Build:** on entry the picture alone. Press 1: line 1. Press 2: line 2
  (the two characters "C#" in the accent colour).
- **Say:** "This is a dungeon crawler running on a 1982 home computer. Every
  line of its game logic is C#, compiled by a compiler that reads .NET IL."
- **Sources:** picture: `tools\catacombs-shot.ps1 -Scene MONSTERROOM`
  (Catacombs/Program.cs). "Game logic is C#": Catacombs/AAA-BRIEF.md
  ("Gameplay state lives in C#; hand-written asm belongs in the generic
  runtime"); the game's `RawAssembly` blocks hold only data tables
  (Catacombs/RoomArt.cs, Catacombs/audio/music.asm), checked at `5216d06`.

### Slide 2 — C# on the C64

- **Layout:** title.
- **Title:** `C# on the C64` (13)
- **On screen:**

```screen
Compiling .NET IL to 6502
Balazs Molnar
```

- **Visual:** the existing C# and Commodore logo sprites
  (`C64Presentation/sprites.asm`, as in today's IntroSlide) drifting slowly
  in the left and right margins, behind nothing. Title large and centred on
  row 9, subtitle row 13, name row 16 in secondary text colour.
- **Build:** on entry everything; the logos keep moving until the press.
- **Say:** "I'm going to show you how C# gets onto a C64, what was hard, how
  we made it small and fast, and then we'll play."
- **Sources:** none (title).

## 2. What it is

### Slide 3 — The machine

- **Layout:** two-column (picture left, facts right).
- **Title:** `The machine` (11)
- **On screen (right column, cells 20-38, 19 wide):**

```screen
64 KB RAM
~1 MHz 6510 CPU
A, X, Y: 8 bits
256-byte stack
320x200, 16 colours
```

- **Visual:** asset **A5**, a pixel-art Commodore 64 breadbox, in the left
  column (cells 1-18, rows 7-14).
- **Build:** on entry the picture. Presses 1-5: one fact each.
- **Say:** "This is the target: 64 kilobytes, one megahertz, three 8-bit
  registers and a 256-byte hardware stack. Everything that follows is about
  fitting C# into that."
- **Sources:** Commodore 64 Programmer's Reference Guide (memory map, 6510,
  VIC-II modes). PAL clock is 0.985 MHz, hence "~1 MHz".

### Slide 4 — Just C#

- **Layout:** code (code top, three lines of text below).
- **Title:** `Just C#` (7)
- **On screen (code):**

```code
using C64Lib;
class Program {
  static void Main() =>
    C64.Screen.Write(1, 1, "HI");
}
```

- **On screen (text, below the code):**

```screen
A normal project, a normal build
C64Lib: an API of empty stubs
The real work: 6502 macros
```

- **Visual:** code in a panel with the theme's code background.
- **Build:** on entry the code. Presses 1-3: one text line each.
- **Say:** "A C64 program is an ordinary .NET project. C64Lib's methods are
  empty; they only give the compiler names and signatures, and the
  behaviour comes from hand-written 6502 routines."
- **Sources:** the snippet compiles with this repo's compiler (built and
  compiled for this storyboard at `5216d06`); `Screen.Write(uint x, uint y,
  string s, ...)` is `C64Lib/Screen.cs:69`. Stubs and macros: CLAUDE.md
  ("What this is").

## 3. How it works

### Slide 5 — The pipeline

- **Layout:** diagram.
- **Title:** `The pipeline` (12)
- **On screen:** only the labels of the diagram. Box labels: `C#`,
  `IL .dll`, `.asm`, `.prg`, `VICE`, `C64`. Arrow labels (small text):
  `dotnet build`, `Compiler.exe`, `64tass`. Caption under `.asm`:
  `64tass syntax`.

- **Visual:** row A (cell rows 6-9): box "C#" (cyan), arrow "dotnet
  build", box "IL .dll" (violet), arrow "Compiler.exe" turning down at
  the right edge. Row B (cell rows 13-16), right to left: box ".asm"
  (light green, small caption "64tass syntax"), arrow "64tass", box ".prg"
  (yellow). Below .prg, two small boxes side by side: "VICE" and "C64".
  Boxes 8 cells wide, 3 high; arrows 1 cell high with the label above.
- **Build:** on entry the C# box. Press 1: arrow + IL box. Press 2: arrow +
  .asm box. Press 3: arrow + .prg box. Press 4: VICE and C64 boxes.
- **Say:** "Roslyn does the C# part for free. Our compiler only ever sees
  IL: it reads the .dll with reflection and writes 64tass assembly, which
  assembles into a .prg that runs in VICE or on the real machine."
- **Sources:** CLAUDE.md ("What this is", "Build & run");
  `tools/compile-and-assemble.bat`.

### Slide 6 — 46 passes

- **Layout:** diagram.
- **Title:** `46 passes` (9)
- **On screen:** four stacked bands, each with a one-line label:

```screen
Whole program: call graph, frames
Read: IL -> operations
Model: labels, evaluation stack
Optimize: rewrites, to a fixpoint
Emit: 6502 macros, vtables, strings
```

- **Visual:** five horizontal bands, full body width, 2 cell rows each,
  1 row apart, joined by short down-arrows. Band colours: grey (whole
  program), violet (read), blue (model), orange (optimize: the accent, it
  is the next section), light green (emit). A thin loop arrow on the right
  of the optimize band (the fixpoint).
- **Build:** on entry the title. Presses 1-5: one band each, top to bottom.
- **Say:** "The compiler is a list of 46 passes. Most of the interesting ones
  are optimizers that rewrite the operation list again and again until
  nothing changes."
- **Sources:** 46 = `new IL...` pass constructions in `Compiler/Program.cs`
  at `5216d06` (counted with grep). Groups: CLAUDE.md ("Solution
  structure"); fixpoint: comment in `Compiler/Program.cs` above
  `ILFieldIncrementOptimizer` ("the optimizer set below runs to a fixpoint
  (see ILCodePass)"); whole-program passes: `ILCallSiteCountPass`,
  `ILStaticFramePass` in the same list.

### Slide 7 — A stack machine

- **Layout:** code (C# on top, then IL and macros side by side).
- **Title:** `A stack machine` (15)
- **On screen (code, 8 lines):**

```code
static uint Add(uint a, uint b)
    => a + b;

IL         6502 macro
ldarg.0    #locals_push_value8 1
ldarg.1    #locals_push_value8 2
add        #add8
ret        #method_exit 4, []
```

- **Visual:** the IL column in violet, the macro column in light green, a
  faint row highlight that moves down with the build.
- **Build:** on entry the C# (2 lines). Press 1: the IL column. Presses
  2-5: one macro per press, next to its IL line.
- **Say:** "IL is a stack machine: push a, push b, add, return. The naive
  translation is one 6502 macro per IL instruction, and that works,
  because the 6502 has a stack too."
- **Sources:** compiled for this storyboard (see "Measurements", snippet
  S1) with `--no-static-frames`; the four macro lines are the compiler's
  own one-per-instruction output (printed as `; OPT` comments in
  `Program.asm` once the optimizer replaced them). The method prologue
  `#init_locals_pull_parameters` is left out for width. The same example was
  on the old deck's CompilerPassesSlide.

### Slide 8 — Two stacks

- **Layout:** diagram.
- **Title:** `Two stacks` (10)
- **On screen:**

```screen
6502 stack: the evaluation stack
localsStack: arguments and locals
Add, compiled naively: 39 bytes
```

- **Visual:** two vertical stacks side by side, 10 cells wide, 9 high.
  Left, labelled "$0100 6502 stack": cells, top down, "ret addr", "b",
  "a", with a "pha / pla" label on the arrow into its top. Right, labelled
  "localsStack, 256 B": a frame of cells "ret addr", "a", "b" and a
  pointer arrow from a small box "Y = stackPointer" to the frame. A curved
  arrow from left to right labelled "pla, sta localsStack,y" (the prologue
  moving the return address and the arguments into the frame).
  The three text lines sit under the stacks.
- **Build:** on entry the left stack. Press 1: the right stack and the
  arrow. Press 2: line 3 in the accent colour.
- **Say:** "Values being computed live on the hardware stack; arguments and
  locals get their own 256-byte stack indexed by Y. Correct, but our tiny
  Add is 39 bytes plus a shared exit. Remember that number."
- **Sources:** the 64tass listing of snippet S1 with `--no-static-frames`:
  `Program_Add` spans `$17B0-$17D6` (39 bytes), ending `jmp Method_Exit_4`;
  its prologue is `pla` / `sta localsStack,y`. Shared exit tails: `0e32d14`.
  256-byte locals stack: CLAUDE.md, `asm/helper/objectTables.asm` header
  ("256-byte localsStack").

## 4. The hard parts

### Slide 9 — The hard parts

- **Layout:** section.
- **Title:** `The hard parts` (14)
- **On screen:**

```screen
8-bit int   stack   objects
GC   strings   floats   bitmaps
```

- **Visual:** section background (the theme's section colour, full
  bleed). A row of six icons (asset **A6**) above the two text lines, each
  icon over its word.
- **Build:** on entry everything.
- **Say:** "Six things C# takes for granted that a 6502 does not have."
- **Sources:** agenda of slides 10-15.

### Slide 10 — int is 8 bits

- **Layout:** two-column (type table left, story right), error below.
- **Title:** `int is 8 bits` (13)
- **On screen (left, type table):**

```screen
int, uint     8 bits
long, ulong  16 bits
float        5 bytes
double       error
```

- **On screen (bottom, code + error):**

```code
for (uint i = 0; i < 700; i++)
```

```screen
error C64006: The constant 700
does not fit in 8 bits ... so it
would become 188.
```

- **Visual:** the error in the theme's error colour (light red) with a thin
  red bar on its left, like an IDE squiggle panel. The literal `700`
  underlined in the code line.
- **Build:** on entry the type table. Press 1: the code line. Press 2: "188"
  appears beside it in grey, struck through ("what it used to do").
  Press 3: the error panel.
- **Say:** "int is 8 bits, because that is what the CPU has. In one of our
  own tests a loop bound of 700 silently became 188; now that is a compile
  error, with the C# line number, like any other unsupported feature."
- **Sources:** sizes: `Compiler/TypeExtensions.cs` `GetStorageBytes()`
  (string/long/ulong 2, float 5, otherwise 1); double: `Compiler/Diagnostics.cs`
  (`Ldc_r8`: "double-precision floating point (double)", hint "Use float").
  700 -> 188: commit `4173f55` ("uint is 8-bit here: 700 silently became
  188"). C64006: commit `93f2aa3`; the message is the compiler's own output
  for the line above (compiled for this storyboard), shortened with "...":
  full text "The constant 700 does not fit in 8 bits: int and uint are 8-bit
  values on the C64 (0..255, or -128..127), so it would become 188."
  Diagnostics with the PDB line: CLAUDE.md ("Unsupported C# is a compile
  error").

### Slide 11 — 256 bytes of stack

- **Layout:** diagram.
- **Title:** `256 bytes of stack` (18)
- **On screen:**

```screen
Deep calls overflow 256 bytes
Static frames: fixed addresses
Callers that can't overlap share
Catacombs: 118 of 125 methods
in 39 bytes of frames
```

- **Visual:** left half: a call tree, Main at the top, children Update and
  Draw, Update's child Move, Draw's child Hud (boxes 6 cells wide). Right
  half: an address strip 4 cells wide, 8 high, with coloured bars showing
  where each frame lives: Main at the top, Update and Draw on the SAME
  rows (side by side, different colours, both pointing at the same
  addresses), Move and Hud again sharing rows below. Label the strip
  "StaticFrames". Caption under the tree: "illustration".
- **Build:** on entry line 1 with a red overflow arrow over a full stack
  icon. Press 1: line 2 and the tree. Press 2: line 3 and the address strip
  (frames slide into place). Press 3: lines 4-5 in the accent colour.
- **Say:** "A 256-byte locals stack overflows silently on a deep call chain.
  The fix was to give every non-recursive method a frame at a fixed
  address and let methods that can never be active together share bytes:
  Catacombs puts 118 of its 125 methods' frames into 39 bytes."
- **Sources:** silent overflow: Catacombs/AAA-BRIEF.md ("Deep call chains
  overflow the 256-byte locals stack silently"); static frames, overlaid
  along the call graph, not for recursive, virtual or delegate-reachable
  methods: commit `18bb09d`; instance methods and references: `421df94`.
  118 of 125 and 39 bytes: Catacombs build output at `5216d06` ("Static
  frames: 118 of 125 methods (9 callee-pulls), 39 bytes") and
  Catacombs/TECH.md ("39 bytes of static frames").

### Slide 12 — One-byte objects

- **Layout:** diagram.
- **Title:** `One-byte objects` (16)
- **On screen:**

```screen
A reference is a 1-byte handle
255 objects; handle 0 is null
Handle -> table -> heap address
Full: ?OUT OF MEMORY ERROR
```

- **Visual:** the old MemoryLayoutSlide, redrawn on the theme: an object
  table (rows "1", "2", "...", "255"; columns HEAP PTR, SIZE, REFS, ROOTS,
  VTABLE) in cyan across rows 4-11, and a heap strip in yellow across rows
  13-15 with blocks "1", "3", "2", "5" and "FREE". Arrows from row 1's
  and row 2's HEAP PTR down to their blocks. The four text lines in rows
  17-20. Nothing in the footer row overlaps the heap (today the slide
  counter overwrites it).
- **Build:** on entry the table header. Press 1: rows 1, 2, 255. Press 2:
  the heap strip and the two arrows. Press 3: the four text lines
  together.
- **Say:** "A reference is one byte: an index into seven 256-byte tables
  that hold each object's address, size, reference fields, root count and
  vtable. That is why a program can have 255 live objects, and why moving
  an object only changes one table entry."
- **Sources:** 1-byte reference: `Compiler/TypeExtensions.cs`
  (`GetStorageBytes()` returns 1 for classes); tables: `asm/helper/objectTables.asm`
  ("The 7x256-byte object/GC tables"; `objTableSize`, `objTableReferences`,
  `objTableRootCount`, ...); handle 0 = null: `asm/GC.asm`
  (`trackObjects`: "mark NULL"); 255 objects: old FeaturesSlide
  ("OBJECT (255)"); faults "out of memory" and "too many objects": commit
  `3f3d760` and `asm/helper/fault.asm` header ("?<MESSAGE> ERROR", red
  border) and `Runtime_FaultMsg0 .text "OUT OF MEMORY"`.

### Slide 13 — Garbage collector

- **Layout:** diagram (animated, the existing GarbageCollectorSlide logic
  on the new theme).
- **Title:** `Garbage collector` (17)
- **On screen (right side, short labels that light up per phase):**

```screen
1 Roots: counted
2 Mark: follow references
3 Sort by address
4 Slide down: compact
```

- **Visual:** the existing animation: six object boxes in a row (1 and 3
  are roots, cyan), a heap strip below, a small mark stack. Marked objects
  turn orange, dead ones fade to black, then the heap blocks slide left to
  close the gaps (new: the compaction step, which today's slide skips).
- **Build:** on entry the objects. Each press advances the animation one
  step (as today); the phase label on the right lights up as its phase
  starts.
- **Say:** "Every store of a reference into a local, a parameter or a
  static field updates a count, so the roots are known without scanning
  anything. References
  between objects are traced; then the live objects are sorted by address
  and slid down. It runs only when you call GC.Collect()."
- **Sources:** `asm/GC.asm` (`GC_Collect`: `trackObjects`,
  `fillSortTable`, `quicksort`, `sweepAndCompact`); root counting:
  `asm/helper/heap.asm` (`stsfld`: `dec`/`inc objTableRootCount`),
  `asm/helper/localsStack.asm` (`locals_pull_param_8`),
  `asm/helper/stack.asm` (`stack_pull_int_ref`, `deref`); heap-to-heap
  references traced, not counted: `asm/helper/object.asm` (comment above
  `NewObjArrayInitL`); only on an explicit call: `Compiler/Diagnostics.cs`
  ("Only GC.Collect() is supported") and
  `Compiler/ILMethodCachedFieldAccessPass.cs` ("This GC only ever moves an
  object via an explicit GC.Collect() call").

### Slide 14 — Strings & floats

- **Layout:** two-column.
- **Title:** `Strings & floats` (16)
- **On screen (left, cells 1-18):**

```screen
STRINGS
Pointer to bytes,
zero-terminated
Not on the heap
a + b: a 40-byte
buffer
```

- **On screen (right, cells 21-38):**

```screen
FLOATS
5-byte C64 format
Math: the BASIC
ROM's own routines
Sin, Cos, Sqrt
```

- **Visual:** column headings in the accent colour; a thin vertical rule
  between the columns.
- **Build:** on entry the headings. Press 1: left column. Press 2: right
  column.
- **Say:** "We borrowed rather than built. A string is a pointer to
  zero-terminated bytes, and concatenation writes into one small buffer.
  A float is the C64's own 5-byte format, so the BASIC ROM, which is in
  every C64 anyway, does the maths."
- **Sources:** strings: `asm/helper/stringops.asm` header ("raw
  null-terminated character bytes", "stringops_buffer's fixed 40-byte
  size"); not reference counted: `Compiler/TypeExtensions.cs`
  (`IsReferenceCounted`: string -> false). Floats: `Compiler/TypeExtensions.cs`
  ("Native C64 MFLPT format", 5 bytes); BASIC ROM routines:
  `asm/helper/floatBanking.asm`; Sin/Cos/Sqrt: `Compiler/Diagnostics.cs`
  ("MathF.Sin/Cos/Sqrt").

### Slide 15 — Bitmap graphics

- **Layout:** diagram (memory map left, text right).
- **Title:** `Bitmap graphics` (15)
- **On screen (right column, 18 columns):**

```screen
Bitmap under the
KERNAL ROM
Sprites under I/O
Frees 9.4 KB
Pictures packed:
8,400 -> 2,118 B
```

- **Visual:** a vertical memory bar, 8 cells wide, rows 5-22, $0000 at the
  bottom, $FFFF at the top, address labels left of it. Segments:
  `$0800` runtime tables (grey); `$1000-$CBFF` program + heap (blue, the
  tallest); `$CC00` screen colours (cyan); `$D000` I/O, with "64 sprite
  frames" inside it (violet, hatched); `$E000-$FF3F` bitmap (yellow), with
  "under ROM" inside it.
- **Build:** on entry the memory bar without the top three segments.
  Press 1: `$E000` bitmap + text lines 1-2. Press 2: `$D000` sprites +
  line 3. Press 3: line 4 (accent). Press 4: lines 5-6.
- **Say:** "Hi-res graphics need 8 KB of bitmap, so it goes into the RAM
  hidden under the KERNAL ROM, and the sprite shapes go under the I/O
  chips. That frees 9.4 KB for code; pictures are stored compressed."
- **Sources:** all from Catacombs/TECH.md: memory map table; "High layout
  itself ... about 110; frees 9.4 KB"; "the tech demo's dithered 40x21
  backdrop is 8400 -> 2118 bytes"; CLAUDE.md ("Graphics memory layout").

## 5. Making it small and fast

### Slide 16 — Small and fast

- **Layout:** section.
- **Title:** `Small and fast` (14)
- **On screen:**

```screen
Pay only for what you use
```

- **Visual:** section background. Under the line, small and grey:
  `.weak Flag_X` (the asm mechanism that links a routine only when used).
- **Build:** on entry everything.
- **Say:** "One rule for every optimization and every feature: a program
  that does not use it pays nothing, not one byte. We check that on every
  change by measuring Hunchback, another full game."
- **Sources:** CLAUDE.md ("Pay only for what you use", "`.weak Flag_X`",
  "an unrelated change must leave its segment sizes identical").

### Slide 17 — 39 bytes to 12

- **Layout:** code (code left, text right).
- **Title:** `39 bytes to 12` (14)
- **On screen (code, 7 lines):**

```code
Program_Add         ; 12 bytes
  lda SF_TOP-512    ; b
  sta zp_param1_low
  lda SF_TOP-511    ; a
  clc
  adc zp_param1_low
  rts               ; sum in A
```

- **On screen (text, under the code):**

```screen
Static frame: no prologue
Caller stores args: lda, sta
Result comes back in A
```

- **Visual:** a two-bar mini chart above the code, full body width, 1 cell
  high each: "before 39 B" (grey, 39 units) and "after 12 B" (accent, 12
  units), same scale.
- **Build:** on entry the before bar. Press 1: the after bar and the code.
  Presses 2-4: one text line each.
- **Say:** "Same Add, today's compiler. The arguments sit at fixed
  addresses, the caller writes them with plain loads and stores, and the
  result never touches the stack: six instructions."
- **Sources:** snippet S1 compiled with today's defaults at `5216d06`:
  `Program.asm` and the 64tass listing (`Program_Add` at `$17B0-$17BB`: `ad
  15 18 / 85 32 / ad 16 18 / 18 / 65 32 / 60`, 12 bytes). `SF_TOP-511` is
  `Ldarg_0` (a), `SF_TOP-512` is `Ldarg_1` (b), per the compiler's `; OPT`
  comments. Call sites `lda score / sta SF_TOP-511 / lda #5 / sta
  SF_TOP-512 / jsr Program_Add / sta score`. 39 bytes: slide 8.
  Mechanisms: `18bb09d` (static frames, result in A/X), `421df94`
  (direct call-site copies), `21e84ec` (top of stack in A).

### Slide 18 — Six optimizations

- **Layout:** bullets (two aligned columns: name, effect).
- **Title:** `Six optimizations` (17)
- **On screen:**

```screen
Shared exit tails  17 B -> 3 B jmp
Skip reloading Y   -2 B per access
Static frames      no prologue
Top of stack in A  400 -> 2 pha/pla
And/or/xor + const 10 B -> 4 B
Object-array table 1 table, 1 call
```

- **Visual:** names in primary text, effects in the accent colour. No
  hashes on screen (they are in the sources below).
- **Build:** on entry the title. Presses 1-6: one line each.
- **Say:** "Six ideas did most of the work. The two biggest are structural:
  frames at fixed addresses, and keeping the top of the stack in the
  accumulator so that a push followed by a pull simply disappears."
- **Sources (one per line):**
  1. `0e32d14`: "Every ret used to expand a 17-byte epilogue inline; it is
     now a 3-byte jmp into a shared Method_Exit_N tail".
  2. `93ab351`: skip `ldy stackPointer` when Y already holds it, "2 bytes
     and 3 cycles each".
  3. `18bb09d`: "Such a method has no prologue".
  4. `21e84ec`: "Boundary pairs: Hunchback 400 -> 2".
  5. `34a08e9`: "pla / and #c / pha, 4 bytes instead of 10".
  6. `7235df6`: an object-array literal "becomes one #newObjArrayInit call
     whose descriptor holds ... every object's initial bytes".

### Slide 19 — Hunchback: -24%

- **Layout:** chart (two horizontal bars) with a one-line caption.
- **Title:** `Hunchback: -24%` (15)
- **On screen:**

```screen
Oct 17  36,847 B
Oct 31  28,157 B
Same game, same C#: -8,690 B
Catacombs: 7,177 -> 10,478 B free
```

- **Visual:** two horizontal bars, 30 cells max length, scale 0-37,000 B,
  rows 7-8 and 10-11: "Oct 17" grey (36,847 -> 30 cells), "Oct 31" accent
  (28,157 -> 23 cells). The value at the end of each bar. A dashed vertical
  line from the end of the shorter bar to the longer one with "-8,690 B"
  between them. Lines 3-4 of the text below the bars.
- **Build:** on entry the "Oct 17" bar. Press 1: the "Oct 31" bar grows
  from zero. Press 2: the gap label and line 3. Press 3: line 4.
- **Say:** "Hunchback's source did not change in these two weeks; only the
  compiler did. The same game got a quarter smaller, and Catacombs got
  3.3 KB of room back."
- **Sources:** measured (see "Measurements"): Hunchback program bytes
  (sum of the `Data:` segments `Hunchback/measure.bat` prints) at
  `1e1a44e` (2026-10-17, just before `0e32d14`) = 13 + 3,247 + 5,760 +
  27,827 = 36,847; at `5216d06` = 13 + 3,479 + 5,760 + 18,905 = 28,157;
  -23.6%. The main code segment alone: 27,827 -> 18,905 (-32%).
  Hunchback's source is unchanged since then: `git log --since=2026-10-10
  -- Hunchback/` shows only `0726949` (a .csproj build-order line).
  Catacombs free memory: Catacombs/TECH.md ("10,478 bytes ... (7,177
  before those optimizations)").

### Slide 20 — Where bytes went

- **Layout:** chart (grouped horizontal bars).
- **Title:** `Where bytes went` (16)
- **On screen (row labels, left, 14 columns; values at the bar ends):**

```screen
Object arrays
Static frames
Exit tails
Top of stack
Static arrays
Skip Y reload
++ on statics
Logic + const
```

- **Visual:** for each row, two thin bars (half a cell row each): Hunchback
  (blue) above, Catacombs (orange) below; legend at the top right. Scale
  0-3,400 B over 20 cells. Values (bytes saved; a dash where the change did
  not apply):

  | Row | Hunchback | Catacombs | Source |
  |---|---|---|---|
  | Object arrays | 3,357 | 0 | `7235df6` |
  | Static frames | 2,304 | 1,818 | measured |
  | Exit tails | 1,983 | 2,579 | `0e32d14` |
  | Top of stack | 824 | 1,168 | `21e84ec` |
  | Static arrays | 0 | 616 | `f140b7c` |
  | Skip Y reload | 311 | 495 | `93ab351` |
  | ++ on statics | 28 | ~340 | `465b9d1` |
  | Logic + const | 85 | 326 | `34a08e9` |

  Footnote in small grey text: `each at its own commit; not additive`.
- **Build:** on entry the axes and legend. Press 1: all Hunchback bars grow
  together. Press 2: all Catacombs bars.
- **Say:** "Which program wins depends on its style: Hunchback builds lots
  of object arrays, Catacombs indexes lots of static arrays. That is why
  we measure on two real games, not on a benchmark."
- **Sources:** each figure is the commit message of the listed commit
  (`465b9d1`: "Catacombs shrinks by about 340 bytes, Hunchback's main
  segment by 28"; `f140b7c`: "Hunchback unchanged"; `7235df6`: "Catacombs
  and Demo unchanged", Hunchback "net -3357 B"). Static frames: measured
  at `5216d06`, main segment with `C64_STATIC_FRAMES=0` vs default:
  Hunchback 21,209 -> 18,905, Catacombs 39,383 -> 37,565. The figures were
  taken on different bases (the four 2026-10-30 changes on separate
  branches), so they do not add up to the merge totals of `bbdf2f5`
  (Hunchback -6,396 B, Catacombs -3,312 B).

### Slide 21 — Frame budget

- **Layout:** chart (horizontal bars against a full-frame outline).
- **Title:** `Frame budget` (12)
- **On screen:**

```screen
Catacombs: worst frame
before  257 of 311 lines
after   208 of 311 lines
Frame work: -21%
```

- **Visual:** an outline box 31 cells wide = 311 raster lines (one frame),
  ticked every 50 lines. Inside it, two filled bars: "before" 257 lines
  (25.7 cells, grey) and "after" 208 lines (20.8 cells, accent). The empty
  part of the outline is "free for the game".
- **Build:** on entry the outline and the "before" bar. Press 1: the "after"
  bar. Press 2: line 4 in the accent colour.
- **Say:** "Smaller code is also faster code here. The worst frame in
  Catacombs went from 257 to 208 raster lines of the 311 a frame has, so
  a fifth of the game's frame work is gone."
- **Sources:** merge commit `bbdf2f5`: "Catacombs frame work -21% (VICE
  worst frame 257 -> 208 of 311 lines)". Supporting (speaker only):
  `21e84ec` top of stack in A alone "9404 -> 8491 cycles/frame (-9.7%)";
  `4aae54e` division by a constant "About 40% fewer cycles than
  Divide16Core".

## 6. Catacombs

### Slide 22 — Catacombs

- **Layout:** full-bleed image.
- **Title:** none on the image; the word "Catacombs" is the picture's own
  logo.
- **On screen (caption band, rows 21-24, hi-res):**

```screen
A dungeon crawler, all in C#
```

- **Visual:** asset **A2** (the Catacombs title screen: logo, skull wall,
  the gate with the hero silhouette).
- **Build:** on entry the picture. Press 1: the caption.
- **Say:** "Catacombs is the showcase: we set it the bar of the best modern
  C64 releases and only allowed generic compiler and library changes to
  get there."
- **Sources:** picture: `tools\catacombs-shot.ps1` with no `-Scene` (the
  real game from power-on: the title). Bar and rules: Catacombs/AAA-BRIEF.md
  ("The bar", "Technical rules"); merge `c9db78e`.

### Slide 23 — By the numbers

- **Layout:** two-column (picture left, numbers right).
- **Title:** `By the numbers` (14)
- **On screen (right column, 15 columns, big number + small label pairs):**

```screen
3,477
lines of C#
61 rooms
4 guardians
5 colour themes
10,478 B free
```

- **Visual:** asset **A3** (the "Bone Warden" encounter: horned skull,
  name banner, health bar) in the left column. Numbers in the large title
  font, labels small and grey under them ("3,477" big, "lines of C#"
  small; "61" big, "rooms" small, and so on).
- **Build:** on entry the picture. Presses 1-5: one number each.
- **Say:** "Three and a half thousand lines of C#, sixty-one rooms, four
  guardians, a three-voice soundtrack, and ten kilobytes still free."
- **Sources:** 3,477: `wc -l Catacombs/*.cs` at `5216d06`. 61 rooms: floor
  cells in `Catacombs/Maze.cs` `Grid` (12x12, nonzero = floor), counted.
  4 guardians: `Catacombs/Monster.cs` ("skeleton warrior, ghoul, wraith
  ... and the giant spider"). 5 themes: `Catacombs/RoomArt.cs`
  (`ThemeCount = 5`). 10,478 B: Catacombs/TECH.md. Soundtrack (speaker
  only): Catacombs/audio/README.md (title jingle 8.6 s, exploration tune
  92 s loop, combat tune 31 s loop). Picture: `-Scene COMBAT`, a shot
  about 1.5 s after start.

### Slide 24 — C64 tricks, in C#

- **Layout:** bullets with a small picture.
- **Title:** `C64 tricks, in C#` (17)
- **On screen:**

```screen
Raster split: room multicolour,
status panel hi-res
Themes: 16-byte colour remaps
HUD redrawn as tiles: no flicker
Music on the raster interrupt
```

- **Visual:** the lower part of asset **A1** (its rows 13-20, the floor
  and the skeleton) as a 40x8 strip above the bullets, with a dashed
  horizontal line labelled "split" along its bottom edge. No new image.
- **Build:** on entry the strip. Presses 1-4: one bullet each (the first
  bullet is two lines).
- **Say:** "Every classic trick is a C# call: the screen changes mode
  halfway down, rooms are recoloured through a lookup table, and the music
  player hangs off the same interrupt."
- **Sources:** Catacombs/TECH.md ("Raster split", `SetSplit(21,
  ScreenMode.Bitmap)`; `RemapCellColors` "through a 16-byte table";
  `DrawTiles`: "swapping a tile cannot flicker ... This is how the status
  panel (`Hud.cs`) updates hearts, digits, the weapon icon and the
  minimap"; "While a split is on, the system
  interrupt runs once per video frame ... That includes a C64.Interrupt
  subscriber (music)"); themes: `Catacombs/RoomArt.cs` (`pal_Theme1..4`).

## 7. Testing

### Slide 25 — Tests on a 6502

- **Layout:** diagram (code top, loop diagram below).
- **Title:** `Tests on a 6502` (15)
- **On screen (code):**

```code
[TestCase(5, ExpectedResult = 6)]
int TestIncrement(int value)
{
    var a = value;
    return ++a;
}
```

- **On screen (diagram labels):**

```screen
NUnit -> PostSharp aspect
-> SimpleEmulator runs the .prg
-> result byte at $20 -> NUnit
~730 test cases, all as 6502
```

- **Visual:** a loop of four boxes clockwise under the code: "NUnit"
  (white), "RunInEmulatorAspect" (violet), "SimpleEmulator +
  unittest.prg" (light green), "$20 result" (yellow), arrows between them
  and back to NUnit, with a green tick inside the NUnit box at the end.
- **Build:** on entry the code. Press 1: the loop draws box by box (one
  press, animated). Press 2: the green tick. Press 3: last line.
- **Say:** "The test body is not run as .NET at all: a PostSharp aspect
  intercepts the call, runs the compiled 6502 routine in our own
  emulator, and hands the result back to NUnit. So every green test
  passed on the target CPU."
- **Sources:** CLAUDE.md (`C64TestFramework`: "intercepts each
  [Test]/[TestCase] method call, loads prg/unittest.prg into a
  SimpleEmulator.Emulator, copies method arguments into emulated memory
  ... result byte at 0x20"); `Test/assembly.cs`; the code is
  `Test/ArithmeticTests.cs` `TestIncrement` (its first `[TestCase]`; the
  `public` modifier left out for width). ~730: `[Test]`/`[TestCase]`
  attributes counted at `5216d06`: 595 in `Test/`, 134 in `Test.Runtime/`
  (plus 136 compiler unit tests in `Compiler.UnitTests/`, run as normal
  .NET).

## 8. Live

### Slide 26 — Live

- **Layout:** bullets (a checklist).
- **Title:** `Live` (4)
- **On screen:**

```screen
1 Catacombs on VICE
2 A breakpoint in C#, on the C64
3 Break the build: C64006
```

- **Visual:** the theme's "demo" variant: dark background, a blinking
  cursor block after the last visible line. Each item gets a tick when
  done (press).
- **Build:** on entry the title. Presses 1-3: one item each (the presenter
  switches to the PC between presses). Press 4-6: ticks.
- **Say / run:**
  1. Run `Catacombs\run.bat`, walk two rooms, fight one guardian.
  2. In VS Code, start "debugging Hunchback on VICE", stop at a breakpoint,
     hover a field, step over.
  3. Type `for (uint i = 0; i < 700; i++)` into a C64Lib project, build,
     show the C64006 error in the error list.
  Fallback if VICE misbehaves: slides 1, 22, 23 are the screenshots.
- **Sources:** Catacombs/run.bat; debugger: commits `98c2604` ("run a
  program on VICE from VS Code and stop at breakpoints"), `c7b9931` ("VS
  Code launch config for debugging Hunchback on VICE"), `950600a`
  (object/array inspection); C64006: slide 10.

## 9. Close

### Slide 27 — Lessons

- **Layout:** bullets.
- **Title:** `Lessons` (7)
- **On screen:**

```screen
Structure beats peepholes
Measure every change
Unsupported = compile error
Test on the target CPU
```

- **Visual:** each line with a small accent bar on the left; nothing else.
- **Build:** on entry the title. Presses 1-4: one line each.
- **Say:**
  1. "The two structural changes saved thousands of bytes; a typical
     peephole saved tens."
  2. "Every change is measured on two real games."
  3. "A silent miscompile costs a week; an error message costs a minute."
  4. "Our emulator tests caught real code-generation bugs."
- **Sources:** 1: `bbdf2f5` (-6,396 B Hunchback) vs `465b9d1` (-28 B) and
  `4173f55` (-41 B). 2: CLAUDE.md ("Check with Hunchback/measure.bat").
  3: `4173f55` (700 -> 188) and CLAUDE.md ("never a silent miscompile").
  4: e.g. `34a08e9` ("fixes a miscompile ... `(c ? x : y) + z`"),
  `eefca0b` ("Fix 8-bit greater-than branches taken on equality").

### Slide 28 — Thank you

- **Layout:** title.
- **Title:** `Thank you` (9)
- **On screen:**

```screen
This deck is C# too.
github.com/balazsmolnar/
C64-IL-Compiler
```

- **Visual:** the two logo sprites from slide 2 return and settle side by
  side above the title.
- **Build:** on entry the title and the URL. Press 1: line 1 (accent).
- **Say:** "Every slide you have seen was C#, compiled by this compiler and
  running on a C64. Thank you."
- **Sources:** this project (`C64Presentation/`); `git remote -v`.

---

## Measurements

Figures marked "measured" were taken for this storyboard; reproduce them
like this (all paths literal, scratch under `.debug-tmp\`).

- **Hunchback before/after (slide 19).** HEAD `5216d06`: build
  `Hunchback\Hunchback.csproj`, run `Hunchback\measure.bat`, add the
  `Data:` lengths: 13 + 3,479 + 5,760 + 18,905 = 28,157. Before: `git
  archive 1e1a44e` into `.debug-tmp\old`, build its `Compiler` and
  `Hunchback`, run its `Hunchback\measure.bat`: 13 + 3,247 + 5,760 +
  27,827 = 36,847. Loaded program end `$A35A` -> `$8080`.
- **Static frames on/off (slide 20).** At `5216d06`, `C64_STATIC_FRAMES=0`
  then default: Hunchback main segment 21,209 -> 18,905 (-2,304);
  Catacombs main segment 39,383 -> 37,565 (-1,818).
- **Snippet S1 (slides 7, 8, 17).** A C64Lib program:

  ```
  static uint score, bonus;
  static uint Add(uint a, uint b) => a + b;
  static void Main()
  {
      score = Add(score, 5);
      bonus = Add(bonus, score);
      score = Add(score, bonus);
      C64.Screen.Write(0, 0, score.ToString());
  }
  ```

  Three call sites so that `Add` is not inlined. Compiled with
  `Compiler.exe` (default, then `--no-static-frames`) and assembled with
  64tass `--list`. Default: `Program_Add` = `$17B0-$17BB`, 12 bytes.
  `--no-static-frames`: `$17B0-$17D6`, 39 bytes, plus the shared
  `Method_Exit_4`.
- **C64006 text (slide 10).** The same compiler on `for (uint i = 0; i <
  700; i++) n++;` prints `error C64006: The constant 700 does not fit in 8
  bits: int and uint are 8-bit values on the C64 (0..255, or -128..127), so
  it would become 188.`

---

## Asset list (for the Art builder)

Budget: 18 KB of compressed images in total. Everything not listed here is
drawn by the engine (diagrams D1-D8 and charts C1-C3 below cost code, not
image memory). Sizes in 8x8 cells; byte estimates use Catacombs' measured
assets (`Catacombs/art/build`: `room.img` 3,239 B for a 40x21 room,
`title.img` 2,475 B).

### Images

| ID | What | Size (cells) | Mode | Est. | Slides | How to get it |
|---|---|---|---|---|---|---|
| A1 | Catacombs room with the skeleton guardian and lit torches, viewport only (no HUD) | 40x21 | multicolour | 3.4 KB | 1, 24 (rows 13-20 reused as a strip) | `tools\catacombs-shot.ps1 -Scene MONSTERROOM`, crop rows 0-20, convert with `tools\c64img.py` |
| A2 | Catacombs title screen: logo, skull wall, gate with the hero silhouette (no "PRESS FIRE" text) | 40x18 (rows 1-18) | multicolour | 2.5 KB | 22 | `tools\catacombs-shot.ps1` with no `-Scene` |
| A3 | The "Bone Warden" encounter: horned skull, name banner, health bar | 22x16 (cols 9-30, rows 2-17) | multicolour | 1.6 KB | 23 | `tools\catacombs-shot.ps1 -Scene COMBAT -Shots 2 -IntervalMs 1500`, second shot |
| A5 | Pixel-art Commodore 64 breadbox (keyboard, rainbow badge), three-quarter view | 18x8 | multicolour | 0.9 KB | 3 | drawn by script in `C64Presentation/art/` |
| A6 | Six icons on one sheet: "8" chip, stack of plates, object box, recycling arrows, quote marks, picture frame | 6 icons x 3x3 | hi-res | 0.4 KB | 9 (and small markers on 10-15) | drawn by script |
| A7 | Optional: Hunchback in-game thumbnail | 16x10 | multicolour | 1.0 KB | 19 (beside the bars) | run `prg\hunchback.prg` with `tools\vice-launch.ps1 -KeepOthers`, then `tools\vice-capture.ps1 -ProcessId <pid>`; cut first if over budget |
| -- | C# and Commodore logos | sprites | -- | 0 (reuse) | 2, 28 | existing `C64Presentation/sprites.asm` |

Total: about 8.8 KB with A7 (9.8 KB), leaving room in the 18 KB budget for
any diagram the engine cannot draw (fallback below).

### Engine-drawn diagrams and charts

If one of these cannot be drawn by the engine, the fallback is a hi-res
image of the same size (line art compresses well: about 0.3-0.8 KB each).

| ID | Slide | Size (cells) | Content |
|---|---|---|---|
| D1 | 5 The pipeline | 38x12 | 5 boxes in a snake + 2 small target boxes, labelled arrows |
| D2 | 6 46 passes | 38x14 | 5 stacked bands, down-arrows, a loop arrow on "Optimize" |
| D3 | 8 Two stacks | 30x11 | two vertical stacks, the Y pointer, the "pla, sta" arrow |
| D4 | 11 256 bytes of stack | 38x10 | call tree + "StaticFrames" address strip with shared rows |
| D5 | 12 One-byte objects | 38x14 | object table (4 rows x 6 columns) + heap strip + 2 arrows |
| D6 | 13 Garbage collector | 38x18 | the existing animated GC scene, plus the compaction step |
| D7 | 15 Bitmap graphics | 16x18 | vertical memory map, 5 segments, address labels |
| D8 | 25 Tests on a 6502 | 38x7 | 4-box loop with arrows and a tick |
| C1 | 19 Hunchback: -24% | 38x8 | 2 horizontal bars (36,847 / 28,157 B), gap label |
| C2 | 20 Where bytes went | 38x18 | 8 rows x 2 bars (values in the slide's table), legend |
| C3 | 21 Frame budget | 38x7 | 311-line outline, bars at 257 and 208, ticks every 50 |
