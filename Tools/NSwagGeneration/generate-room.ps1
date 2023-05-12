dotnet tool update --global NSwag.ConsoleCore
nswag run .\DefaultService.nswag /variables:Port=21301,ServiceName=ImoutoRebirth.Room
