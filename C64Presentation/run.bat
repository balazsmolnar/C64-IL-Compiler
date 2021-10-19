..\compiler\bin\Debug\compiler.exe .\bin\debug\C64Presentation.dll ..\asm\presentation
@if %errorlevel% neq 0 exit /b %errorlevel%
del ..\prg\presentation.prg
rem c:/tools/64tass-1.55.2200/64tass.exe -o .\prg\main.prg --long-branch --vice-labels -l all.l --export-labels .\prg\main.labels --list .\prg\dump.asm --no-monitor .\asm\main.asm  
c:/tools/64tass-1.55.2200/64tass.exe -o ..\prg\presentation.prg --long-branch --vice-labels -l ..\prg\presentation.labels --list ..\prg\dump.asm --no-monitor ..\asm\presentation.asm  
@if %errorlevel% neq 0 exit /b %errorlevel%
start C:/tools/VICE/x64sc.exe -moncommands ..\prg\presentation.labels ..\prg\presentation.prg