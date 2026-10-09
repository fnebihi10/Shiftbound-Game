param(
 [Parameter(Mandatory=$true)][string]$Output,
 [ValidateSet('Orbit','Route','Layout')][string]$Scenario='Orbit',
 [int]$Width=1280,[int]$Height=720,[int]$Seconds=22,
 [string]$Player='',
 [string]$FFmpeg=''
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(-not $Player){$Player=Join-Path $root 'UnityProject/Builds/WindowsPolished/Shiftbound.exe'}
if(-not $FFmpeg){$FFmpeg=Join-Path $root '.validation/AndroidTooling/ffmpeg.exe'}
[void][IO.Directory]::CreateDirectory($Output)
$scenarioArgs=if($Scenario -eq 'Orbit'){'-shiftboundOrbitScenario'}elseif($Scenario -eq 'Route'){'-shiftboundCompareRun - -shiftboundProductionInput -shiftboundStepRate 60'}else{''}
$launch="-batchmode -force-d3d11 -screen-width $Width -screen-height $Height -screen-fullscreen 0 -shiftboundTouchUI -shiftboundExperienceRecord `"$Output`" -shiftboundExperienceSeconds $Seconds $scenarioArgs -logFile `"$Output/player.log`""
[IO.File]::WriteAllText((Join-Path $Output 'launch.txt'),$launch)
$p=Start-Process -FilePath $Player -ArgumentList $launch -WindowStyle Hidden -PassThru
try {
 if(-not $p.WaitForExit(($Seconds+30)*1000)){$p.Kill(); throw 'Experience capture timed out.'}
 if($p.ExitCode -ne 0){throw 'Experience player failed.'}
 $log=[IO.File]::ReadAllText((Join-Path $Output 'player.log'))
 if(-not $log.Contains('SHIFTBOUND EXPERIENCE CAPTURE COMPLETE') -or -not (Test-Path -LiteralPath (Join-Path $Output 'gameplay.wav'))){throw 'Missing actual video/audio evidence.'}
 if($Scenario -eq 'Route' -and -not $log.Contains('SHIFTBOUND FULL ROUTE PASSED')){throw 'Recorded route did not complete.'}
 Add-Type -AssemblyName System.Drawing
 $first=[Drawing.Bitmap]::new((Join-Path $Output 'frame-00000.jpg'))
 try {
  $colors=[Collections.Generic.HashSet[int]]::new()
  for($x=0;$x -lt $first.Width;$x+=100){for($y=0;$y -lt $first.Height;$y+=100){[void]$colors.Add($first.GetPixel($x,$y).ToArgb())}}
  if($colors.Count -lt 5){throw 'Capture contains no rendered scene; reject black/empty evidence.'}
 }finally{$first.Dispose()}
 & $FFmpeg -hide_banner -loglevel error -y -f concat -safe 0 -i (Join-Path $Output 'frames.ffconcat') -i (Join-Path $Output 'gameplay.wav') -fps_mode vfr -c:v libx264 -crf 20 -pix_fmt yuv420p -c:a aac -b:a 160k -shortest -movflags +faststart (Join-Path $Output 'gameplay.mp4')
 if($LASTEXITCODE -ne 0){throw 'Evidence encoding failed.'}
 Write-Output "EXPERIENCE CAPTURE COMPLETE: $Output. Real-time timestamps; synthetic production input; readback load is not profiling."
} finally {if(-not $p.HasExited){[void]$p.CloseMainWindow()}}
