<#
.SYNOPSIS
  Writes a .d64 disk image holding a program and the data files it loads
  with C64.Disk.Load (C64Lib/Disk.cs).

.DESCRIPTION
  Called by tools\compile-and-assemble.bat after assembling, when the
  project directory has a disk.txt. The program goes on first, so a disk
  autostart (VICE) or LOAD"*",8 picks it.

  disk.txt: one file per line, the name on the disk and the host file,
  relative to disk.txt itself. Blank lines and lines starting with # are
  ignored:

      # name on disk   host file
      shotroom         art\build\disk\shot_room.bin

  Names are at most 16 characters; write them in lower case (c1541 turns
  lower-case ASCII into the PETSCII upper case that C64.Disk.Load asks
  for). Each host file is data: it is written as a PRG file with a 2-byte
  load address of $0000 in front, which Disk.Load skips (it loads at the
  address it is given, secondary address 0).

  Prints the disk's directory. Exits with 1 on an error.
#>
param(
    [Parameter(Mandatory=$true)][string]$Prg,
    [Parameter(Mandatory=$true)][string]$Manifest,
    [Parameter(Mandatory=$true)][string]$Out,
    [string]$Name = [System.IO.Path]::GetFileNameWithoutExtension($Prg),
    [string]$C1541
)

$ErrorActionPreference = 'Stop'
$CbmNameMax = 16
$DiskId = '64'

if (-not $C1541) {
    $C1541 = if ($env:C1541_EXE) { $env:C1541_EXE }
             elseif ($env:VICE_EXE) { Join-Path (Split-Path -Parent $env:VICE_EXE) 'c1541.exe' }
             else { 'C:\tools\VICE\bin\c1541.exe' }
}
if (-not (Test-Path $C1541)) { Write-Error "make-d64: c1541 not found: $C1541 (set C1541_EXE)"; exit 1 }
if (-not (Test-Path $Prg)) { Write-Error "make-d64: program not found: $Prg"; exit 1 }

$baseDir = Split-Path -Parent (Resolve-Path $Manifest)
$stage = Join-Path (Split-Path -Parent $Out) ([System.IO.Path]::GetFileNameWithoutExtension($Out) + '.d64files')
New-Item -ItemType Directory -Force $stage | Out-Null

$diskName = $Name.ToLowerInvariant()
$args1541 = @('-format', "$diskName,$DiskId", 'd64', $Out, '-write', $Prg, $diskName)
$lineNo = 0
foreach ($line in Get-Content $Manifest) {
    $lineNo++
    $text = $line.Trim()
    if ($text -eq '' -or $text.StartsWith('#')) { continue }
    $fields = $text -split '\s+', 2
    if ($fields.Count -ne 2) { Write-Error "make-d64: ${Manifest}($lineNo): expected '<name on disk> <host file>'"; exit 1 }
    $cbmName, $hostFile = $fields[0], $fields[1]
    if ($cbmName.Length -gt $CbmNameMax) { Write-Error "make-d64: ${Manifest}($lineNo): '$cbmName' is longer than $CbmNameMax characters"; exit 1 }
    $source = Join-Path $baseDir $hostFile
    if (-not (Test-Path $source)) { Write-Error "make-d64: ${Manifest}($lineNo): file not found: $source"; exit 1 }

    # The 2-byte load address every CBM PRG file starts with.
    $bytes = [System.IO.File]::ReadAllBytes($source)
    $withHeader = New-Object byte[] ($bytes.Length + 2)
    [Array]::Copy($bytes, 0, $withHeader, 2, $bytes.Length)
    $staged = Join-Path $stage ($cbmName + '.prg')
    [System.IO.File]::WriteAllBytes($staged, $withHeader)
    $args1541 += @('-write', $staged, $cbmName.ToLowerInvariant())
}

if (Test-Path $Out) { Remove-Item $Out }
$log = & $C1541 @args1541 2>&1
if ($LASTEXITCODE -ne 0 -or -not (Test-Path $Out)) { $log; Write-Error "make-d64: c1541 failed"; exit 1 }
& $C1541 -attach $Out -dir 2>&1 | Where-Object { $_ -match '\S' }
exit 0
