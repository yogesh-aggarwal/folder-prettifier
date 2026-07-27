@echo off
setlocal enabledelayedexpansion

set PROJECT=%~dp0..\Folder Prettifier\Folder Prettifier.csproj
set WXS=%~dp0setup.wxs
set WIX="C:\Program Files (x86)\WiX Toolset v3.14\bin\"
set OUT=%~dp0Output

echo === Building x86 MSI ===
msbuild "%PROJECT%" /p:Configuration=Release /p:Platform=x86 /t:Rebuild /p:OutputPath="Build\Release\x86"
if %errorlevel% neq 0 exit /b %errorlevel%

%WIX%candle.exe "%WXS%" -dPlatform=x86 -out "%TEMP%\FolderPrettifier-x86.wixobj"
if %errorlevel% neq 0 exit /b %errorlevel%

%WIX%light.exe "%TEMP%\FolderPrettifier-x86.wixobj" -out "%OUT%\FolderPrettifier-x86-%VERSION%.msi" -cultures:en-us -ext WixUIExtension
if %errorlevel% neq 0 exit /b %errorlevel%

echo === Building x64 MSI ===
msbuild "%PROJECT%" /p:Configuration=Release /p:Platform=x64 /t:Rebuild /p:OutputPath="Build\Release\x64"
if %errorlevel% neq 0 exit /b %errorlevel%

%WIX%candle.exe "%WXS%" -dPlatform=x64 -out "%TEMP%\FolderPrettifier-x64.wixobj"
if %errorlevel% neq 0 exit /b %errorlevel%

%WIX%light.exe "%TEMP%\FolderPrettifier-x64.wixobj" -out "%OUT%\FolderPrettifier-x64-%VERSION%.msi" -cultures:en-us -ext WixUIExtension
if %errorlevel% neq 0 exit /b %errorlevel%

echo === Done! MSI files in %OUT% ===
