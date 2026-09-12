@echo off
call "%~dp0tools\compile-and-assemble.bat" ".\Demo\bin\debug\demo.dll" ".\asm\generated" ".\asm\main.asm" ".\prg" "main" --run
