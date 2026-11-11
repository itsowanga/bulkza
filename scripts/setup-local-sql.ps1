#requires -Version 7
<#
.SYNOPSIS
    Creates (or rotates the secrets of) the local BulkZA SQL Server in Docker.

.DESCRIPTION
    First run:
      - generates the sa password, an app login password and a JWT signing key with a cryptographic RNG
      - writes the two passwords to an env file OUTSIDE the repository (default ~/.bulkza/sql.env),
        readable only by you, so nothing secret is committed or synced by OneDrive
      - starts the container, creates the bulkza database and a limited 'bulkza_app' login
      - stores the app connection string and JWT key in .NET user secrets for the Api and Worker
      - applies the EF Core migrations

    Re-run with no switches: reuses the existing secrets and only makes sure everything is up.
    Re-run with -Rotate: generates new passwords and a new JWT key, changes them in SQL Server,
    recreates the container with the new env file and updates user secrets. Old tokens stop working.

    No secret value is ever printed.
#>
[CmdletBinding()]
param(
    [string] $EnvFile = (Join-Path $HOME '.bulkza/sql.env'),
    [switch] $Rotate,
    [switch] $SkipMigrations
)

$ErrorActionPreference = 'Stop'

$repo      = Split-Path $PSScriptRoot -Parent
$compose   = Join-Path $repo 'docker-compose.yml'
$container = 'bulkza-sql'
$database  = 'bulkza'
$appLogin  = 'bulkza_app'
$sqlcmd    = '/opt/mssql-tools18/bin/sqlcmd'
$defaultPort = '14330'   # host port; not 1433 because a SQL Server installed on the host usually owns it

function New-RandomString([int] $Length, [string] $Alphabet) {
    -join (1..$Length | ForEach-Object { $Alphabet[[Security.Cryptography.RandomNumberGenerator]::GetInt32($Alphabet.Length)] })
}

# 28 characters with upper, lower, digit and symbol so SQL Server's policy accepts it.
# Only characters that are safe in an env file and in a connection string (no ; = ' " $ # { } \).
function New-SqlPassword {
    $upper = 'ABCDEFGHJKLMNPQRSTUVWXYZ'; $lower = 'abcdefghijkmnopqrstuvwxyz'; $digit = '23456789'; $symbol = '-_'
    $chars = @(
        (New-RandomString 4 $upper), (New-RandomString 4 $lower), (New-RandomString 4 $digit), (New-RandomString 2 $symbol),
        (New-RandomString 14 ($upper + $lower + $digit + $symbol))
    ) -join ''
    # Shuffle with the crypto RNG.
    -join ($chars.ToCharArray() | Sort-Object { [Security.Cryptography.RandomNumberGenerator]::GetInt32([int]::MaxValue) })
}

function New-JwtKey { [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64)) }

function Read-EnvFile([string] $Path) {
    $values = @{}
    foreach ($line in Get-Content $Path) {
        if ($line -match '^\s*([A-Za-z_][A-Za-z0-9_]*)=(.*)$') { $values[$Matches[1]] = $Matches[2] }
    }
    $values
}

function Write-EnvFile([string] $Path, [string] $SaPassword, [string] $AppPassword, [string] $Port, [string] $JwtKey) {
    New-Item -ItemType Directory -Force -Path (Split-Path $Path) | Out-Null
    Set-Content -Path $Path -Value @("MSSQL_SA_PASSWORD=$SaPassword", "BULKZA_APP_PASSWORD=$AppPassword", "BULKZA_SQL_PORT=$Port", "JWT_SECRET=$JwtKey") -Encoding ascii
    # Owner only: remove inherited permissions, then grant the current user.
    icacls $Path /inheritance:r /grant:r "$($env:USERNAME):(R,W)" | Out-Null
}

# Runs T-SQL inside the container. The password travels in an environment variable, never on a command line.
function Invoke-Sql([string] $User, [string] $Password, [string] $Sql) {
    $env:SQLCMDPASSWORD = $Password
    try {
        $Sql | docker exec -i -e SQLCMDPASSWORD $container $sqlcmd -C -S localhost -U $User -b | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed for login '$User'." }
    }
    finally { $env:SQLCMDPASSWORD = $null }
}

function Set-UserSecret([string] $Project, [string] $Key, [string] $Value) {
    dotnet user-secrets set $Key $Value --project (Join-Path $repo $Project) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not set user secret '$Key' for $Project." }
}

function Start-Server([string] $File, [switch] $Recreate) {
    $composeArgs = @('compose', '-f', $compose, '--env-file', $File, 'up', '-d', '--wait')
    if ($Recreate) { $composeArgs += '--force-recreate' }
    docker @composeArgs sqlserver | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'docker compose failed to start the SQL Server container.' }
}

