param([Parameter(Mandatory=$true)][string]$Frames,[Parameter(Mandatory=$true)][string]$Output,[switch]$Slow)
$ErrorActionPreference='Stop'
$ffmpeg=Join-Path (Split-Path -Parent $PSScriptRoot) '.validation/AndroidTooling/ffmpeg.exe'
$filter=if($Slow){'[0:v][1:v]hstack=inputs=2,setpts=2*PTS[v]'}else{'[0:v][1:v]hstack=inputs=2[v]'}
& $ffmpeg -hide_banner -loglevel error -y -framerate 30 -i (Join-Path $Frames 'baseline-%05d.jpg') -framerate 30 -i (Join-Path $Frames 'candidate-%05d.jpg') -filter_complex $filter -map '[v]' -an -c:v libx264 -crf 19 -pix_fmt yuv420p -movflags +faststart $Output
if($LASTEXITCODE -ne 0){throw 'Courier comparison encoding failed'}
Write-Output "Encoded $Output. Baseline left, candidate right. Fixed 60 Hz simulation sampled at 30 Hz. No audio or frame-pacing claim."
