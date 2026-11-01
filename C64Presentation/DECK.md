# C64Presentation: the storyboard

The talk, slide by slide: what is on screen, what appears on each press, what
to say, and where every fact comes from. The Slides phase builds from this
file; the Art phase builds the asset list at the end. Owner: Story (see
`DECK-BRIEF.md`).

**Talk:** "C# on the C64", 28 slides, about 17 minutes of slides plus a
3-minute live demo: about 20 minutes, leaving 5 minutes of slack in a
25-minute slot. Audience: .NET developers and retro fans. One idea per
slide; titles say something, they don't just label.

## Conventions

- **Screen budget.** 40x25 cells. Body text: at most 36 characters per line,
  at most 11 lines (inside a 1-cell margin, below the title, above the
  footer). Titles: at most 18 characters (a 2x2-cell title font gives 19 in
  38 columns). Code: at most 34 columns, at most 8 lines. Every block marked
  `screen` or `code` below has been counted by script.
- **One mode per raster row.** Multicolour and hi-res are whole-screen
  modes; they can only be mixed by a raster split (`C64.Screen.SetSplit`,
  as Catacombs does): multicolour above the split row, hi-res below it.
  Every slide below says which. Text is always hi-res.
- **Images are drawn whole.** `DrawImage` cannot clip, scale or flip
  (Catacombs/TECH.md); every crop below is its own asset.
- **Charts** list a column budget `label | bar | value` that sums to 38 or
  less. Bars are whole cells, one colour per cell row, one series per row.
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

| # | Section | Title | Layout | Time |
|---|---|---|---|---|
| 1 | Hook | (none) | full-bleed + split | 0:40 |
| 2 | Hook | C# on the C64 | title | 0:20 |
| 3 | What it is | 64 KB at 1 MHz | two-column | 0:35 |
| 4 | What it is | It's ordinary C# | code | 0:40 |
| 5 | What it is | C#, minus generics | two-column | 0:45 |
| 6 | How it works | Roslyn does half | diagram | 0:50 |
| 7 | How it works | IL is a stack | code | 0:50 |
| 8 | How it works | Add costs 39 bytes | diagram | 0:40 |
| 9 | Hard parts | The hard parts | section | 0:10 |
| 10 | Hard parts | int is 8 bits | two-column | 0:50 |
| 11 | Hard parts | Our tests lied | bullets | 0:50 |
| 12 | Hard parts | Frames share bytes | diagram | 0:50 |
| 13 | Hard parts | References: 1 byte | diagram | 0:45 |
| 14 | Hard parts | GC without a scan | diagram (animated) | 0:50 |
| 15 | Hard parts | Borrow, not build | two-column | 0:35 |
| 16 | Hard parts | Hide under the ROM | diagram | 0:45 |
| 17 | Small and fast | Small and fast | section | 0:10 |
| 18 | Small and fast | 39 bytes to 12 | code | 0:50 |
| 19 | Small and fast | Hunchback: -24% | chart | 0:40 |
| 20 | Small and fast | No single winner | chart | 0:40 |
| 21 | Small and fast | Worst frame: -19% | chart | 0:35 |
| 22 | Catacombs | (none) | full-bleed | 0:15 |
| 23 | Catacombs | 3,477 lines of C# | image + split | 0:35 |
| 24 | Catacombs | Every trick is C# | full-bleed + split | 0:35 |
| 25 | Testing | Tests run on 6502 | diagram | 0:50 |
| 26 | Live | Change it live | bullets | 3:00 |
| 27 | Close | Four lessons | bullets | 0:45 |
| 28 | Close | Thank you | title | 0:20 |
| | | | **Total** | **20:10** |

The story: a hook (a real game), what you can and cannot write, how it
works (ending on "39 bytes" for a one-line method), the hard parts with a
setback in the middle (our own tests were wrong), then the payoff (39 bytes
become 12, a game gets a quarter smaller), the showcase, proof (tests on the
target CPU), the live edit, and the lessons.

---

## 1. Hook

### Slide 1 — (no title): a C64 game

- **Layout:** full-bleed image with a raster split: picture in multicolour
  above row 21, a hi-res caption band in rows 21-24.
- **Title:** none.
- **On screen (caption band, centred, one per press):**

```screen
A Commodore 64 game.
Every line of game logic: C#.
```

- **Visual:** asset **A1** (Catacombs room with the skeleton guardian,
  torches lit), rows 0-20. No deck chrome: no title, no footer, no slide
  number.
- **Build:** on entry the picture alone. Press 1: line 1. Press 2: line 2
  (the characters "C#" in the accent colour).
- **Say:** "This is a dungeon crawler running on a 1982 home computer. Every
  line of its game logic is C#, compiled by a compiler that reads .NET IL."
