param(
 [Parameter(Mandatory=$true)][string]$Apk,
 [Parameter(Mandatory=$true)][string]$Output,
 [string]$AndroidTools='C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Data/PlaybackEngines/AndroidPlayer'
)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$artifactRoot=[IO.Path]::GetFullPath($Output)
[void][IO.Directory]::CreateDirectory($artifactRoot)
$archive=[IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $Apk).Path)
$nativeResults=@()
try {
 foreach($entry in $archive.Entries | Where-Object {$_.FullName -match '^lib/[^/]+/[^/]+\.so$'}) {
  $abi=($entry.FullName -split '/')[1]
  $abiRoot=Join-Path $artifactRoot $abi;[void][IO.Directory]::CreateDirectory($abiRoot)
  $nativePath=Join-Path $abiRoot $entry.Name
  [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$nativePath,$true)
  $headers=@(& "$AndroidTools/NDK/toolchains/llvm/prebuilt/windows-x86_64/bin/llvm-readelf.exe" -lW $nativePath)
  if($LASTEXITCODE -ne 0){throw "Cannot inspect $($entry.Name)"}
  $headers | Set-Content -LiteralPath ($nativePath+'.headers.txt')
  $loadAlign=@($headers | Where-Object {$_ -match '^\s*LOAD\s'} | ForEach-Object {[Convert]::ToInt64(($_.Trim() -split '\s+')[-1].Replace('0x',''),16)})
  $relro=@($headers | Where-Object {$_ -match '^\s*GNU_RELRO\s'} | ForEach-Object {
   $parts=$_.Trim() -split '\s+'
   $start=[Convert]::ToInt64($parts[2].Replace('0x',''),16)
   $size=[Convert]::ToInt64($parts[5].Replace('0x',''),16)
   ($start+$size)%16384 -eq 0
  })
  $pass=$loadAlign.Count -gt 0 -and @($loadAlign | Where-Object {$_ -lt 16384}).Count -eq 0 -and @($relro | Where-Object {-not $_}).Count -eq 0
  $nativeResults+=[ordered]@{abi=$abi;library=$entry.Name;sha256=(Get-FileHash $nativePath -Algorithm SHA256).Hash.ToLowerInvariant();load_alignments=$loadAlign;relro_end_aligned=$relro;static_16kb_pass=$pass}
 }
} finally {$archive.Dispose()}
$alignment=@(& "$AndroidTools/SDK/build-tools/36.0.0/zipalign.exe" -c -P 16 -v 4 $Apk)
$alignmentPass=$LASTEXITCODE -eq 0
$alignment | Set-Content -LiteralPath (Join-Path $artifactRoot 'zipalign.txt')
$env:JAVA_HOME=Join-Path $AndroidTools 'OpenJDK'
$signature=@(& "$AndroidTools/SDK/build-tools/36.0.0/apksigner.bat" verify --verbose --print-certs $Apk)
$signaturePass=$LASTEXITCODE -eq 0
$signature | Set-Content -LiteralPath (Join-Path $artifactRoot 'signature.txt')
$manifest=@(& "$AndroidTools/SDK/build-tools/36.0.0/aapt.exe" dump badging $Apk)
$permissions=@(& "$AndroidTools/SDK/build-tools/36.0.0/aapt.exe" dump permissions $Apk)
$manifest | Set-Content -LiteralPath (Join-Path $artifactRoot 'badging.txt')
$permissions | Set-Content -LiteralPath (Join-Path $artifactRoot 'permissions.txt')
[ordered]@{apk_sha256=(Get-FileHash $Apk -Algorithm SHA256).Hash.ToLowerInvariant();native=$nativeResults;zipalign_pass=$alignmentPass;signature_pass=$signaturePass;runtime_16kb='NOT VERIFIED: supported 16KB device/emulator run recorded separately';install_update='NOT VERIFIED: no connected phone'} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $artifactRoot 'inspection.json')
if(-not $alignmentPass -or -not $signaturePass -or $nativeResults.Count -eq 0 -or @($nativeResults | Where-Object {-not $_.static_16kb_pass}).Count -gt 0){throw 'Android static artifact check failed; inspect inspection.json.'}
Write-Output "ANDROID STATIC ARTIFACT CHECKS PASSED: $Apk. Runtime/device compatibility remains separate."
