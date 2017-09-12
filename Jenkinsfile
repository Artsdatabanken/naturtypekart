#!/usr/bin/groovy
def BuildConfiguration='Debug'
echo '--- Source control branch: ' + env.BRANCH_NAME
echo '--- Build configuration:   ' + BuildConfiguration
echo '--- Build URL:             ' + env.BUILD_URL

void failure_notification(Closure task) {
  try {
    task()
  }catch (e) {
    slack_notification('danger', 'Build has failed! Check the build url below.')
    throw e
  }
}
pipeline {
	agent any
  parameters {
    string(name: 'BuildConfiguration', defaultValue: 'Release', description: 'Debug/Release/...')
  }
	triggers { pollSCM('H/2 * * * *') }
	options {
//		failure_notification()
		timestamps()
		ansiColor 'xterm'
	}
	environment {
		BuildToolRelPath = 'build'
		REACT_APP_BASENAME = 'nin_${env.BRANCH_NAME}'
	}

	stages() {
		stage('Frontend deps & lint') {
			steps {

echo "Building configuration: ${params.BuildConfiguration}"
echo "Building configuration:  " + params.BuildConfiguration
	    notifyBuild('STARTED')
//	deleteDir()
			}
		}

		stage('Backend build') {
			steps {
				echo 'Commit hash: ' + gitCommitHash()

				dotnetBuild('src', BuildConfiguration)
				dotnetBuild('test', 'Debug')
				copyToDist(binPath('src\\console', BuildConfiguration))
	//		stash includes: 'database/**', name: 'database'
	//		stash includes: 'test/*/bin/'+BuildConfiguration+'/net47/win7-x64/**', name: 'test'
  			archiveArtifacts 'dist/*'

// broken with x64 build? and won't build others?
//			  calculateDotnetCodeMetrics()
//				archiveArtifacts('build\\report\\metrics.xml')
//				build job: 'Nin Metrics', parameters: [[$class: 'StringParameterValue', name: 'MetricsFile', value: workspace+'\\build\\report\\metrics.xml']]
			}
		}
// 	//				unstash 'test'
		stage('Unit tests') {
			steps {
				runTestsWithCoverage('Unit', 'report\\CodeCoverage')
//				publishHTML(target: [reportDir:'report/CodeCoverage', reportFiles: 'index.htm', reportName: 'Unit test code coverage'])
			}
		}
		stage('Integration tests') {
			steps {
					runTestsWithCoverage('Integration', 'report\\CodeCoverage')
//				publishHTML(target: [reportDir:'report/CodeCoverage', reportFiles: 'index.htm', reportName: 'Integration test code coverage'])
			}
		}
		stage('Email') {
			steps {
				step([$class: 'Mailer', notifyEveryUnstableBuild: true, recipients: 'bjorn.reppen@artsdatabanken.no', sendToIndividuals: true])
			}
		}
		stage('Deploy') {
			steps {
//				createDatabase('Test','database')

		    dotnetPublish("Api.Document", BuildConfiguration)
		    dotnetPublish("Api", BuildConfiguration)

    		copy('web', '\\\\it-webadbtest01.it.ntnu.no\\d$\\Websites\\Nin\\' + env.BRANCH_NAME)
			}
		}
	}
}

//-----

def basename() {
	return 'nin_'+env.BRANCH_NAME
}

def deployWeb(baseUnc, appName) {
		dir('Prod.Web\\build') { // client
			deploy('.', baseUnc+'\\Fab\\'+env.BRANCH_NAME)
	}
}

def addIisApplication(appName, virtualPath, physicalPath) {
	iisAppCmd("add app /site.name:"+appName+" /path:"+virtualPath+" /physicalpath:"+physicalPath)
	iisAppCmd("set app "+appName + virtualPath+" /applicationpool:"+appName)
}

def iisAppCmd(args) {
	bat 'C:\\Windows\\System32\\inetsrv\\appcmd.exe ' + args + " & exit 0" // ignore already exists
}

def dotnetPublish(projectSubDir, BuildConfiguration) {
	mkdir('dist\\' + projectSubDir)
  bat 'dotnet publish src\\' + projectSubDir + ' --configuration ' + BuildConfiguration
  bat 'pskill -t \\\\it-webadbtest01.it.ntnu.no ' + projectSubDir + '.exe /accepteula & EXIT 0'
  
	copy('src\\' + projectSubDir + '\\bin\\debug\\net47\\publish', '\\\\it-webadbtest01.it.ntnu.no\\d$\\Websites\\Nin\\'+env.BRANCH_NAME+'\\' + projectSubDir + "\\")
}

def copy(src, dest) {
	bat 'xcopy '+ src + ' ' + dest + ' /s /y'
}

def move(src, dest) {
	bat 'move /y  '+ src +' ' + dest
}

def calculateDotnetCodeMetrics() {
  bat '"' + pwd() + '"\\' + BuildToolRelPath + '\\metrics.cmd"'
}

