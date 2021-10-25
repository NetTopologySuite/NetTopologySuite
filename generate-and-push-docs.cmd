@echo off
set DOCFX_CONSOLE_PACKAGE_VERSION=2.45.0
set GITBRANCH=
for /f %%I in ('git.exe rev-parse --abbrev-ref HEAD 2^> NUL') do set GITBRANCH=%%I
if "%GITBRANCH%" neq "main" (
    echo This should only be used on the main branch, for the sake of "View Source" links.
    exit /b 1
)
pushd %~dp0
dotnet restore
pushd tools
rd /s /q docfx.console.%DOCFX_CONSOLE_PACKAGE_VERSION%
nuget install docfx.console -Version %DOCFX_CONSOLE_PACKAGE_VERSION%
popd
%~dp0tools\docfx.console.%DOCFX_CONSOLE_PACKAGE_VERSION%\tools\docfx doc\docfx.json
pushd doc\obj
rd /s /q gh-pages
git clone --branch gh-pages %~dp0 gh-pages
pushd gh-pages
git rm -r .
xcopy /Q /E /R /Y ..\generated-site-content .
git add .
git commit -m "Update docs.  This was performed automatically."
git push origin gh-pages
popd
rd /s /q gh-pages
popd
git push origin gh-pages
popd
