# "C# on the Commodore 64": a casual talk script

A sample of how to give the talk: about ten minutes, spoken, relaxed. Say it
your own way; it's here to show the flow and where the jokes and pauses fit.
`[SPACE]` means press SPACE (a build step). `[N]` jumps ahead and `[B]` goes
back if someone asks a question. The facts behind every line are in `DECK.md`.

---

## 1. C# on the Commodore 64 (0:30)

*(The cover is up. Let it sit for a second.)*

Hi everyone. Quick question before I start: how many of you have opened a
PowerPoint today?

*(Beat.)*

Well, this isn't one.

`[SPACE]` *(the caption types itself out: "This slide is a C# program.")*

Yep. This slide is a C# program. I wrote it in C#, compiled it to 6502 machine
code, and it's running on a Commodore 64 right now. The next ten minutes are
about how that's even possible.

## 2. 64 KB at 1 MHz (0:30)

So, the machine. Let me remind you what we're working with.

`[SPACE]` 64 kilobytes of RAM.
`[SPACE]` A one-megahertz CPU.
`[SPACE]` `[SPACE]` Registers that are 8 bits wide, so `A`, `X` and `Y` can
count to 255 and then they're done.
`[SPACE]` A stack of 256 bytes. Not kilobytes. Bytes.
`[SPACE]` And a screen of 320 by 200, in 16 colours.

`[SPACE]` Everything you're about to see is me trying to squeeze C# into that.

## 3. It's ordinary C# (0:45)

Now you'd expect me to say "well, it's C#-*ish*". It isn't. Look.

*(The class is on screen.)* A primary constructor. An auto-property with a
private setter. An expression-bodied method.

`[SPACE]` And down here: a collection expression with `new(1)` target-typed,
and a `foreach`. That's current, normal C#. I didn't have to learn a dialect.

`[SPACE]` Primary constructors, properties, collection expressions. All fine.
`[SPACE]` And to talk to the machine there's a small library, C64Lib: screen,
sprites, sound, joystick, all as plain C# calls. Under the hood those are
stubs, and the compiler swaps them for hand-written 6502 routines.

## 4. What compiles (0:40)

Of course there are limits. Let's be honest about them.

`[SPACE]` On the left, what works: classes with virtual methods, arrays,
strings, string interpolation, floats, `Func<T>`, and you can even call
`GC.Collect()`.

`[SPACE]` On the right, what doesn't: generics, interfaces, structs, double,
try and throw, and no LINQ. Sorry.

`[SPACE]` The important bit: when you hit one of those, you don't get a weird
crash on the C64. You get a compile error with a code and a hint. It never
silently produces wrong code. I care about that a lot.

## 5. Roslyn does half (0:45)

How does it work? Roslyn does half the job for free.

