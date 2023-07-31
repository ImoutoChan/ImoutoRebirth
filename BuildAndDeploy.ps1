$ErrorActionPreference = 'Stop'

cd .\Tools\NukeBuild
.\build.cmd Test Publish

cd ..\..\Artifacts\latest
.\install-update.ps1

cd ..\..
