#ifndef MyAppVersion
  #define MyAppVersion "0.3.0"
#endif

#define MyAppName "SwitchBack"
#define MyAppPublisher "err0r4o4-dev"
#define MyAppExeName "SwitchBack.exe"

[Setup]
AppId={{17C8B287-DF79-4A21-81F2-F638080AA75C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\artifacts
OutputBaseFilename=SwitchBack-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\src\SwitchBack.App\Assets\SwitchBack.ico
PrivilegesRequired=lowest
LanguageDetectionMethod=uilanguage
ShowLanguageDialog=yes
ArchitecturesAllowed=x86compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "thai"; MessagesFile: "compiler:Languages\Thai.isl"

[CustomMessages]
english.DesktopIcon=Create a desktop shortcut
thai.DesktopIcon=สร้างทางลัดบนเดสก์ท็อป
english.AdditionalShortcuts=Additional shortcuts:
thai.AdditionalShortcuts=ทางลัดเพิ่มเติม:
english.LaunchApp=Launch SwitchBack
thai.LaunchApp=เปิด SwitchBack

[Tasks]
Name: "desktopicon"; Description: "{cm:DesktopIcon}"; GroupDescription: "{cm:AdditionalShortcuts}"; Flags: unchecked

[Files]
Source: "..\artifacts\publish\win-x86\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: not IsWin64
Source: "..\artifacts\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsWin64

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--ui-language={language}"; Description: "{cm:LaunchApp}"; Flags: nowait postinstall skipifsilent
