# "C# on the Commodore 64": the story of the talk

About ten minutes, 16 slides, for .NET developers and retro fans. The deck is
itself a C# program running on a C64, and the talk keeps coming back to that.
Keys: SPACE steps a build, N jumps to the next slide, B goes back one.
Per-slide detail (exact text, builds, sources) is in `DECK.md`.

## The arc

**Hook, then what it is, then how it works, then proof it is real, then a
callback.** The audience first learns that the slides are the demo. They then
learn what C# can and cannot do on this machine, and how the compiler gets
there, in a story that follows one line of code, `a + b`, from 43 bytes down to
12. Catacombs shows it works at scale. A garbage collector on a 6502 is the
hard part. Tests and a debugger show it is real engineering. The closing slide
answers the opening one.

## Act 1: the hook (slide 1, 0:30)

**C# on the Commodore 64.** The cover is a picture: the title, a breadbox
C64 and a speech bubble of C# (`C64.Screen.Clear(); Hud.SetHealth(100);`). A
caption types itself out like a BASIC `PRINT`: *"This slide is a C# program."*

The first thing said is that this is not PowerPoint. What you are looking at
was compiled from C# to 6502 machine code and is running on a Commodore 64. The
rest of the talk is how.

## Act 2: what it is (slides 2-4, 1:55)

**2. 64 KB at 1 MHz.** The target, one fact per press: 64 KB RAM, a 1 MHz CPU,
8-bit registers, a 256-byte stack, 320x200 in 16 colours. Everything that
follows is fitting C# into that.

**3. It's ordinary C#.** A primary constructor, a property with a private
setter, an expression-bodied method, a collection expression, `foreach`. It is
current C#, not a dialect. C64Lib exposes the screen, sprites, sound and
joystick as C# calls, which the compiler maps to 6502 routines.

**4. What compiles.** WORKS: classes and virtuals, arrays, strings,
interpolation, floats, `Func<T>`, `GC.Collect()`. COMPILE ERROR: generics,
interfaces, structs, `double`, `try`/`throw`, LINQ. The compiler tells you with
a code and a hint; it never produces wrong code.

## Act 3: how it works (slides 5-7, 2:05)

