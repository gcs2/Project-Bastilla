# Axiom Engine - Test Runner Script
# Usage: .\RunTests.ps1 [-TestMode EditMode|PlayMode] [-ResultFile results.xml]

param(
    [string]$TestMode = "EditMode",
    [string]$ResultFile = "TestResults.xml"
)

# 1. Detect Unity Path
$unityHubPath = "C:\Program Files\Unity Hub\Unity Hub.exe"
$projectPath = Get-Location
$unityVersionFile = Join-Path $projectPath "ProjectSettings\ProjectVersion.txt"
$version = Select-String -Path $unityVersionFile -Pattern "m_EditorVersion: (.*)" | ForEach-Object { $_.Matches.Groups[1].Value }

if (-not $version) {
    Write-Error "Could not detect Unity version from ProjectVersion.txt"
    exit 1
}

$unityExe = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"

if (-not (Test-Path $unityExe)) {
    # Try a common fallback location if Hub path logic fails
    $unityExe = Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor\" -Filter "Unity.exe" -Recurse | Select-Object -First 1 -ExpandProperty FullName
}

if (-not (Test-Path $unityExe)) {
    Write-Error "Unity.exe not found for version $version. Please check your installation."
    exit 1
}

Write-Host ">>> Running Axiom Engine Tests ($TestMode) using Unity $version..." -ForegroundColor Cyan
Write-Host ">>> Project Path: $projectPath"

# 2. Execute Unity in Batch Mode
$logFile = "unity_test_log.txt"
if (Test-Path $logFile) { Remove-Item $logFile }
if (Test-Path $ResultFile) { Remove-Item $ResultFile }

$args = @(
    "-batchmode",
    "-projectPath", "`"$projectPath`"",
    "-runTests",
    "-testPlatform", $TestMode,
    "-testResults", "`"$ResultFile`"",
    "-nographics",
    "-logfile", $logFile
)

Write-Host ">>> Unity is now running tests in the background..." -ForegroundColor Gray
Write-Host ">>> Tailing $logFile (Press Ctrl+C to stop trailing, though tests will continue):" -ForegroundColor Gray

# Start Unity
$process = Start-Process -FilePath $unityExe -ArgumentList $args -PassThru

# Track the last line read
$lastLine = 0
while (-not $process.HasExited) {
    if (Test-Path $logFile) {
        $lines = Get-Content $logFile
        $currentCount = $lines.Count
        if ($currentCount -gt $lastLine) {
            $lines[$lastLine..($currentCount - 1)] | ForEach-Object { Write-Host $_ }
            $lastLine = $currentCount
        }
    }
    Start-Sleep -Seconds 1
}

# Final tail catch-up
if (Test-Path $logFile) {
    $lines = Get-Content $logFile
    $currentCount = $lines.Count
    if ($currentCount -gt $lastLine) {
        $lines[$lastLine..($currentCount - 1)] | ForEach-Object { Write-Host $_ }
    }
}

Write-Host ">>> Unity process exited with code: $($process.ExitCode)" -ForegroundColor Gray

# 3. Check Results
if (Test-Path $ResultFile) {
    [xml]$xml = Get-Content $ResultFile
    $testRun = $xml."test-run"
    $total = $testRun.total
    $passed = $testRun.passed
    $failed = $testRun.failed
    $skipped = $testRun.skipped
    
    Write-Host "`n>>> TEST RESULTS <<<" -ForegroundColor Yellow
    Write-Host "Total: $total"
    Write-Host "Passed: $passed" -ForegroundColor Green
    if ($failed -gt 0) {
        Write-Host "Failed: $failed" -ForegroundColor Red
        Write-Host "Check $ResultFile for details."
        
        # Optionally show failed tests
        $xml.SelectNodes("//test-case[@result='Failed']") | ForEach-Object {
            Write-Host "[FAIL] $($_.fullname)" -ForegroundColor Red
            Write-Host "  Error: $($_.failure.message)" -ForegroundColor DarkRed
        }
    }
    else {
        Write-Host "All tests passed!" -ForegroundColor Green
    }
}
else {
    Write-Error "Test results file not generated. Consult $logFile for errors."
}
