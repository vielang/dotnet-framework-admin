<#
  Brings up the Oracle container and applies db/schema.sql to the "ems" schema.

  Usage:  powershell -ExecutionPolicy Bypass -File db\setup-db.ps1
#>
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host '==> Starting Oracle container...' -ForegroundColor Cyan
docker compose -f (Join-Path $here 'docker-compose.yml') up -d
if ($LASTEXITCODE -ne 0) { throw 'docker compose up failed' }

Write-Host '==> Waiting for the database to become healthy (first run takes a few minutes)...' -ForegroundColor Cyan
$deadline = (Get-Date).AddMinutes(10)
while ($true) {
    $health = (docker inspect -f '{{.State.Health.Status}}' ems-oracle 2>$null)
    if ($health -eq 'healthy') { break }
    if ((Get-Date) -gt $deadline) { throw "Timed out waiting for ems-oracle (last status: $health)" }
    Write-Host "    status: $health" -ForegroundColor DarkGray
    Start-Sleep -Seconds 10
}
Write-Host '    healthy.' -ForegroundColor Green

Write-Host '==> Applying schema.sql...' -ForegroundColor Cyan
docker cp (Join-Path $here 'schema.sql') ems-oracle:/tmp/schema.sql
if ($LASTEXITCODE -ne 0) { throw 'docker cp failed' }

# sqlplus exits 0 even for SP2-xxxx script errors, so inspect the output as well.
$out = docker exec ems-oracle sqlplus -s 'ems/Ems_Pass2026@localhost:1521/FREEPDB1' '@/tmp/schema.sql' 2>&1
$out | ForEach-Object { Write-Host "    $_" }
if ($LASTEXITCODE -ne 0 -or ($out -join "`n") -match '(^|\s)(ORA-|SP2-)\d+') {
    throw 'schema.sql failed'
}

Write-Host '==> Done. Connect with: User Id=ems;Password=Ems_Pass2026;Data Source=localhost:1521/FREEPDB1' -ForegroundColor Green
