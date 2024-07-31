pipeline
{
    agent any
    
	parameters {
        string(name: 'version', defaultValue: params.version ? params.version : '1.0.0')
        string(name: 'prerelease_version', defaultValue: params.prerelease_version ? params.prerelease_version : '')
		string(name: 'steam_branch', defaultValue: params.steam_branch ? params.steam_branch : '')
    }
    options {
        buildDiscarder(logRotator(numToKeepStr: '100', artifactNumToKeepStr: '100'))
    }
    stages {
        stage ('Setup Build Environment') {
            steps {
                script {
					STEAM_BRANCH = (params.steam_branch == null || params.steam_branch.allWhitespace) ? '' : params.steam_branch
				}
			
                sh "sudo sh setup-dev-linux.sh ${STEAM_BRANCH}"
            }
        }
        stage ('Build Release') {
            steps {
                script {
                    BUILD_CONFIGURATION = env.BRANCH_NAME == 'stable' ? "Release" : "Debug"
                }

                sh "sudo msbuild ./Source/ModManager.csproj /p:Configuration=${BUILD_CONFIGURATION}"
            }
        }
    }
    post {
        success {
            script {
                GIT_COMMIT_HASH = sh (
                    script: "sudo git log -n 1 --pretty=format:'%h'",
                    returnStdout: true
                ).trim()

                GIT_COMMIT_COUNT = sh (
                    script: "git rev-list --count HEAD",
                    returnStdout: true
                ).trim()

                OLD_MODINFO_VERSION = sh (
                    script: "xmlstarlet sel -t -v '/ModInfo/Version/@value' 000-ModManager/ModInfo.xml",
                    returnStdout: true
                ).trim()

                OLD_MANIFEST_VERSION = sh (
                    script: "xmlstarlet sel -t -v '/ModManifest/Version/text()' 000-ModManager/Manifest.xml",
                    returnStdout: true
                ).trim()

                MODINFO_VERSION = params.version
                MANIFEST_VERSION = params.version + ((params.prerelease_version == null || params.prerelease_version.allWhitespace) ? '' : ('-' + params.prerelease_version))

                withCredentials([usernamePassword(credentialsId: "${env.CREDENTIALS}", usernameVariable: 'USER', passwordVariable: 'PASSWORD')]) {
                    sh "git config --global user.email '${env.CREDENTIALS_EMAIL}'"
                    sh "git config --global user.name \$USER"
                    
                    sh "git checkout -b ${env.BRANCH_NAME}"
                    sh "git pull"

                    if(env.BRANCH_NAME == 'dev') {
                        if(OLD_MODINFO_VERSION != MODINFO_VERSION || OLD_MANIFEST_VERSION != MANIFEST_VERSION) {
                            sh "sudo xmlstarlet edit --inplace --update '/ModInfo/Version/@value' --value '${MODINFO_VERSION}' 000-ModManager/ModInfo.xml"
                            sh "sudo xmlstarlet edit --inplace --update '/ModManifest/Version' --value '${MANIFEST_VERSION}' 000-ModManager/Manifest.xml"

                            sh "git add 000-ModManager/ModInfo.xml 000-ModManager/Manifest.xml"
                            sh "git commit -m 'Updated version to ${MANIFEST_VERSION}'."
                        }

                        try {                    
                            UPDATED_GAME_VERSION = sh (
                                script: "mono ../../VersionRelease.exe Dependencies/7DaysToDieServer_Data/Managed/Assembly-CSharp.dll 000-ModManager/Manifest.xml",
                                returnStdout: true
                            ).trim()

                            sh "git add 000-ModManager/Manifest.xml"
                            sh "git commit -m '${UPDATED_GAME_VERSION}'"
                        } catch (err) {

                        }
                    }

                    sh "git push https://\$USER:\$PASSWORD@github.com/FilUnderscore/ModManager.git ${env.BRANCH_NAME}"
                }

                sh "sudo xmlstarlet edit --inplace --update '/ModInfo/Version/@value' --value '${MODINFO_VERSION}.${GIT_COMMIT_COUNT}' 000-ModManager/ModInfo.xml"
                sh "sudo xmlstarlet edit --inplace --update '/ModManifest/Version' --value '${MANIFEST_VERSION}+${env.BRANCH_NAME}.${GIT_COMMIT_COUNT}.${GIT_COMMIT_HASH}' 000-ModManager/Manifest.xml"

                sh "sudo xmlstarlet edit --inplace --update '/ModManifest/ManifestUrl' --value 'https://raw.githubusercontent.com/FilUnderscore/ModManager/${env.BRANCH_NAME}/000-ModManager/Manifest.xml' 000-ModManager/Manifest.xml"
            }

            sh "mv 000-ModManager 000-ModManager-temp"
            sh "mkdir 000-ModManager"
            sh "mv 000-ModManager-temp 000-ModManager/000-ModManager"
            zip zipFile: 'ModManager.zip', archive: false, dir: '000-ModManager'
            archiveArtifacts artifacts: 'ModManager.zip', onlyIfSuccessful: true, fingerprint: true

            buildName "${MANIFEST_VERSION}+${GIT_COMMIT_COUNT}.${GIT_COMMIT_HASH}"
        }

        cleanup {
            deleteDir()
            
            // Delete tmp dir
            dir("${workspace}@tmp") {
                deleteDir()
            }
        }
    }
}