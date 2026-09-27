@echo off
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\catacombs.dll" "..\asm\catacombs" "..\asm\catacombs.asm" "..\prg" "catacombs" --run
