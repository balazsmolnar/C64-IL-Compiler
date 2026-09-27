rem @echo off
rem Anchors to this script's own directory regardless of the caller's cwd --
rem without this, running "Catacombs\run.bat" from anywhere other than
rem Catacombs\ itself resolves ".\bin\debug\catacombs.dll" against the
rem WRONG directory and fails with a confusing FileNotFoundException.
cd /d "%~dp0"
call "%~dp0..\tools\compile-and-assemble.bat" ".\bin\debug\catacombs.dll" "..\asm\catacombs" "..\asm\catacombs.asm" "..\prg" "catacombs" --run
