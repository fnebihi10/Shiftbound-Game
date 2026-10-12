param([Parameter(Mandatory=$true)][string]$Output,[int]$Minutes=15)
$ErrorActionPreference='Stop'
$adb='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb.exe'
[void][IO.Directory]::CreateDirectory($Output)
$start=[DateTime]::UtcNow
[IO.File]::WriteAllText((Join-Path $Output 'session-start.txt'),$start.ToString('o')+"`nUSB connected; interactive owner play; no screen recording during pacing collection. SurfaceFlinger statistics cleared before session. Histograms include menus/idle unless gameplay occupancy is established separately.")
for($i=0; $i -le $Minutes*2; $i++) {
    $prefix=Join-Path $Output ('sample-{0:D3}' -f $i)
    [IO.File]::WriteAllText(($prefix+'-utc.txt'),[DateTime]::UtcNow.ToString('o'))
    & $adb shell dumpsys SurfaceFlinger --timestats -dump | Out-File ($prefix+'-presentation.txt')
    & $adb shell dumpsys battery | Out-File ($prefix+'-battery.txt')
    & $adb shell dumpsys thermalservice | Out-File ($prefix+'-thermal.txt')
    & $adb shell dumpsys activity activities | Select-String 'mResumedActivity' | Out-File ($prefix+'-foreground.txt')
    if($i -lt $Minutes*2){Start-Sleep -Seconds 30}
}
& $adb logcat -d -s Unity AndroidRuntime | Out-File (Join-Path $Output 'player-log.txt')
& $adb shell dumpsys meminfo com.shiftboundproject.shiftbound | Out-File (Join-Path $Output 'memory-end.txt')
[IO.File]::WriteAllText((Join-Path $Output 'session-end.txt'),[DateTime]::UtcNow.ToString('o'))