docker info *> $null
if ($LASTEXITCODE -ne 0) { throw 'Docker is not running. Start Docker Desktop and run this script again.' }

$firstRun = -not (Test-Path $EnvFile)
$rotating = $Rotate -and -not $firstRun

if ($firstRun) {
    $saPassword  = New-SqlPassword
    $appPassword = New-SqlPassword
    $port        = $defaultPort
    $jwtKey      = New-JwtKey
    $newJwtKey   = $true
    Write-EnvFile $EnvFile $saPassword $appPassword $port $jwtKey
    Write-Host "Generated new secrets and saved them to $EnvFile"
}
else {
    $current = Read-EnvFile $EnvFile
    if (-not $current['MSSQL_SA_PASSWORD'] -or -not $current['BULKZA_APP_PASSWORD']) { throw "$EnvFile is missing a password. Delete it and run the script again." }
    $saPassword  = $current['MSSQL_SA_PASSWORD']
    $appPassword = $current['BULKZA_APP_PASSWORD']
    $port        = if ($current['BULKZA_SQL_PORT']) { $current['BULKZA_SQL_PORT'] } else { $defaultPort }
    $jwtKey      = $current['JWT_SECRET']
    if (-not $jwtKey) {
        $jwtKey    = New-JwtKey
        $newJwtKey = $true
        Write-EnvFile $EnvFile $saPassword $appPassword $port $jwtKey
    }
}

Start-Server $EnvFile

if ($rotating) {
    $oldSa = $saPassword
    $saPassword  = New-SqlPassword
    $appPassword = New-SqlPassword
    $jwtKey      = New-JwtKey
    $newJwtKey   = $true

    # Change the passwords inside SQL Server first, using the old sa password. Only then replace the env file.
    Invoke-Sql 'sa' $oldSa @"
ALTER LOGIN sa WITH PASSWORD = '$saPassword' UNLOCK;
IF SUSER_ID('$appLogin') IS NOT NULL ALTER LOGIN [$appLogin] WITH PASSWORD = '$appPassword' UNLOCK;
"@
    Write-EnvFile $EnvFile $saPassword $appPassword $port $jwtKey
    # The container keeps the old password in its environment (used by the health check), so recreate it.
    Start-Server $EnvFile -Recreate

    # Api and Worker containers hold the old passwords and JWT key in their environment, so recreate any that exist.
    $running = docker ps -a --format '{{.Names}}'
    $services = @(@{ Container = 'bulkza-api'; Service = 'api' }, @{ Container = 'bulkza-worker'; Service = 'worker' }) |
        Where-Object { $running -contains $_.Container } | ForEach-Object { $_.Service }
    if ($services) {
        docker compose -f $compose --env-file $EnvFile up -d --force-recreate --wait @services | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'docker compose failed to recreate the Api and Worker containers.' }
        Write-Host "Recreated: $($services -join ', ')."
    }
    Write-Host 'Rotated the sa and app passwords and the JWT key.'
}

# Database and a limited login. The app never uses sa.
Invoke-Sql 'sa' $saPassword @"
IF DB_ID('$database') IS NULL CREATE DATABASE [$database];
GO
IF SUSER_ID('$appLogin') IS NULL CREATE LOGIN [$appLogin] WITH PASSWORD = '$appPassword', CHECK_POLICY = ON;
GO
USE [$database];
IF USER_ID('$appLogin') IS NULL CREATE USER [$appLogin] FOR LOGIN [$appLogin];
ALTER ROLE db_owner ADD MEMBER [$appLogin];
GO
"@

$connectionString = "Server=127.0.0.1,$port;Database=$database;User Id=$appLogin;Password=$appPassword;TrustServerCertificate=True"

foreach ($project in 'src/Api', 'src/Worker') {
    Set-UserSecret $project 'ConnectionStrings:DefaultConnection' $connectionString
}

Set-UserSecret 'src/Api' 'Jwt:Secret' $jwtKey
if ($newJwtKey) { Write-Host 'Set a new JWT signing key. Existing tokens are no longer valid.' }

if (-not $SkipMigrations) {
    Push-Location $repo
    try {
        dotnet tool restore | Out-Null
        dotnet ef database update --project src/Infrastructure --connection $connectionString | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Applying migrations failed.' }
        Write-Host 'Migrations applied.'
    }
    finally { Pop-Location }
}

# Prove the app login works (and is not sa).
Invoke-Sql $appLogin $appPassword "SELECT 1; SELECT COUNT(*) FROM [$database].dbo.__EFMigrationsHistory;"

Write-Host ''
Write-Host "SQL Server is ready: container '$container' on localhost,$port, database '$database'."
Write-Host "Env file (owner only): $EnvFile"
Write-Host 'User secrets set for src/Api and src/Worker. The env file also feeds docker compose (Api and Worker containers).'
Write-Host 'No secret was printed or written to the repository.'
