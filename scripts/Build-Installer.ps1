param(
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "Publish-Portable.ps1")

$command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
$candidates = @(
    $env:INNO_SETUP_ISCC,
    $command.Source,
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 7\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 7\ISCC.exe"),
    (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
    (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
)
$iscc = $candidates | Where-Object { $_ -and (Test-Path -LiteralPath $_) } | Select-Object -First 1

if (-not $iscc) {
    throw "Inno Setup was not found. Install it from https://jrsoftware.org/isinfo.php or set INNO_SETUP_ISCC."
}

& $iscc "/DMyAppVersion=$Version" (Join-Path $root "installer\SwitchBack.iss")
Write-Host "Installer written to: $(Join-Path $root 'artifacts')"
