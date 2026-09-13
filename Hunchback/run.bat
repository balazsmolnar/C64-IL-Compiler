@echo off
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\hunchback.dll" "..\asm\hunchback" "..\asm\hunchback.asm" "..\prg" "hunchback" --run
