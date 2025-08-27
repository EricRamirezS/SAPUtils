using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SAPUtils
{
    public partial class SapAddon
    {
        /// <summary>
        /// Generates the necessary setup files for the specified SAP addon based on the provided addon information.
        /// </summary>
        /// <param name="addonInformation">An object containing details about the SAP addon for which the setup files need to be generated.</param>
        public static void GenerateSetupFiles(AddonInformation addonInformation)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string csprojPath = Directory.GetFiles(baseDir, "*.csproj", SearchOption.AllDirectories)
                    .FirstOrDefault()
                ?? SearchCsprojUpwards(baseDir);
            if (csprojPath == null)
                throw new FileNotFoundException("No se encontró ningún archivo .csproj en la ruta del ejecutable o carpetas superiores.");

            string projectDir = Path.GetDirectoryName(csprojPath);
            string csprojFileName = Path.GetFileName(csprojPath);

            XDocument csprojXml = XDocument.Load(csprojPath);
            string assemblyName = csprojXml
                    .Descendants("AssemblyName")
                    .FirstOrDefault()?.Value
                ?? Path.GetFileNameWithoutExtension(csprojPath);

            string binFileName = $"{assemblyName}.exe";
            string platformFlag = addonInformation.X64 ? "X" : "N";
            string batFileName = addonInformation.X64 ? "Buildx64.bat" : "Buildx86.bat";

            string batPath = Path.Combine(projectDir ?? string.Empty, batFileName);
            string issPath = Path.Combine(projectDir ?? string.Empty, $"{addonInformation.AddonName}.iss");

            string batContent = $@"
chcp 65001 >nul
:: Rutas y nombres
set BaseDir=%~dp0
set csproj=""{csprojFileName}""
set BinDir=""%~dp0\bin\Release""
set BinFile=""%~dp0\bin\Release\{binFileName}""
set AddOnExecutable=""{binFileName}""
set SetupFile=""%~dp0\Output\Setup.exe""
set OutputXml=""%~dp0Output\Setup.ard""

:: Metadata del Addon
set PartnerName=""{addonInformation.PartnerName}""
set PartnerNamespace=""{addonInformation.PartnerNamespace}""

set AddonName=""{addonInformation.AddonName}""
set Version={addonInformation.Version}
set X64=""{platformFlag}""
set ContactData=""{addonInformation.ContactData}""

call ""C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat""

msbuild %BaseDir%\%csproj% /t:Clean,Build /p:Configuration=Release;Platform={addonInformation.Platform.ToString()}
set BUILD_STATUS=%ERRORLEVEL%
if %BUILD_STATUS%==0 GOTO INNO
pause
EXIT 
	
:INNO
""C:\Program Files (x86)\Inno Setup 5\ISCC.exe"" ""%~dp0\{addonInformation.AddonName}.iss""
set INNO_STATUS=%ERRORLEVEL%
if %INNO_STATUS%==0 GOTO ARD
pause
EXIT

:ARD
setlocal EnableDelayedExpansion

for /f %%i in ('powershell -command ""(Get-FileHash -Algorithm MD5 -Path '%BinFile%').Hash""') do set ""AddonSig=%%i""
if errorlevel 1 (
    goto :HandleError
)
for /f %%i in ('powershell -command ""(Get-FileHash -Algorithm MD5 -Path '%SetupFile%').Hash""') do set ""InstSig=%%i""
if errorlevel 1 (
    goto :HandleError
)
powershell -NoProfile -Command ^
    ""$xml = '<?xml version=\""1.0\"" encoding=\""UTF-16\""?><AddOnRegData><addon addonexe=\""%AddOnExecutable%\"" addongroup=\""M\"" addonname=\""%AddonName%\"" addonsig=\""%AddonSig%\"" addonver=\""%Version%\"" clienttype=\""W\"" contdata=\""%ContactData%\"" esttime=\""20\"" instname=\""Setup.exe\"" instparams=\""\"" instsig=\""%InstSig%\"" partnername=\""%PartnerName%\"" partnernmsp=\""%PartnerNamespace%\"" platform=\""%X64%\"" silentinst=\""\"" silentugd=\""\"" silentuninst=\""\"" ugdcmdargs=\""\"" ugdesttime=\""\"" ugdname=\""\"" ugdsig=\""\"" uncmdarg=\""/u\"" unesttime=\""20\"" uninstname=\""Setup.exe\"" uninstsig=\""%InstSig%\"" zipnameinst=\""\"" zipnameugd=\""\"" zipnameuninst=\""\"" zipsiginst=\""\"" zipsigugd=\""\"" zipsiguninst=\""\""/></AddOnRegData>'; [System.IO.File]::WriteAllText('%OutputXml%', $xml, [System.Text.Encoding]::Unicode)""
if errorlevel 1 (
    goto :HandleError
)
echo Archivo XML generado correctamente en: %OutputXml%
%SystemRoot%\explorer.exe ""%~dp0Output""
EXIT /b 0

:HandleError
echo Error durante la creación del ARD.

set ""GEN_PATH=C:\Program Files (x86)\SAP\SAP Business One SDK\Tools\AddOnRegDataGen\AddOnRegDataGen.exe""
if exist ""%GEN_PATH%"" (
    start """" ""%GEN_PATH%""
) else (
    echo ""AddOnRegDataGen"" no encontrado en: %GEN_PATH%
)
pause
EXIT /b 1
";

            File.WriteAllText(batPath, batContent.Trim(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            string filesSection = string.Join("\n", addonInformation.Files.Select(f => $"Source: {f.Source}; DestDir: {{app}}{f.DestinationDir}"));
            string dirsSection = string.Join("\n", addonInformation.Dirs.Select(d => $"Name: \"{{app}}\\{d}\""));

            string issContent = $@"
[Files]
{filesSection}

[Dirs]
{dirsSection}

[Setup]
UsePreviousLanguage=no
AppName={addonInformation.AddonName}
VersionInfoVersion={addonInformation.Version}
AppPublisher={addonInformation.PartnerNamespace}
AppVerName={addonInformation.AddonName} {addonInformation.Version}
AppPublisherURL={addonInformation.AppPublisherUrl}
AppSupportURL={addonInformation.AppSupportUrl}
AppUpdatesURL={addonInformation.AppUpdatesUrl}
DefaultDirName={{code:GetDefaultAddOnDir}}
;OutputBaseFileName={{code:GetSetupName}}
OutputBaseFileName=Setup
DisableDirPage=true
Compression=lzma
SolidCompression=true
UsePreviousAppDir=false
AppendDefaultDirName=true
PrivilegesRequired=admin
WindowVisible=false
AppContact={addonInformation.ContactData}

[Messages]
BeveledLabel={addonInformation.BeveledLabel}

[Registry]
Root: HKLM; Subkey: {addonInformation.Registry}\{{code:GetAddOnName}}; ValueType: string; ValueName: InstallDir; ValueData: {{code:GetDefaultAddOnDir}}; Flags: uninsdeletevalue

[Languages]
Name: spanish; MessagesFile: compiler:Languages\\Spanish.isl

[UninstallDelete]
Type: files; Name: {{app}}\\{{code:GetSetupName}}.EXE
Name: {{app}}\\*.dll; Type: files; Languages:

[Code]
type
   TSHFileOpStruct = record
     Wnd    				: HWND;
     wFunc  				: UINT;
     pFrom  				: PChar;
     pTo    				: PChar;
     fFlags  				: Word;
     fAnyOperationsAborted 	: BOOL;
     hNameMappings			: HWND;
     lpszProgressTitle: PChar;
   end;

const
   {{ $EXTERNALSYM FO_COPY }}
   FO_COPY = $0002;
   {{ $EXTERNALSYM FOF_SILENT }}
   FOF_SILENT = $0004;
   {{ $EXTERNALSYM FOF_NOCONFIRMATION }}
   FOF_NOCONFIRMATION = $0010;

var
CurrentLocation : string;
AddOnDir        : string;
FinishedInstall : Boolean;
Params          : string;
i               : integer;

//Copy
function SHFileOperation(const lpFileOp: TSHFileOpStruct):Integer; external 'SHFileOperation@shell32.dll stdcall';


//SAP B1
function EndInstallEx(Dir : String; Ok:Boolean): integer; external 'EndInstallEx@files:AddOnInstallAPI.dll stdcall';
function EndUninstall(path: string; succeed: Boolean): integer; external 'EndUninstall@files:AddOnInstallAPI.dll stdcall';
function SetAddOnFolder(srcPath : string): Integer; external 'SetAddOnFolder@files:AddOnInstallAPI.dll stdcall';
function RestartNeeded :integer; external 'RestartNeeded@files:AddOnInstallAPI.dll stdcall delayload ';

function ExistStrInParam(StrInParam:string) : boolean;
var
 j : integer;
begin
result:=false;
  for j := 0 to ParamCount do
  if UpperCase(ParamStr(j)) = UpperCase(StrInParam) then
  begin
   result := true;
   break;
  end;
end;

function GetAddOnName(dummy: String): string;
begin
   result := '{addonInformation.AddonName}';
end;

function GetSetupName(dummy: String): string;
begin
   result := 'Setup';
end;

function PreparePaths() : Boolean;
var
  position : integer;
  aux : string;
begin
   if pos('|', paramstr(2)) <> 0 then //la ruta puede venir en el parametro 2 o 4
   begin
      aux := paramstr(2);
      position := Pos('|', aux);
      AddOnDir := Copy(aux,1, position - 1);
      Result := True;
   end
   else
   if pos('|', paramstr(4)) <> 0 then //la ruta puede venir en el parametro 2 o 4
   begin
      aux := paramstr(4);
      position := Pos('|', aux);
      AddOnDir := Copy(aux,1, position - 1);
      Result := True;
   end
   else
   if not ExistStrInParam('/U') then
   begin
     //result:=True;
     MsgBox('El Instalador debe ser ejecutado desde SAP Business One.', mbInformation, MB_OK);
     EndInstallEx('',false); //Avisa a Sap B1 que se aborto la instalacion
     Result := False;
   end;
end;

function GetDefaultAddOnDir(Param : string): string;
begin
   result := AddOnDir;
end;


function InitializeSetup(): Boolean;
var
 i, ResultCode      : Integer;
 UninstallerPath : String;
begin
   Params := '';
   for i:=0 to ParamCount do
   Params:=Params + ' Param' + inttoStr(i) + ' = ' + paramstr(i) + #13;
   //MsgBox(Params, mbInformation, MB_OK);

   if ExistStrInParam('/U') then
   begin
    if RegQueryStringValue(HKEY_LOCAL_MACHINE, '{addonInformation.Registry}\'+GetAddOnName(''),'InstallDir', CurrentLocation) then
    UninstallerPath := CurrentLocation + '\unins000.exe';
    //MsgBox('UninstallerPath '+UninstallerPath, mbInformation, MB_OK);
    Exec(UninstallerPath, '/SILENT', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
    EndUninstall('', true);
    result:=False;
   end
   else
   begin
     result := PreparePaths;
   end;
end;

function NextButtonClick(CurPageID : Integer): Boolean;
var
 LRes : Integer;
begin
   Result := True;
   case CurPageID of
       wpSelectDir :
       begin
           AddOnDir := ExpandConstant('{{app}}');
       end;
       wpFinished :
       begin
          if FinishedInstall then
          begin
             SetAddOnFolder(AddOnDir);
             LRes := EndInstallEx('', True);
			 if LRes <> 0 then
			  MsgBox('EndInstallEx Value : ' + IntToStr(LRes), mbInformation, MB_OK);
			   
          end;
       end;
   end;
end;

Function ExtractFileNameParam(StrParam : string) : string;
var
 j: integer;
begin
 Result := StrParam;
 j := 1;
 repeat
  Result := Copy(StrParam, j, Length(StrParam));
  j := j + 1;
 until FileExists(Result) or (j >= Length(StrParam));
end;

procedure CopyFile(FromFileName, ToFileName: string);
var
  ShellInfo: TSHFileOpStruct;
  Files    : String;
begin
  Files := FromFileName+#0+#0;
  ShellInfo.wFunc := FO_COPY;
  ShellInfo.pFrom := PChar(Files);
  ShellInfo.pTo   := PChar(ToFileName);
  ShellInfo.fFlags := FOF_NOCONFIRMATION or FOF_SILENT;
  SHFileOperation(shellinfo);
end;

procedure CurStepChanged(CurStep: TSetupStep);
Var
SetupFile : string;
begin
   if CurStep = ssPostInstall then
   begin
    SetupFile := ExtractFileNameParam(ParamStr(1));
    DeleteFile(AddOnDir + '\' + GetSetupName('') + '.exe');
    CopyFile(SetupFile, AddOnDir + '\' + GetSetupName('') + '.exe');
    FinishedInstall := True;
   end;
end;


procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
{{ no se usa por el momento debido a que el instalador llama al unist000.exe
	case CurUninstallStep of
		ssDone : EndUninstall('', true);
	end;
}}
end;

";

            File.WriteAllText(issPath, issContent.Trim(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            Console.WriteLine($"Archivos generados:\n{batPath}\n{issPath}");
        }

        private static string SearchCsprojUpwards(string startDir)
        {
            DirectoryInfo dir = new DirectoryInfo(startDir);
            while (dir != null)
            {
                FileInfo csproj = dir.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (csproj != null)
                    return csproj.FullName;

                dir = dir.Parent;
            }
            return null;
        }
    }

    /// <summary>
    /// Represents metadata and configuration information required for an SAP add-on setup process.
    /// </summary>
    public class AddonInformation
    {
        /// <summary>
        /// Gets or sets the name of the partner associated with the add-on.
        /// </summary>
        public string PartnerName { get; set; }

        /// <summary>
        /// Gets or sets the partner namespace associated with the addon.
        /// This property is intended to represent the namespace
        /// under which the partner's functionality is encapsulated.
        /// </summary>
        public string PartnerNamespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the add-on.
        /// </summary>
        public string AddonName { get; set; }

        /// <summary>
        /// Gets or sets the version information related to the SAP Addon.
        /// </summary>
        /// <remarks>
        /// The property represents the versioning details of the SAP Addon,
        /// providing a way to identify its current version.
        /// It can be utilized to track changes or updates within the addon functionality.
        /// </remarks>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the platform architecture for which the SAP add-on is intended to run.
        /// </summary>
        /// <remarks>
        /// The platform can be specified as AnyCPU, x86, or x64 to ensure compatibility with the target system.
        /// </remarks>
        /// <seealso cref="SAPUtils.Platform"/>
        public Platform Platform { get; set; }

        /// <summary>
        /// Represents a property indicating whether the add-on is built for a 64-bit architecture.
        /// </summary>
        public bool X64 { get; set; }

        /// <summary>
        /// Gets or sets the contact information associated with an add-on.
        /// </summary>
        /// <remarks>
        /// The ContactData property is intended to store details regarding the contact
        /// for the SAP add-on, which could include relevant communication information
        /// related to the add-on's support or development team.
        /// </remarks>
        public string ContactData { get; set; }

        /// <summary>
        /// Gets or sets a collection of file mappings where each entry consists of a source file path and its corresponding destination directory.
        /// </summary>
        /// <remarks>
        /// This property is intended to manage and store pairs of source file locations and their respective target directories,
        /// facilitating file copying or organization operations.
        /// </remarks>
        public List<(string Source, string DestinationDir)> Files { get; set; }

        /// <summary>
        /// Gets or sets an array of directory paths.
        /// </summary>
        public string[] Dirs { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the URL of the application publisher.
        /// </summary>
        public string AppPublisherUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for application support.
        /// This property provides the link to access resources or support materials
        /// related to the application.
        /// </summary>
        public string AppSupportUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for application updates.
        /// </summary>
        /// <remarks>
        /// This property represents the location from which updates for the application can be retrieved.
        /// The value should be a valid URL pointing to the update source, such as a web server or cloud-hosted update repository.
        /// </remarks>
        public string AppUpdatesUrl { get; set; }

        /// <summary>
        /// Gets or sets the value of the BeveledLabel property.
        /// </summary>
        /// <remarks>
        /// The BeveledLabel property is designed to hold a string value that represents a specific label
        /// with a beveled appearance. This property can be used to assign or retrieve relevant label
        /// information within the context of the SAPUtils namespace.
        /// </remarks>
        public string BeveledLabel { get; set; }

        /// <summary>
        /// Represents the registry information used for storing and retrieving settings
        /// related to the SAP Addon. This property provides access to registry-specific details
        /// essential for managing configuration and integration.
        /// </summary>
        public string Registry { get; set; }
    }

    /// <summary>
    /// Specifies the platform architecture for which the MSBUILD is intended to run.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Platform
    {
        /// <summary>
        /// Represents a platform configuration that allows the output of a build to run on any CPU architecture.
        /// This setting is commonly used to create applications that can run on both x86 (32-bit) and x64 (64-bit) systems.
        /// </summary>
        /// <remarks>
        /// When targeting this platform, the application will dynamically adapt to the architecture of the host machine at runtime.
        /// </remarks>
        /// <seealso cref="Platform"/>
        AnyCPU,

        /// <summary>
        /// 
        /// </summary>
        x86,

        /// <summary>
        /// Represents a platform configuration targeting the 64-bit architecture (x64).
        /// This setting is optimized for applications designed to run on systems utilizing x64 instruction sets.
        /// </summary>
        /// <remarks>
        /// Use this platform setting when building applications specifically for 64-bit environments, ensuring compatibility and taking full advantage of the 64-bit architecture's capabilities.
        /// </remarks>
        /// <seealso cref="Platform"/>
        x64
    }
}