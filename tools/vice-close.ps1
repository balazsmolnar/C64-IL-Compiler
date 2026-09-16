<#
.SYNOPSIS
  Closes VICE -- the last step of the vice-verify skill's launch/capture/
  close sequence. Safe to call even if nothing is running.
#>
Get-Process -Name x64sc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Output "CLOSED"
