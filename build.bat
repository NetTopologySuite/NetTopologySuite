@echo off

echo Looking for VS 2015/17 Installation folder
if not exist "packages\vswhere.2.4.1\" (
	echo ... via env variable
	set InstallDir="%VSINSTALLDIR%"
) else (
	echo ... via Microsoft\vswhere package
	for /f "usebackq tokens=*" %%i in (`packages\vswhere.2.4.1\tools\vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
		set InstallDir="%%i\"
	)
)

if "%InstallDir:"=%"=="" (
	echo "Error trying to determine VS installation"
	exit /b 1
)

set msbuild="%InstallDir:"=%MSBuild\15.0\Bin\msbuild.exe"
if not exist %msbuild% (
	echo "Error trying to find MSBuild executable"
	exit /b 1
)

set SolutionDir=%~dp0

echo MsBuild location: %msbuild%
%msbuild% %SolutionDir%NetTopologySuite.sln /t:"Restore;Build" /v:minimal /p:Configuration=Release
