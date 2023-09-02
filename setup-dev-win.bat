@ECHO off
ECHO Setting up dependencies
ECHO Downloading DepotDownloader
mkdir "temp"
cd "temp"
curl -JL "https://github.com/SteamRE/DepotDownloader/releases/latest/download/DepotDownloader-windows-x64.zip" -o "DepotDownloader-windows-x64.zip"
ECHO Extracting DepotDownloader
tar -xf "DepotDownloader-windows-x64.zip"
ECHO Downloading Latest Binaries
DepotDownloader.exe -app 294420 -filelist ../setup-filelist.txt -dir ../Dependencies
ECHO Dependencies successfully downloaded
ECHO Cleaning up
cd ../
rmdir /s /q "temp"
rmdir /s /q "Dependencies/.DepotDownloader"
ECHO Development Environment setup
PAUSE