.\compiler\bin\Debug\compiler.exe .\demo\bin\debug\demo.dll .\asm\generated
@if %errorlevel% neq 0 exit /b %errorlevel%
del .\prg\main.prg
rem c:/tools/64tass-1.55.2200/64tass.exe -o .\prg\main.prg --long-branch --vice-labels -l all.l --export-labels .\prg\main.labels --list .\prg\dump.asm --no-monitor .\asm\main.asm  
c:/tools/64tass-1.55.2200/64tass.exe -o .\prg\main.prg --long-branch --vice-labels -l .\prg\main.labels --list .\prg\dump.asm --no-monitor .\asm\main.asm  
@if %errorlevel% neq 0 exit /b %errorlevel%
start C:/tools/VICE/x64sc.exe -moncommands .\prg\main.labels .\prg\main.prg
