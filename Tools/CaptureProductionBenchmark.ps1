param(
 [Parameter(Mandatory=$true)][string]$Player,
 [Parameter(Mandatory=$true)][string]$Output,
 [int]$Width=1280,
 [int]$Height=720,
 [string]$Arguments='',
 [switch]$Audio,
 [switch]$NoVideo,
 [switch]$ObserveOnly,
 [int]$Seconds=25,
 [int]$Fps=30,
 [string]$FFmpeg=''
)
# Records normal Windows keyboard/mouse input and real client pixels for 25 seconds.
# Optional audio records the actual player mix; capture FPS is not presentation evidence.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing @'
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
public static class BenchmarkRecorder {
 [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left,Top,Right,Bottom; }
 [StructLayout(LayoutKind.Sequential)] public struct Point { public int X,Y; }
 [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr w,out Rect r);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr w,ref Point p);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr w,int n);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr w);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr w);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern IntPtr FindWindow(string c,string t);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
 [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a,uint b,bool attach);
 [DllImport("user32.dll")] public static extern void keybd_event(byte k,byte s,uint f,UIntPtr e);
 [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint k,uint t);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern void mouse_event(uint f,int x,int y,uint d,UIntPtr e);
 static void Key(byte k,bool down) { keybd_event(k,(byte)MapVirtualKey(k,0),down?0u:2u,UIntPtr.Zero); }
 public static void Focus(IntPtr handle) {
  uint unused;
  uint foreground=GetWindowThreadProcessId(GetForegroundWindow(),out unused);
  uint current=GetCurrentThreadId();
  bool attached=foreground!=current && AttachThreadInput(current,foreground,true);
  try { ShowWindow(handle,5); SetForegroundWindow(handle); }
  finally { if(attached)AttachThreadInput(current,foreground,false); }
 }
 public static void Record(IntPtr handle,string directory,int fps,int seconds,bool video,bool observe) {
  Rect r; GetClientRect(handle,out r); Point p=new Point(); ClientToScreen(handle,ref p);
  Directory.CreateDirectory(directory);
  SetCursorPos(p.X+r.Right/2,p.Y+r.Bottom/2);
  // Fixed normal-input route: first jump, Shift bridge, shared landing, orbit, recovery.
  int[] ms={4000,4570,5000,5650,7000,7100,8000,8180,8500,9150,11000,11110,11300,11950,21000,22800};
  byte[][] keys={new byte[]{87},new byte[]{87},new byte[]{87,32},new byte[]{87,32},new byte[]{160},new byte[]{160},
   new byte[]{87},new byte[]{87},new byte[]{87,32},new byte[]{87,32},new byte[]{87},new byte[]{87},
   new byte[]{87,32},new byte[]{87,32},new byte[]{87},new byte[]{87}};
  int eventIndex=0; bool orbit=false;
  using(Bitmap image=new Bitmap(r.Right,r.Bottom))
  using(Graphics g=Graphics.FromImage(image))
  using(StreamWriter audit=new StreamWriter(Path.Combine(directory,"capture-times.csv"))) {
   audit.WriteLine("frame,elapsed_ms,deadline_ms");
   Stopwatch watch=Stopwatch.StartNew();
   try {
    for(int frame=0;frame<seconds*fps;frame++) {
     int deadline=(int)(frame*1000.0/fps);
     int remaining=deadline-(int)watch.ElapsedMilliseconds;
     if(remaining>0)Thread.Sleep(remaining);
     if(!IsWindow(handle))break; // An observed route can complete and close normally.
     if(GetForegroundWindow()!=handle)throw new InvalidOperationException("Benchmark player lost focus (expected="+handle+",actual="+GetForegroundWindow()+"). No further input sent.");
     long elapsed=watch.ElapsedMilliseconds;
     while(!observe && eventIndex<ms.Length && elapsed>=ms[eventIndex]) {
      foreach(byte key in keys[eventIndex])Key(key,eventIndex%2==0);
      eventIndex++;
     }
     bool wantedOrbit=!observe && elapsed>=16000 && elapsed<19000;
     if(wantedOrbit!=orbit){mouse_event(wantedOrbit?8u:16u,0,0,0,UIntPtr.Zero);orbit=wantedOrbit;}
     if(orbit)mouse_event(1,elapsed<17500?12:-12,0,0,UIntPtr.Zero);
     if(video) {
      g.CopyFromScreen(p.X,p.Y,0,0,image.Size);
      image.Save(Path.Combine(directory,"frame-"+frame.ToString("D5")+".jpg"),ImageFormat.Jpeg);
     }
     audit.WriteLine(frame+","+watch.ElapsedMilliseconds+","+deadline);
    }
   } finally {
    if(!observe)foreach(byte key in new byte[]{87,32,160})Key(key,false);
    if(orbit)mouse_event(16,0,0,0,UIntPtr.Zero);
   }
  }
 }
}
'@
$benchmarkLog=Join-Path $Output 'player.log'
[void][System.IO.Directory]::CreateDirectory($Output)
$benchmarkAudio=Join-Path $Output 'gameplay.wav'
$benchmarkExtra=if ($Audio) { '-shiftboundRecordMix "' + $benchmarkAudio + '"' } else { '' }
$benchmarkLaunch="-force-d3d11 -screen-width $Width -screen-height $Height -screen-fullscreen 0 $Arguments $benchmarkExtra -logFile `"$benchmarkLog`""
[IO.File]::WriteAllText((Join-Path $Output 'launch.txt'),$benchmarkLaunch)
$benchmarkPlayer=Start-Process -FilePath $Player -ArgumentList $benchmarkLaunch -WindowStyle Hidden -PassThru
try {
 Start-Sleep -Seconds 3
 $benchmarkHandle=[BenchmarkRecorder]::FindWindow('UnityWndClass','Shiftbound')
 $benchmarkPid=0
 [void][BenchmarkRecorder]::GetWindowThreadProcessId($benchmarkHandle,[ref]$benchmarkPid)
 if($benchmarkPid -ne $benchmarkPlayer.Id){throw 'Cannot identify the owned benchmark player window.'}
 [void][BenchmarkRecorder]::ShowWindow($benchmarkHandle,5)
 [void][BenchmarkRecorder]::SetForegroundWindow($benchmarkHandle)
 Start-Sleep -Seconds 5
 [BenchmarkRecorder]::Focus($benchmarkHandle)
 Start-Sleep -Milliseconds 250
 if($Audio) {
  [IO.File]::WriteAllText($benchmarkAudio+'.start',[DateTime]::UtcNow.ToString('o'))
  $benchmarkDeadline=[DateTime]::UtcNow.AddSeconds(5)
  while(-not (Test-Path -LiteralPath ($benchmarkAudio+'.ready'))) {
   if([DateTime]::UtcNow -gt $benchmarkDeadline){throw 'Actual mix recorder did not initialize.'}
   Start-Sleep -Milliseconds 10
  }
 }
 [BenchmarkRecorder]::Record($benchmarkHandle,$Output,$Fps,$Seconds,(!$NoVideo),[bool]$ObserveOnly)
 if($Audio) {
  [IO.File]::WriteAllText($benchmarkAudio+'.stop',[DateTime]::UtcNow.ToString('o'))
  $benchmarkDeadline=[DateTime]::UtcNow.AddSeconds(5)
  while(-not (Test-Path -LiteralPath $benchmarkAudio)) {
   if([DateTime]::UtcNow -gt $benchmarkDeadline){throw 'Actual mix recorder produced no WAV.'}
   Start-Sleep -Milliseconds 50
  }
 }
 if($NoVideo) { return }
 if($FFmpeg) {
  $benchmarkEncode=@('-hide_banner','-loglevel','error','-y','-framerate',"$Fps",'-i',(Join-Path $Output 'frame-%05d.jpg'))
  if($Audio){$benchmarkEncode+=@('-i',$benchmarkAudio)}
  $benchmarkEncode+=@('-t',"$Seconds",'-c:v','libx264','-crf','20','-pix_fmt','yuv420p')
  if($Audio){$benchmarkEncode+=@('-c:a','aac','-b:a','160k')}
  $benchmarkEncode+=@('-movflags','+faststart',(Join-Path $Output 'gameplay-25s.mp4'))
  & $FFmpeg @benchmarkEncode
  if($LASTEXITCODE -ne 0){throw 'Gameplay video encoding failed.'}
 } else {
  & "$PSScriptRoot/PackMotionCapture.ps1" -Frames $Output -Output "$Output/gameplay-25s.avi" -Fps $Fps
 }
} finally {
 if(!$benchmarkPlayer.HasExited){[void]$benchmarkPlayer.CloseMainWindow()}
}
