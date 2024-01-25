Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
choco install dotnet-8.0-aspnetruntime --version 8.0.1 -y
choco install dotnet-8.0-desktopruntime --version 8.0.1 -y
choco install postgresql16 --version 16.1.0 -y --params '/Password:postgres /Port:5432'
pause
