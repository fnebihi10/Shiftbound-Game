param(
    [Parameter(Mandatory=$true)][string]$Frames,
    [Parameter(Mandatory=$true)][string]$Output,
    [int]$Fps = 20
)
# Packages actual player JPEG frames as a silent MJPEG AVI. No interpolation or generated imagery.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$motionFiles = @(Get-ChildItem -LiteralPath $Frames -Filter 'frame-*.jpg' | Sort-Object Name)
if ($motionFiles.Count -eq 0 -or $Fps -lt 1) { throw 'Need captured frames and a positive frame rate.' }
$motionImage = [System.Drawing.Image]::FromFile($motionFiles[0].FullName)
$motionWidth = $motionImage.Width; $motionHeight = $motionImage.Height; $motionImage.Dispose()
$motionStream = [System.IO.File]::Create($Output)
$motionWriter = [System.IO.BinaryWriter]::new($motionStream)
$motionIndex = [System.Collections.Generic.List[object]]::new()
function FourCC([string]$Value) { $motionWriter.Write([System.Text.Encoding]::ASCII.GetBytes($Value)) }
function Begin-Chunk([string]$Name, [string]$Type = '') {
    FourCC $Name
    $position = $motionStream.Position
    $motionWriter.Write([uint32]0)
    if ($Type) { FourCC $Type }
    return $position
}
function End-Chunk([long]$Position) {
    $end = $motionStream.Position
    [void]$motionStream.Seek($Position, [System.IO.SeekOrigin]::Begin)
    $motionWriter.Write([uint32]($end - $Position - 4))
    [void]$motionStream.Seek($end, [System.IO.SeekOrigin]::Begin)
}
try {
    $riff = Begin-Chunk 'RIFF' 'AVI '
    $header = Begin-Chunk 'LIST' 'hdrl'
    FourCC 'avih'; $motionWriter.Write([uint32]56)
    foreach ($value in @([uint32](1000000/$Fps),0,0,16,$motionFiles.Count,0,1,0,$motionWidth,$motionHeight,0,0,0,0)) { $motionWriter.Write([uint32]$value) }
    $streamHeader = Begin-Chunk 'LIST' 'strl'
    FourCC 'strh'; $motionWriter.Write([uint32]56); FourCC 'vids'; FourCC 'MJPG'
    $motionWriter.Write([uint32]0); $motionWriter.Write([uint16]0); $motionWriter.Write([uint16]0)
    foreach ($value in @(0,1,$Fps,0,$motionFiles.Count,0,[uint32]::MaxValue,0)) { $motionWriter.Write([uint32]$value) }
    foreach ($value in @(0,0,$motionWidth,$motionHeight)) { $motionWriter.Write([int16]$value) }
    FourCC 'strf'; $motionWriter.Write([uint32]40); $motionWriter.Write([uint32]40)
    $motionWriter.Write([int32]$motionWidth); $motionWriter.Write([int32]$motionHeight)
    $motionWriter.Write([uint16]1); $motionWriter.Write([uint16]24); FourCC 'MJPG'
    foreach ($value in @(($motionWidth*$motionHeight*3),0,0,0,0)) { $motionWriter.Write([uint32]$value) }
    End-Chunk $streamHeader; End-Chunk $header
    $movie = Begin-Chunk 'LIST' 'movi'; $movieOrigin = $motionStream.Position - 4
    foreach ($file in $motionFiles) {
        $data = [System.IO.File]::ReadAllBytes($file.FullName)
        $motionIndex.Add([pscustomobject]@{Offset=$motionStream.Position-$movieOrigin;Length=$data.Length})
        FourCC '00dc'; $motionWriter.Write([uint32]$data.Length); $motionWriter.Write($data)
        if ($data.Length % 2) { $motionWriter.Write([byte]0) }
    }
    End-Chunk $movie
    FourCC 'idx1'; $motionWriter.Write([uint32]($motionIndex.Count*16))
    foreach ($entry in $motionIndex) { FourCC '00dc'; $motionWriter.Write([uint32]16); $motionWriter.Write([uint32]$entry.Offset); $motionWriter.Write([uint32]$entry.Length) }
    End-Chunk $riff
}
finally { $motionWriter.Dispose(); $motionStream.Dispose() }
Write-Output "Packed $($motionFiles.Count) real player frames at $Fps FPS into $Output (silent scripted traversal; not a performance recording)."
