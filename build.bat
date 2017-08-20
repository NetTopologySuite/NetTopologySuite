@echo off

REM we need a better way to detect where MSBuild is located.

set msbuild="C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"
if not exist %msbuild% (
	echo "Error trying to find MSBuild executable"
	exit 1
)
set SolutionDir=%~dp0

%msbuild% %SolutionDir%NetTopologySuite.sln /verbosity:minimal /property:Configuration=Release
