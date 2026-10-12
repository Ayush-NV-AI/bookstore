<#
.SYNOPSIS
  Day 3 PM capstone — runs the full regression suite and writes docs/lab/regression-<date>.md.
.USAGE
  powershell -ExecutionPolicy Bypass -File .\scripts\regression.ps1              # local stack
  powershell -ExecutionPolicy Bypass -File .\scripts\regression.ps1 -Containers  # also the Docker full stack
#>
param([switch]$Containers)

$ErrorActionPreference = 'Continue'
$results = New-Object System.Collections.Generic.List[object]

function Step([string]$Name, [scriptblock]$Action) {
    Write-Host "`n=== $Name ===" -ForegroundColor Cyan
    $sw = [Diagnostics.Stopwatch]::StartNew()
    & $Action
    $ok = ($LASTEXITCODE -eq 0)
    $sw.Stop()
    $results.Add([pscustomobject]@{ Step = $Name; Result = $(if ($ok) { 'PASS' } else { 'FAIL' }); Seconds = [math]::Round($sw.Elapsed.TotalSeconds) })
}

Step 'Database up'              { docker compose up -d --wait db }
Step 'Constitution hook cases'  { node .gemini/hooks/test-constitution-guard.mjs }
Step 'API tests (xUnit)'        { dotnet test --nologo }
Step 'Web install'              { if (Test-Path web/node_modules) { $global:LASTEXITCODE = 0 } else { npm --prefix web ci } }
Step 'Web build (strict TS)'    { npm --prefix web run build }
Step 'UI tests (local stack)'   { npm --prefix web run test:e2e }

if ($Containers) {
    Step 'Full stack build + up' { docker compose --profile full up -d --build --wait }
    Step 'UI tests (containers)' {
        $env:E2E_BASE_URL = 'http://localhost:8080'
        npm --prefix web run test:e2e
        $code = $LASTEXITCODE
        Remove-Item Env:E2E_BASE_URL
        $global:LASTEXITCODE = $code
    }
}

$fails = @($results | Where-Object { $_.Result -eq 'FAIL' }).Count
$md = @("# Regression run - $(Get-Date -Format 'yyyy-MM-dd HH:mm')", '', "Result: $(if ($fails -eq 0) { 'GREEN' } else { "$fails FAILED" })", '', '| Step | Result | Seconds |', '|---|---|---|')
foreach ($r in $results) { $md += "| $($r.Step) | $($r.Result) | $($r.Seconds) |" }
New-Item -ItemType Directory -Force -Path docs/lab | Out-Null
$file = "docs/lab/regression-$(Get-Date -Format 'yyyyMMdd-HHmm').md"
$md | Set-Content -Path $file -Encoding UTF8

$results | Format-Table -AutoSize
Write-Host "Wrote $file"
if ($fails -gt 0) { exit 1 } else { exit 0 }
