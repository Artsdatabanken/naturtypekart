SET Configuration=%1
pushd %~dp0\..
mkdir report
mkdir report\CodeCoverage

test\Test.Smoke\bin\%Configuration%\net46\win7-x64\dotnet-test-nunit.exe test\Test.Smoke\bin\%Configuration%\net46\win7-x64\test.Smoke.dll -result=report\Smoke-test-results3.xml 
rem script\xsl.exe report\Smoke-test-results3.xml script\nunit3-junit.xslt  -o report\Smoke-test-results.utf16.xml

popd

