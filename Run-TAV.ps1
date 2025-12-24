# ============================================================================
# Axiom RPG Engine - TAV CLI Runner
# Interactive Text Adventure Verification in PowerShell
# ============================================================================

$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe"
$LogPath = "tav_cli.log"

function Execute-AxiomCommand($cmd) {
    Write-Host "`n[EXECUTING] $cmd..." -ForegroundColor Cyan
    $process = Start-Process -FilePath $UnityPath -ArgumentList "-batchmode", "-nographics", "-projectPath .", "-executeMethod RPGPlatform.Editor.TAV.AxiomShell.ExecuteFromCLI", "-command `"$cmd`"", "-quit", "-logFile $LogPath" -Wait -PassThru
    
    if (Test-Path $LogPath) {
        $logs = Get-Content $LogPath | Where-Object { $_ -match "^\[TAV\]" -or $_ -match "^\[STATE\]" -or $_ -match "^\[DialogueManager\]" }
        foreach ($line in $logs) {
            if ($line -match "FAILURE|ERR") { Write-Host $line -ForegroundColor Red }
            elseif ($line -match "SUCCESS|VICTORY") { Write-Host $line -ForegroundColor Green }
            elseif ($line -match "COMBAT") { Write-Host $line -ForegroundColor Yellow }
            else { Write-Host $line }
        }
        Remove-Item $LogPath -ErrorAction SilentlyContinue
    }
}

Write-Host "=================================================" -ForegroundColor Magenta
Write-Host "    AXIOM RPG ENGINE - STANDALONE TAV CLI        " -ForegroundColor Magenta
Write-Host "=================================================" -ForegroundColor Magenta
Write-Host "Type 'exit' to quit. Available commands: talk, choose, stat, help"

# Initial Stat Print
Execute-AxiomCommand "stat"

while ($true) {
    $input = Read-Host "`n> "
    if ($input -eq "exit") { break }
    if ([string]::IsNullOrWhiteSpace($input)) { continue }
    
    Execute-AxiomCommand $input
}

Write-Host "`n[TAV] Session Ended." -ForegroundColor Magenta
