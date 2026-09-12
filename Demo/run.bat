@echo off
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\demo.dll" "..\asm\generated" "..\asm\main.asm" "..\prg" "main" --run
