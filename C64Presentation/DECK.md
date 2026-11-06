# C64Presentation: the storyboard

The talk, slide by slide: what is on screen, what appears on each press, what
to say, and where every fact comes from. The Slides phase builds from this
file; the Art phase builds the asset list at the end. Owner: Story (see
`DECK-BRIEF.md`).

**Talk:** "C# on the C64", 28 slides, about 16.5 minutes of slides plus a
3-minute live demo: about 19:35, leaving over 5 minutes of slack in a
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
| 1 | Hook | C# on the Commodore 64 (picture) | full-screen picture | 0:30 |
| 2 | What it is | 64 KB at 1 MHz | two-column | 0:30 |
| 3 | What it is | It's ordinary C# | code | 0:40 |
| 4 | What it is | What compiles | two-column | 0:40 |
| 5 | How it works | Roslyn does half | diagram | 0:45 |
| 6 | How it works | IL is a stack | code | 0:45 |
| 7 | How it works | Add costs 39 bytes | diagram | 0:35 |
| 8 | Hard parts | The hard parts | section | 0:10 |
| 9 | Hard parts | int is 8 bits | two-column | 0:45 |
| 10 | Hard parts | Our tests lied | bullets | 0:45 |
| 11 | Hard parts | Frames share bytes | diagram | 0:45 |
| 12 | Hard parts | References: 1 byte | diagram | 0:40 |
| 13 | Hard parts | GC without a scan | diagram (animated) | 0:45 |
| 14 | Hard parts | Borrow, not build | two-column | 0:30 |
| 15 | Hard parts | Hide under the ROM | diagram | 0:40 |
| 16 | Small and fast | Small and fast | section | 0:10 |
| 17 | Small and fast | 39 bytes to 12 | code | 0:45 |
| 18 | Small and fast | Hunchback: -24% | chart | 0:35 |
| 19 | Small and fast | No single winner | chart | 0:35 |
| 20 | Small and fast | Worst frame: -19% | chart | 0:30 |
| 21 | Built with this | Built with this | full-bleed + split | 0:35 |
| 22 | Testing and debugging | Tests run on 6502 | code | 0:40 |
| 23 | Testing and debugging | PostSharp swaps it | diagram | 0:50 |
| 24 | Testing and debugging | Debug C# on a C64 | diagram (mock) | 0:50 |
| 25 | Testing and debugging | Line to address | diagram | 0:40 |
| 26 | Live | Change it live | bullets | 3:00 |
| 27 | Close | Four lessons | bullets | 0:40 |
| 28 | Close | Thank you | title | 0:20 |
| | | | **Total** | **19:35** |

