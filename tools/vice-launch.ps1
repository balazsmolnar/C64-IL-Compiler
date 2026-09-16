<#
.SYNOPSIS
  Launches VICE (x64sc) autostarting a .prg, and reports whether it's still
  running after a wait -- the first step of the vice-verify skill's
  launch/capture/close sequence (see .claude/skills/vice-verify/SKILL.md).

.DESCRIPTION
  Always kills any pre-existing x64sc.exe first, so there's never ambiguity
  about which window a later vice-capture.ps1 call targets (Get-Process
  -Name x64sc returns exactly one process for the rest of the sequence).

  Prints exactly one line to stdout:
    RUNNING:<pid>   -- VICE is up and (presumably) sitting in the program's
                        own infinite loop; safe to capture now.
    EXITED:<code>   -- VICE's process itself quit (not the same as the
                        emulated program returning to BASIC, which still
                        looks like RUNNING -- see the skill doc's crash-vs-
                        normal-return note).
#>
param(
    [Parameter(Mandatory=$true)][string]$Prg,
    [int]$WaitSeconds = 8,
    [string]$ViceExe
)

$ErrorActionPreference = 'Stop'

if (-not $ViceExe) {
    $ViceExe = if ($env:VICE_EXE) { $env:VICE_EXE } else { 'C:\tools\VICE\bin\x64sc.exe' }
}
if (-not (Test-Path $Prg)) {
    Write-Output "ERROR:prg not found: $Prg"
    exit 1
}
if (-not (Test-Path $ViceExe)) {
    Write-Output "ERROR:VICE not found: $ViceExe"
    exit 1
}

Get-Process -Name x64sc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 300

$p = Start-Process -FilePath $ViceExe -ArgumentList '-autostart', $Prg -PassThru
Start-Sleep -Seconds $WaitSeconds

if ($p.HasExited) {
    Write-Output "EXITED:$($p.ExitCode)"
} else {
    Write-Output "RUNNING:$($p.Id)"
}
