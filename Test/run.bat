@echo off
rem Anchors to this script's own directory regardless of the caller's cwd --
rem see Catacombs\run.bat's comment for why this matters.
cd /d "%~dp0"
copy .\bin\debug\NUnit* .\obj\Debug\Before-PostSharp\ >nul
copy .\bin\debug\C64* .\obj\Debug\Before-PostSharp\ >nul
copy .\bin\debug\PostSharp* .\obj\Debug\Before-PostSharp\ >nul
call "%~dp0..\tools\compile-and-assemble.bat" ".\obj\Debug\Before-PostSharp\Compiler.Test.dll" "..\asm\unittest" "..\asm\unittest.asm" "..\prg" "unittest" --quiet --unittest
