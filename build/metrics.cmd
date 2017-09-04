@ECHO OFF
SETLOCAL EnableDelayedExpansion
pushd %~dp0\..

IF NOT EXIST build         mkdir build
IF NOT EXIST build\metrics mkdir build\metrics
IF NOT EXIST build\report  mkdir build\report

for /r %%a in (*.exe *.dll *.pdb) do (
  COPY "%%a" "build\metrics\" >nul
)

cd build\metrics
echo _METRICS:
dir
SET ARGS=
for %%a in (*.exe *.dll) do (
  if exist %%~na.pdb (
    SET ARGS=!ARGS! /f:"%%a"
  )
)

rem temp HACK
rem copy api.exe api.document.exe 

rem "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Team Tools\Static Analysis Tools\FxCop\metrics" %ARGS% /out:..\report\metrics.xml
popd
exit 0
