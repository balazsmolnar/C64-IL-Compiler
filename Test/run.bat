copy .\bin\debug\NUnit* .\obj\Debug\Before-PostSharp\
copy .\bin\debug\C64* .\obj\Debug\Before-PostSharp\

..\compiler\bin\Debug\compiler.exe .\obj\Debug\Before-PostSharp\Compiler.Test.dll  ..\asm\unittest
@if %errorlevel% neq 0 exit /b %errorlevel%
del .\prg\unittest.prg
c:/tools/64tass-1.55.2200/64tass.exe -o ..\prg\unittest.prg --long-branch --vice-labels -l ..\prg\unittest.labels --list ..\prg\dump.asm --no-monitor ..\asm\unittest.asm  > c:\temp\stdout 
@if %errorlevel% neq 0 exit /b %errorlevel%
