<#
.SYNOPSIS
  Builds C64Presentation (optionally booting straight into one slide, held
  or auto-advancing), runs it in its own VICE instance and saves screenshots.

.DESCRIPTION
  One call = build + assemble + launch + capture + close. Safe to run from
  several checkouts/worktrees at once: it never touches another VICE
  instance, and everything it builds stays under -Root.

  -Slide N boots into slide N (0-based). -Scene HOLD keeps it up (advance
  is never pressed), -Scene AUTO advances by itself; see
  C64Presentation.csproj and Helper/KeyBoard.cs. No scene = the real deck,
  waiting for SPACE.

  Screenshots are cropped to the emulated screen (border included) and
  written as <OutPrefix>1.png, <OutPrefix>2.png, ...

  Prints BUILD_FAILED (with the build output), EXITED:<code>, or one
  OK:<path> line per screenshot.
#>
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Scene = 'HOLD',
    [int]$Slide = 0,
    [Parameter(Mandatory=$true)][string]$OutPrefix,
    [int]$WaitSeconds = 10,
    [int]$Shots = 1,
    [int]$IntervalMs = 400,
    [switch]$NoBuild,
    # How VICE's drive 8 runs the deck's disk: True = true drive emulation
    # (real 1541 timing), Virtual = VICE's fast virtual drive, Default =
    # whatever VICE is configured for.
    [ValidateSet('Default', 'True', 'Virtual')][string]$Drive = 'Default'
)

$ErrorActionPreference = 'Stop'
$tools = $PSScriptRoot

if (-not $NoBuild) {
    $out = & dotnet build "$Root\Compiler\Compiler.csproj" -nologo -v q 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Output "BUILD_FAILED"; $out; exit 1 }

    $sceneArg = if ($Scene) { "-p:Scene=$Scene" } else { "-p:Scene=" }
    $out = & dotnet build "$Root\C64Presentation\C64Presentation.csproj" -nologo -v q $sceneArg "-p:StartSlide=$Slide" 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Output "BUILD_FAILED"; $out; exit 1 }

    New-Item -ItemType Directory -Force "$Root\asm\presentation", "$Root\prg" | Out-Null
    $out = & cmd /c "`"$Root\C64Presentation\measure.bat`"" 2>&1
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path "$Root\prg\presentation.prg")) {
        Write-Output "BUILD_FAILED"; $out; exit 1
    }
    # The segment map: how much memory is left.
    $out | Where-Object { $_ -match '^(Data|Gap):' }
}

# The disk image when the deck has files on the disk (disk.txt), else the .prg.
$image = if (Test-Path "$Root\prg\presentation.d64") { "$Root\prg\presentation.d64" } else { "$Root\prg\presentation.prg" }
$driveArgs = switch ($Drive) {
    'True'    { @('-drive8truedrive') }
    'Virtual' { @('+drive8truedrive', '-trapdevice8') }
    default   { @() }
}
$launch = & "$tools\vice-launch.ps1" -Prg $image -WaitSeconds $WaitSeconds -KeepOthers -ViceArgs $driveArgs
if ($launch -notmatch '^RUNNING:(\d+)') { Write-Output $launch; exit 1 }
$vicePid = [int]$Matches[1]

try {
    Add-Type -AssemblyName System.Drawing
    for ($i = 1; $i -le $Shots; $i++) {
        $png = "$OutPrefix$i.png"
        $raw = "$OutPrefix$i.raw.png"
        $r = & "$tools\vice-capture.ps1" -OutPng $raw -ProcessId $vicePid
        if ($r -notmatch '^OK:') { Write-Output $r; continue }

        # Drop the window chrome: title/menu bar on top, status bar below.
        $src = [System.Drawing.Bitmap]::FromFile($raw)
        $top = 56; $bottom = 84; $side = 8
        $rect = New-Object System.Drawing.Rectangle $side, $top, ($src.Width - 2 * $side), ($src.Height - $top - $bottom)
        $crop = $src.Clone($rect, $src.PixelFormat)
        $src.Dispose()
        $crop.Save($png)
        $crop.Dispose()
        Remove-Item $raw
        Write-Output "OK:$png"
        if ($i -lt $Shots) { Start-Sleep -Milliseconds $IntervalMs }
    }
}
finally {
    & "$tools\vice-close.ps1" -ProcessId $vicePid | Out-Null
}
