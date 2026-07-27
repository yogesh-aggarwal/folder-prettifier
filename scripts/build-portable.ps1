param(
  [string]$BuildRoot = (Join-Path (Split-Path $PSScriptRoot -Parent) "src\Build\Release"),
  [string]$Version = "2.0.0"
)

$OutDir = Join-Path (Split-Path $PSScriptRoot -Parent) "dist"

$x86Path = Join-Path $BuildRoot "x86\Folder Prettifier.exe"
if (Test-Path $x86Path) {
  Copy-Item -LiteralPath $x86Path -Destination (Join-Path $OutDir "FolderPrettifier-Portable-x86-$Version.exe") -Force
  Write-Host "Created: FolderPrettifier-Portable-x86-$Version.exe" -ForegroundColor Green
}

$x64Path = Join-Path $BuildRoot "x64\Folder Prettifier.exe"
if (Test-Path $x64Path) {
  Copy-Item -LiteralPath $x64Path -Destination (Join-Path $OutDir "FolderPrettifier-Portable-x64-$Version.exe") -Force
  Write-Host "Created: FolderPrettifier-Portable-x64-$Version.exe" -ForegroundColor Green
}


