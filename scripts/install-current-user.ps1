$ErrorActionPreference = 'Stop'

$source = Join-Path $PSScriptRoot '..\dist\mpv-protocol-handler.exe'
$targetDir = 'C:\Program Files\mpv.net'
$target = Join-Path $targetDir 'mpv-protocol-handler.exe'

if (-not (Test-Path -LiteralPath $source)) {
    throw "Build output not found: $source"
}

New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
Copy-Item -LiteralPath $source -Destination $target -Force

$key = 'HKCU:\Software\Classes\mpv'
New-Item -Path $key -Force | Out-Null
Set-Item -Path $key -Value 'URL:mpv Protocol'
New-ItemProperty -Path $key -Name 'URL Protocol' -Value '' -PropertyType String -Force | Out-Null

$commandKey = 'HKCU:\Software\Classes\mpv\shell\open\command'
New-Item -Path $commandKey -Force | Out-Null
Set-Item -Path $commandKey -Value ('"' + $target + '" "%1"')

Write-Host "Installed mpv:// handler for current user: $target"
