param(
  [Parameter(Mandatory=$true)]
  [string]$NewVersion
)

$root = Split-Path $PSScriptRoot -Parent
$files = @(
  @{ Path = "$root\scripts\build.ps1"; Pattern = '(\$Version = ").*(")'; Replacement = "`${1}$NewVersion`${2}" }
  @{ Path = "$root\scripts\setup.iss"; Pattern = '(#define MyAppVersion ").*(")'; Replacement = "`${1}$NewVersion`${2}" }
  @{ Path = "$root\scripts\build-portable.ps1"; Pattern = '(\$Version = ").*(")'; Replacement = "`${1}$NewVersion`${2}" }
  @{ Path = "$root\src\Properties\AssemblyInfo.cs"; Pattern = '(AssemblyVersion\(").*("\))'; Replacement = "`${1}$NewVersion.0`${2}" }
  @{ Path = "$root\src\Properties\AssemblyInfo.cs"; Pattern = '(AssemblyFileVersion\(").*("\))'; Replacement = "`${1}$NewVersion.0`${2}" }
)

$count = 0
foreach ($f in $files) {
  $content = Get-Content $f.Path -Raw
  if ($content -match $f.Pattern) {
    $content = $content -replace $f.Pattern, $f.Replacement
    Set-Content $f.Path $content -NoNewLine
    $count++
    Write-Host "[OK] $($f.Path)" -ForegroundColor Green
  } else {
    Write-Host "[!!] Pattern not found in $($f.Path)" -ForegroundColor Red
  }
}

Write-Host ("`nUpdated " + $count + " files to version " + $NewVersion) -ForegroundColor Cyan
