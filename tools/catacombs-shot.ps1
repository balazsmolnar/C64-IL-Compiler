<#
.SYNOPSIS
  Builds Catacombs (optionally as a "scene" build that boots straight into
  one game state), runs it in its own VICE instance and saves screenshots.

.DESCRIPTION
  One call = build + assemble + launch + capture + close. Safe to run from
  several checkouts/worktrees at once: it never touches another VICE
  instance, and everything it builds stays under -Root.

  -Scene NAME builds with SCENE_NAME defined (see Catacombs.csproj), so
  code under "#if SCENE_NAME" can put the game into a state that normally
  needs player input to reach. No scene = the real game from power-on.

  Screenshots are cropped to the emulated screen (border included) and
  written as <OutPrefix>1.png, <OutPrefix>2.png, ...

  Prints BUILD_FAILED (with the build output), EXITED:<code>, or one
  OK:<path> line per screenshot.
#>
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Scene = '',
    [Parameter(Mandatory=$true)][string]$OutPrefix,
    [int]$WaitSeconds = 10,
    [int]$Shots = 1,
    [int]$IntervalMs = 400,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$tools = $PSScriptRoot

if (-not $NoBuild) {
    $out = & dotnet build "$Root\Compiler\Compiler.csproj" -nologo -v q 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Output "BUILD_FAILED"; $out; exit 1 }

    $sceneArg = if ($Scene) { "-p:Scene=$Scene" } else { "-p:Scene=" }
    $out = & dotnet build "$Root\Catacombs\Catacombs.csproj" -nologo -v q $sceneArg 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Output "BUILD_FAILED"; $out; exit 1 }

    New-Item -ItemType Directory -Force "$Root\asm\catacombs", "$Root\prg" | Out-Null
    $out = & cmd /c "`"$Root\Catacombs\measure.bat`"" 2>&1
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path "$Root\prg\catacombs.prg")) {
        Write-Output "BUILD_FAILED"; $out; exit 1
    }
    # The segment map: how much memory is left.
    $out | Where-Object { $_ -match '^(Data|Gap):' }
}

$launch = & "$tools\vice-launch.ps1" -Prg "$Root\prg\catacombs.prg" -WaitSeconds $WaitSeconds -KeepOthers
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
