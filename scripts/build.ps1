param(
  [string]$Version = "2.0.0"
)

$ScriptDir = Split-Path $PSScriptRoot -Parent
$Proj = Join-Path -Path $ScriptDir -ChildPath "src\App.csproj"
$Iss = Join-Path -Path $PSScriptRoot -ChildPath "setup.iss"
$Out = Join-Path -Path $ScriptDir -ChildPath "dist"
$Msbuild = Join-Path -Path $PSScriptRoot -ChildPath "msbuild.ps1"
$ErrorActionPreference = "Stop"

function exec {
  param([scriptblock]$Cmd)
  & $Cmd
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
if (-not $iscc) {
  foreach ($p in @(
    "C:\Program Files (x86)\Inno Setup 7\ISCC.exe",
    "C:\Program Files\Inno Setup 7\ISCC.exe"
  )) {
    if (Test-Path $p) { $iscc = $p; break }
  }
}

if (-not (Test-Path $Out)) { New-Item -ItemType Directory -Path $Out -Force | Out-Null }

Write-Host "=== Restoring NuGet packages ==="
exec { & $Msbuild $Proj /t:Restore /p:RestorePackagesConfig=true }

Write-Host "=== Building x86 (32-bit) Release ==="
exec { & $Msbuild $Proj /p:Configuration=Release /p:Platform=x86 /t:Rebuild "/p:OutputPath=Build\Release\x86" }

Write-Host "=== Building x64 (64-bit) Release ==="
exec { & $Msbuild $Proj /p:Configuration=Release /p:Platform=x64 /t:Rebuild "/p:OutputPath=Build\Release\x64" }

Write-Host "=== Building Inno Setup installer ==="
if ($iscc) {
  & $iscc $Iss
  if ($LASTEXITCODE -eq 0) {
    $setupPath = Join-Path $Out "FolderPrettifier-Setup-$Version.exe"
    Write-Host "Created: $setupPath" -ForegroundColor Green
  }
} else {
  Write-Host "WARNING: Inno Setup not found, skipping installer." -ForegroundColor Yellow
}

Write-Host "=== Creating portable executables ==="
& "$PSScriptRoot\build-portable.ps1" -Version $Version

Write-Host "`n=== All done! Files in $Out ===" -ForegroundColor Cyan
Get-ChildItem $Out
