@echo off
rem ============================================================================
rem tools\compile-and-assemble.bat
rem
rem Shared build step for every "compile IL to 6502 asm with Compiler.exe,
rem assemble with 64tass, optionally launch in VICE" run.bat in this repo
rem (root run.bat, Demo\run.bat, Test\run.bat, C64Presentation\run.bat).
rem Having one copy means tool paths/versions and the Compiler.exe location
rem only need to be correct in one place instead of four.
rem
rem Usage (called from a project directory, e.g. Demo\run.bat):
rem   call "%%~dp0..\tools\compile-and-assemble.bat" <assembly.dll> <asmOutDir> <asmEntryFile> <prgDir> <prgName> [--run] [--quiet] [--unittest]
rem
rem   assembly.dll   .dll fed into Compiler.exe (relative to the caller's cwd)
rem   asmOutDir      folder Compiler.exe writes generated .asm into
rem   asmEntryFile   top-level .asm file Compiler.exe (re)generates and
rem                  64tass assembles -- see ILEntryPointPass.cs
rem   prgDir         folder for the resulting .prg/.labels/dump.asm
rem   prgName        output base name -> <prgDir>\<prgName>.prg/.labels
rem   --run          launch the result in VICE after assembling
rem   --quiet        redirect 64tass output to NUL (used by the test run,
rem                  which is otherwise noisy with portable-filename warnings)
rem   --unittest     generate the unittest.asm-style test harness entry
rem                  point (Run_Test/Assert_*) instead of a normal
rem                  Program_Main entry point
rem
rem Tool locations are machine-specific; override them per-machine via
rem environment variables (TASS_EXE, VICE_EXE, COMPILER_EXE) instead of
rem editing this file.
rem ============================================================================
setlocal

if "%TASS_EXE%"=="" set "TASS_EXE=c:\tools\64tass-1.60.3243\64tass.exe"
if "%VICE_EXE%"=="" set "VICE_EXE=c:\tools\VICE\bin\x64sc.exe"
if "%COMPILER_EXE%"=="" set "COMPILER_EXE=%~dp0..\Compiler\bin\Debug\Compiler.exe"

set "ASSEMBLY=%~1"
set "ASM_OUT_DIR=%~2"
set "ASM_ENTRY=%~3"
set "PRG_DIR=%~4"
set "PRG_NAME=%~5"

if "%PRG_NAME%"=="" (
    echo compile-and-assemble.bat: missing arguments. 1>&2
    echo Usage: compile-and-assemble.bat ^<assembly.dll^> ^<asmOutDir^> ^<asmEntryFile^> ^<prgDir^> ^<prgName^> [--run] [--quiet] 1>&2
    exit /b 1
)

set "DO_RUN=0"
set "QUIET=0"
set "PROJECT_TYPE=program"
for %%A in (%6 %7 %8) do (
    if /I "%%~A"=="--run" set "DO_RUN=1"
    if /I "%%~A"=="--quiet" set "QUIET=1"
    if /I "%%~A"=="--unittest" set "PROJECT_TYPE=unittest"
)

"%COMPILER_EXE%" "%ASSEMBLY%" "%ASM_OUT_DIR%" "%ASM_ENTRY%" "%PROJECT_TYPE%"
if errorlevel 1 exit /b %errorlevel%

if exist "%PRG_DIR%\%PRG_NAME%.prg" del "%PRG_DIR%\%PRG_NAME%.prg"

if "%QUIET%"=="1" (
    "%TASS_EXE%" -o "%PRG_DIR%\%PRG_NAME%.prg" --long-branch --vice-labels -l "%PRG_DIR%\%PRG_NAME%.labels" --list "%PRG_DIR%\dump.asm" --no-monitor "%ASM_ENTRY%" >nul
) else (
    "%TASS_EXE%" -o "%PRG_DIR%\%PRG_NAME%.prg" --long-branch --vice-labels -l "%PRG_DIR%\%PRG_NAME%.labels" --list "%PRG_DIR%\dump.asm" --no-monitor "%ASM_ENTRY%"
)
if errorlevel 1 exit /b %errorlevel%

if "%DO_RUN%"=="1" start "" "%VICE_EXE%" -moncommands "%PRG_DIR%\%PRG_NAME%.labels" "%PRG_DIR%\%PRG_NAME%.prg"

exit /b 0
