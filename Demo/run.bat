@echo off
rem Anchors to this script's own directory regardless of the caller's cwd --
rem see Catacombs\run.bat's comment for why this matters.
cd /d "%~dp0"
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\demo.dll" "..\asm\generated" "..\asm\main.asm" "..\prg" "main" --run
