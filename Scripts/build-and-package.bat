@echo off
setlocal enabledelayedexpansion

set PROJECT=%~dp0..\Folder Prettifier\Folder Prettifier.csproj
set ISS=%~dp0setup.iss
set ISCC="C:\Program Files\Inno Setup 7\ISCC.exe"

echo === Restoring NuGet packages ===
msbuild "%PROJECT%" /t:Restore /p:RestorePackagesConfig=true
if %errorlevel% neq 0 exit /b %errorlevel%

echo === Building x86 (32-bit) Release ===
msbuild "%PROJECT%" /p:Configuration=Release /p:Platform=x86 /t:Rebuild /p:OutputPath="Build\Release\x86"
if %errorlevel% neq 0 exit /b %errorlevel%

echo === Building x64 (64-bit) Release ===
msbuild "%PROJECT%" /p:Configuration=Release /p:Platform=x64 /t:Rebuild /p:OutputPath="Build\Release\x64"
if %errorlevel% neq 0 exit /b %errorlevel%

if exist %ISCC% (
    echo === Compiling installer ===
    %ISCC% "%ISS%"
    if !errorlevel! equ 0 (
        echo === Done! Check Scripts\Output\ ===
    )
) else (
    echo === Build complete. Install Inno Setup from https://jrsoftware.org/isdl.php
    echo     Then run: ISCC.exe "%ISS%"
)
