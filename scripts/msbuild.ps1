$msbuild = Get-Command "msbuild.exe" -ErrorAction SilentlyContinue
if ($msbuild) { & $msbuild $args; exit $LASTEXITCODE }

$vswhere = @(
  "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe",
  "$env:ProgramFiles\Microsoft Visual Studio\Installer\vswhere.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($vswhere) {
  $vsPath = & $vswhere -latest -products * -property installationPath 2>$null
  if ($vsPath) {
    $candidates = @(
      "$vsPath\MSBuild\Current\Bin\MSBuild.exe",
      "$vsPath\MSBuild\Current\Bin\amd64\MSBuild.exe",
      "$vsPath\MSBuild\15.0\Bin\MSBuild.exe"
    )
    foreach ($c in $candidates) {
      if (Test-Path $c) { $msbuild = $c; break }
    }
  }
}

if (-not $msbuild) {
  $pf = [Environment]::GetFolderPath('ProgramFiles')
  $pf86 = [Environment]::GetFolderPath('ProgramFilesX86')
  $wild = @("$pf\Microsoft Visual Studio", "$pf86\Microsoft Visual Studio")
  foreach ($root in $wild) {
    if (Test-Path $root) {
      $exe = Get-ChildItem $root -Filter "MSBuild.exe" -Recurse -Depth 6 -ErrorAction SilentlyContinue |
             Select-Object -First 1 -ExpandProperty FullName
      if ($exe) { $msbuild = $exe; break }
    }
  }
}

if (-not $msbuild) {
  Write-Error "MSBuild not found. Install Visual Studio or run from Developer Command Prompt."
  exit 1
}

& $msbuild $args
exit $LASTEXITCODE
