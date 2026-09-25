# C64 Test Debugger (VS Code extension)

Lets you set breakpoints and step through a compiled `Test/*.cs` unit test
(the ones that actually run as 6502 code in `SimpleEmulator`) directly in
VS Code, using the same engine as the [`debug-test` skill](../.claude/skills/debug-test/SKILL.md)'s
CLI (`TestDebugger`). This extension is a thin Debug Adapter Protocol (DAP)
wrapper around that same CLI project -- see `TestDebugger/DapServer.cs`.

It's local/unpublished -- not on the Marketplace -- so it needs to be loaded
one of two ways:

## Option A: Extension Development Host (for trying it out / iterating)

1. Open this `vscode-extension/` folder in VS Code (`code
   vscode-extension`), separately from (or in addition to) the main repo
   window.
2. Press F5 (or Run > Start Debugging). VS Code opens a new
   "Extension Development Host" window with the extension loaded.
3. In that new window, open the C64-IL-Compiler repo folder (if it isn't
   already open) -- the extension resolves `TestDebugger/bin/Debug/
   TestDebugger.dll` relative to the workspace folder, so the repo must be
   the open folder.
4. Build `TestDebugger` first if you haven't (`dotnet build TestDebugger/
   TestDebugger.csproj`) -- and `Test/Compiler.Test.csproj` at least once,
   same prerequisite as the CLI (see the `debug-test` skill).
5. Open a `Test/*.cs` file, click in the gutter next to a line inside a
   `[Test]`/`[TestCase]` method to set a breakpoint, place the cursor
   inside that method, and either press F5 in THAT window or run the
   "C64 Test Debugger: Debug Test at Cursor" command (Ctrl+Shift+P).

## Option B: Install as a local extension (persists across normal windows)

```
npm install -g @vscode/vsce
cd vscode-extension
vsce package
code --install-extension c64-test-debugger-0.1.0.vsix
```
Then it's available in your normal VS Code windows without needing the
Extension Development Host step.

## What works

- Gutter breakpoints on any `Test/*.cs` line with a corresponding source
  line in the compiled method (snaps to the next executable line otherwise,
  same as the CLI).
- Step (F10), Continue (F5 once stopped).
- Variables panel shows the current method's locals (same "innermost frame
  only" limitation as the CLI -- see the `debug-test` skill's notes), and
  object/array locals are expandable (click the arrow) to drill into
  fields/elements, recursively -- backed by the same handle/object-table
  resolution the CLI's `print obj.field`/`print arr[i]` syntax uses.
- Debug Console shows `PASSED`/`FAILED: <message>` when the test finishes.
- **Debug all tests**: run the "C64 Test Debugger: Debug All Tests" command
  (or the "Debug All C64 Tests" launch config), and it runs every test in
  the assembly in turn (fresh emulator per test, same as the real suite),
  auto-advancing past each PASSED/FAILED and only actually pausing when
  *whichever* test happens to hit a breakpoint you've set -- useful when you
  don't know in advance which test exercises the code path you're watching.
  Continuing past that test resumes running the rest of the suite.
- **Registers and disassembly**: a "Registers" scope sits alongside
  "Locals" in the Variables panel (A/X/Y/PC/SP/P + a compact flags
  string). Right-click a stack frame (or the source editor) → "Open
  Disassembly View" for real 6502 mnemonics with symbolic operands and the
  *original* macro-call source comments preserved (this is 64tass's own
  `--list` output for the debug build, not a from-scratch decoder). Step
  (F10)/Continue (F5) switch to single-instruction granularity automatically
  while the Disassembly View has focus, and back to source-line granularity
  when the source editor has focus -- no separate toggle, just whichever
  tab you're on. Setting a breakpoint directly in the Disassembly View
  (gutter click on an instruction row) isn't supported yet -- only source-line
  breakpoints.
- **Watch panel, Debug Console, and hover tooltips**: type any `print`-style
  expression (`obj.Child.Id`, `arr[3]`, a bare local name) into the WATCH
  panel or the Debug Console while stopped, and it resolves the same way
  the CLI's `print` does. Hovering over a variable name in the source
  editor while stopped shows its value as a tooltip too.

## Launch config

Add to `.vscode/launch.json` (a `c64test` snippet is registered too --
type "C64 Test" in the config picker):
```json
{
  "name": "Debug C64 Test",
  "type": "c64test",
  "request": "launch",
  "testSelector": "StringInterpolationTests.Interpolation_SingleHoleNoLiteral"
}
```
Omit `testSelector` to have it auto-resolved from the class/method the
cursor is inside when you start debugging (a regex heuristic, not a real
C# parser -- works for this project's flat, non-nested `Test/*.cs` layout).
Set it to `"*"` explicitly (not omitted -- omitting it means "resolve from
cursor", not "run all") to run every test instead.

## Debugging a whole program on VICE

Besides unit tests, the same adapter can build a C64 program (`Demo`,
`Hunchback` or `C64Presentation`), run it on a real VICE and stop at
breakpoints in its C# source.

1. Build `TestDebugger` (`dotnet build TestDebugger/TestDebugger.csproj`) and
   make sure VICE is installed (`VICE_EXE`, or `c:\tools\VICE\bin\x64sc.exe`).
2. Set a breakpoint in e.g. `Demo/Program.cs` or `Demo/RotatingCube.cs`.
3. Run the **"Debug Demo on VICE"** launch config (or the "C64 on VICE: Debug
   Program" command; the project is taken from the open file's folder).

The adapter builds a debug copy into `asm/<name>_debug/` and
`prg/<name>_debug.*` (the normal build is untouched), starts x64sc with its
binary monitor (`-binarymonitor`) on a free local port, attaches, sets your
breakpoints while the machine is still paused, then lets it run. VICE's window
is the C64 screen; stops show up in VS Code.

Runtime faults (out of memory, too many objects, float overflow / division by zero,
see `asm/helper/fault.asm`) stop the debugger as an exception ("Runtime fault: Out of
memory"), with the stack frame on the C# line that triggered it.

Works: breakpoints, continue, step (F10/F11, line or instruction), pause,
locals and object/array inspection, registers, disassembly, watch/hover,
closing the session (kills VICE).

Limits: locals are only resolved for the innermost method (no call-stack
walking); breakpoints added while the program is running take effect at the
next stop; while the program runs, memory-inspecting requests are refused
(reading memory would stop the machine); VICE reads use the RAM bank, so
the CPU port at `$01` isn't visible.

`TestDebugger.dll vice-probe <port> [<labels> <label>...]` attaches to an
already running VICE (`x64sc -binarymonitor -binarymonitoraddress
ip4://127.0.0.1:<port>`) and prints registers/memory, a manual check of the
monitor client. `tools/vice-dap-smoke.py` drives the whole launch flow
headlessly over DAP.
