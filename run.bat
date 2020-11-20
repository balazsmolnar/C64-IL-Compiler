.\compiler\bin\Debug\compiler.exe
@if %errorlevel% neq 0 exit /b %errorlevel%
del .\.cache\asm\main.prg
C:/tools/acme/acme -o .\.cache\asm\main.prg -f cbm .\asm\main.asm  
@if %errorlevel% neq 0 exit /b %errorlevel%
start C:/tools/VICE/x64sc.exe .\.cache\asm\main.prg
