param([string]$Output=(Join-Path $PSScriptRoot '../UnityProject/Assets/Shiftbound/AudioCandidate'))
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force $Output | Out-Null
Add-Type @'
using System;
using System.IO;
public static class RooftopAmbienceAuthor {
 public static void Write(string path,bool leaves) {
  const int rate=22050, seconds=24, overlap=2;
  int n=rate*seconds, fade=rate*overlap;
  float[] raw=new float[n+fade];
  Random rng=new Random(leaves?1309:6103);
  double low=0, middle=0, previous=0;
  for(int i=0;i<raw.Length;i++) {
   double t=(double)i/rate;
   double noise=rng.NextDouble()*2-1;
   low+=.018*(noise-low); middle+=.13*(noise-middle);
   double gust=.6+.2*Math.Sin(t*.67)+.15*Math.Sin(t*.21);
   // Air + distant ventilation for Present, higher rustle and tiny insect
   // texture for Overgrown. Original procedural sound design, not field recordings.
   double air=low*.19*gust+(middle-low)*(leaves?.105:.025);
   double machine=leaves?0:.008*(Math.Sin(2*Math.PI*83*t)+.4*Math.Sin(2*Math.PI*166*t));
   double insect=leaves?.009*Math.Sin(2*Math.PI*(2300+120*Math.Sin(t*2))*t)*Math.Pow(Math.Max(0,Math.Sin(t*1.1)),8):0;
   double value=air+machine+insect;
   // Gentle DC rejection.
   raw[i]=(float)(value-previous*.05); previous=value;
  }
  // Equal-period crossfade: first two seconds blend the tail into the head;
  // end of output connects to raw[n], the beginning of the blended tail.
  for(int i=0;i<fade;i++) {
   float a=(float)i/fade;
   raw[i]=raw[n+i]*(1-a)+raw[i]*a;
  }
  using(var stream=File.Create(path)) using(var w=new BinaryWriter(stream)) {
   w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);
   w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);
   w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);
   w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);
   for(int i=0;i<n;i++)w.Write((short)(Math.Max(-.9,Math.Min(.9,raw[i]))*32767));
  }
 }
}
'@
[RooftopAmbienceAuthor]::Write((Join-Path $Output 'Present-Air.wav'),$false)
[RooftopAmbienceAuthor]::Write((Join-Path $Output 'Overgrown-Rustle.wav'),$true)
Write-Output 'Authored deterministic 24-second mono loops at 22050Hz; listening review required.'
