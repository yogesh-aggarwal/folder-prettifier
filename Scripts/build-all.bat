@echo off
setlocal enabledelayedexpansion
set VERSION=2.0.0

set PROJ=%~dp0..\Folder Prettifier\Folder Prettifier.csproj
set ISS=%~dp0setup.iss
set ISCC="C:\Program Files\Inno Setup 7\ISCC.exe"
set OUT=%~dp0Output

REM === Step 0: Restore NuGet packages ===
echo === Restoring NuGet packages ===
msbuild "%PROJ%" /t:Restore /p:RestorePackagesConfig=true
if %errorlevel% neq 0 exit /b %errorlevel%

REM === Step 1: Build x86 ===
echo === Building x86 (32-bit) Release ===
msbuild "%PROJ%" /p:Configuration=Release /p:Platform=x86 /t:Rebuild /p:OutputPath="Build\Release\x86"
if %errorlevel% neq 0 exit /b %errorlevel%

REM === Step 2: Build x64 ===
echo === Building x64 (64-bit) Release ===
msbuild "%PROJ%" /p:Configuration=Release /p:Platform=x64 /t:Rebuild /p:OutputPath="Build\Release\x64"
if %errorlevel% neq 0 exit /b %errorlevel%

REM === Step 3: Inno Setup Installer ===
echo === Building Inno Setup installer ===
if exist %ISCC% (
    %ISCC% "%ISS%"
    if !errorlevel! equ 0 (
        echo Created: %OUT%\FolderPrettifier-Setup-%VERSION%.exe
    )
) else (
    echo WARNING: Inno Setup not found, skipping installer.
)

REM === Step 4: Portable builds ===
echo === Creating portable executables ===
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-portable.ps1" -Version "%VERSION%"

echo.
echo === All done! Files in %OUT% ===
dir "%OUT%"
