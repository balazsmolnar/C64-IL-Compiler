@echo off
rem Same compile+assemble step as run.bat, without launching VICE -- use
rem this (not run.bat) when the goal is just to inspect/measure the
rem generated asm or .prg size, not to actually run the program.
rem cd /d anchors to this script's own directory regardless of the caller's
rem cwd -- see Catacombs\run.bat's comment.
cd /d "%~dp0"
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\C64Presentation.dll" "..\asm\presentation" "..\asm\presentation.asm" "..\prg" "presentation"
