SHELL := cmd
.PHONY: dev build restore build-x86 build-x64 installer portable bump

MSBUILD := powershell -NoProfile -ExecutionPolicy Bypass -File scripts/msbuild.ps1

dev: restore
	@$(MSBUILD) src/App.csproj /p:Configuration=Debug /p:Platform=AnyCPU /t:Build
	@start "" "src\Build\Debug\x86\Folder Prettifier.exe"
	@echo Dev build running.

build: restore build-x86 build-x64 installer portable
	@echo All done.

restore:
	@$(MSBUILD) src/App.csproj /t:Restore /p:RestorePackagesConfig=true

build-x86:
	@$(MSBUILD) src/App.csproj /p:Configuration=Release /p:Platform=x86 /t:Rebuild "/p:OutputPath=Build\Release\x86"

build-x64:
	@$(MSBUILD) src/App.csproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild "/p:OutputPath=Build\Release\x64"

installer:
	@powershell -NoProfile -ExecutionPolicy Bypass -File scripts/iscc.ps1 scripts/setup.iss || echo WARNING: Inno Setup not found, skipping installer.

portable:
	@powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build-portable.ps1

bump:
	@powershell -NoProfile -ExecutionPolicy Bypass -File scripts/bump-version.ps1 -NewVersion "$(word 2,$(MAKECMDGOALS))"

%:
	@true