The story: a hook (this slide is a C# program), what you can and cannot
write, how it works (ending on "39 bytes" for a one-line method), the hard
parts with a setback in the middle (our own tests were wrong), the payoff
(39 bytes become 12, a game gets a quarter smaller), one showcase (what was
built with it), then the toolchain: tests that run on an emulated 6502,
breakpoints in C# on VICE, a live edit-build-run-debug loop, and the
lessons. Catacombs appears once as the showcase; its numbers also serve
the optimization charts.

---

## 1. Hook

### Slide 1 — C# on the Commodore 64

- **Layout:** title, full-screen hi-res picture (no deck chrome).
- **Title:** part of the picture: "C# on the" / "Commodore 64".
- **On screen:** the picture only, then a two-line caption in the 8x8 font
  in the empty area right of the breadbox (cells 21-38, rows 20-21; the
  picture is black there, checked on `art/src/title.c64.png`):

```screen
This slide is
a C# program.
```

- **Visual:** the extra asset **title** (`art/build/title.img`, 40x25,
  hi-res, 1,715 B): "C# on the" in cyan, "Commodore 64", a cyan rule and
  the subtitle, the breadbox (A5) bottom left and a dark-grey card with two
  lines of syntax-coloured C#. It must move out of the `#if
  SCENE_ART || ART_EXTRAS` block in `Assets.cs`.
- **Build:** on entry the picture. Press 1: the caption, typed out one
  character at a time like a C64 `PRINT`.
- **Say:** "Before anything else: what you are looking at is not
  PowerPoint. This slide is a C# program, compiled to 6502 machine code and
  running on a Commodore 64. The rest of the talk is how."
- **Sources:** this deck (`C64Presentation/`, C#); the picture:
  `C64Presentation/art/ART.md` ("Extras": `title.img`, 40x25, hires,
  1715 bytes).

## 2. What it is

### Slide 2 — 64 KB at 1 MHz

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

### Slide 3 — It's ordinary C#

- **Layout:** code (hi-res), two text lines below.
- **Title:** `It's ordinary C#` (16)
- **On screen (code, 8 lines):**

```code
class Star
{
  public uint X, Dx;
  public void Move() => X += Dx;
}
Star[] sky = [new() { Dx = 1 },
              new() { Dx = 2 }];
foreach (var s in sky) s.Move();
```

- **On screen (text, below the code):**

```screen
Classes, collection expressions
C64Lib: the C64 as a C# API
```

- **Visual:** code in a panel with the theme's code background; a thin
  rule between the class (lines 1-5) and the three statements (lines 6-8),
  with a small grey label `in Main` beside line 6.
- **Build:** on entry the class. Press 1: lines 6-8. Presses 2-3: one text
  line each.
- **Say:** "A field list, an expression-bodied method, a collection
  expression with target-typed new, foreach. Normal, current C#. C64Lib
  gives you the screen, sprites, sound and joystick as C# calls; its
  methods are empty stubs that the compiler maps to 6502 routines."
- **Sources:** compiled and run for this storyboard (snippet S2 under
  "Measurements"): built with `dotnet build`, compiled by this repo's
  compiler, assembled by 64tass and run in VICE, where it prints `2`
  (`sky[1].X`). It was verified with the field named `Speed`; on the slide
  it is `Dx` and indented by 2, only to fit 34 columns. C64Lib stubs:
  CLAUDE.md ("What this is"). The statements
  must sit in a `Main` method: top-level statements compile to a method
  named `<Main>$`, which the program entry does not find (tried: 64tass
  "not defined symbol 'Program_Main'").

### Slide 4 — What compiles

- **Layout:** two-column (hi-res).
- **Title:** `What compiles` (13)
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

### Slide 5 — Roslyn does half

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

### Slide 6 — IL is a stack

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

### Slide 7 — Add costs 39 bytes

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

### Slide 8 — The hard parts

- **Layout:** section (hi-res).
- **Title:** `The hard parts` (14)
- **On screen:**

```screen
8-bit int   stack   objects
GC   strings & floats   bitmaps
```

- **Visual:** section background (the theme's section colour, full
  bleed). A row of six hi-res icons (asset **A6**) above the two text
  lines.
- **Build:** on entry everything.
- **Say:** "Six things C# takes for granted that a 6502 does not have."
- **Sources:** agenda of slides 9-15.

### Slide 9 — int is 8 bits

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

### Slide 10 — Our tests lied

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
  this very deck waited 244 where the code said 500. Green tests
  had been testing nothing."
- **Sources:** commit `93f2aa3` message: "C64Presentation
  KeyBoard.WaitForKeys: Delay.Wait(500) always waited 244"; "i < 256
  loops that ran zero times (DivConstRoutineTests, ShiftConstTests ...)";
  "RandomTests' new uint[256] (an empty array)". Arithmetic: 256 & 255 =
  0; 500 & 255 = 244.

### Slide 11 — Frames share bytes

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

### Slide 12 — References: 1 byte

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

### Slide 13 — GC without a scan

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

### Slide 14 — Borrow, not build

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

### Slide 15 — Hide under the ROM

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
  (grey, 1 row); `$1000-$CBFF` program + heap (blue, 9 rows); `$CC00`
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

### Slide 16 — Small and fast

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

### Slide 17 — 39 bytes to 12

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
  SF_TOP-512 / jsr Program_Add / sta score`. 39 bytes: slide 7.
  Mechanisms: `18bb09d` (static frames, result in A/X), `421df94` (direct
  call-site copies), `21e84ec` (top of stack in A). Hand-written size:
  6502 opcode sizes (lda/adc absolute 3 bytes, clc and rts 1 byte each).

### Slide 18 — Hunchback: -24%

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

### Slide 19 — No single winner

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

### Slide 20 — Worst frame: -19%

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
  lines". -49 = 257 - 208; -19% = 49 / 257 = 19.1%. The deck uses only
  this VICE worst-frame figure. Speaker extras: `21e84ec` top of stack in
  A "9404 -> 8491 cycles/frame (-9.7%)"; `4aae54e` division by a constant
  "About 40% fewer cycles than Divide16Core".

## 6. Built with this

### Slide 21 — Built with this

- **Layout:** full-bleed + split: asset A1 in multicolour on rows 0-20,
  split at row 21, a hi-res band below. **No deck chrome** (no footer,
  slide number or progress bar). Row 21 stays empty and black (the first
  line under a split must be blank); the title and two lines use rows
  22-24.
- **Title:** `Built with this` (15), in the 8x8 font on row 22 (the band
  has no room for the large font), accent colour.
- **On screen (rows 23-24):**

```screen
Catacombs: 3,477 lines of C#
61 rooms, 10,478 bytes still free
```

- **Visual:** the whole of A1 (Catacombs room with the skeleton guardian,
  torches lit). A short arrow in the left margin of row 22 pointing up at
  the split line.
- **Build:** on entry the picture and the title. Presses 1-2: one line
  each.
- **Say:** "The biggest thing built with it so far: Catacombs, a dungeon
  crawler. Every line of game logic is C#, and this very screen shows one
  of its tricks: the room is multicolour, the text below it hi-res,
  switched by a raster interrupt, which is one C# call."
- **Sources:** 3,477: `wc -l Catacombs/*.cs` at `5216d06`. 61 rooms: floor
  cells in `Catacombs/Maze.cs` `Grid` (12x12, nonzero = floor), counted.
  10,478: Catacombs/TECH.md ("10,478 bytes ($A312-$CBFF) are left for more
  code, assets and the heap"). Game logic is C#: Catacombs/AAA-BRIEF.md
  ("Gameplay state lives in C#"); its `RawAssembly` blocks hold only data
  tables, checked at `5216d06`. The split: Catacombs/TECH.md ("Raster
  split", `SetSplit(21, ScreenMode.Bitmap)`, "Keep the first pixel line
  of the lower part ... empty"). Picture: `art/ART.md` (A1 from
  `MONSTERROOM_12.png`).

## 7. Testing and debugging

### Slide 22 — Tests run on 6502

- **Layout:** code (hi-res), three text lines below.
- **Title:** `Tests run on 6502` (17)
- **On screen (code, 7 lines):**

```code
[Test]
public void Set_Get_Field()
{
  var sut = new Test();
  sut.field1 = 5;
  Assert.AreEqual(sut.field1, 5);
}
```

- **On screen (text):**

```screen
Assert = C64TestFramework.Assert
NUnit finds it, runs it, reports it
The body never runs as .NET
```

- **Visual:** code panel; the `Assert` in the code and the first text line
  in the accent colour (the alias is the trick). A small NUnit-style green
  tick beside `[Test]` on the last press.
- **Build:** on entry the code. Presses 1-3: one text line each; the tick
  with the third.
- **Say:** "This is an ordinary NUnit test in Visual Studio's test
  explorer. But Assert is our own stand-in, and the method body is
  compiled to 6502 with the rest of the test project. When NUnit calls the
  method, the call is intercepted."
- **Sources:** `Test/ObjectTest.cs` `Set_Get_Field` (lines 41-49 at
  `5216d06`), reindented to 2 spaces, and its two `field2` lines left out.
  `using Assert = C64TestFramework.Assert;`: line 2 of the same file.
  `C64TestFramework/Assert.cs` (`AreEqual(int actual, int expected, ...)`
  with an empty body). CLAUDE.md (`C64TestFramework`, `Test/`: "the test
  method body is actually compiled to 6502 and executed on the emulator,
  not run as normal C#").

### Slide 23 — PostSharp swaps it

- **Layout:** diagram (hi-res): a numbered flow down the left, a strip of
  memory addresses on the right.
- **Title:** `PostSharp swaps it` (18)
- **On screen:**

```screen
1 Load prg/unittest.prg
2 Method address -> $09FE
3 Arguments -> $09E0
4 Run from $1000
5 $20: 0 pass, $FF fail, $FE fault
6 Message at $2E/$2F, value at $2C
~730 test cases, all on 6502
```

- **Visual:** left (cells 1-25): the six steps as a vertical flow with
  small down-arrows; a violet box around steps 1-4 labelled
  `RunInEmulatorAspect.OnInvoke` and a green box around steps 5-6
  labelled `CopyResultFromEmulator`. Right (cells 28-38): a narrow memory
  strip, top to bottom `$0020`, `$002C`, `$002E`, `$09E0`, `$09FE`,
  `$1000`, each joined by a thin line to its step. Line 7 at the bottom in
  the accent colour.
- **Build:** on entry steps 1-2. Press 1: steps 3-4. Press 2: steps 5-6.
  Press 3: line 7.
- **Say:** "PostSharp weaves an aspect into every test method at build
  time. It loads the one compiled test program into SimpleEmulator, our
  own 6502 emulator, writes the method's address and arguments into
  memory, runs it, and reads back one result byte: zero passes, otherwise
  the message pointer becomes the NUnit failure. Every green test passed
  on an emulated 6502."
- **Sources:** `C64TestFramework/RunInEmulatorAspect.cs` at `5216d06`:
  `OnInvoke` (InitEmulator, CopyMethodArgumentsToEmulator,
  `emulator.Start(0x1000)`, CopyResultFromEmulator); `InitEmulator`
  (`SetMemory(0x09fe, GetMethodAddress(...))`, `LoadPrg(... ".prg")`);
  `CopyMethodArgumentsToEmulator` (`ARGUMENTS_ADDRESS = 0x9e0`, count byte
  first); `CopyResultFromEmulator` (`GetMemory(0x20)`, `FAULT_RESULT =
  0xFE` with the code in `$2E`, a nonzero result fails with
  `GetMessage`, `RETURN_VALUE_ADDRESS = 0x2c`). `$FF` and the message
  pointer at `$2E/$2F`: the comment in the same method ("Distinct from
  Assert_Fail's $FF, whose $2E/$2F is a message pointer") and CLAUDE.md.
  Assembly-wide weaving: `Test/assembly.cs` (`[assembly:
  RunInEmulatorAspect(AttributeTargetTypes = "Compiler.Test.*", ...)]`);
  PostSharp at build time: CLAUDE.md ("Running the full test suite").
  ~730: `[Test]`/`[TestCase]` attributes counted at `5216d06`: 595 in
  `Test/`, 134 in `Test.Runtime/`.

### Slide 24 — Debug C# on a C64

- **Layout:** diagram (hi-res): a mock of the VS Code debugger, drawn by
  the engine, feature list on the right.
- **Title:** `Debug C# on a C64` (17)
- **On screen (right column, cells 20-38):**

```screen
Breakpoints on
C# lines
F10, F11, Shift+F11
Locals, fields,
arrays, hover
Edit a value
Registers and
6502 disassembly
```

- **Visual:** left (cells 1-18, rows 4-21): a VS Code mock in the theme's
  greys. An editor panel (rows 4-13) with four real lines of
  `Demo/RotatingCube.cs` in the 8x8 font (each at most 18 columns):
  `public void Step()`, `{`, `  BuildBasis();`, `  float bcx = cx_;`.
  A red dot in the gutter and a yellow line highlight on
  `float bcx = cx_;`. Below it a Variables panel (rows 15-21): `> this`,
  `  cx_`, `  velX_`, `  cosAy_`, values shown as grey dots (no invented
  numbers). A small breadbox-blue square in the corner labelled `VICE`
  with a thin line to the highlighted row (the stop).
- **Build:** on entry the editor. Press 1: the red dot. Press 2: the
  yellow stop line and the `VICE` link. Press 3: the Variables panel.
  Presses 4-7: the feature lines in pairs.
- **Say:** "And when it goes wrong, you debug it as C#. Set a breakpoint in
  VS Code, press F5: the program is built, started in VICE, and stops on
  your line. Step over, into and out, look at locals and object fields,
  even change a value and continue. Only the innermost method's locals are
  shown; that's the current limit."
- **Sources:** `vscode-extension/README.md` at `5216d06`: "What works"
  (gutter breakpoints, "Step over (F10), step into (F11), step out
  (Shift+F11)", Variables with locals, parameters and `this`'s fields,
  expandable objects/arrays, "Editing a value ... in either test mode or a
  live VICE session", "Registers and disassembly", "Watch panel, Debug
  Console, and hover tooltips"), "Debugging a whole program on VICE"
  ("build a C64 program (`Demo`, `Hunchback` or `C64Presentation`), run it
  on a real VICE and stop at breakpoints in its C# source"; "Limits:
  locals are only resolved for the innermost method"). Launch configs:
  `.vscode/launch.json` ("Debug Demo on VICE", "Debug Hunchback on VICE",
  type `c64vice`). The code lines: `Demo/RotatingCube.cs` lines 126, 127, 128 and 136
  (the comment lines between them left out).
  Commits: `57d05fe` (step-debugger for unit tests, CLI + VS Code DAP),
  `950600a` (object/array inspection, registers, disassembly), `a3ade3f`
  (watch, hover), `98c2604` (run a program on VICE from VS Code and stop
  at breakpoints), `ac88e7f` (step-over/step-out, editable locals).

### Slide 25 — Line to address

- **Layout:** diagram (hi-res): a five-step pipeline, top to bottom.
- **Title:** `Line to address` (15)
- **On screen:**

```screen
PDB: C# line -> IL offset
Compiler: a label per line
debugmap.txt: line -> label
64tass .labels: label -> address
VICE monitor: break there
Debug build: bigger, no fusion
```

- **Visual:** five stacked boxes (cells 1-30, two rows each), joined by
  down-arrows; the box colours follow the pipeline slide (violet for IL,
  cyan for the compiler, light green for asm/64tass, yellow for VICE).
  Line 6 in grey below the boxes, as a footnote.
- **Build:** on entry box 1. Presses 1-4: one box each. Press 5: line 6.
- **Say:** "Nothing magic: the compiler reads the PDB's sequence points,
  puts a label on the first instruction of every C# line, and writes a
  map. 64tass turns labels into addresses, and the debugger sets VICE
  breakpoints over its binary monitor. A debug build keeps those labels,
  so it skips the optimizations that would merge lines, and it's bigger."
- **Sources:** `Compiler/ILMethodDebugLabelPass.cs` (comment: "Reads the
  portable PDB ... for each non-hidden sequence point, gives the matching
  ILOperation ... a label"; runs only when `EmitDebugInfo` is set);
  `Compiler/ILDebugMapPass.cs` (writes `debugmap.txt`, format `LINE
  <methodLabel> <label> <sourceFile> <line>` and `LOCAL ...`; "resolved to
  an address afterward by reading the .labels file 64tass produces");
  `Compiler/DebugMap.cs` (`DebugSequencePoint`, `DebugLocal`);
  `TestDebugger/ViceMonitorClient.cs` ("Client for VICE's binary remote
  monitor (x64sc -binarymonitor ...)"); `TestDebugger/ProgramCompiler.cs`
  (a debug copy into `asm/<name>_debug`). Debug builds bigger: static
  frames off in debug builds (`18bb09d`: "debug builds keep dynamic
  frames"), labels block top-of-stack pairing (`21e84ec`: "Labels block
  pairing, so with EmitDebugInfo every step point still has the value on
  the stack"); measured: Catacombs' debug build ends at `$B6FA` against
  `$AEB2` for the normal build (2,120 bytes more).

## 8. Live

### Slide 26 — Change it live

- **Layout:** bullets (hi-res checklist), shown while the presenter works
  on the PC.
- **Title:** `Change it live` (14)
- **On screen:**

```screen
1 Run: a cube on light blue
2 Edit one line, build, run
3 Breakpoint in Step(), F5
4 Set velX_ to 0, continue
```

- **Visual:** dark "demo" variant of the theme, a blinking cursor block
  after the last visible line. Each item gets a tick when done.
- **Build:** on entry the title. Presses 1-4: one item each (the presenter
  switches to the PC between presses).
- **Program: Demo** (the rotating cube). Why this one: the demo is about
  the toolchain, not a game. Demo is small (611 lines of C#, so the code
  on screen is readable), it is a documented debugger target with a ready
  launch config ("Debug Demo on VICE"), a one-line edit changes the whole
  screen, and its float fields make a good thing to inspect and edit.
  Catacombs is not a documented debugger target, and its walk to a
  guardian costs stage time.
- **Run:**
  1. `dotnet build Demo\Demo.csproj`, then `Demo\run.bat`: the cube spins
     and drifts on a light-blue background.
  2. In `Demo/Program.cs` change `SetBackgroundColor(Colors.LightBlue)` to
     `SetBackgroundColor(Colors.Black)`; `dotnet build Demo\Demo.csproj`;
     `Demo\run.bat`: the same cube on black. (`run.bat` alone does not
     rebuild the C#: it only compiles the existing `bin\debug\demo.dll`,
     and Demo.csproj has no post-build step.)
  3. In VS Code open `Demo/RotatingCube.cs`, set a breakpoint on line 136
     (`float bcx = cx_;`), F5 with "Debug Demo on VICE": VICE starts and
     stops there. Show `this` in Variables, hover `cx_`, F10 once.
  4. Double-click `velX_` (4) in Variables, set it to 0, F5: the cube
     stops drifting sideways and only bounces up and down.
- **Time budget (3:00):** run 0:30, edit + build + run 0:40, debugger
  start and stop 1:00, edit a value 0:40, spare 0:10. Measured with this
  exact edit at `5216d06`: `dotnet build Demo\Demo.csproj` 2.5 s, the
  compile-to-6502 + 64tass step (`Demo\measure.bat`, the step `run.bat`
  runs before VICE) 0.5 s; the generated code then pushes colour 0
  (black) before `jsr Screen_SetBackgroundColor`.
- **Must-pass rehearsal checks:**
  1. "After the edit and rebuild, the background is black." If not, the
     C# wasn't rebuilt.
  2. "The breakpoint at `RotatingCube.cs:136` is hit on VICE, and setting
     `velX_` to 0 stops the sideways drift." If it fails, cut steps 3-4
     and item 3-4 from the slide; never switch programs on stage.
- **Say:** "Edit, build, run, debug: the loop you already know. Change one
  line of C#, the C64 changes; stop on a C# line, change a field, and the
  cube obeys."
- **Sources:** `Demo/Program.cs` (`SetBackgroundColor(Colors.LightBlue)`);
  `Demo/RotatingCube.cs` (line 136 `float bcx = cx_;`; `velX_ = 4f` line
  104; `centerX_ = centerX_ + velX_;` line 184); `wc -l Demo/*.cs` = 611;
  `Demo/Demo.csproj` (no post-build target); `.vscode/launch.json` ("Debug
  Demo on VICE"); `vscode-extension/README.md` (editing a float in a live
  VICE session; "Set a breakpoint in e.g. `Demo/Program.cs` or
  `Demo/RotatingCube.cs`").

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
  (Kept as its own slide on purpose: the talk still ends on slide 28's
  number, not on these bullets.)
- **Build:** on entry the title. Presses 1-4: one line each.
- **Say (one sourced line each):**
  1. "Static frames saved 2,304 bytes in Hunchback; unrolled shifts saved
     41."
  2. "The object-array table saved 3,357 bytes in one game and nothing in
     the other."
  3. "One new error message found tests that had never tested anything."
  4. "A branch bug that our tests missed was found because Catacombs' health
     bar hung after a killing blow; the tests came after."
- **Sources:** 1: measured static frames (slide 19) vs `4173f55`
  ("Catacombs -81 bytes, Hunchback -41"). 2: `7235df6` (Hunchback net
  -3,357 B; "Catacombs and Demo unchanged"). 3: `93f2aa3` (slide 10). 4:
  `eefca0b` ("Catacombs' encounter health bar hung after the killing
  blow"; "Test.Runtime/GreaterBranchEqualityTests ... (7 failed before the
  fix)").

### Slide 28 — Thank you

- **Layout:** title (hi-res, plus sprites).
- **Title:** `Thank you` (9)
- **On screen:** the number line in the large title font (at most 18
  characters, e.g. `N,NNN BYTES FREE`, in capitals like the C64's own
  boot screen), the rest in the 8x8 font:

```screen
This deck is C#, too.
N,NNN BYTES FREE
github.com/balazsmolnar/
C64-IL-Compiler
```

- **Visual:** the C# and Commodore logo sprites (`sprites.asm`) settle
  side by side above the title. "N,NNN BYTES FREE" centred in the large font in
  the accent colour, rows 12-13; "This deck is C#, too." on row 10; the
  URL small on rows 18-19.
- **Build:** on entry the title and the URL. Press 1: "This deck is C#,
  too." Press 2: the number, counting up from 0 to its value in about a
  second.
- **Say:** "Every slide you have seen was C#, compiled by this compiler and
  running on a C64. Every C64 greets you with 38911 BASIC BYTES FREE;
  this deck leaves this many. Thank you."
- **Required output of the Slides phase:** the real number, measured on
  the final build and put on this slide; the deck is not done while the
  slide says N,NNN. It is the free memory **before the heap**: the
  Catacombs/TECH.md "Free memory" formula with no heap reserve, so it is
  not a judgement call. Run `C64Presentation\measure.bat`, take the end
  of the last `Data:` line, then free = $CC00 - that end - 1,080 (object
  tables, locals stack and string buffers with `DrawText`; 56 without) -
  the static frames (the compiler's "Static frames: ..., N bytes") - 64
  per sprite frame beyond 64. Write it with a thousands comma. Report it
  with the commit it was measured on. Slide 21's Catacombs figure (10,478)
  is the same quantity ("left for more code, assets and the heap").
- **Sources:** this project (`C64Presentation/`); `git remote -v`;
  Catacombs/TECH.md (free-memory formula). "38911 BASIC BYTES FREE": the
  C64's power-on screen (Commodore 64 Programmer's Reference Guide).

---

## Measurements

Figures marked "measured" were taken for this storyboard; reproduce them
like this (all paths literal, scratch under `.debug-tmp\`).

- **Hunchback before/after (slide 18).** HEAD `5216d06`: build
  `Hunchback\Hunchback.csproj`, run `Hunchback\measure.bat`, add the
  `Data:` lengths: 13 + 3,479 + 5,760 + 18,905 = 28,157. Before: `git
  archive 1e1a44e` into `.debug-tmp\old`, build its `Compiler` and
  `Hunchback`, run its `Hunchback\measure.bat`: 13 + 3,247 + 5,760 +
  27,827 = 36,847. Loaded program end `$A35A` -> `$8080`.
- **Static frames on/off (slide 19).** At `5216d06`, `C64_STATIC_FRAMES=0`
  then default: Hunchback main segment 21,209 -> 18,905 (-2,304);
  Catacombs main segment 39,383 -> 37,565 (-1,818).
- **Snippet S1 (slides 6, 7, 17).** A C64Lib program:

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
- **Snippet S2 (slide 3).** A C64Lib project (`LangVersion` latest):

  ```
  class Star
  {
      public uint X, Speed;
      public void Move() => X += Speed;
  }
  class Program
  {
      static void Main()
      {
          Star[] sky = [new() { Speed = 1 },
                        new() { Speed = 2 }];
          foreach (var s in sky) s.Move();
          C64.Screen.Write(0, 0, sky[1].X.ToString());
          for (;;) { }
      }
  }
  ```

  Built with the compiler at `70da6e5` (presentation-modern), compiled,
  assembled (`$1000-$18C3`) and run in VICE (`tools\vice-launch.ps1
  -KeepOthers`, then `tools\vice-capture.ps1`): `2` in the top-left
  corner. The same statements as top-level statements fail to link ("not
  defined symbol 'Program_Main'").
- **C64006 text (slide 9).** The same compiler on `for (uint i = 0; i <
  700; i++) n++;` prints `error C64006: The constant 700 does not fit in 8
  bits: int and uint are 8-bit values on the C64 (0..255, or -128..127), so
  it would become 188.`
- **Debug build size (slide 25).** `Compiler.exe
  Catacombs\bin\Debug\catacombs.dll asm\catacombs_debug
  asm\catacombs_debug.asm program debug`, then 64tass: last segment
  `$1BF6-$B6FA`, against `$1BF6-$AEB2` for the normal build.
- **Live demo rebuild (slide 26).** With the background edit in
  `Demo/Program.cs`: `dotnet build Demo\Demo.csproj` 2.5 s,
  `Demo\measure.bat` 0.5 s (compiler already built); the generated code
  then has `#stack_push_int8 0` before `jsr Screen_SetBackgroundColor`.
  The edit was reverted afterwards.

---

## Asset list (for the Art builder)

Budget: 18 KB of compressed images in total. Everything not listed here is
drawn by the engine (diagrams and charts below cost code, not image
memory). Sizes in 8x8 cells; bytes are the real compressed sizes from
`art/ART.md`. Multicolour images are only ever shown above a raster split
or full-screen; everything that shares rows with text is hi-res.

### Images still needed

| ID | What | Cells | Mode | Bytes | Slides | Status |
|---|---|---|---|---|---|---|
| title | Full-screen title: "C# on the Commodore 64", rule, subtitle, breadbox, C# code card | 40x25 | hi-res | 1,715 | 1 | built (extra): move its line out of the `#if` in `Assets.cs` |
| A1 | Catacombs room with the skeleton guardian, lit torches, no HUD | 40x21 | multicolour, split at 21 | 3,755 | 21 | built |
| A5 | Commodore 64 breadbox | 18x8 | hi-res | 377 | 2 | built |
| A6 | Six icons: chip "8", stack, object, GC arrows, `"A"`, picture frame | 6 x 3x3 | hi-res, pixels only | 407 | 8 | built |
| -- | C# and Commodore logos | sprites | -- | 0 (reuse) | 28 | existing `sprites.asm` |

Total: **6,254 bytes** of the 18 KB budget.

### No longer needed (can be undeclared)

| ID | What | Bytes saved | Why |
|---|---|---|---|
| A2 | Catacombs title screen | 2,748 | the full-screen Catacombs slide is gone |
| A3 | "Bone Warden" encounter | 2,570 | the Catacombs numbers are folded into slide 21 |

The other built extras stay optional. The pipeline cards would suit slide
5 if the engine's own boxes look plainer (`ArtScene.Pipeline(top)`, 40x14
cells, 1,260 bytes for the six cards, the compiler card, the arrow and the
elbow).

### Engine-drawn diagrams and charts (all hi-res)

If one of these cannot be drawn by the engine, the fallback is a hi-res
image of the same size (line art compresses well: about 0.3-0.8 KB each).

| ID | Slide | Size (cells) | Content |
|---|---|---|---|
| D1 | 5 Roslyn does half | 38x12 | 5 boxes in a snake + 2 small target boxes, labelled arrows, "46 passes" |
| D2 | 7 Add costs 39 bytes | 30x11 | two vertical stacks, the Y pointer, the "pla, sta" arrow |
| D3 | 10 Our tests lied | 36x4 | strike-through on three numbers, replacement values |
| D4 | 11 Frames share bytes | 38x10 | call tree + "StaticFrames" address strip with shared rows |
| D5 | 12 References: 1 byte | 38x12 | object table (4 rows x 6 columns) + heap strip + 2 arrows |
| D6 | 13 GC without a scan | 38x18 | the existing animated GC scene, plus the compaction step |
| D7 | 15 Hide under the ROM | 16x18 | vertical memory map, 6 segments, "not to scale" |
| D8 | 23 PostSharp swaps it | 38x16 | six-step flow, two group boxes, address strip |
| D9 | 24 Debug C# on a C64 | 18x18 | VS Code mock: editor with breakpoint dot and stop line, Variables panel, `VICE` tag |
| D10 | 25 Line to address | 30x12 | five stacked boxes with down-arrows |
| C1 | 17 39 bytes to 12 | 38x2 | 2 bars: 26 and 8 cells |
| C2 | 18 Hunchback: -24% | 38x6 | 2 bars: 22 and 17 cells, dashed gap |
| C3 | 19 No single winner | 38x19 | 8 items x 2 bars (cells in the slide's table), legend, footnote |
| C4 | 20 Worst frame: -19% | 38x6 | 26-cell outline (311 lines), bars of 21 and 17 cells |
