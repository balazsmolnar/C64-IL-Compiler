.\compiler\bin\Debug\compiler.exe .\demo\bin\debug\demo.dll .\asm
@if %errorlevel% neq 0 exit /b %errorlevel%
del .\prg\main.prg
C:/tools/acme/acme -o .\prg\main.prg -f cbm .\asm\main.asm  
@if %errorlevel% neq 0 exit /b %errorlevel%
start C:/tools/VICE/x64sc.exe .\prg\main.prg
