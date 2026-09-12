@echo off
rem Same compile+assemble step as run.bat, without launching VICE -- use
rem this (not run.bat) when the goal is just to inspect/measure the
rem generated asm or .prg size, not to actually run the program.
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\C64Presentation.dll" "..\asm\presentation" "..\asm\presentation.asm" "..\prg" "presentation"
