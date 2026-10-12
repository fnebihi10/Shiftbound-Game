param([string]$Editor='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe')
$ErrorActionPreference='Stop'
$verificationRoot=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'CandidateSource.ps1')
$verificationArgs='-batchmode -force-d3d11 -buildTarget Win64 -projectPath "'+(Join-Path $verificationRoot 'UnityProject')+'" -executeMethod UpgradePlayModeVerification.Run -shiftboundCompareRun - -shiftboundProductionInput -shiftboundStepRate 60 -shiftboundIncludeRecovery -shiftboundRepeatRoute -shiftboundFacingRegression'
Invoke-CandidateChecked $Editor $verificationArgs (Join-Path $verificationRoot 'Logs/upgrade-unity-playmode.log') 'SHIFTBOUND UNITY PLAY MODE PASSED' 5
if(-not ([IO.File]::ReadAllText((Join-Path $verificationRoot 'Logs/upgrade-unity-playmode.log'))).Contains('SHIFTBOUND COURIER FACING PASSED')){throw 'Anatomical facing was not verified.'}
