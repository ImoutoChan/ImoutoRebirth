dotnet tool update --global NSwag.ConsoleCore
nswag run .\DefaultService.nswag /variables:Port=11401,ServiceName=ImoutoRebirth.Room
