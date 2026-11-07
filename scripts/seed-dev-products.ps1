#requires -Version 7
<#
.SYNOPSIS
    Adds sample products to the local development database (see seed-dev-products.sql).

.DESCRIPTION
    Runs inside the bulkza-sql container created by setup-local-sql.ps1, as the limited bulkza_app login.
    The password is read from the env file and passed through an environment variable, never printed.
#>
[CmdletBinding()]
param(
    [string] $EnvFile = (Join-Path $HOME '.bulkza/sql.env')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $EnvFile)) { throw "Env file not found: $EnvFile. Run scripts/setup-local-sql.ps1 first." }

$password = (Get-Content $EnvFile | Where-Object { $_ -match '^BULKZA_APP_PASSWORD=' }) -replace '^BULKZA_APP_PASSWORD=', ''
if (-not $password) { throw "BULKZA_APP_PASSWORD is missing from $EnvFile." }

$env:SQLCMDPASSWORD = $password
try {
    Get-Content (Join-Path $PSScriptRoot 'seed-dev-products.sql') -Raw |
        docker exec -i -e SQLCMDPASSWORD bulkza-sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U bulkza_app -d bulkza -b
    if ($LASTEXITCODE -ne 0) { throw 'Seeding failed.' }
}
finally { $env:SQLCMDPASSWORD = $null }
