<#
.SYNOPSIS
  Closes VICE -- the last step of the vice-verify skill's launch/capture/
  close sequence. Safe to call even if nothing is running. With -ProcessId
  only that one instance is closed (see vice-launch.ps1's -KeepOthers).
#>
param(
    [int]$ProcessId
)
if ($ProcessId) {
    Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
} else {
    Get-Process -Name x64sc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}
Write-Output "CLOSED"