**5. Roslyn does half.** A diagram builds across the slide: C# to IL by
`dotnet build` (Roslyn's half, violet), then 46 compiler passes of ours (cyan)
down to 6502 assembly, 64tass and a `.prg` that runs in VICE or on the real
machine.

**6. IL is a stack.** `Add(a, b) => a + b` becomes four IL instructions, and
each becomes one 6502 macro. The naive translation works because the 6502 has a
stack too.

**7. Add costs 43 bytes.** Two stacks side by side: the hardware stack for
values being computed, and the 256-byte locals stack for arguments and locals,
indexed by Y. Correct, but one line of C# costs 43 bytes. The talk ends up
spending several slides on that number.

## Act 4: built with this (slide 8, 0:35)

**8. Built with this.** Catacombs, a dungeon crawler written in C#: 3,477
lines, 61 rooms, 10,478 bytes still free. The screen is alive: torches
flicker, an adventurer walks across the room, a skeleton guardian paces in the
doorway and a bat flaps by. The room is multicolour, the text below it is
hi-res, and a raster interrupt switches between them, which is one C# call.

## Act 5: the hard part (slides 9-10, 0:55)

**9. The hard parts.** A section slide: *a garbage collector, on a 6502.*

**10. GC without a scan.** An animation, one phase per press:
1. Roots are counted, because every store of a reference updates a count.
2. Mark: follow references.
3. Sort by address, `1 3 4 5` becoming `3 1 4 5`.
4. Slide down: a dead cycle, `2 -> 6 -> 2`, is dimmed, then the live blocks
   slide left to close the gaps.

It runs only when you call `GC.Collect()`.

## Act 6: small and fast (slides 11-12, 1:20)

**11. Small and fast.** One rule: a program that doesn't use a feature pays
nothing for it. Two numbers: **43 to 12 bytes** for `Add`, and **-24%** for
Hunchback, a whole game, with its C# unchanged.

**12. 43 bytes to 12.** The payoff of slide 7. The same `Add` with compiler
passes switched on one at a time, all numbers measured:

| Step | Bytes | Saved | What changed |
|---|---|---|---|
| One macro per IL | 43 | | `#locals_push_value8, #add8` |
| + Static frames | 18 | -25 | a and b get fixed addresses; no prologue |
| + Stack top in A | 14 | -4 | two `pha`/`pla` pairs gone |
| + Operand fusion | 12 | -2 | operands load straight where the add wants them |

Prepared answer for "I'd write that in 8 bytes": yes, `lda a / clc / adc b /
rts` is 8. We spend 4 more copying b to zero page. 12 is close to hand-written;
43 was not.

## Act 7: proof (slides 13-15, 2:15)

**13. Tests run on 6502.** An ordinary NUnit test, `[TestCase(5,
ExpectedResult = 6)]`. Its body is compiled to 6502 with everything else: the 5
goes into emulated memory at `$09E0` and the 6 comes back at `$2C`. It never
runs as .NET.

**14. PostSharp swaps it.** The six-step flow, as a timeline. An aspect woven
in at build time loads the compiled test program into our own 6502 emulator,
writes the method address and arguments, runs it and reads back one result
byte. Over 700 test cases, all on 6502.

**15. Debug C# on a C64.** A mock of the VS Code debugger: set a breakpoint on
a C# line, press F5, and the program starts in VICE and stops there. Step,
inspect locals and fields, edit a value, see registers and disassembly.

## Act 8: the close (slide 16, 0:25)

**16. Thank you.** The C# and Commodore logos settle in. Then the callback:
*"Remember slide one, 'this slide is a C# program'? So were the other
fifteen."* A C64 boots with 38911 BASIC BYTES FREE; after all 16 slides this
deck still has **1727 BYTES FREE**, counted up on screen. The talk ends on that
number.

## Running order

| # | Slide | Time |
|---|---|---|
| 1 | C# on the Commodore 64 | 0:30 |
| 2 | 64 KB at 1 MHz | 0:30 |
| 3 | It's ordinary C# | 0:45 |
| 4 | What compiles | 0:40 |
| 5 | Roslyn does half | 0:45 |
| 6 | IL is a stack | 0:45 |
| 7 | Add costs 43 bytes | 0:35 |
| 8 | Built with this (live) | 0:35 |
| 9 | The hard parts | 0:10 |
| 10 | GC without a scan | 0:45 |
| 11 | Small and fast | 0:30 |
| 12 | 43 bytes to 12 | 0:50 |
| 13 | Tests run on 6502 | 0:40 |
| 14 | PostSharp swaps it | 0:50 |
| 15 | Debug C# on a C64 | 0:45 |
| 16 | Thank you | 0:25 |

## Threads that run through the talk

- **The deck is the demo.** Slide 1 says it, and slide 16 pays it off.
- **One line, followed all the way.** `a + b` appears on slides 6 and 7 as 43
  bytes, is previewed on slide 11, and is explained on slide 12.
- **Honest limits.** Slide 4 shows what does not compile, and slide 15 admits
  that only the innermost method's locals are shown.
- **Measured, not claimed.** Every number (46 passes, 43 to 12, -24%, over 700
  tests, 1727 bytes free) was measured from this repo; `DECK.md` lists where.
- **No live demo.** If time allows, the presenter can run `Demo` in VICE after
  slide 16, during questions.

## Notes for the presenter

- The deck is on the `.d64` disk: slide 8's picture loads while slide 1 is up,
  so keys are briefly ignored on a real 1541.
- One SPACE too many on slide 16 ends the program and blanks the screen.
- Cut slides are kept in `DECK.md` under "Cut slides" if you want one back,
  e.g. "Borrow, not build" (strings and floats).
