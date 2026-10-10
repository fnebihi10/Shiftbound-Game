param(
    [string]$Sdk='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK',
    [string]$Jdk='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK'
)
$ErrorActionPreference='Stop'
$probeRoot=Split-Path -Parent $PSScriptRoot
$probeOutput=Join-Path $probeRoot '.validation/ProductionUpgrade/PhoneTouchProbe'
$probeClasses=Join-Path $probeOutput 'classes'
$probeDex=Join-Path $probeOutput 'dex'
New-Item -ItemType Directory -Force $probeClasses,$probeDex | Out-Null
$probePlatform=Join-Path $Sdk 'platforms/android-36/android.jar'
$probeTools=Join-Path $Sdk 'build-tools/36.0.0'
function Invoke-ProbeTool([string]$File,[string[]]$ToolArgs) {
    & $File @ToolArgs
    if($LASTEXITCODE -ne 0){throw "Probe build failed: $File"}
}
Invoke-ProbeTool (Join-Path $Jdk 'bin/javac.exe') @('-source','8','-target','8','-classpath',$probePlatform,'-d',$probeClasses,(Join-Path $PSScriptRoot 'PhoneTouchProbe/PhoneTouchProbe.java'))
$probeJar=Join-Path $probeOutput 'probe.jar'
Invoke-ProbeTool (Join-Path $Jdk 'bin/jar.exe') @('cf',$probeJar,'-C',$probeClasses,'.')
Invoke-ProbeTool (Join-Path $Jdk 'bin/java.exe') @('-classpath',(Join-Path $probeTools 'lib/d8.jar'),'com.android.tools.r8.D8','--min-api','26','--lib',$probePlatform,'--output',$probeDex,$probeJar)
$probeUnsigned=Join-Path $probeOutput 'probe-unsigned.apk'
Invoke-ProbeTool (Join-Path $probeTools 'aapt.exe') @('package','-f','-M',(Join-Path $PSScriptRoot 'PhoneTouchProbe/AndroidManifest.xml'),'-I',$probePlatform,'-F',$probeUnsigned)
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$probeZip=[IO.Compression.ZipFile]::Open($probeUnsigned,[IO.Compression.ZipArchiveMode]::Update)
try { [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($probeZip,(Join-Path $probeDex 'classes.dex'),'classes.dex') | Out-Null }
finally {$probeZip.Dispose()}
$probeApk=Join-Path $probeOutput 'Shiftbound-TouchProbe.apk'
Invoke-ProbeTool (Join-Path $probeTools 'zipalign.exe') @('-f','4',$probeUnsigned,$probeApk)
# Standard Android SDK debug key, never release credentials. Companion APK has
# no native libraries, permissions, launcher activity or Internet capability.
$probeKey=Join-Path $env:USERPROFILE '.android/debug.keystore'
if(-not(Test-Path -LiteralPath $probeKey)){throw 'SDK debug keystore unavailable'}
Invoke-ProbeTool (Join-Path $Jdk 'bin/java.exe') @('-jar',(Join-Path $probeTools 'lib/apksigner.jar'),'sign','--ks',$probeKey,'--ks-key-alias','androiddebugkey','--ks-pass','pass:android','--key-pass','pass:android',$probeApk)
Invoke-ProbeTool (Join-Path $Jdk 'bin/java.exe') @('-jar',(Join-Path $probeTools 'lib/apksigner.jar'),'verify',$probeApk)
Write-Output "PASS: separate Android touch probe -> $probeApk"
