# Installation

1. Download ImoutoRebirth.exe to any folder
2. Run it (it self extracting 7z archive), it will extract the program to latest folder
3. Start Powershell as Administrator
4. Edit configuration.json and fill with your data
5. Run commands in the following order:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force;
```

```powershell
cd "path to the extracted latest folder"
``` 

```powershell
./install-dependencies.ps1
```

```powershell
./install-update.ps1
```

# Configuration
## configuration.json
```json

```
