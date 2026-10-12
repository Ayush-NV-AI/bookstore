<#
.SYNOPSIS
  Day 1 AM - verifies the licensed Gemini Code Assist environment end to end
  and writes docs/lab/env-check-<machine>.md as the lab artifact.

.USAGE
  From the repository root, in a VS Code terminal (PowerShell 5.1 or 7):
    powershell -ExecutionPolicy Bypass -File .\scripts\verify-env.ps1
  Skip the live model call (e.g. when quota is exhausted):
    powershell -ExecutionPolicy Bypass -File .\scripts\verify-env.ps1 -SkipModelCall
#>
param(
    [switch]$SkipModelCall
)

$ErrorActionPreference = 'Continue'
$results = New-Object System.Collections.Generic.List[object]

function Add-Result([string]$Check, [string]$Status, [string]$Detail) {
    $results.Add([pscustomobject]@{ Check = $Check; Status = $Status; Detail = $Detail })
    $color = 'Green'
    if ($Status -eq 'FAIL') { $color = 'Red' } elseif ($Status -eq 'WARN') { $color = 'Yellow' }
    Write-Host ("[{0}] {1} - {2}" -f $Status, $Check, $Detail) -ForegroundColor $color
}

function Get-Cmd([string]$Name) {
    return Get-Command $Name -ErrorAction SilentlyContinue
}

function Test-Version([string]$Check, [string]$Exe, [string]$VersionArgs, [string]$Pattern, [int]$MinMajor) {
    if (-not (Get-Cmd $Exe)) { Add-Result $Check 'FAIL' "$Exe not found on PATH"; return }
    $out = (& $Exe $VersionArgs.Split(' ') 2>&1 | Out-String).Trim()
    $m = [regex]::Match($out, $Pattern)
    if (-not $m.Success) { Add-Result $Check 'WARN' "could not parse version from: $out"; return }
    $major = [int]$m.Groups[1].Value
    if ($major -ge $MinMajor) { Add-Result $Check 'PASS' $m.Value } else { Add-Result $Check 'FAIL' "$($m.Value) (need >= $MinMajor)" }
}

Write-Host "`n== Book Store SDD - environment verification ==`n" -ForegroundColor Cyan

# 1. Toolchain
Test-Version 'Git'        'git'    '--version' '(\d+)\.\d+\.\d+' 2
Test-Version 'Node.js'    'node'   '--version' 'v?(\d+)\.\d+\.\d+' 20
Test-Version 'Python'     'python' '--version' '(\d+)\.(\d+)' 3
if (Get-Cmd 'python') {
    $pv = (& python --version 2>&1 | Out-String)
    $pm = [regex]::Match($pv, '3\.(\d+)')
    if ($pm.Success -and [int]$pm.Groups[1].Value -lt 11) { Add-Result 'Python minor' 'FAIL' "Spec Kit needs Python 3.11+ ($($pv.Trim()))" }
}
Test-Version 'uv'         'uv'     '--version' 'uv (\d+)\.\d+' 0

if (Get-Cmd 'dotnet') {
    $sdks = (& dotnet --list-sdks 2>&1 | Out-String)
    if ($sdks -match '(?m)^10\.') { Add-Result '.NET SDK 10' 'PASS' (($sdks -split "`n" | Where-Object { $_ -match '^10\.' } | Select-Object -First 1).Trim()) }
    else { Add-Result '.NET SDK 10' 'FAIL' "no 10.x SDK found. Installed: $($sdks.Trim())" }
} else { Add-Result '.NET SDK 10' 'FAIL' 'dotnet not found on PATH' }

# 2. Docker Desktop + PostgreSQL
if (Get-Cmd 'docker') {
    $null = & docker info 2>&1
    if ($LASTEXITCODE -eq 0) {
        Add-Result 'Docker daemon' 'PASS' 'Docker Desktop is running'
        $null = & docker compose up -d db 2>&1
        Start-Sleep -Seconds 3
        $ready = (& docker compose exec -T db pg_isready 2>&1 | Out-String).Trim()
        if ($ready -match 'accepting connections') { Add-Result 'PostgreSQL (compose db)' 'PASS' $ready }
        else { Add-Result 'PostgreSQL (compose db)' 'WARN' "not ready yet: $ready (re-run in 10 s)" }
    } else { Add-Result 'Docker daemon' 'FAIL' 'docker found but daemon not running - start Docker Desktop' }
} else { Add-Result 'Docker' 'FAIL' 'docker not found on PATH' }

