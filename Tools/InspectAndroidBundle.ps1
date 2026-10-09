param(
 [Parameter(Mandatory=$true)][string]$Bundle,
 [Parameter(Mandatory=$true)][string]$Output,
 [string]$Bundletool='',
 [string]$AndroidTools='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Data/PlaybackEngines/AndroidPlayer'
)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
if(-not $Bundletool){$Bundletool=Join-Path $root '.validation/AndroidTooling/bundletool-all-1.18.3.jar'}
$bundlePath=(Resolve-Path -LiteralPath $Bundle).Path
$toolPath=(Resolve-Path -LiteralPath $Bundletool).Path
$outPath=[IO.Path]::GetFullPath($Output)
[void][IO.Directory]::CreateDirectory($outPath)
function Run-Tool($file,$arguments,$name){
 $p=Start-Process -FilePath $file -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $outPath ($name+'.txt')) -RedirectStandardError (Join-Path $outPath ($name+'.stderr.txt'))
 $null=$p.Handle
 $deadline=[DateTime]::UtcNow.AddMinutes(4)
 while(-not $p.WaitForExit(1000)){if([DateTime]::UtcNow -gt $deadline){$p.Kill();throw "Timeout: $name"}}
 $p.WaitForExit()
 if($p.ExitCode -ne 0){throw "Bundle check failed: $name ($($p.ExitCode))"}
}
$java=Join-Path $AndroidTools 'OpenJDK/bin/java.exe'
$jar='-jar "'+$toolPath+'" '
$bundleArg=' --bundle="'+$bundlePath+'"'
Run-Tool $java ($jar+'validate'+$bundleArg) 'validate'
Run-Tool $java ($jar+'dump config'+$bundleArg) 'config'
Run-Tool $java ($jar+'dump manifest'+$bundleArg) 'manifest'
Run-Tool (Join-Path $AndroidTools 'OpenJDK/bin/jarsigner.exe') ('-verify "'+$bundlePath+'"') 'signature'
# Local debug signing only. This APK set is not a Play-distributed installation.
$set=Join-Path $outPath 'candidate.apks'
Run-Tool $java ($jar+'build-apks'+$bundleArg+' --output="'+$set+'" --overwrite') 'build-apks'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive=[IO.Compression.ZipFile]::OpenRead($set)
$nativeApk=$null
try{
 foreach($entry in $archive.Entries | Where-Object {$_.FullName -match '\.apk$'}){
  $path=Join-Path $outPath $entry.Name
  [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$path,$true)
  $apkArchive=[IO.Compression.ZipFile]::OpenRead($path)
  try{if(@($apkArchive.Entries | Where-Object {$_.FullName -match '^lib/arm64-v8a/[^/]+\.so$'}).Count){$nativeApk=$path}}
  finally{$apkArchive.Dispose()}
 }
}finally{$archive.Dispose()}
if(-not $nativeApk){throw 'Generated APK set contains no ARM64 native split.'}
$alignment=(Get-Content (Join-Path $outPath 'config.txt') -Raw).Contains('PAGE_ALIGNMENT_16K')
[ordered]@{aab_sha256=(Get-FileHash $bundlePath -Algorithm SHA256).Hash.ToLowerInvariant();bundletool_sha256=(Get-FileHash $toolPath -Algorithm SHA256).Hash.ToLowerInvariant();bundle_validation_pass=$true;jar_signature_verified=$true;page_alignment_16k=$alignment;apk_set_sha256=(Get-FileHash $set -Algorithm SHA256).Hash.ToLowerInvariant();native_split=[IO.Path]::GetFileName($nativeApk);device_install='NOT VERIFIED';play_distribution='NOT VERIFIED: local debug-signed APK set only'} | ConvertTo-Json | Set-Content (Join-Path $outPath 'inspection.json')
& (Join-Path $PSScriptRoot 'InspectAndroidArtifact.ps1') -Apk $nativeApk -Output (Join-Path $outPath 'NativeSplit') -AndroidTools $AndroidTools
if(-not $alignment){throw 'AAB does not request 16KB page alignment.'}
