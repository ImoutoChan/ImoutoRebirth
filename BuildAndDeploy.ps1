$ErrorActionPreference = 'Stop'

cp .\Source\global.json .\global.json

cd .\Tools\NukeBuild

.\build.cmd Test Publish --VersionedFolder False

If ($LASTEXITCODE -ne 0) {
    cd ..\..
    exit $LASTEXITCODE
}

cd ..\..\Artifacts\latest
.\ImoutoRebirth.Tori\ImoutoRebirth.Tori.UI.exe .\ force auto

cd ..\..

rm .\global.json
