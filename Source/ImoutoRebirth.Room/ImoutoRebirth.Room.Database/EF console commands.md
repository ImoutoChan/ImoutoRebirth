```powershell
dotnet tool update --global dotnet-ef
```

```powershell
cd Source
```

```powershell
dotnet ef migrations add <NAME> `
  --project ImoutoRebirth.Room/ImoutoRebirth.Room.Database/ImoutoRebirth.Room.Database.csproj `
  --startup-project ImoutoRebirth.Room/ImoutoRebirth.Room.Host/ImoutoRebirth.Room.Host.csproj

dotnet ef migrations remove `
  --project ImoutoRebirth.Room/ImoutoRebirth.Room.Database/ImoutoRebirth.Room.Database.csproj `
  --startup-project ImoutoRebirth.Room/ImoutoRebirth.Room.Host/ImoutoRebirth.Room.Host.csproj
```
