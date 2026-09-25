@echo off
copy .\bin\debug\NUnit* .\obj\Debug\Before-PostSharp\ >nul
copy .\bin\debug\C64* .\obj\Debug\Before-PostSharp\ >nul
copy .\bin\debug\PostSharp* .\obj\Debug\Before-PostSharp\ >nul
call "%~dp0..\tools\compile-and-assemble.bat" ".\obj\Debug\Before-PostSharp\Compiler.Test.Runtime.dll" "..\asm\unittest_runtime" "..\asm\unittest_runtime.asm" "..\prg" "unittest_runtime" --quiet --unittest
