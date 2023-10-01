$ErrorActionPreference = 'Stop'

cp .\Source\global.json .\global.json

cd .\Tools\NukeBuild
.\build.cmd Test Publish --VersionedFolder False

cd ..\..\Artifacts\latest
.\install-update.ps1

cd ..\..

rm .\global.json
