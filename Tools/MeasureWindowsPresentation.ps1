param(
 [Parameter(Mandatory=$true)][string]$Player,
 [Parameter(Mandatory=$true)][string]$Output,
 [Parameter(Mandatory=$true)][string]$PresentMon,
 [int]$Seconds=90,[switch]$NoTiming,[switch]$PhoneRendering
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
. (Join-Path $PSScriptRoot 'CandidateSource.ps1')
$identity=Get-Content ($Player+'.manifest.json') -Raw | ConvertFrom-Json
if($identity.sourceFingerprint -ne (Get-CandidateSource $root)){throw 'Profile source differs from build.'}
foreach($binary in $identity.binaries){
 $path=Join-Path (Split-Path -Parent $Player) $binary.path
 if((Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $binary.sha256){throw "Profile binary changed: $path"}
}
[void][IO.Directory]::CreateDirectory($Output)
[IO.File]::WriteAllText((Join-Path $Output 'identity.json'),($identity|ConvertTo-Json -Depth 8))
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class PresentationWindow {
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr w,out uint p);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr w,int n);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr w);
 [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
 [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a,uint b,bool attach);
 public static void Focus(IntPtr w){
  uint p;uint t=GetWindowThreadProcessId(GetForegroundWindow(),out p),c=GetCurrentThreadId();
  bool attached=t!=c&&AttachThreadInput(c,t,true);
  try{ShowWindow(w,5);SetForegroundWindow(w);}finally{if(attached)AttachThreadInput(c,t,false);}
 }
}
'@
$extra=if($NoTiming){' -shiftboundNoTiming'}else{''}
if($PhoneRendering){$extra+=' -shiftboundMobileRendering'}
$launch='-force-d3d11 -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -shiftboundTouchUI -shiftboundOrbitScenario -shiftboundRepeatScenario -shiftboundGameplayProfile "'+$Output+'" -shiftboundProfileSeconds '+$Seconds+$extra+' -logFile "'+(Join-Path $Output 'player.log')+'"'
[IO.File]::WriteAllText((Join-Path $Output 'launch.txt'),$launch)
$started=[DateTime]::UtcNow
$playerProcess=Start-Process -FilePath $Player -ArgumentList $launch -WindowStyle Hidden -PassThru
$monitor=$null
try {
 $deadline=[DateTime]::UtcNow.AddSeconds(15)
 do{
  Start-Sleep -Milliseconds 200;$playerProcess.Refresh()
  if($playerProcess.HasExited){throw 'Player exited before foreground rendering.'}
  $window=$playerProcess.MainWindowHandle
 }while($window -eq [IntPtr]::Zero -and [DateTime]::UtcNow -lt $deadline)
 if($window -eq [IntPtr]::Zero){throw 'Owned player window not available.'}
 Start-Sleep -Seconds 5
 $playerProcess.Refresh();$window=$playerProcess.MainWindowHandle
 for($attempt=0;$attempt -lt 5;$attempt++){
  [PresentationWindow]::Focus($window);Start-Sleep -Milliseconds 300
  if([PresentationWindow]::GetForegroundWindow() -eq $window){break}
  $playerProcess.Refresh();$window=$playerProcess.MainWindowHandle
 }
 if([PresentationWindow]::GetForegroundWindow() -ne $window){throw 'Cannot establish foreground rendering.'}
 $captureStarted=[DateTime]::UtcNow
 $session='Shiftbound-'+$playerProcess.Id+'-'+$captureStarted.ToString('HHmmss')
 $pmArgs='--process_id '+$playerProcess.Id+' --output_file "'+(Join-Path $Output 'presents.csv')+'" --timed '+$Seconds+' --terminate_after_timed --no_console_stats --date_time --session_name '+$session
 $monitor=Start-Process -FilePath $PresentMon -ArgumentList $pmArgs -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $Output 'presentmon.log') -RedirectStandardError (Join-Path $Output 'presentmon-errors.log')
 $foreground=[Collections.Generic.List[string]]::new();$foreground.Add('utc,elapsed_s,owned_player_foreground')
 while(!$playerProcess.HasExited -and [DateTime]::UtcNow -lt $started.AddSeconds($Seconds+20)){
  $now=[DateTime]::UtcNow
  $owned=[PresentationWindow]::GetForegroundWindow() -eq $window
  $foreground.Add($now.ToString('o')+','+($now-$started).TotalSeconds.ToString('F4',[Globalization.CultureInfo]::InvariantCulture)+','+$owned)
  Start-Sleep -Milliseconds 250;$playerProcess.Refresh()
 }
 [IO.File]::WriteAllLines((Join-Path $Output 'foreground.csv'),$foreground)
 if(!$playerProcess.HasExited){throw 'Profile exceeded its duration.'}
 if(!$monitor.WaitForExit(15000)){throw 'Owned PresentMon session did not finish.'}
 if($monitor.ExitCode -ne 0 -or !(Test-Path -LiteralPath (Join-Path $Output 'presents.csv'))){throw 'PresentMon failed; retain callback data but reject presentation acceptance.'}
 [ordered]@{player_pid=$playerProcess.Id;player_launch_utc=$started.ToString('o');presentmon_launch_utc=$captureStarted.ToString('o');duration_requested_s=$Seconds;capture_readbacks=$false;audio_capture=$false;input='Synthetic gamepad events through shipped input/motor/camera';foreground_samples=$foreground.Count-1;foreground_failures=@($foreground|Where-Object{$_ -match ',False$'}).Count;presentmon_sha256=(Get-FileHash $PresentMon -Algorithm SHA256).Hash;limitations='Windows diagnostic. No Android performance or physical responsiveness claim.'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $Output 'conditions.json')
 & (Join-Path $PSScriptRoot 'SummarizeGameplayProfile.ps1') -Directory $Output -Log (Join-Path $Output 'player.log') -Output (Join-Path $Output 'callbacks.json')
 Write-Output "PRESENTATION CAPTURE COMPLETE: $Output"
} finally {
 if(!$playerProcess.HasExited){[void]$playerProcess.CloseMainWindow()}
 if($monitor -and !$monitor.HasExited){$monitor.Kill()}
}
