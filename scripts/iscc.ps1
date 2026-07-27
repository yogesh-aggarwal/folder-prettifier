$iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
if (-not $iscc) {
  foreach ($p in @(
    "C:\Program Files (x86)\Inno Setup 7\ISCC.exe",
    "C:\Program Files\Inno Setup 7\ISCC.exe"
  )) {
    if (Test-Path $p) { $iscc = $p; break }
  }
}

if (-not $iscc) {
  Write-Error "ISCC not found. Install Inno Setup."
  exit 1
}

& $iscc $args
exit $LASTEXITCODE
