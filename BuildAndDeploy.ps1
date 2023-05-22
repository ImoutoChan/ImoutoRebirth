$ErrorActionPreference = 'Stop'

cd .\Tools\NukeBuild
.\build.cmd

cd ..\..\Artifacts\latest
.\install-update.ps1

cd ..\..