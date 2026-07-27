$SolutionDir = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $SolutionDir "Folder Prettifier\Folder Prettifier.csproj"
$IssFile = Join-Path $PSScriptRoot "setup.iss"
$IsccPath = "C:\Program Files\Inno Setup 7\ISCC.exe"

# Step 1: Restore NuGet packages
Write-Host "=== Restoring NuGet packages ===" -ForegroundColor Cyan
& "$env:USERPROFILE\.nuget\packages\NuGet.CommandLine\*\tools\NuGet.exe" restore $SolutionDir\Folder Prettifier.sln 2>$null
if (-not $?) {
    Write-Host "Trying msbuild restore..." -ForegroundColor Yellow
    msbuild $Project /t:Restore /p:RestorePackagesConfig=true
}

# Step 2: Build x86 (32-bit)
Write-Host "=== Building x86 (32-bit) Release ===" -ForegroundColor Cyan
msbuild $Project /p:Configuration=Release /p:Platform=x86 /t:Rebuild /p:OutputPath="Build\Release\x86"
if (-not $?) { exit 1 }

# Step 3: Build x64 (64-bit)
Write-Host "=== Building x64 (64-bit) Release ===" -ForegroundColor Cyan
msbuild $Project /p:Configuration=Release /p:Platform=x64 /t:Rebuild /p:OutputPath="Build\Release\x64"
if (-not $?) { exit 1 }

# Step 4: Compile Inno Setup installer
if (Test-Path $IsccPath) {
    Write-Host "=== Compiling installer ===" -ForegroundColor Cyan
    & $IsccPath $IssFile
    if ($?) {
        Write-Host "=== Done! Installer is at: $((Split-Path $PSScriptRoot -Parent))\dist ===" -ForegroundColor Green
    }
} else {
    Write-Host "=== Build complete. Install Inno Setup from https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "    Then run: ISCC.exe `"$IssFile`"" -ForegroundColor Yellow
}
