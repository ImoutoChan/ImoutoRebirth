$ErrorActionPreference = 'Stop'

cp .\Source\global.json .\global.json

cd .\Tools\NukeBuild

.\build.cmd Publish --VersionedFolder False

If ($LASTEXITCODE -ne 0) {
    cd ..\..
    exit $LASTEXITCODE
}

cd ..\..\Artifacts\latest
.\install-update.ps1

cd ..\..

rm .\global.json
