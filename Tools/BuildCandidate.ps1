param(
    [ValidateSet('Windows','Apk','Bundle','EmulatorApk','Configure')][string]$Target='Windows',
    [string]$Editor='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe'
)
$ErrorActionPreference='Stop'
$candidateRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'CandidateSource.ps1')
$candidateLogs = Join-Path $candidateRoot 'Logs'
New-Item -ItemType Directory -Force $candidateLogs | Out-Null
$candidateMethod = @{Windows='SliceDelivery.BuildWindows';Apk='AndroidDelivery.BuildApk';Bundle='AndroidDelivery.BuildBundle';EmulatorApk='AndroidDelivery.BuildEmulatorApk';Configure='AndroidDelivery.Configure'}[$Target]
$candidatePlatform = if ($Target -eq 'Windows') { 'Win64' } else { 'Android' }
$candidateMarker = if ($Target -eq 'Windows') { 'SHIFTBOUND BUILD PASSED' } elseif ($Target -eq 'Configure') { 'SHIFTBOUND ANDROID CONFIGURED' } else { 'SHIFTBOUND ANDROID BUILD PASSED' }
Invoke-CandidateChecked $Editor ('-batchmode -quit -force-d3d11 -buildTarget ' + $candidatePlatform + ' -projectPath "' + (Join-Path $candidateRoot 'UnityProject') + '" -executeMethod ' + $candidateMethod) (Join-Path $candidateLogs "candidate-$Target-build.log") $candidateMarker
