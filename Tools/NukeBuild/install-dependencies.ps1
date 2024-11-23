Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
choco install dotnet-9.0-aspnetruntime --version 9.0.0 -y
choco install dotnet-9.0-desktopruntime --version 9.0.0 -y
choco install postgresql16 --version 16.1.0 -y --params '/Password:postgres /Port:5432'
pause
