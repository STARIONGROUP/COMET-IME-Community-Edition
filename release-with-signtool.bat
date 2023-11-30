@echo off

ECHO.
ECHO This release script requires powershell to be runnable from cmd.
ECHO Be sure to have the Visual Studio MSBUILD\Bin folder in your local environment PATH settings, as this is the only correct way to build new SDK style projects using legacy WIX (3.1)
ECHO.

IF %1.==. GOTO VersionError
set version=%1
set installers86path=.\CDP4IMEInstaller\bin\Release\CDP4-COMET-CE.x86.msi
set filename86=CDP4-COMET-CE.x86_%version%.msi
set installers64path=.\CDP4IMEInstaller\bin\x64\Release\CDP4-COMET-CE.x64.msi
set filename64=CDP4-COMET-CE.x64_%version%.msi

set releasefolder=.\Release

GOTO Setup

:VersionError
ECHO.
ECHO ERROR: No version was specified
ECHO.

GOTO End

:Setup

ECHO Releasing IME Version %version%

:Begin

ECHO.
ECHO Creating release folders
ECHO.

if not exist %releasefolder% mkdir %releasefolder%
if exist %releasefolder%\%version% rmdir %releasefolder%\%version% /S /Q
if not exist %releasefolder%\%version% mkdir %releasefolder%\%version%

ECHO.
ECHO Building x86 Release Version
ECHO.

set platform=x86

call MSBuild.exe CDP4-CE.sln -target:Clean -p:Configuration=Release;Platform=x86
call MSBuild.exe .\CDP4PluginPackager\CDP4PluginPackager.csproj -p:Configuration=Release;Platform=x86
call MSBuild.exe CDP4-CE.sln -restore -p:Configuration=Release;Platform=x86

ECHO Error Level %errorlevel%

IF %errorlevel%==1 GOTO BuildError

ECHO.
ECHO Moving x86 installer
ECHO.

call powershell -Command "& {Copy-Item -Path %installers86path% -Destination %releasefolder%\%version%\%filename86%;}"

ECHO.
ECHO Release x86 %version% Completed
ECHO.

ECHO.
ECHO Building x64 Release Version
ECHO.

set platform=x64

call MSBuild.exe CDP4-CE.sln -target:Clean -p:Configuration=Release;Platform=x64
call MSBuild.exe .\CDP4PluginPackager\CDP4PluginPackager.csproj -p:Configuration=Release;Platform=x86
call MSBuild.exe CDP4-CE.sln -restore -p:Configuration=Release;Platform=x64

ECHO Error Level %errorlevel%

IF %errorlevel%==1 GOTO BuildError

ECHO.
ECHO Moving x64 installer
ECHO.

call powershell -Command "& {Copy-Item -Path %installers64path% -Destination %releasefolder%\%version%\%filename64%;}"

ECHO.
ECHO Release x64 %version% Completed
ECHO.

ECHO.
ECHO Generating Verification Hashes
ECHO.

call "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\SignTool" sign /n "RHEA System" /td sha256 /fd sha256 %releasefolder%\%version%\%filename86%
call "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64\SignTool" sign /n "RHEA System" /td sha256 /fd sha256 %releasefolder%\%version%\%filename64%

call powershell -Command "& {Get-ChildItem -Path %releasefolder%\%version% -Recurse -Filter *.msi | Get-FileHash -Algorithm MD5 | Format-List | Out-File %releasefolder%\%version%\verification_hashes_%version%.txt}"
call powershell -Command "& {Get-ChildItem -Path %releasefolder%\%version% -Recurse -Filter *.msi | Get-FileHash | Format-List | Out-File %releasefolder%\%version%\verification_hashes_%version%.txt -Append}"

:End

ECHO.
ECHO Done
ECHO.

EXIT /B 0

:BuildError

ECHO.
ECHO Release %version% %platform% Failed
ECHO.
ECHO Possible causes: 
ECHO - This release script requires powershell to be runnable from cmd.
ECHO - Be sure to have the Visual Studio MSBUILD\Bin folder in your local environment PATH settings, as this is the only correct way to build SDK style projects using legacy WIX (3.1)
ECHO.

EXIT /B 1