# 3. VS Code + Gemini Code Assist extension
if (Get-Cmd 'code') {
    $ext = (& code --list-extensions 2>&1 | Out-String)
    if ($ext -match '(?i)google\.geminicodeassist') { Add-Result 'VS Code + Gemini Code Assist' 'PASS' 'google.geminicodeassist installed' }
    else { Add-Result 'VS Code + Gemini Code Assist' 'FAIL' 'extension google.geminicodeassist not installed' }
} else { Add-Result 'VS Code CLI' 'WARN' "'code' not on PATH - check the extension manually in the Extensions view" }

# 4. Gemini CLI + licensed project
Test-Version 'Gemini CLI' 'gemini' '--version' '(\d+)\.\d+\.\d+' 0

if ($env:GOOGLE_CLOUD_PROJECT) { Add-Result 'GOOGLE_CLOUD_PROJECT' 'PASS' $env:GOOGLE_CLOUD_PROJECT }
else { Add-Result 'GOOGLE_CLOUD_PROJECT' 'FAIL' 'not set - Standard/Enterprise seats must point at the licensed project' }

if ($env:GEMINI_API_KEY) { Add-Result 'GEMINI_API_KEY' 'WARN' 'set - an API key overrides the licensed seat. Unset it for this course.' }

if (-not $SkipModelCall -and (Get-Cmd 'gemini')) {
    $raw = (& gemini -p 'Reply with exactly the text LICENSED-OK and nothing else.' --output-format json 2>&1 | Out-String)
    $code = $LASTEXITCODE
    $resp = ''
    try { $resp = ($raw | ConvertFrom-Json).response } catch { $resp = '' }
    if ($code -eq 0 -and $resp -match 'LICENSED-OK') { Add-Result 'Licensed model call (headless)' 'PASS' 'exit 0, response LICENSED-OK' }
    elseif ($code -eq 41) { Add-Result 'Licensed model call (headless)' 'FAIL' 'exit 41 - authentication. Run gemini interactively once and sign in with the CGI account.' }
    else { Add-Result 'Licensed model call (headless)' 'FAIL' "exit $code - $($raw.Trim() -replace '\s+', ' ')" }
}

# 5. Artifact
$failCount = @($results | Where-Object { $_.Status -eq 'FAIL' }).Count
$machine = $env:COMPUTERNAME
if (-not $machine) { $machine = 'unknown' }
$outDir = Join-Path $PSScriptRoot '..\docs\lab'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$outFile = Join-Path $outDir ("env-check-{0}.md" -f $machine.ToLower())

$md = @()
$md += "# Environment verification - $machine"
$md += ''
$md += "Run at: $(Get-Date -Format 'yyyy-MM-dd HH:mm')  |  Result: $(if ($failCount -eq 0) { 'VERIFIED' } else { "$failCount FAIL(S)" })"
$md += ''
$md += '| Check | Status | Detail |'
$md += '|---|---|---|'
foreach ($r in $results) { $md += "| $($r.Check) | $($r.Status) | $($r.Detail -replace '\|', '/') |" }
$md += ''
$md += '## Interactive checks (do these by hand, then tick)'
$md += ''
$md += '- [ ] VS Code status bar shows Gemini Code Assist signed in with the CGI account and the licensed project selected'
$md += '- [ ] Gemini Code Assist chat panel opens and answers; Agent mode toggle is visible'
$md += '- [ ] `gemini` interactive: `/about` shows the auth type as Code Assist / the CGI project'
$md += '- [ ] `/stats model` runs and shows request counts for this session'
$md | Set-Content -Path $outFile -Encoding UTF8

Write-Host "`nWrote $outFile" -ForegroundColor Cyan
if ($failCount -gt 0) { Write-Host "$failCount check(s) failed - fix before the lab continues." -ForegroundColor Red; exit 1 }
Write-Host 'Environment VERIFIED.' -ForegroundColor Green
exit 0
