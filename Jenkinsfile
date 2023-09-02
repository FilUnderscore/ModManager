pipeline
{
    agent any
    
    stages {
        stage ('Setup Build Environment') {
            steps {
                sh "sudo sh setup-dev-linux.sh"
            }
        }
        stage ('Build Release') {
            steps {
                sh "sudo msbuild ./Source/ModManager.csproj /p:Configuration=Release"
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

                MODINFO_VERSION = sh (
                    script: "xmlstarlet sel -t -v '/ModInfo/Version/@value' 000-ModManager/ModInfo.xml",
                    returnStdout: true
                ).trim()

                MANIFEST_VERSION = sh (
                    script: "xmlstarlet sel -t -v '/ModManifest/Version/text()' 000-ModManager/Manifest.xml",
                    returnStdout: true
                ).trim()

                UPDATED_GAME_VERSION = sh (
                    script: "mono ../../VersionRelease.exe Dependencies/7DaysToDieServer_Data/Managed/Assembly-CSharp.dll 000-ModManager/Manifest.xml",
                    returnStdout: true
                ).trim()

                withCredentials([usernamePassword(credentialsId: "${env.CREDENTIALS}", usernameVariable: 'USER', passwordVariable: 'PASSWORD')]) {                
                    sh "git config --global user.email '${env.CREDENTIALS_EMAIL}'"
                    sh "git config --global user.name \$USER"

                    sh "git fetch"
                    sh "git add 000-ModManager/Manifest.xml"
                    sh "git commit -m '${UPDATED_GAME_VERSION}'"
                    sh "git show-ref"
                    sh "git push https://\$USER:\$PASSWORD@github.com/FilUnderscore/ModManager.git -u origin ${env.BRANCH_NAME}"
                }

                sh "sudo xmlstarlet edit --inplace --update '/ModInfo/Version/@value' --value '${MODINFO_VERSION}.${GIT_COMMIT_COUNT}' 000-ModManager/ModInfo.xml"
                sh "sudo xmlstarlet edit --inplace --update '/ModManifest/Version' --value '${MANIFEST_VERSION}+${env.BRANCH_NAME}.${GIT_COMMIT_COUNT}.${GIT_COMMIT_HASH}' 000-ModManager/Manifest.xml"
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