param (
    [switch]$skipDotnet,
    [switch]$skipPostgres
)

$requiredAspNetVersion = "9.0.4"
$requiredDesktopVersion = "9.0.4"
$requiredPostgresVersion = "16.8.0"

$tryInstallDotnetRuntimes = -not $SkipDotnet
$tryInstallPostgres = -not $SkipPostgres

if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
    Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
} else {
    Write-Output "Choco is up to date"
}

$aspnetInstalled = $false
$desktopInstalled = $false

function IsPortInUse($port) {
    if (Get-Command Get-NetTCPConnection -ErrorAction SilentlyContinue) {
        return [bool](Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue)
    }
    $netstatOutput = netstat -ano | Select-String ":$port\s"
    return $netstatOutput -ne $null
}

function Is-PostgresInstalled {
    # Option 1: Check for a PostgreSQL service.
    $pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if ($pgService) {
        Write-Output "PostgreSQL service is found: $($pgService.Name)"
        return $true
    }

    # Option 2: Check if the PostgreSQL package is installed via Chocolatey.
    $pgPackage = choco list --local-only postgresql16 2>$null | Select-String "postgresql16"
    if ($pgPackage) {
        Write-Output "Chocolatey package for PostgreSQL is installed."
        return $true
    }

    # Option 3: Check if the PostgreSQL port is in use
    if (IsPortInUse 5432) {
        Write-Output "PostgreSQL is running on port 5432."
        return $true
    }

    return $false
}

if ($tryInstallDotnetRuntimes)
{
    if (Get-Command dotnet -ErrorAction SilentlyContinue) {
        $runtimes = & dotnet --list-runtimes 2>&1
        foreach ($line in $runtimes) {
            if ($line -match "Microsoft.AspNetCore.App\s+$requiredAspNetVersion\b") {
                $aspnetInstalled = $true
            }

            if ($line -match "Microsoft.WindowsDesktop.App\s+$requiredDesktopVersion\b") {
                $desktopInstalled = $true
            }
        }
    }

    if (-not $aspnetInstalled -and $tryInstallDotnetRuntimes) {
        choco install dotnet-9.0-aspnetruntime --version $requiredAspNetVersion -y
    }
    else {
        Write-Output "ASP.NET Core runtime $requiredAspNetVersion is up to date"
    }

    if (-not $desktopInstalled)
    {
        choco install dotnet-9.0-desktopruntime --version $requiredDesktopVersion -y
    }
    else
    {
        Write-Output "Windows Desktop runtime $requiredDesktopVersion is up to date"
    }
}
else {
    Write-Output "Skipping .NET runtimes installation"
}

if ($tryInstallPostgres) {
    if (Is-PostgresInstalled) {
        Write-Output "PostgreSQL is already installed"
    } else {
        choco install postgresql16 --version $requiredPostgresVersion -y --params '/Password:postgres /Port:5432'
    }
} else {
    Write-Output "Skipping PostgreSQL installation"
}

pause