- **Sources:** picture: `tools\catacombs-shot.ps1 -Scene MONSTERROOM`
  (Catacombs/Program.cs). "Game logic is C#": Catacombs/AAA-BRIEF.md
  ("Gameplay state lives in C#; hand-written asm belongs in the generic
  runtime"); the game's `RawAssembly` blocks hold only data tables
  (Catacombs/RoomArt.cs, Catacombs/audio/music.asm), checked at `5216d06`.

### Slide 2 — C# on the C64

- **Layout:** title (hi-res, plus sprites).
- **Title:** `C# on the C64` (13)
- **On screen:**

```screen
Compiling .NET IL to 6502
Balazs Molnar
```

- **Visual:** the existing C# and Commodore logo sprites
  (`C64Presentation/sprites.asm`, as in today's IntroSlide) drifting slowly
  in the left and right margins. Title large and centred on row 9,
  subtitle row 13, name row 16 in the secondary text colour.
- **Build:** on entry everything; the logos keep moving until the press.
- **Say:** "How C# gets onto a C64, what was hard, how it got small and
  fast, and then we change a game live."
- **Sources:** none (title).

## 2. What it is

### Slide 3 — 64 KB at 1 MHz

- **Layout:** two-column, all hi-res: picture left, facts right.
- **Title:** `64 KB at 1 MHz` (14)
- **On screen (right column, cells 20-38, 19 wide):**

```screen
64 KB RAM
~1 MHz 6510 CPU
A, X, Y: 8 bits
256-byte stack
320x200, 16 colours
```

- **Visual:** asset **A5**, a hi-res (two colours per cell) pixel-art
  Commodore 64 breadbox, in the left column (cells 1-18, rows 7-14).
- **Build:** on entry the picture. Presses 1-5: one fact each.
- **Say:** "This is the target: 64 kilobytes, one megahertz, three 8-bit
  registers and a 256-byte hardware stack. Everything that follows is about
  fitting C# into that."
- **Sources:** Commodore 64 Programmer's Reference Guide (memory map, 6510,
  VIC-II modes). PAL clock is 0.985 MHz, hence "~1 MHz".

### Slide 4 — It's ordinary C#

- **Layout:** code (hi-res), a caption line above the code, two text lines
  below.
- **Title:** `It's ordinary C#` (16)
- **On screen (code, 8 lines):**

```code
// Catacombs, Monster.Init()
alive_ = new bool[Count];
hp_ = new uint[Count];
for (uint i = 0; i < Count; i++)
{
    alive_[i] = true;
    hp_[i] = MaxHp;
}
```

- **On screen (text, below the code):**

```screen
A normal project, a normal build
C64Lib: the C64 as a C# API
```

- **Visual:** code in a panel with the theme's code background; the
  comment line in the secondary colour.
- **Build:** on entry the code. Presses 1-2: one text line each.
- **Say:** "This is real Catacombs code: arrays, a loop, fields, nothing
  special. C64Lib gives you the screen, sprites, sound and joystick as
  C# calls; its methods are empty stubs, and the compiler maps them to
  hand-written 6502 routines."
- **Sources:** `Catacombs/Monster.cs`, the body of `Init()` (lines 41-47
  at `5216d06`), verbatim; only the method header and its outer braces are
  left out. C64Lib stubs and the asm helper macros: CLAUDE.md ("What this
  is").

### Slide 5 — C#, minus generics

- **Layout:** two-column (hi-res).
- **Title:** `C#, minus generics` (18)
- **On screen (left, cells 1-18):**

```screen
WORKS
classes, virtuals
arrays, strings
$"x = {x}"
float, Func<T>
GC.Collect()
```

- **On screen (right, cells 21-38):**

```screen
COMPILE ERROR
generics
interfaces
struct, double
try, throw
LINQ, most BCL
```

- **Visual:** "WORKS" heading in light green, "COMPILE ERROR" in light
  red; thin vertical rule between the columns. Below both columns, one
  line in the secondary colour:

```screen
Every error has a code and a hint
```

- **Build:** on entry the headings. Press 1: left column. Press 2: right
  column. Press 3: the bottom line.
- **Say:** "You get classes with virtual methods, arrays, strings, floats
  and a garbage collector. You don't get generics, interfaces, structs or
  exceptions, and the compiler tells you so with an error and a hint,
  never by producing wrong code."
- **Sources:** works: `Compiler/Diagnostics.cs` ("Only your own code,
  C64Lib.*, MathF.Sin/Cos/Sqrt, Console.Write/WriteLine(string),
  GC.Collect(), Func<T>.Invoke(), numeric ToString() and the string basics
  (+, Length, PadLeft) are available"); virtual/abstract methods:
  `Test/VirtualMethodTest.cs`; string interpolation: `Test/StringInterpolationTests.cs`;
  float: `Test/FloatTests.cs`. Errors: generics `Compiler/ILMethodCodePass.cs`
  ("The generic method ... is not supported"); interfaces
  `Compiler/Operands/OperandBase.cs` ("Interfaces are not supported yet");
  struct, double, try/throw `Compiler/Diagnostics.cs` (`Describe`: "A
  struct (user-defined value type)", "double-precision floating point",
  "try/catch/finally", "The 'throw' statement"); LINQ and the rest of the
  class library: C64002 ("a call into the .NET class library ... with no
  C64 implementation"). Codes and hints: CLAUDE.md ("Unsupported C# is a
  compile error").

## 3. How it works

### Slide 6 — Roslyn does half

- **Layout:** diagram (hi-res).
- **Title:** `Roslyn does half` (16)
- **On screen:** only the labels of the diagram. Box labels: `C#`,
  `IL .dll`, `.asm`, `.prg`, `VICE`, `C64`. Arrow labels (small text):
  `dotnet build`, `Compiler.exe`, `64tass`. Caption under `Compiler.exe`:
  `46 passes`.
- **Visual:** row A (cell rows 6-9): box "C#" (cyan), arrow "dotnet
  build", box "IL .dll" (violet), arrow "Compiler.exe" turning down at
  the right edge, with "46 passes" under it in the accent colour. Row B
  (cell rows 13-16), right to left: box ".asm" (light green), arrow
  "64tass", box ".prg" (yellow). Below .prg, two small boxes side by side:
  "VICE" and "C64". Boxes 8 cells wide, 3 high; arrows 1 cell high with
  the label above. Column budget of row A: 8 box + 7 arrow + 8 box + 7
  arrow = 30.
- **Build:** on entry the C# box. Press 1: arrow + IL box (Roslyn's half).
  Press 2: arrow + "46 passes" + .asm box. Press 3: arrow + .prg box.
  Press 4: VICE and C64 boxes.
- **Say:** "Roslyn does the C# half for free. Our compiler only ever sees
  IL: 46 passes read it, model it, rewrite it until nothing changes, and
  write 64tass assembly that becomes a .prg for VICE or the real machine."
- **Sources:** CLAUDE.md ("What this is", "Build & run");
  `tools/compile-and-assemble.bat`. 46 = `new IL...` pass constructions in
  `Compiler/Program.cs` at `5216d06` (grep count; the critic got the same).
  Rewriting to a fixpoint: comment in `Compiler/Program.cs` above
  `ILFieldIncrementOptimizer` ("the optimizer set below runs to a fixpoint
  (see ILCodePass)").

### Slide 7 — IL is a stack

- **Layout:** code (hi-res): C# on top, then IL and macros side by side.
- **Title:** `IL is a stack` (13)
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
  `#init_locals_pull_parameters` is left out for width.

### Slide 8 — Add costs 39 bytes

- **Layout:** diagram (hi-res).
- **Title:** `Add costs 39 bytes` (18)
- **On screen:**

```screen
6502 stack: the evaluation stack
localsStack: arguments and locals
One line of C#: 39 bytes of 6502
```

- **Visual:** two vertical stacks side by side, 10 cells wide, 9 high.
  Left, labelled "$0100 6502 stack": cells, top down, "ret addr", "b",
  "a", with a "pha / pla" label on the arrow into its top. Right, labelled
  "localsStack, 256 B": a frame of cells "ret addr", "a", "b" and a
  pointer arrow from a small box "Y = stackPointer" to the frame. A curved
  arrow from left to right labelled "pla, sta localsStack,y" (the prologue
  moving the return address and the arguments into the frame). The three
  text lines sit under the stacks.
- **Build:** on entry the left stack and line 1. Press 1: the right stack,
  the arrow and line 2. Press 2: line 3 in the accent colour.
- **Say:** "Values being computed live on the hardware stack; arguments and
  locals get their own 256-byte stack indexed by Y. Correct, but our
  one-line Add is 39 bytes plus a shared exit. Remember that number."
- **Sources:** the 64tass listing of snippet S1 with `--no-static-frames`:
  `Program_Add` spans `$17B0-$17D6` (39 bytes), ending `jmp Method_Exit_4`;
  its prologue is `ldy stackPointer` / `pla` / `sta localsStack,y`. Shared
  exit tails: `0e32d14`. 256-byte locals stack: CLAUDE.md,
  `asm/helper/objectTables.asm` header ("256-byte localsStack").

## 4. The hard parts

### Slide 9 — The hard parts

- **Layout:** section (hi-res).
- **Title:** `The hard parts` (14)
- **On screen:**

```screen
8-bit int   stack   objects
GC   strings   floats   bitmaps
```

- **Visual:** section background (the theme's section colour, full
  bleed). A row of six hi-res icons (asset **A6**) above the two text
  lines.
- **Build:** on entry everything.
- **Say:** "Seven things C# takes for granted that a 6502 does not have."
- **Sources:** agenda of slides 10-16.

### Slide 10 — int is 8 bits

- **Layout:** two-column (hi-res): type table on top, code and error below.
- **Title:** `int is 8 bits` (13)
- **On screen (type table):**

```screen
int, uint     8 bits
long, ulong  16 bits
float        5 bytes
double       error
```

- **On screen (code + error):**

```code
for (uint i = 0; i < 700; i++)
```

```screen
error C64006: The constant 700
does not fit in 8 bits ... so it
would become 188.
```

- **Visual:** the error in light red with a thin red bar on its left, like
  an IDE error panel. The literal `700` underlined in the code line.
- **Build:** on entry the type table. Press 1: the code line. Press 2:
  "188" appears beside it in grey ("what it used to do"). Press 3: the
  error panel.
- **Say:** "int is 8 bits, because that is what the CPU has. A loop bound
  of 700 used to become 188 without a word; since October it is a compile
  error with the C# line number."
- **Sources:** sizes: `Compiler/TypeExtensions.cs` `GetStorageBytes()`
  (string/long/ulong 2, float 5, otherwise 1); double:
  `Compiler/Diagnostics.cs` (`Ldc_r8`: "double-precision floating point
  (double)", hint "Use float"). 700 -> 188: commit `4173f55` ("uint is
  8-bit here: 700 silently became 188"). C64006: commit `93f2aa3`
  (2026-10-28); the message is the compiler's own output for the line
  above (compiled for this storyboard), shortened with "...": full text
  "The constant 700 does not fit in 8 bits: int and uint are 8-bit values
  on the C64 (0..255, or -128..127), so it would become 188."

### Slide 11 — Our tests lied

- **Layout:** bullets (hi-res).
- **Title:** `Our tests lied` (14)
- **On screen:**

```screen
The new error found, in our code:
i < 256 loops that ran 0 times
new uint[256]: an empty array
This deck: Wait(500) waited 244
```

- **Visual:** each of lines 2-4 with "256", "256" and "500" struck
  through in grey and the real value ("0", "0", "244") after it in light
  red. Line 1 in the secondary colour.
- **Build:** on entry line 1. Presses 1-3: one line each.
- **Say:** "The day C64006 went in, it failed our own build. Test loops that
  were meant to cover every byte never ran, a test array was empty, and
  this very deck waited 244 jiffies where the code said 500. Green tests
  had been testing nothing."
- **Sources:** commit `93f2aa3` message: "C64Presentation
  KeyBoard.WaitForKeys: Delay.Wait(500) always waited 244"; "i < 256
  loops that ran zero times (DivConstRoutineTests, ShiftConstTests ...)";
  "RandomTests' new uint[256] (an empty array)". Arithmetic: 256 & 255 =
  0; 500 & 255 = 244.

### Slide 12 — Frames share bytes

- **Layout:** diagram (hi-res).
- **Title:** `Frames share bytes` (18)
- **On screen:**

```screen
Deep calls overflow 256 bytes
Fix: frames at fixed addresses
Never-concurrent calls share
Catacombs: 118 of 125 methods
in 39 bytes of frames
```

- **Visual:** left half: a call tree, Main at the top, children Update and
  Draw, Update's child Move, Draw's child Hud (boxes 6 cells wide). Right
  half: an address strip 4 cells wide, 8 high, labelled "StaticFrames",
  with whole-cell coloured blocks showing where each frame lives: Main at
  the top; Update and Draw on the SAME rows (drawn as alternating
  outlines, one colour per cell); Move and Hud again sharing rows below.
  Caption under the tree: "illustration".
- **Build:** on entry line 1 with a red overflow arrow over a full stack
  icon. Press 1: line 2 and the tree. Press 2: line 3 and the address
  strip (frames slide into place). Press 3: lines 4-5 in the accent
  colour.
- **Say:** "A 256-byte locals stack overflows silently on a deep call
  chain. Now every method that can't recurse gets a frame at a fixed
  address, and methods that are never active at the same time share the
  same bytes: Catacombs fits 118 of its 125 methods into 39 bytes."
- **Sources:** silent overflow: Catacombs/AAA-BRIEF.md ("Deep call chains
  overflow the 256-byte locals stack silently"); static frames, "overlaid
  along the call graph", not for recursive, virtual or delegate-reachable
  methods: commit `18bb09d`; instance methods and references: `421df94`.
  118 of 125 and 39 bytes: Catacombs build output at `5216d06` ("Static
  frames: 118 of 125 methods (9 callee-pulls), 39 bytes") and
  Catacombs/TECH.md ("39 bytes of static frames").

### Slide 13 — References: 1 byte

- **Layout:** diagram (hi-res).
- **Title:** `References: 1 byte` (18)
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
  17-20. Nothing overlaps the footer row (today the slide counter
  overwrites the heap).
- **Build:** on entry the table header. Press 1: rows 1, 2, 255. Press 2:
  the heap strip and the two arrows. Press 3: the four text lines
  together.
- **Say:** "A reference is one byte: an index into seven 256-byte tables
  holding each object's address, size, reference fields, root count and
  vtable. So a program can have 255 live objects, and moving an object
  only changes one table entry."
- **Sources:** 1-byte reference: `Compiler/TypeExtensions.cs`
  (`GetStorageBytes()` returns 1 for classes); tables:
  `asm/helper/objectTables.asm` ("The 7x256-byte object/GC tables";
  `objTableSize`, `objTableReferences`, `objTableRootCount`, ...); handle
  0 = null: `asm/GC.asm` (`trackObjects`: "mark NULL"); faults "out of
  memory" and "too many objects": commit `3f3d760` and
  `asm/helper/fault.asm` (header: "?<MESSAGE> ERROR", red border;
  `Runtime_FaultMsg0 .text "OUT OF MEMORY"`).

### Slide 14 — GC without a scan

- **Layout:** diagram (hi-res, animated: the existing GarbageCollectorSlide
  logic on the new theme).
- **Title:** `GC without a scan` (17)
- **On screen (right side, labels that light up per phase):**

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
  step (as today); the phase label lights up as its phase starts.
- **Say:** "Every store of a reference into a local, a parameter or a
  static field updates a count, so the roots are known without scanning
  the stack. References between objects are traced; then the live objects
  are sorted by address and slid down. It runs only when you call
  GC.Collect()."
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

### Slide 15 — Borrow, not build

- **Layout:** two-column (hi-res).
- **Title:** `Borrow, not build` (17)
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
- **Say:** "A string is a pointer to zero-terminated bytes, and
  concatenation writes into one small buffer. A float is the C64's own
  5-byte format, so the BASIC ROM that is in every C64 does the maths."
- **Sources:** strings: `asm/helper/stringops.asm` header ("raw
  null-terminated character bytes", "stringops_buffer's fixed 40-byte
  size"); not reference counted: `Compiler/TypeExtensions.cs`
  (`IsReferenceCounted`: string -> false). Floats:
  `Compiler/TypeExtensions.cs` ("Native C64 MFLPT format", 5 bytes);
  BASIC ROM routines: `asm/helper/floatBanking.asm`; Sin/Cos/Sqrt:
  `Compiler/Diagnostics.cs` ("MathF.Sin/Cos/Sqrt").

### Slide 16 — Hide under the ROM

- **Layout:** diagram (hi-res): memory map left, text right.
- **Title:** `Hide under the ROM` (18)
- **On screen (right column, cells 20-38):**

```screen
Bitmap under the
KERNAL ROM
Sprites under I/O
Frees 9.4 KB
Pictures packed:
8,400 -> 2,118 B
```

- **Visual:** a vertical memory bar, 8 cells wide, rows 4-21, $0000 at the
  bottom, $FFFF at the top, address labels left of it (cells 1-6, bar in
  cells 8-15), and "not to scale" in small grey text under it. Six
  segments, each a whole number of cell rows, one colour each:
  `$0000` zero page, 6502 stack (grey, 1 row); `$0800` runtime tables
  (grey, 1 row); `$1000-$CBFF` program + heap (blue, 8 rows); `$CC00`
  screen colours (cyan, 1 row); `$D000` I/O, "64 sprite frames" under it
  (violet, 2 rows); `$E000-$FF3F` bitmap, "under ROM" (yellow, 4 rows).
- **Build:** on entry the bar without the top two segments. Press 1:
  `$E000` bitmap + text lines 1-2. Press 2: `$D000` sprites + line 3.
  Press 3: line 4 (accent). Press 4: lines 5-6.
- **Say:** "Hi-res graphics need 8 KB of bitmap, so it goes into the RAM
  hidden under the KERNAL ROM, and the sprite shapes go under the I/O
  chips. That frees 9.4 KB for code; pictures are stored compressed."
- **Sources:** Catacombs/TECH.md: memory map table; "High layout itself
  ... about 110; frees 9.4 KB"; "the tech demo's dithered 40x21 backdrop
  is 8400 -> 2118 bytes"; CLAUDE.md ("Graphics memory layout"). $0000-$07FF
  zero page and 6502 stack ($0100): C64 Programmer's Reference Guide.

## 5. Making it small and fast

### Slide 17 — Small and fast

- **Layout:** section (hi-res).
- **Title:** `Small and fast` (14)
- **On screen:**

```screen
Pay only for what you use
```

- **Visual:** section background. Under the line, small and grey:
  `.weak Flag_X` (the asm mechanism that links a routine only when used).
- **Build:** on entry everything.
- **Say:** "One rule for every optimization and feature: a program that does
  not use it pays nothing. We check that on every change by measuring
  Hunchback, another full game."
- **Sources:** CLAUDE.md ("Pay only for what you use", "`.weak Flag_X`",
  "an unrelated change must leave its segment sizes identical").

### Slide 18 — 39 bytes to 12

- **Layout:** code (hi-res): a mini bar chart, the code, three lines.
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

- **Visual:** a two-bar mini chart above the code. Column budget: label 7
  ("before ", "after  ") | bar 26 | value 5 (" 39 B", " 12 B") = 38. Scale
  1.5 bytes per cell: before = 26 cells (grey), after = 8 cells (accent).
- **Build:** on entry the before bar. Press 1: the after bar and the code.
  Presses 2-4: one text line each.
- **Say:** "Same Add, today's compiler. The arguments sit at fixed
  addresses, the caller writes them with plain loads and stores, and the
  result never touches the stack: six instructions."
- **Prepared answer** ("I'd write that in 8 bytes"): "Yes: `lda a / clc /
  adc b / rts` is 8 bytes with absolute addresses. We spend 4 more copying
  b to zero page, and the caller does the two stores either way. 12 is
  close to hand-written; 39 was not."
- **Sources:** snippet S1 compiled with today's defaults at `5216d06`:
  `Program.asm` and the 64tass listing (`Program_Add` at `$17B0-$17BB`:
  `ad 15 18 / 85 32 / ad 16 18 / 18 / 65 32 / 60`, 12 bytes). `SF_TOP-511`
  is `Ldarg_0` (a), `SF_TOP-512` is `Ldarg_1` (b), per the compiler's
  `; OPT` comments. Call sites: `lda score / sta SF_TOP-511 / lda #5 / sta
  SF_TOP-512 / jsr Program_Add / sta score`. 39 bytes: slide 8.
  Mechanisms: `18bb09d` (static frames, result in A/X), `421df94` (direct
  call-site copies), `21e84ec` (top of stack in A). Hand-written size:
  6502 opcode sizes (lda/adc absolute 3 bytes, clc and rts 1 byte each).

### Slide 19 — Hunchback: -24%

- **Layout:** chart (hi-res): two horizontal bars and a caption line.
- **Title:** `Hunchback: -24%` (15)
- **On screen:**

```screen
Same game, same C#: -8,690 B
```

- **Visual:** column budget: label 7 ("Oct 17 ", "Oct 31 ") | bar 22 |
  value 9 (" 36,847 B", " 28,157 B") = 38. Scale: 22 cells = 36,847 B, so
  Oct 31 = 17 cells (16.8 rounded). Bars on cell rows 7-8 (Oct 17, grey)
  and 10-11 (Oct 31, accent). Cells 18-22 of the Oct 31 row (the
  difference) outlined with a dashed border, "-8,690 B" centred under
  them on row 12. The caption line on row 15.
- **Build:** on entry the "Oct 17" bar. Press 1: the "Oct 31" bar grows
  from zero. Press 2: the dashed gap, its label and the caption.
- **Say:** "Hunchback's source didn't change in these two weeks; only the
  compiler did. The same game got a quarter smaller, and its code segment
  a third."
- **Sources:** measured (see "Measurements"): Hunchback program bytes
  (sum of the `Data:` segments `Hunchback/measure.bat` prints) at
  `1e1a44e` (2026-10-17, just before `0e32d14`) = 13 + 3,247 + 5,760 +
  27,827 = 36,847; at `5216d06` (2026-10-31) = 13 + 3,479 + 5,760 + 18,905
  = 28,157; -23.6%. The main code segment alone: 27,827 -> 18,905 (-32%,
  speaker only). Hunchback's source is unchanged since then: `git log
  --since=2026-10-10 -- Hunchback/` shows only `0726949` (a .csproj
  build-order line).

### Slide 20 — No single winner

- **Layout:** chart (hi-res): horizontal bars, two rows per optimization.
- **Title:** `No single winner` (16)
- **On screen (row labels):**

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

- **Visual:** column budget: label 14 | bar 18 | value 6 = 38. Each
  optimization takes two cell rows: Hunchback's bar (blue) on the first,
  Catacombs' bar (orange) on the second; the label sits on the first row.
  Scale: 18 cells = 3,420 B (190 B per cell, rounded to whole cells; a
  value under 95 B gets no bar, only its number). 8 x 2 = 16 rows (rows
  4-19), a legend "Hunchback / Catacombs, bytes saved" on row 3, and the
  footnote `each at its own commit` in grey on row 21.

  | Row | Hunchback | cells | Catacombs | cells | Source |
  |---|---|---|---|---|---|
  | Object arrays | 3,357 | 18 | 0 | 0 | `7235df6` |
  | Static frames | 2,304 | 12 | 1,818 | 10 | measured |
  | Exit tails | 1,983 | 10 | 2,579 | 14 | `0e32d14` |
  | Top of stack | 824 | 4 | 1,168 | 6 | `21e84ec` |
  | Static arrays | 0 | 0 | 616 | 3 | `f140b7c` |
  | Skip Y reload | 311 | 2 | 495 | 3 | `93ab351` |
  | ++ on statics | 28 | 0 | ~340 | 2 | `465b9d1` |
  | Logic + const | 85 | 0 | 326 | 2 | `34a08e9` |

- **Build:** on entry the labels and legend. Press 1: all Hunchback bars
  grow together. Press 2: all Catacombs bars.
- **Say:** "Which optimization pays depends on the program: Hunchback
  builds object arrays, so the array table wins there; Catacombs indexes
  static arrays, so that wins there. The structural changes, frames and
  exit tails, pay everywhere. That's why we measure on two real games."
  (The one-line mechanisms, if asked: exit tails turn each 17-byte
  epilogue into a 3-byte jmp; skip Y reload saves 2 bytes per access; top
  of stack in A drops pha/pla pairs, 400 -> 2 in Hunchback; logic + const
  makes `x & c` 4 bytes instead of 10; object arrays become one table and
  one call.)
- **Sources:** each figure is the commit message of the listed commit
  (`465b9d1`: "Catacombs shrinks by about 340 bytes, Hunchback's main
  segment by 28"; `f140b7c`: "Hunchback unchanged"; `7235df6`: "Catacombs
  and Demo unchanged", Hunchback "net -3357 B"). Static frames: measured at
  `5216d06`, main segment with `C64_STATIC_FRAMES=0` vs default: Hunchback
  21,209 -> 18,905, Catacombs 39,383 -> 37,565. The figures were taken on
  different bases, so they do not add up to the merge totals of `bbdf2f5`
  (Hunchback -6,396 B, Catacombs -3,312 B). Mechanisms: `0e32d14` ("a
  17-byte epilogue ... now a 3-byte jmp"), `93ab351` ("2 bytes and 3
  cycles each"), `21e84ec` ("Boundary pairs: Hunchback 400 -> 2"),
  `34a08e9` ("4 bytes instead of 10"), `7235df6` ("one #newObjArrayInit
  call").

### Slide 21 — Worst frame: -19%

- **Layout:** chart (hi-res): bars against a full-frame outline.
- **Title:** `Worst frame: -19%` (17)
- **On screen:**

```screen
Catacombs, raster lines per frame
-49 lines
```

- **Visual:** column budget: label 7 ("before ", "after  ") | bar 26 |
  value 5 (" 257", " 208") = 38. The bar area is an outline 26 cells wide
  = 311 raster lines (one PAL frame as the commit counts it), ticked every
  50 lines. Filled bars, whole cells (12 lines per cell): before = 21
  cells (257 lines, grey), after = 17 cells (208 lines, accent). Cells
  18-21 of the "after" row outlined dashed with "-49 lines" under them.
  The empty part of the outline is labelled "free for the game".
- **Build:** on entry the outline and the "before" bar. Press 1: the
  "after" bar. Press 2: the dashed gap and "-49 lines".
- **Say:** "Smaller code is faster code here. Catacombs' worst frame went
  from 257 to 208 of the frame's 311 raster lines: 49 lines, almost a
  fifth, handed back to the game."
- **Sources:** merge commit `bbdf2f5`: "VICE worst frame 257 -> 208 of 311
  lines". -49 = 257 - 208; -19% = 49 / 257 = 19.1%. (The same commit's
  "frame work -21%" is a different measure, the profiler's average frame
  work; it is not on the slide.) Speaker extras: `21e84ec` top of stack in
  A "9404 -> 8491 cycles/frame (-9.7%)"; `4aae54e` division by a constant
  "About 40% fewer cycles than Divide16Core".

## 6. Catacombs

### Slide 22 — (no title): Catacombs

- **Layout:** full-bleed image (multicolour, no split, no caption).
- **Title:** none; the word "Catacombs" is the picture's own logo.
- **On screen:** nothing but the picture.
- **Visual:** asset **A2** (the Catacombs title screen: logo, skull wall,
  the gate with the hero silhouette), rows 1-18, black around it.
- **Build:** on entry the picture.
- **Say:** "Catacombs is the showcase. We set it the bar of the best modern
  C64 releases and allowed only generic compiler and library changes to
  get there."
- **Sources:** picture: `tools\catacombs-shot.ps1` with no `-Scene` (the
  real game from power-on: the title). Bar and rules:
  Catacombs/AAA-BRIEF.md ("The bar", "Technical rules"); merge `c9db78e`.

### Slide 23 — 3,477 lines of C#

- **Layout:** image + split: picture in multicolour on rows 0-14, raster
  split at row 15, title and numbers hi-res on rows 16-24.
- **Title:** `3,477 lines of C#` (17), in the title font on rows 16-17.
- **On screen (rows 19-23, 8x8 font):**

```screen
61 rooms        4 guardians
5 themes        3 SID voices
10,478 bytes still free
```

- **Visual:** asset **A3** (the "Bone Warden" encounter: horned skull,
  torch pillars, name banner), rows 0-14. Row 15 (the first line below the
  split) empty and black, as TECH.md requires. Numbers in the accent
  colour, words in primary text.
- **Build:** on entry the picture and the title. Press 1: line 1. Press 2:
  line 2. Press 3: line 3.
- **Say:** "Three and a half thousand lines of C#: sixty-one rooms, four
  guardians, a three-voice soundtrack, and ten kilobytes still free."
- **Sources:** 3,477: `wc -l Catacombs/*.cs` at `5216d06`. 61 rooms: floor
  cells in `Catacombs/Maze.cs` `Grid` (12x12, nonzero = floor), counted.
  4 guardians: `Catacombs/Monster.cs` ("skeleton warrior, ghoul, wraith
  ... and the giant spider"). 5 themes: `Catacombs/RoomArt.cs`
  (`ThemeCount = 5`). 3 voices: Catacombs/audio/README.md ("Voice 1 is
  bass and percussion ..., voice 2 the lead, voice 3 chords"). 10,478:
  Catacombs/TECH.md. Picture: `-Scene COMBAT`, a shot about 1.5 s after
  start. Split rule: Catacombs/TECH.md ("Keep the first pixel line of the
  lower part ... empty").

### Slide 24 — Every trick is C#

- **Layout:** full-bleed + split: asset A1 again in multicolour on rows
  0-20, split at row 21, the title and three lines in the hi-res band.
- **Title:** `Every trick is C#` (17), in the 8x8 font on row 21 (the band
  has no room for the large font), accent colour.
- **On screen (rows 22-24):**

```screen
This split: room MC, text hi-res
Themes: 16-byte colour remaps
Music on the raster interrupt
```

- **Visual:** the whole of A1 (no crop; the same picture as slide 1, no new
  memory). A short arrow in the left margin of row 21 pointing at the
  split line.
- **Build:** on entry the picture and the title. Presses 1-3: one line
  each.
- **Say:** "You have been looking at one of these tricks since slide one:
  the room is multicolour, the text under it hi-res, switched halfway
  down the screen. Room themes are a 16-byte colour lookup, and the music
  player hangs off the same interrupt. All of it is a C# call."
- **Sources:** Catacombs/TECH.md ("Raster split", `SetSplit(21,
  ScreenMode.Bitmap)`; `RemapCellColors` "through a 16-byte table"; "While
  a split is on, the system interrupt runs once per video frame ... That
  includes a C64.Interrupt subscriber (music)"); themes:
  `Catacombs/RoomArt.cs` (`pal_Theme1..4`).

## 7. Testing

### Slide 25 — Tests run on 6502

- **Layout:** diagram (hi-res): code on top, loop diagram below.
- **Title:** `Tests run on 6502` (17)
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
- **Say:** "The test body never runs as .NET: a PostSharp aspect intercepts
  the call, runs the compiled 6502 routine in our own emulator, and hands
  the result back to NUnit. Every green test passed on the target CPU, and
  after slide 11 we also check that it tests what it says."
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

### Slide 26 — Change it live

- **Layout:** bullets (hi-res checklist), shown while the presenter works
  on the PC.
- **Title:** `Change it live` (14)
- **On screen:**

```screen
1 Play: one hit = half a heart
2 MonsterDamage = 4 -> 20
3 Rebuild: one hit = a heart
4 Breakpoint: hover playerHp
```

- **Visual:** dark "demo" variant of the theme, a blinking cursor block
  after the last visible line. Each item gets a tick when done.
- **Build:** on entry the title. Presses 1-4: one item each (the presenter
  switches to the PC between presses).
- **Run (one program, Catacombs, throughout; rehearse it):**
  1. `Catacombs\run.bat`: walk into a guarded room, take a hit (100 HP,
     10 per half heart, 4 per hit: the bar barely moves).
  2. In `Catacombs/Monster.cs` change `public const uint MonsterDamage =
     4;` to `20`.
  3. `Catacombs\run.bat` again: same room, one hit now takes a whole
     heart.
  4. In VS Code put a breakpoint on `Catacombs/Encounter.cs` line 106
     (`playerHp = playerHp <= Monster.MonsterDamage ? ...`), run the "C64
     on VICE: Debug Program" command with that file open, take a hit,
     hover `playerHp`, step over (F10).
  Fallback: the debugger documents Demo, Hunchback and C64Presentation as
  targets; Catacombs' debug build compiles and assembles below `$CC00`
  (checked) but has not been debugged live. If it fails in rehearsal, do
  step 4 on this deck instead (break in `Presenter.cs`) and keep it to
  30 seconds.
- **Say:** "Everything you saw is a normal edit-build-run loop: change a
  constant in C#, rebuild, and the C64 plays differently; set a
  breakpoint, and VICE stops on the C# line."
- **Sources:** `Catacombs/Monster.cs` (`MonsterDamage = 4`),
  `Catacombs/Program.cs` (`PlayerMaxHp = 100`), `Catacombs/Hud.cs`
  (`SetHealth`: ten half hearts, a threshold every 10 HP),
  `Catacombs/Encounter.cs:106`. Debugger: commits `98c2604` ("run a program
  on VICE from VS Code and stop at breakpoints"), `950600a` (object/array
  inspection), `vscode-extension/README.md` ("Debugging a whole program on
  VICE": "the project is taken from the open file's folder"). Catacombs
  debug build: `Compiler.exe ... program debug` + 64tass at `5216d06`, last
  segment `$1BF6-$B6FA`.

## 9. Close

### Slide 27 — Four lessons

- **Layout:** bullets (hi-res).
- **Title:** `Four lessons` (12)
- **On screen:**

```screen
Structure beats peepholes
Measure on real programs
Make silent bugs loud
Play the game, too
```

- **Visual:** each line with a small accent bar on the left; nothing else.
- **Build:** on entry the title. Presses 1-4: one line each.
- **Say (one sourced line each):**
  1. "Static frames saved 2,304 bytes in Hunchback; unrolled shifts saved
     41."
  2. "The object-array table saved 3,357 bytes in one game and nothing in
     the other."
  3. "One new error message found tests that had never tested anything."
  4. "A branch bug that our tests missed was found because Catacombs' health
     bar hung after a killing blow; the tests came after."
- **Sources:** 1: measured static frames (slide 20) vs `4173f55`
  ("Catacombs -81 bytes, Hunchback -41"). 2: `7235df6` (Hunchback net
  -3,357 B; "Catacombs and Demo unchanged"). 3: `93f2aa3` (slide 11). 4:
  `eefca0b` ("Catacombs' encounter health bar hung after the killing
  blow"; "Test.Runtime/GreaterBranchEqualityTests ... (7 failed before the
  fix)").

### Slide 28 — Thank you

- **Layout:** title (hi-res, plus sprites).
- **Title:** `Thank you` (9)
- **On screen:**

```screen
This deck is C# too: TBD B free
github.com/balazsmolnar/
C64-IL-Compiler
```

- **Visual:** the two logo sprites from slide 2 return and settle side by
  side above the title.
- **Build:** on entry the title and the URL. Press 1: line 1 (accent).
- **Say:** "Every slide you have seen was C#, compiled by this compiler and
  running on a C64, with this much memory to spare. Thank you."
- **TBD at build:** fill in the deck's free bytes from its final build,
  measured like Catacombs/TECH.md "Free memory": run
  `C64Presentation\measure.bat`, take the end of the last `Data:` line,
  free = $CC00 - end - 1 - 1,080 (tables, locals stack and string buffers
  with `DrawText`; 56 without) - the static frames ("Static frames: ...,
  N bytes") - the heap the deck needs - 64 per sprite frame beyond 64.
  Round down to a whole number, write it with a thousands comma; the line
  stays at most 36 characters.
- **Sources:** this project (`C64Presentation/`); `git remote -v`;
  Catacombs/TECH.md (free-memory formula).

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
- **Snippet S1 (slides 7, 8, 18).** A C64Lib program:

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
- **Catacombs debug build (slide 26).** `Compiler.exe
  Catacombs\bin\Debug\catacombs.dll asm\catacombs_debug
  asm\catacombs_debug.asm program debug`, then 64tass: last segment
  `$1BF6-$B6FA`, below `$CC00`.

---

## Asset list (for the Art builder)

Budget: 18 KB of compressed images in total. Everything not listed here is
drawn by the engine (diagrams D1-D8 and charts C1-C4 below cost code, not
image memory). Sizes in 8x8 cells; byte estimates use Catacombs' measured
assets (`Catacombs/art/build`: `room.img` 3,239 B for a 40x21 room,
`title.img` 2,475 B). Multicolour images are only ever shown above a raster
split or full-screen; everything that shares rows with text is hi-res.

### Images

| ID | What | Size (cells) | Mode | Est. | Slides | How to get it |
|---|---|---|---|---|---|---|
| A1 | Catacombs room with the skeleton guardian and lit torches, viewport only (no HUD) | 40x21 | multicolour (above the split at row 21) | 3.4 KB | 1, 24 | `tools\catacombs-shot.ps1 -Scene MONSTERROOM`, crop rows 0-20, convert with `tools\c64img.py` |
| A2 | Catacombs title screen: logo, skull wall, gate with the hero silhouette (no "PRESS FIRE" text) | 40x18 (rows 1-18) | multicolour (full screen, no text) | 2.5 KB | 22 | `tools\catacombs-shot.ps1` with no `-Scene` |
| A3 | The "Bone Warden" encounter: horned skull, torch pillars, name banner (no health bar, no HUD) | 40x15 (rows 0-14 of the shot) | multicolour (above the split at row 15) | 2.1 KB | 23 | `tools\catacombs-shot.ps1 -Scene COMBAT -Shots 2 -IntervalMs 1500`, second shot, crop rows 0-14 |
| A5 | Pixel-art Commodore 64 breadbox (keyboard, badge), three-quarter view | 18x8 | hi-res (two colours per cell) | 0.5 KB | 3 | drawn by script in `C64Presentation/art/` |
| A6 | Six icons on one sheet: "8" chip, stack of plates, object box, recycling arrows, quote marks, picture frame | 6 icons x 3x3 | hi-res | 0.4 KB | 9 | drawn by script |
| -- | C# and Commodore logos | sprites | -- | 0 (reuse) | 2, 28 | existing `C64Presentation/sprites.asm` |

Total: about 8.9 KB, leaving room in the 18 KB budget for any diagram the
engine cannot draw (fallback below).

### Engine-drawn diagrams and charts (all hi-res)

If one of these cannot be drawn by the engine, the fallback is a hi-res
image of the same size (line art compresses well: about 0.3-0.8 KB each).

| ID | Slide | Size (cells) | Content |
|---|---|---|---|
| D1 | 6 Roslyn does half | 38x12 | 5 boxes in a snake + 2 small target boxes, labelled arrows, "46 passes" |
| D2 | 8 Add costs 39 bytes | 30x11 | two vertical stacks, the Y pointer, the "pla, sta" arrow |
| D3 | 11 Our tests lied | 36x4 | strike-through on three numbers, replacement values |
| D4 | 12 Frames share bytes | 38x10 | call tree + "StaticFrames" address strip with shared rows |
| D5 | 13 References: 1 byte | 38x12 | object table (4 rows x 6 columns) + heap strip + 2 arrows |
| D6 | 14 GC without a scan | 38x18 | the existing animated GC scene, plus the compaction step |
| D7 | 16 Hide under the ROM | 16x18 | vertical memory map, 6 segments, "not to scale" |
| D8 | 25 Tests run on 6502 | 38x7 | 4-box loop with arrows and a tick |
| C1 | 18 39 bytes to 12 | 38x2 | 2 bars: 26 and 8 cells |
| C2 | 19 Hunchback: -24% | 38x6 | 2 bars: 22 and 17 cells, dashed gap |
| C3 | 20 No single winner | 38x19 | 8 items x 2 bars (cells in the slide's table), legend, footnote |
| C4 | 21 Worst frame: -19% | 38x6 | 26-cell outline (311 lines), bars of 21 and 17 cells |
