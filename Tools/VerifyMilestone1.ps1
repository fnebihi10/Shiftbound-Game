param(
    [switch]$Build,
    [switch]$ExistingBinary,
    [string]$Editor = 'C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe',
    [string]$Player = ''
)
$ErrorActionPreference = 'Stop'
$milestoneRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'CandidateSource.ps1')
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
dotnet run --no-restore --project (Join-Path $PSScriptRoot 'JumpIntentChecks/JumpIntentChecks.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Production jump-intent checks failed.' }
dotnet run --no-restore --project (Join-Path $PSScriptRoot 'TouchInputChecks/TouchInputChecks.csproj')
if ($LASTEXITCODE -ne 0) { throw 'Production touch-router checks failed. Restore the project first if necessary.' }
if (-not $ExistingBinary -or $Build) { & (Join-Path $PSScriptRoot 'BuildCandidate.ps1') -Target Windows -Editor $Editor }
$milestonePlayer = if ($Player) { [IO.Path]::GetFullPath($Player) } else { Join-Path $milestoneRoot 'UnityProject/Builds/WindowsPolished/Shiftbound.exe' }
$milestoneManifest = $milestonePlayer + '.manifest.json'
if (-not (Test-Path -LiteralPath $milestoneManifest)) { throw 'No source/binary manifest. A fresh build is required.' }
$milestoneIdentity = Get-Content -LiteralPath $milestoneManifest -Raw | ConvertFrom-Json
if ($milestoneIdentity.sourceFingerprint -ne (Get-CandidateSource $milestoneRoot)) { throw 'Current source differs from recorded build. Rebuild.' }
foreach ($milestoneBinary in $milestoneIdentity.binaries) {
    $milestoneFile = Join-Path (Split-Path -Parent $milestonePlayer) $milestoneBinary.path
    if (-not (Test-Path -LiteralPath $milestoneFile) -or (Get-FileHash -LiteralPath $milestoneFile -Algorithm SHA256).Hash.ToLowerInvariant() -ne $milestoneBinary.sha256) {
        throw "Binary correspondence failed: $($milestoneBinary.path)"
    }
}
Run-Checked $milestonePlayer '-batchmode -nographics -shiftboundRegression' (Join-Path $milestoneLogs 'milestone1-final-regression.log') 'SHIFTBOUND REGRESSION PASSED'
Run-Checked $milestonePlayer '-batchmode -nographics -shiftboundSmoke' (Join-Path $milestoneLogs 'milestone1-final-smoke.log') 'SHIFTBOUND SMOKE PASSED'
Run-Checked $milestonePlayer '-batchmode -nographics -shiftboundReachAudit -shiftboundRequireShiftLessons' (Join-Path $milestoneLogs 'milestone1-final-reach.log') 'SHIFTBOUND REACH AUDIT COMPLETE'
Run-Checked $milestonePlayer '-batchmode -force-d3d11 -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -shiftboundPhoneRegression -shiftboundTouchUI' (Join-Path $milestoneLogs 'candidate-phone-regression.log') 'SHIFTBOUND PHONE REGRESSION PASSED'
foreach ($milestoneRate in @(30, 60, 120, 144, 0)) {
    Run-Checked $milestonePlayer ("-batchmode -nographics -shiftboundCompareRun - -shiftboundStepRate $milestoneRate") (Join-Path $milestoneLogs "milestone1-final-route-$milestoneRate.log") 'SHIFTBOUND FULL ROUTE PASSED'
}
foreach ($milestoneRate in @(30, 60, 120, 144)) {
    Run-Checked $milestonePlayer ("-batchmode -force-d3d11 -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -shiftboundCompareRun - -shiftboundProductionInput -shiftboundStepRate $milestoneRate") (Join-Path $milestoneLogs "milestone1-production-route-$milestoneRate.log") 'SHIFTBOUND FULL ROUTE PASSED'
    if (-not (Get-Content (Join-Path $milestoneLogs "milestone1-production-route-$milestoneRate.log") -Raw).Contains('input=production GameInput/Update/camera')) { throw 'Production input route did not run.' }
}
Write-Output "CURRENT-SOURCE AUTOMATED VERIFICATION PASSED: $($milestoneIdentity.sourceFingerprint). Physical touch, Android pacing and human review remain separate gates."
