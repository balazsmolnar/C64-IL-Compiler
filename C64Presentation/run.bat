@echo off
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\C64Presentation.dll" "..\asm\presentation" "..\asm\presentation.asm" "..\prg" "presentation" --run
