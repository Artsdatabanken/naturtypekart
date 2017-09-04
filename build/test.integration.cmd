SET Configuration=%1
pushd %~dp0\..
mkdir report
mkdir report\CodeCoverage

"%USERPROFILE%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console" -target:test\Test.Integration\bin\%Configuration%\net46\win7-x64\dotnet-test-nunit.exe -targetargs:"test\Test.Integration\bin\%Configuration%\net46\win7-x64\test.integration.dll -result=report\integration-test-results3.xml" -register:user -output:"report\%JOB_BASE_NAME%_coverage.xml" -filter:+[Nin*]*
script\msxsl.exe report\integration-test-results3.xml script\nunit3-xunit.xslt  -o report\integration-test-results.utf16.xml
"%USERPROFILE%\.nuget\packages\OpenCoverToCoberturaConverter\0.2.4\tools\OpenCoverToCoberturaConverter" -input:"report\%JOB_BASE_NAME%_coverage.xml" -output:"report\%JOB_BASE_NAME%_cobertura.xml" -sources:src\
"%USERPROFILE%\.nuget\packages\ReportGenerator\2.4.5\tools\ReportGenerator" -reports:"report\%JOB_BASE_NAME%_coverage.xml" -targetDir:report\CodeCoverage

popd

