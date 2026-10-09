param(
 [Parameter(Mandatory=$true)][string]$Output,
 [ValidateSet(30,60,120,144)][int]$Rate=60,
 [switch]$NoTiming,[switch]$NoPost,[switch]$Record
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'CandidateSource.ps1')
$player=Join-Path $root 'UnityProject/Builds/WindowsPolished/Shiftbound.exe'
$identity=Get-Content ($player+'.manifest.json') -Raw | ConvertFrom-Json
if($identity.sourceFingerprint -ne (Get-CandidateSource $root)){throw 'Source changed: rebuild before profiling.'}
foreach($binary in $identity.binaries){
 $path=Join-Path (Split-Path -Parent $player) $binary.path
 if((Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $binary.sha256){throw "Binary changed: $path"}
}
[void][IO.Directory]::CreateDirectory($Output)
$extra=''
if($NoTiming){$extra+=' -shiftboundNoTiming'}
if($NoPost){$extra+=' -shiftboundNoPost'}
if($Record){$extra+=' -shiftboundExperienceRecord "'+$Output+'" -shiftboundExperienceSeconds 90'}
# Normal windowed player, launched hidden; no OS input or focus manipulation.
# Keep competing workloads documented. This is a Windows diagnostic only.
$launch='-force-d3d11 -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -shiftboundTouchUI -shiftboundCompareRun - -shiftboundProductionInput -shiftboundStepRate '+$Rate+' -shiftboundGameplayProfile "'+$Output+'" -shiftboundProfileSeconds 90'+$extra
[IO.File]::WriteAllText((Join-Path $Output 'launch.txt'),$launch)
[IO.File]::WriteAllText((Join-Path $Output 'identity.json'),($identity | ConvertTo-Json -Depth 8))
Invoke-CandidateChecked $player $launch (Join-Path $Output 'player.log') 'SHIFTBOUND FULL ROUTE PASSED' 3
& (Join-Path $PSScriptRoot 'SummarizeGameplayProfile.ps1') -Directory $Output -Log (Join-Path $Output 'player.log') -Output (Join-Path $Output 'summary.json')
