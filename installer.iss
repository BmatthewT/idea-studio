[Setup]
AppId=IdeaStudio.Desktop
AppName=Idea Studio
AppVersion=4.0.0
DefaultDirName={localappdata}\Programs\Idea Studio
DefaultGroupName=Idea Studio
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=dist
OutputBaseFilename=IdeaStudio-Setup-4.0.0
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile=IdeaStudio\Assets\app.ico
UninstallDisplayIcon={app}\IdeaStudio.exe
CloseApplications=no

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Idea Studio"; Filename: "{app}\IdeaStudio.exe"
Name: "{group}\Uninstall Idea Studio"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\IdeaStudio.exe"; Description: "Open Idea Studio"; Flags: nowait postinstall skipifsilent