*(The C# box is there.)* `[SPACE]` `dotnet build` turns your C# into IL. That's
the violet half, and it's Microsoft's.

`[SPACE]` From there it's ours. Our compiler only ever sees IL. 46 passes read
it, rewrite it again and again until nothing changes any more, and then write
64tass assembly.

`[SPACE]` 64tass turns that into a `.prg`...
`[SPACE]` ...which runs in VICE, the emulator, or on a real C64. I've done both.

## 6. IL is a stack (0:45)

Let's follow one tiny method all the way through. `Add(a, b)` returns a plus b.

`[SPACE]` This is its IL. IL is a stack machine: push a, push b, add, return.

`[SPACE]` `[SPACE]` `[SPACE]` `[SPACE]` And the dumbest possible translation
is one 6502 macro per IL instruction. *(Press once for each arrow.)* It works,
because the 6502 has a stack too.

`[SPACE]` One macro per IL instruction. Easy. Hold that thought.

## 7. Add costs 43 bytes (0:35)

Here's what that looks like in memory. There are two stacks. The hardware
stack on the left holds the values being computed. And on the right a second,
256-byte stack that I keep for arguments and locals, indexed by Y.

`[SPACE]` The arguments get copied across, so a function call has real
overhead.

`[SPACE]` And the result: that one-line `Add` costs 43 bytes. Forty-three bytes
to add two numbers. On a machine with 64 K that adds up fast. Remember that
number. I'm going to come back to it.

## 8. Built with this (0:35)

So is this just a toy? No. This is Catacombs, a dungeon crawler I built
entirely in C#. 3,477 lines, 61 rooms.

*(Torches flicker, the adventurer walks, the skeleton paces, a bat flaps.)*
All that movement is hardware sprites, each moved from C# code.

`[SPACE]` Here's my favourite detail: the room is multicolour, but the text
below it is high-resolution. A raster interrupt switches between the two
modes partway down the screen, and that's a single C# call.

`[SPACE]` And it still has 10,478 bytes free. So the compiler isn't wasting
them.

## 9. The hard parts (0:10)

Right. The hard part.

*(Section slide. Just say it and move on.)* C# takes one thing for granted that
a 6502 definitely doesn't have: a garbage collector.

## 10. GC without a scan (0:45)

So I wrote one. I'll go through it in four steps.

*(Objects on screen.)* `[SPACE]` Step one, roots. Every time a reference is
stored into a local, a parameter or a static, a counter is updated. So I always
know the roots without scanning the stack. Here, 1 and 3.

`[SPACE]` Step two, mark. Follow the references from the roots. Everything
reachable lights up.

`[SPACE]` Step three, sort the live objects by address.

`[SPACE]` And step four, slide them down to close the gaps. See 2 and 6?
They point at each other, but nothing points at them. A dead cycle, and it's
gone. Everything else slides left.

It only runs when you call `GC.Collect()`, so you decide when to pay for it.

## 11. Small and fast (0:30)

Now, back to those 43 bytes. The rule I stuck to: if your program doesn't use
a feature, it pays nothing for it.

`[SPACE]` And inside that rule, the compiler got a lot tighter. That one-line
`Add` went from 43 bytes to 12.

`[SPACE]` And Hunchback, a whole other game, got 24% smaller. Same C#, I
didn't touch the source. Let me show you how.

## 12. 43 bytes to 12 (0:50)

Here's `Add` again, with the compiler passes turned on one at a time. Every
number here is measured, by turning the passes off.

*(The 43 B bar is on screen.)* One macro per IL instruction: 43 bytes.

`[SPACE]` First, static frames. `a` and `b` get fixed addresses, so there's no
prologue copying them onto the locals stack. 18 bytes. That's 25 saved.

`[SPACE]` Next, top of stack in A. A value stays in the accumulator from where
it's made to where it's used, so two push-pull pairs just vanish. 14.

`[SPACE]` And last, operand fusion, which is the newest. Each operand is loaded
straight to where the add wants it. 12 bytes. Six instructions.

*(If someone says "I'd do that in 8 bytes":)* You're right, I would too. The
difference is four bytes to copy b to zero page. 12 is close to hand-written.
43 definitely wasn't.

And since each of those passes can break your program, we'd better test them.

## 13. Tests run on 6502 (0:40)

Here's a normal NUnit test. Five goes in, six should come out.

`[SPACE]` NUnit finds it, runs it, reports it.
`[SPACE]` The 5 goes into emulated memory at `$09E0`.
`[SPACE]` And the 6 comes back from `$2C`.
`[SPACE]` And here's the twist: the body of that test never runs as .NET. It's
compiled to 6502, just like the rest.

## 14. PostSharp swaps it (0:50)

How does that work? PostSharp. It weaves an aspect into every test method at
build time, so when NUnit calls the test, the call gets intercepted.

`[SPACE]` The aspect loads the compiled test program into my own 6502 emulator,
writes the method's address and arguments into memory, and runs it.

`[SPACE]` Then it reads back one result byte. Zero is a pass. Anything else and
the message pointer becomes your NUnit failure.

`[SPACE]` Over 700 test cases. Every green test passed on an emulated 6502.

## 15. Debug C# on a C64 (0:45)

And when something does go wrong, you debug it as C#.

`[SPACE]` You set a breakpoint in VS Code.
`[SPACE]` Press F5, and it builds, starts in VICE, and stops on your line.
`[SPACE]` *(The stop line lights up.)* Step over, into, out. Look at locals and
fields. Even change a value and carry on.
`[SPACE]` `[SPACE]` `[SPACE]` `[SPACE]` Registers, 6502 disassembly, hover
tooltips. One honest limit: you only see locals of the innermost method for
now.

## 16. Thank you (0:25)

*(The logos settle in.)* Remember slide one? "This slide is a C# program."

`[SPACE]` So were the other fifteen.

`[SPACE]` A C64 boots up saying "38911 BASIC bytes free". After all 16 slides,
this deck still has...

*(The number counts up.)* ...1727 bytes free.

Thank you!

*(Don't press SPACE again, it ends the program. Leave it up for questions.)*

---

## If it goes off the rails

- **Short on time:** skip slides 4, 9 and 14 with `N`. The story still holds
  together without them.
- **A question about an earlier slide:** `B` goes back, `N` jumps ahead again.
- **"Can I see it live?"** After the last slide you can run `Demo` in VICE.
- **"Why not generics?"** Slide 4: the compiler would rather refuse them with a
  clear error than guess.
