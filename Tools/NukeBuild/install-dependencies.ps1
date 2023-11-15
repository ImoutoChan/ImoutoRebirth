Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
choco install dotnet-8.0-aspnetruntime --version 8.0.0 -y
choco install dotnet-8.0-desktopruntime --version 8.0.0 -y
choco install postgresql15 --version 15.0.1 -y --params '/Password:postgres /Port:5432'
choco install rabbitmq --version 3.11.16 -y
pause
