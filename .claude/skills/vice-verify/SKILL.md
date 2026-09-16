---
name: vice-verify
description: Visually verify a compiled C64 .prg by running it in the real VICE emulator (x64sc) and inspecting a screenshot. Use whenever a change needs confirming on real hardware-accurate emulation rather than just SimpleEmulator/dotnet test -- e.g. ROM banking, ROM-call correctness, ASYNCHRONOUS interrupt firing (SimpleEmulator has no interrupt-timer model at all), sprite/graphics output, or anything a screenshot can settle that a unit test can't. Trigger phrases: "check in VICE", "run it in the emulator", "verify visually", "take a screenshot", "does it actually work on real hardware".
tools: Bash, PowerShell, Read
---

# VICE verification

Launches VICE headfully, captures the window without needing focus, and lets
you inspect the result via the `Read` tool (screenshots are images -- `Read`
renders them for you directly, no separate OCR/description step needed).
Three small scripts in `tools/` do the actual work; this skill is really just
"how to drive them correctly," because the naive version of this workflow
(inline ad-hoc PowerShell, re-launching VICE for every screenshot, capturing
whatever window happens to have focus) failed in several distinct ways
across earlier sessions before landing on this shape.

## The one thing that will fool you: crash vs. normal return

**A screenshot showing BASIC's `READY.` prompt does NOT mean the program
crashed.** It's *also* exactly what a program looks like after it finishes
normally and returns to BASIC -- `jsr Program_Main` / `rts` in `asm/main.asm`
hands control right back to the `SYS` line that launched it, and that looks
identical to a crash in a single screenshot. This produced a real, extended
misdiagnosis earlier in this project's history (see git log / conversation
history around the float-support work) before the fix was understood: it
wasn't a bug, the test just had no way to distinguish "finished and
returned" from "crashed."

**Before writing the program you're about to verify, make sure it ends in
an infinite loop** (`for(;;){}` in C#) after setting some observable final
state (a border color via `C64.SetBorderColor`, text via `C64.Write`, etc.).
Only then does a screenshot proving that state is showing actually prove the
program got there *and is still running* -- not merely that it returned.
This matters especially for anything interrupt-related: an interrupt
handler firing correctly is only provable by something changing on screen
*continuously*, which requires the mainline code to still be alive in a
loop, not sitting back at a BASIC prompt.

For proving an interrupt fires *repeatedly* (not just once), take two
captures a couple seconds apart and diff them by eye -- or even a single
capture can show mid-frame tearing between two colors if the handler is
toggling something fast enough (a real, useful artifact, not a bug in the
capture).

## Workflow

1. **Build the .prg first** (this skill doesn't do that part) -- e.g. for
   `Demo/`: `cd Demo && ./measure.bat` (assembles only) or `./run.bat`
   (assembles *and* launches VICE itself, which you don't want here --
   use `measure.bat` so this skill controls the launch instead).
2. **Launch**:
   ```
   & "C:\Balazs\Projects\C64-IL-Compiler\tools\vice-launch.ps1" -Prg "C:\Balazs\Projects\C64-IL-Compiler\prg\main.prg" -WaitSeconds 8
   ```
   Prints `RUNNING:<pid>` or `EXITED:<code>`. Pick `-WaitSeconds` for how
   long the program needs to reach its final state (a few float ops:
   the default 8s is plenty; a long loop: more). Always kills any stray
   `x64sc.exe` first, so you never capture a leftover window from an
   earlier run.
3. **Capture** (repeatable -- call again for a second/third shot without
   relaunching):
   ```
   & "C:\Balazs\Projects\C64-IL-Compiler\tools\vice-capture.ps1" -OutPng "C:\Balazs\Projects\C64-IL-Compiler\.debug-tmp\check1.png"
   ```
   Prints `OK:<path>`, `NOT_RUNNING`, or `BAD_WINDOW`. Put the PNG under
   `.debug-tmp/` (gitignored scratch space, per this repo's convention --
   see root `CLAUDE.md`), not somewhere permanent.
4. **Read the screenshot**: use the `Read` tool on the PNG path directly --
   it's an image, Claude Code renders it inline, no extra step.
5. **Close** when done verifying:
   ```
   & "C:\Balazs\Projects\C64-IL-Compiler\tools\vice-close.ps1"
   ```
6. **Delete the screenshot(s)** from `.debug-tmp/` once you've drawn your
   conclusion -- they're scratch, not artifacts worth keeping around.

## Permissions

`tools/vice-launch.ps1`, `tools/vice-capture.ps1`, and `tools/vice-close.ps1`
are allow-listed in `.claude/settings.local.json` (matched on the fixed
script path with a wildcard for arguments), so calling them through the
`PowerShell` tool should not prompt. If a prompt still appears (e.g. the
permission pattern doesn't match how this Claude Code build normalizes the
command string), approve it and, ideally, tell the user so the pattern in
`.claude/settings.local.json` can be corrected -- don't silently work around
it with a different invocation shape, since that'll just prompt again next
time too.

## Why three scripts instead of one inline block

Earlier attempts inlined the whole launch+capture sequence as ad-hoc
PowerShell in each call. Two concrete problems came from that, both solved
structurally here rather than by being more careful each time:

- **`Add-Type` redefinition errors.** The `PowerShell` tool's shell state
  persists across calls within a session, so defining the same P/Invoke
  type twice (`ViceWin32`, a `RECT` struct, ...) throws on the second call.
  Working around it required incrementing a namespace/type name suffix
  every single time (`Win32`, `Win32b`, `Win32c`, ...) -- fragile and easy
  to forget. Each of these scripts runs as its own fresh `powershell.exe`
  process instead (via `-File`, implicitly, through the `&` call), so this
  never comes up.
- **Capturing the wrong window.** Multi-step manual sequences (find the
  window in one call, capture in a separate later call) left a gap where
  another application could steal focus in between, and once genuinely
  captured unrelated content this way (see this session's own history --
  handled per this repo's security-sensitive-content policy at the time).
  `vice-launch.ps1` always kills stray `x64sc.exe` processes first, and
  `vice-capture.ps1` finds the window by process name (not by a
  previously-cached handle or by whatever currently has focus) and uses
  `PrintWindow`, which doesn't need focus at all -- there's no window in
  which the wrong content could get captured.
