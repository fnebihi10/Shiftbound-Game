param(
    [switch]$Build,
    [string]$Editor = 'C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe'
)
$ErrorActionPreference = 'Stop'
$milestoneRoot = Split-Path -Parent $PSScriptRoot
$milestoneLogs = Join-Path $milestoneRoot 'Logs'
[void][System.IO.Directory]::CreateDirectory($milestoneLogs)
function Run-Checked([string]$Executable, [string]$Arguments, [string]$Log, [string]$Marker) {
    $milestoneJob = Start-Process -FilePath $Executable -ArgumentList ($Arguments + ' -logFile "' + $Log + '"') -WindowStyle Hidden -PassThru
    $milestoneDeadline = [DateTime]::UtcNow.AddMinutes(5)
    while (-not $milestoneJob.WaitForExit(1000)) {
        if ([DateTime]::UtcNow -gt $milestoneDeadline) {
            $milestoneJob.Kill()
            throw "Timed out: $Arguments. See $Log"
        }
    }
    $milestoneText = [System.IO.File]::ReadAllText($Log)
    if ($milestoneJob.ExitCode -ne 0 -or -not $milestoneText.Contains($Marker) -or
        $milestoneText -match 'error CS\d+|Unhandled Exception|SHIFTBOUND .*FAILED:') {
        throw "Verification failed (exit $($milestoneJob.ExitCode)): $Arguments. See $Log"
    }
    Write-Output "PASS: $Arguments -> $Log"
}
if ($Build) {
    Run-Checked $Editor ('-batchmode -nographics -quit -projectPath "' + (Join-Path $milestoneRoot 'UnityProject') + '" -executeMethod SliceDelivery.BuildWindows') (Join-Path $milestoneLogs 'milestone1-final-build.log') 'SHIFTBOUND BUILD PASSED'
}
$milestonePlayer = Join-Path $milestoneRoot 'UnityProject/Builds/WindowsPolished/Shiftbound.exe'
Run-Checked $milestonePlayer '-batchmode -nographics -shiftboundRegression' (Join-Path $milestoneLogs 'milestone1-final-regression.log') 'SHIFTBOUND REGRESSION PASSED'
Run-Checked $milestonePlayer '-batchmode -nographics -shiftboundSmoke' (Join-Path $milestoneLogs 'milestone1-final-smoke.log') 'SHIFTBOUND SMOKE PASSED'
Run-Checked $milestonePlayer '-batchmode -nographics -shiftboundReachAudit -shiftboundRequireShiftLessons' (Join-Path $milestoneLogs 'milestone1-final-reach.log') 'SHIFTBOUND REACH AUDIT COMPLETE'
foreach ($milestoneRate in @(30, 60, 120, 144, 0)) {
    Run-Checked $milestonePlayer ("-batchmode -nographics -shiftboundCompareRun - -shiftboundStepRate $milestoneRate") (Join-Path $milestoneLogs "milestone1-final-route-$milestoneRate.log") 'SHIFTBOUND FULL ROUTE PASSED'
}
foreach ($milestoneRate in @(30, 60, 120, 144)) {
    Run-Checked $milestonePlayer ("-batchmode -force-d3d11 -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -shiftboundCompareRun - -shiftboundProductionInput -shiftboundStepRate $milestoneRate") (Join-Path $milestoneLogs "milestone1-production-route-$milestoneRate.log") 'input=production GameInput/Update/camera'
}
Write-Output 'MILESTONE 1 AUTOMATED VERIFICATION PASSED. Human camera, animation and audio review is still required.'
