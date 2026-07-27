; Inno Setup script for Folder Prettifier
; Install Inno Setup from https://jrsoftware.org/isdl.php
; Then compile: ISCC.exe setup.iss

#define MyAppName "Folder Prettifier"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Yogesh Aggarwal"
#define MyAppURL "https://programmingwithyogesh.live"
#define MyAppExeName "Folder Prettifier.exe"

[Setup]
AppId={{688C8822-8EC3-491C-8767-1CD6B881EA78}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/report
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.\Output
OutputBaseFilename=FolderPrettifier-Setup-{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
SetupIconFile=..\Folder Prettifier\Icons\icon.ico
WizardImageFile=.\WizardImage.bmp
WizardSmallImageFile=.\WizardSmallImage.bmp
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional options:"; Flags: checkedonce
Name: "contextmenu"; Description: "Add &Folder Prettifier to folder right-click menu"; GroupDescription: "Additional options:"; Flags: checkedonce

[Registry]
Root: "HKCR"; Subkey: "Directory\shell\FolderPrettifier"; ValueType: string; ValueName: ""; ValueData: "Folder Prettifier"; Flags: uninsdeletekey; Tasks: contextmenu
Root: "HKCR"; Subkey: "Directory\shell\FolderPrettifier"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey; Tasks: contextmenu
Root: "HKCR"; Subkey: "Directory\shell\FolderPrettifier\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%V"""; Flags: uninsdeletekey; Tasks: contextmenu
Root: "HKCR"; Subkey: "Directory\Background\shell\FolderPrettifier"; ValueType: string; ValueName: ""; ValueData: "Folder Prettifier"; Flags: uninsdeletekey; Tasks: contextmenu
Root: "HKCR"; Subkey: "Directory\Background\shell\FolderPrettifier"; ValueType: string; ValueName: "Icon"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey; Tasks: contextmenu
Root: "HKCR"; Subkey: "Directory\Background\shell\FolderPrettifier\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%V"""; Flags: uninsdeletekey; Tasks: contextmenu

[Files]
; x86 (32-bit) files - installed on 32-bit Windows
Source: "..\Folder Prettifier\Build\Release\x86\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: not Is64BitInstallMode
Source: "..\Folder Prettifier\Build\Release\x86\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion; Check: not Is64BitInstallMode

; x64 (64-bit) files - installed on 64-bit Windows
Source: "..\Folder Prettifier\Build\Release\x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: Is64BitInstallMode
Source: "..\Folder Prettifier\Build\Release\x64\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion; Check: Is64BitInstallMode

; Shared files
Source: "..\catalog.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
var
  ShouldInstallDotNet: Boolean;

function IsDotNet481Installed: Boolean;
var
  version: Cardinal;
begin
  Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full');
  if Result then
  begin
    Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', version);
    if Result then
      Result := (version >= 528040);
  end;
end;

function DownloadFile(URL, FileName: string): Boolean;
var
  script: string;
  resultCode: Integer;
begin
  script :=
    'try { $web = New-Object System.Net.WebClient; ' +
    '$web.DownloadFile(''' + URL + ''', ''' + FileName + '''); exit 0 } ' +
    'catch { exit 1 }';
  Result := Exec('powershell', '-Command "' + script + '"',
    '', SW_HIDE, ewWaitUntilTerminated, resultCode) and (resultCode = 0);
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  ShouldInstallDotNet := False;
  if not IsDotNet481Installed then
  begin
    if MsgBox('Folder Prettifier requires .NET Framework 4.8.1.'#13#13
      'Download and install it now? (Internet connection needed)',
      mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShouldInstallDotNet := True;
    end
    else
      Result := False;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  tmpPath, setupPath: String;
  resultCode: Integer;
begin
  if (CurStep = ssInstall) and ShouldInstallDotNet then
  begin
    tmpPath := ExpandConstant('{tmp}');
    setupPath := tmpPath + '\NDP481-Web.exe';

    WizardForm.StatusLabel.Caption := 'Downloading .NET Framework 4.8.1...';
    WizardForm.ProgressGauge.Style := npbstMarquee;

    if DownloadFile('https://go.microsoft.com/fwlink/?linkid=2203306', setupPath) then
    begin
      WizardForm.StatusLabel.Caption := 'Installing .NET Framework 4.8.1...';
      if Exec(setupPath, '/q /norestart', '', SW_SHOW, ewWaitUntilTerminated, resultCode) then
      begin
        if resultCode = 0 then
          Sleep(2000);
      end;
    end;

    if not IsDotNet481Installed then
    begin
      MsgBox('.NET Framework 4.8.1 installation failed or was cancelled.'#13#13
        'Please install it manually and rerun this setup.',
        mbError, MB_OK);
      Abort;
    end;
  end;
end;
