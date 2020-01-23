$repoDirectory = ".\.."
$clientDirectory = $repoDirectory + "\ImoutoRebirth.Room.HttpClient"
$tempDirectory = "ImoutoRebirth.Room.HttpClient"
$namespace = "ImoutoRebirth.Room.HttpClient"
$swaggerConfig = "http://localhost:5000/swagger/v1.0/swagger.json"

Get-ChildItem -Path $clientDirectory -Recurse *.cs | foreach { Remove-Item -Path $_.FullName }

#autorest [version: 3.0.5231; node: v10.16.3
autorest --input-file=$swaggerConfig --csharp --output-folder=$clientDirectory --namespace=$namespace autorest-configuration.yml