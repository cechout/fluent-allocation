; installer for the WPF version 1, Kurszuteilung
; build-wpf-installer.ps1 publishes the app first and passes the version from Kurszuteilung.csproj via /DMyAppVersion;
; a manual compile falls back to the version below and expects an existing publish output

#define MyAppName "Kurszuteilung"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "Daniel Čech"
#define MyAppExeName "Kurszuteilung.exe"
; must stay in sync with the dotnet publish call in build-wpf-installer.ps1
#define PublishDir "..\Kurszuteilung\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
AppId={{0C4F2DBF-A60A-457F-854D-DA02BCC3C42E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoDescription={#MyAppName} Setup
VersionInfoVersion={#MyAppVersion}.0
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
; the app is published self-contained for win-x64, so setup refuses to run anywhere else
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE.txt
; installs into Program Files and therefore asks for elevation; the app itself never needs it, because it only reads
; the database template in its own folder and works on a copy in the users %LocalAppData%
OutputDir=.\Output
OutputBaseFilename=Kurszuteilung_Installer
SetupIconFile=..\Kurszuteilung\Icons\icon6.ico
SolidCompression=yes
WizardStyle=modern dynamic
; the app is German, so setup follows the Windows display language and only asks when it matches neither
ShowLanguageDialog=auto

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[CustomMessages]
english.MissingPrerequisites=Kurszuteilung needs the following to run, and setup could not find it on this PC:%n%n%1%nThe app can be installed anyway, but it cannot evaluate anything until this is installed as well.%n%nContinue with the installation?
german.MissingPrerequisites=Kurszuteilung benötigt Folgendes, das auf diesem PC nicht gefunden wurde:%n%n%1%nDie App lässt sich trotzdem installieren, kann aber erst auswerten, wenn auch das installiert ist.%n%nMit der Installation fortfahren?
english.PrerequisiteExcel=Microsoft Excel
german.PrerequisiteExcel=Microsoft Excel
english.PrerequisiteLocalDB=SQL Server Express LocalDB, available as a free download from Microsoft
german.PrerequisiteLocalDB=SQL Server Express LocalDB, kostenlos bei Microsoft erhältlich

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

; deliberately no [UninstallDelete] for the working database under %LocalAppData%\Kurszuteilung: LocalDB keeps a
; database registered by the path of its file, so deleting the file while it is still registered makes the next
; attach at that path fail after a reinstall; left in place it is simply picked up again

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// excel is automated over COM, so its ProgID is registered wherever it is installed
function IsExcelInstalled: Boolean;
begin
  Result := RegKeyExists(HKCR, 'Excel.Application');
end;

// every installed LocalDB version adds a subkey here, for example 17.0 for SQL Server 2025
function IsLocalDBInstalled: Boolean;
var
  Versions: TArrayOfString;
begin
  Result := RegGetSubkeyNames(HKLM64, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions', Versions)
    and (GetArrayLength(Versions) > 0);
end;

// a missing prerequisite is a warning, not a stop: either one can be installed before or after the app
function InitializeSetup: Boolean;
var
  Missing: String;
begin
  Result := True;
  Missing := '';

  if not IsExcelInstalled then
    Missing := Missing + '- ' + CustomMessage('PrerequisiteExcel') + #13#10;
  if not IsLocalDBInstalled then
    Missing := Missing + '- ' + CustomMessage('PrerequisiteLocalDB') + #13#10;

  if Missing <> '' then
    Result := SuppressibleMsgBox(FmtMessage(CustomMessage('MissingPrerequisites'), [Missing]), mbConfirmation, MB_YESNO, IDYES) = IDYES;
end;
