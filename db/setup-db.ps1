<#
.SYNOPSIS
    Starts the local Oracle container and brings its schema up to date with Flyway.

.DESCRIPTION
    Safe to re-run. Flyway applies only the migrations that have not been applied
    yet, so this never destroys data unless -Recreate is passed.

.PARAMETER Seed
    Replaces all rows with the development sample data in db\seed\dev-seed.sql.
    Destroys existing rows. Never use against anything but a local database.

.PARAMETER Recreate
    Runs "flyway clean" first, dropping every object in the schema, then migrates
    from scratch. Use when the schema has drifted or you want a known-clean start.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File db\setup-db.ps1
    powershell -ExecutionPolicy Bypass -File db\setup-db.ps1 -Recreate -Seed
#>
[CmdletBinding()]
param(
    [switch] $Seed,
    [switch] $Recreate
)

$ErrorActionPreference = 'Stop'
$here    = Split-Path -Parent $MyInvocation.MyCommand.Path
$compose = Join-Path $here 'docker-compose.yml'

$dbUser     = if ($env:EMS_DB_USER)     { $env:EMS_DB_USER }     else { 'ems' }
$dbPassword = if ($env:EMS_DB_PASSWORD) { $env:EMS_DB_PASSWORD } else { 'Ems_Pass2026' }

function Invoke-Step {
    param([string] $Description, [scriptblock] $Action)

    Write-Host "==> $Description" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) { throw "$Description failed (exit $LASTEXITCODE)" }
}

# ---------------------------------------------------------------- container
Invoke-Step 'Starting Oracle container' { docker compose -f $compose up -d oracle }

Write-Host '==> Waiting for the database to become healthy (first run takes a few minutes)' -ForegroundColor Cyan
$deadline = (Get-Date).AddMinutes(10)
while ($true) {
    $health = (docker inspect -f '{{.State.Health.Status}}' ems-oracle 2>$null)
    if ($health -eq 'healthy') { break }
    if ((Get-Date) -gt $deadline) { throw "Timed out waiting for ems-oracle (last status: $health)" }
    Write-Host "    status: $health" -ForegroundColor DarkGray
    Start-Sleep -Seconds 10
}
Write-Host '    healthy.' -ForegroundColor Green

# ---------------------------------------------------------------- migrations
if ($Recreate) {
    Write-Warning 'Dropping every object in the schema (-Recreate).'
    Invoke-Step 'flyway clean' { docker compose -f $compose --profile migrate run --rm flyway clean }
}

Invoke-Step 'flyway migrate' { docker compose -f $compose --profile migrate run --rm flyway migrate }
Invoke-Step 'flyway info'    { docker compose -f $compose --profile migrate run --rm flyway info }

# ---------------------------------------------------------------- seed data
if ($Seed) {
    Write-Warning 'Replacing all rows with development sample data (-Seed).'

    docker cp (Join-Path $here 'seed\dev-seed.sql') ems-oracle:/tmp/dev-seed.sql
    if ($LASTEXITCODE -ne 0) { throw 'docker cp failed' }

    # sqlplus exits 0 even for SP2-xxxx script errors, so inspect the output too.
    $out = docker exec ems-oracle sqlplus -s "$dbUser/$dbPassword@localhost:1521/FREEPDB1" '@/tmp/dev-seed.sql' 2>&1
    $out | ForEach-Object { Write-Host "    $_" }
    if ($LASTEXITCODE -ne 0 -or ($out -join "`n") -match '(^|\s)(ORA-|SP2-)\d+') {
        throw 'dev-seed.sql failed'
    }
}

Write-Host ''
Write-Host "==> Ready. Data Source=localhost:1521/FREEPDB1  User Id=$dbUser" -ForegroundColor Green
