---
name: debug-test
description: Step-debug a compiled unit test (breakpoints, step, print locals) using the TestDebugger CLI project, instead of guessing from a bare pass/fail. Use when a Test/*.cs test is failing and the reason isn't obvious from the assertion message alone, or when you need to inspect a local variable's actual runtime value/step through a test's compiled 6502 execution. Trigger phrases: "debug this failing test", "why is this test failing", "step through this test", "set a breakpoint in this test", "inspect locals in this test", "what value does X have at runtime".
tools: Bash, Read, Grep
---

# Debugging a failing unit test with TestDebugger

`Test/*.cs` methods don't run as real .NET -- `RunInEmulatorAspect` compiles
the whole `Test` project to 6502 asm and executes each test's *compiled*
routine inside `SimpleEmulator`. A normal C# debugger has nothing real to
attach to. `TestDebugger` (project at `TestDebugger/`) is a CLI step-debugger
built specifically for this: set a breakpoint by `file:line` against a
specific test method, run it in the emulator, stop there, print local
variables, step further -- all from the terminal, no IDE needed.

(A VS Code extension also exists at `vscode-extension/` -- gutter
breakpoints, F10/F5, a real Variables panel -- built as a thin DAP wrapper
around this same `TestDebugger` engine. See its README for setup. This
skill covers the terminal workflow.)

Use this when a failing test's NUnit assertion message alone doesn't explain
*why* -- e.g. "expected 5, got 0" tells you nothing about which intermediate
step produced the wrong value. Stepping through with real local-variable
inspection does.

## Prerequisites

`TestDebugger` compiles against `Test/obj/Debug/Before-PostSharp/
Compiler.Test.dll` -- make sure the `Test` project has been built at least
once (`dotnet build Test/Compiler.Test.csproj`, or it already built earlier
in the session). `TestDebugger` recompiles its own separate debug build
automatically (cached by DLL timestamp) -- you don't need to run `Test/
run.bat` yourself for this.

## Workflow

1. **Find the failing test and a plausible breakpoint line.** Read the test
   method's source (`Read`/`Grep` on `Test/<TestClass>.cs`) to find a line
   right before the point where behavior diverges from expectation -- e.g.
   the line that computes the value the failing `Assert` checks.

2. **Run TestDebugger, piping a command sequence via stdin.** The REPL is
   interactive, but from an agent you drive it non-interactively by piping
   every command up front with `printf` (each command on its own `\n`-
   terminated line, ending in `quit`) -- there's no live back-and-forth
   within one invocation, so plan the command sequence ahead of time and
   re-run with a revised sequence if you need to look further:
   ```
   cd "C:\Balazs\Projects\C64-IL-Compiler\TestDebugger"
   printf "print a\nstep\nprint a\ncontinue\nquit\n" | dotnet bin/Debug/TestDebugger.dll "<TestClass>.<TestMethod>" --break "<TestClass>.cs:<line>"
   ```
   - The test selector is `TestClass.TestMethod` (bare class/method name, no
     namespace needed). For a `[TestCase(...)]`-parameterized method with
     multiple cases, append `:N` (0-based) to pick one, e.g.
     `ArithmeticTests.TestIncrement:2`.
   - `--break` can be passed multiple times for multiple breakpoints, and
     you can also add more with the `break file:line` REPL command mid-
     session.
   - A breakpoint on a line with no exact sequence point (blank line,
     optimized-away statement) snaps down to the next executable line in
     that file -- the reported stop location tells you where it actually
     landed.

3. **REPL commands available:**
   - `break <file>:<line>` -- add a breakpoint (usable before or during a
     session).
   - `run <TestClass>.<TestMethod>[:N]` -- start (or restart) a test run.
   - `run all` -- run every test in the assembly in turn (fresh emulator
     per test, same as the real suite, [Ignore]d tests skipped), printing
     `PASSED <method>`/`FAILED <method>: <message>` for each, and only
     actually *stopping* if one of them hits a currently-set breakpoint --
     useful when you don't know in advance which test exercises the code
     you're watching. `continue`ing past that test resumes running the rest
     of the queue automatically.
   - `step` -- run to the next source line (any line, not just breakpoints).
   - `continue` -- run until the next breakpoint hit, or the test finishes.
   - `print <name>` -- print one local variable's current value.
   - `locals` -- print every local currently in scope.
   - `quit` -- exit.

4. **Reading the output:** a stop prints `Stopped at <file>:<line> (in
   <Method>)` -- this means execution is paused *before* that line runs (so
   a variable that line is about to assign still holds its old value; `step`
   past it first if you want the new one). When the test finishes it prints
   `PASSED` (optionally `PASSED (returned X)` for a value-returning test) or
   `FAILED: <message>` -- for an `[Assert.*]`-based test this is the real
   assertion failure message; for an `ExpectedResult`-style `[TestCase]`
   (no in-body assertion at all) it's `expected X, got Y`, replicating
   NUnit's own outside-the-emulator comparison.

## Known limitations (by design, not bugs)

- **Debug builds are less optimized than the real test build.** Giving
  (almost) every source line its own label suppresses several of the
  compiler's peephole optimizers (see `Compiler/ILMethodDebugLabelPass.cs`'s
  comment) -- this is expected and only affects the separate debug
  assembly/`.prg` (`asm/unittest_debug/`, `prg/unittest_debug.*`), never the
  real `asm/unittest.asm`/`prg/unittest.prg` the actual test suite runs
  against.
- **No call-stack walking.** `print`/`locals` only see the one method you
  started with `run` -- if a breakpoint somehow lands inside a different
  method (unusual given this project's flat `Test/*.cs` layout), locals
  won't resolve there.
- If you change `Test/*.cs` source, rebuild `Test/Compiler.Test.csproj`
  first so `TestDebugger` picks up the new IL (it auto-recompiles its own
  debug build once the DLL's timestamp is newer than the cached
  `debugmap.txt`, or pass `--force-recompile` to force it).
