<#
.SYNOPSIS
  Captures the currently-running VICE window to a PNG, without requiring
  focus -- uses PrintWindow (PW_RENDERFULLCONTENT), which reliably works
  even if another window is on top or has stolen focus. Call
  vice-launch.ps1 first; call this as many times as needed afterward (e.g.
  to prove something changes over time, like an interrupt-driven flicker)
  before a final vice-close.ps1.

.DESCRIPTION
  Prints exactly one line to stdout:
    OK:<path>       -- screenshot written.
    NOT_RUNNING     -- no x64sc.exe process found; call vice-launch.ps1 first.
    BAD_WINDOW      -- process found but its window has no usable size yet
                        (rare; try again after a short extra wait).
#>
param(
    [Parameter(Mandatory=$true)][string]$OutPng
)

$ErrorActionPreference = 'Stop'

$proc = Get-Process -Name x64sc -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $proc) {
    Write-Output "NOT_RUNNING"
    exit 1
}

Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public struct ViceRect { public int Left, Top, Right, Bottom; }
public static class ViceWin32 {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out ViceRect rect);
}
'@

$hwnd = $proc.MainWindowHandle
$rect = New-Object ViceRect
[ViceWin32]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top
if ($w -le 0 -or $h -le 0) {
    Write-Output "BAD_WINDOW"
    exit 1
}

$bmp = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $g.GetHdc()
# flags=2 -> PW_RENDERFULLCONTENT (correct content even when composited/
# occluded, which plain BitBlt-based capture is not reliable for).
[ViceWin32]::PrintWindow($hwnd, $hdc, 2) | Out-Null
$g.ReleaseHdc($hdc)
$g.Dispose()
$bmp.Save($OutPng)
$bmp.Dispose()
Write-Output "OK:$OutPng"