def runTestsWithCoverage(testCategory, reportPath) {
		def	CodeCoveragePath = '"'+ pwd() + '\\' + reportPath + '_' + testCategory + '"'
		def BuildToolPath= '"'+ pwd() + '\\' + BuildToolRelPath + '"'
		def workspace = pwd()
		mkdir(CodeCoveragePath)
		dir('test/Test.'+testCategory+'/bin/Debug/net47/') {
//			nuget('OpenCover\\4.6.519\\tools\\OpenCover.Console', '-target:dotnet-test-nunit.exe -targetargs:"test.'+testCategory+'.dll -result=testresults3.xml" -register:user -output:"coverage.xml"')
//			nuget('OpenCover\\4.6.519\\tools\\OpenCover.Console', '-target:dotnet-test-nunit.exe -targetargs:"test.integration.dll -result=testresults3.xml" -register:user -output:"coverage.xml" -filter:+[Nin*]*')
//			bat BuildToolPath+'\\xsl testresults3.xml '+ BuildToolPath +'\\nunit3-junit.xslt testresults.xml'
//			nuget('OpenCoverToCoberturaConverter\\0.2.4\\tools\\OpenCoverToCoberturaConverter', '-input:coverage.xml -output:cobertura.xml -sources:"'+workspace+'"')
//			nuget('ReportGenerator\\2.4.5\\tools\\ReportGenerator', '-reports:coverage.xml -targetDir:' + CodeCoveragePath)
//			bat 'copy coverage.xml ' + CodeCoveragePath + '\\coverage_' + testCategory+'.xml'
//			junit('testresults.xml')
		}
//		publishHTML(target: [reportDir:'report/CodeCoverage_'+testCategory, reportFiles: 'index.htm', reportName: testCategory+' test code coverage'])
}

def yarn(args) {
	bat '"C:\\program files (x86)\\yarn\\bin\\yarn" ' + args
}

def npm(args) {
	bat 'npm ' + args
}

def nuget(packageRelPath, args) {
  echo 'Nuget: ' + packageRelPath + ': ' +args
	def NugetPackagePath=env.USERPROFILE+'\\.nuget\\packages\\'
	bat NugetPackagePath + packageRelPath + ' ' + args
}

def mkdir(path) {
	bat 'if not exist ' + path + ' mkdir ' + path
}

def createDatabase(environment, scriptPath) {
	retry(count: 3) {
		withEnv(['ASPNETCORE_ENVIRONMENT=' + environment]) {
			console('CreateDatabase ' +scriptPath)
		}
	}
}

def console(args) {
  bat 'dist\\console ' + args
}

def dotnetBuild(rootPath, BuildConfiguration) {
	bat 'nuget add lib\\artsdatabanken.systemintegrasjon2\\1.0.0\\artsdatabanken.systemintegrasjon2.1.0.0.nupkg -Source LocalNuget'
	bat 'dotnet restore -v minimal'
	//dotnet restore -s $HOME/.nuget

	bat 'dotnet build --configuration '+BuildConfiguration
}

def binPath(projectDir, BuildConfiguration) {
	return projectDir + '\\bin\\' + BuildConfiguration + '\\net47'
}

def copyToDist(src) {
	mkdir('dist')
	copy(src+"\\*.*", 'dist\\')
}

def gitCommitHash() {
	gitCommit = bat(returnStdout: true, script: 'git rev-parse HEAD').trim()
	echo gitCommit // full commit hash
	shortCommit = gitCommit.take(6) // short hash
	return shortCommit
}

def notifyBuild(String buildStatus = 'STARTED') {
  // build status of null means successful
  buildStatus = buildStatus ?: 'SUCCESS'

  // Default values
  def colorName = 'RED'
  def colorCode = '#FF0000'
  def subject = "${buildStatus}: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'"
  def summary = "${subject} (${env.BUILD_URL})"
  def details = """<p>STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':</p>
    <p>Check console output at &QUOT;<a href='${env.BUILD_URL}'>${env.JOB_NAME} [${env.BUILD_NUMBER}]</a>&QUOT;</p>"""

  // Override default values based on build status
  if (buildStatus == 'STARTED') {
    color = 'YELLOW'
    colorCode = '#FFFF00'
  } else if (buildStatus == 'SUCCESS') {
    color = 'GREEN'
    colorCode = '#00FF00'
  } else {
    color = 'RED'
    colorCode = '#FF0000'
  }

//slackSend "Build Started - ${env.JOB_NAME} ${env.BUILD_NUMBER} (<${env.BUILD_URL}|Open>)"
 // slackSend (color: colorCode, message: summary)
//  hipchatSend (color: color, notify: true, message: summary)
  emailext (
      subject: subject,
      body: details,
      recipientProviders: [[$class: 'DevelopersRecipientProvider']]
    )
}
