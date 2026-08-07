function Get-SdkDotnet {
  $candidate = $args[0]
  if (-not (Test-Path $candidate)) { return $null }
  $sdk = & $candidate --list-sdks 2>&1
  if ($LASTEXITCODE -eq 0 -and "$sdk" -match "\d+\.\d+\.\d+") { return $candidate }
  return $null
}

$dotnet = Get-Command "dotnet.exe" -ErrorAction SilentlyContinue
if ($dotnet) {
  $resolved = Get-SdkDotnet $dotnet.Source
  if ($resolved) { & $resolved $args; exit $LASTEXITCODE }
}

$resolved = Get-SdkDotnet "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
if ($resolved) { & $resolved $args; exit $LASTEXITCODE }

Write-Error "No .NET SDK found. Install it from https://dotnet.microsoft.com/download or run the dotnet-install script."
exit 1
