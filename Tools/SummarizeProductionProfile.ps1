param([Parameter(Mandatory=$true)][string]$InputDirectory,[Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
$profileRows=@(Import-Csv (Join-Path $InputDirectory 'frames.csv'))
function Percentile($Rows,[string]$Field,[double]$Quantile) {
 $values=@($Rows | ForEach-Object {if($_.$Field -ne 'unavailable'){[double]::Parse($_.$Field,[Globalization.CultureInfo]::InvariantCulture)}} | Where-Object {$_ -gt 0} | Sort-Object)
 if($values.Count -eq 0){return $null}
 return [math]::Round($values[[math]::Max(0,[math]::Ceiling($values.Count*$Quantile)-1)],3)
}
$summary=[ordered]@{hardware=[System.IO.File]::ReadAllLines((Join-Path (Resolve-Path $InputDirectory).Path 'hardware.txt'));samples=$profileRows.Count;raw_sha256=(Get-FileHash (Join-Path $InputDirectory 'frames.csv') -Algorithm SHA256).Hash;phases=@()}
foreach($phaseGroup in @($profileRows | Group-Object phase)) {
 $summary.phases+=[ordered]@{phase=$phaseGroup.Name;samples=$phaseGroup.Count;cpu_frame_p50_ms=Percentile $phaseGroup.Group 'cpu_frame_ms' 0.5;cpu_frame_p95_ms=Percentile $phaseGroup.Group 'cpu_frame_ms' 0.95;cpu_main_p95_ms=Percentile $phaseGroup.Group 'cpu_main_ms' 0.95;gpu_p50_ms=Percentile $phaseGroup.Group 'gpu_ms' 0.5;gpu_p95_ms=Percentile $phaseGroup.Group 'gpu_ms' 0.95;batches_p50=Percentile $phaseGroup.Group 'batches' 0.5;draws_p50=Percentile $phaseGroup.Group 'draws' 0.5;triangles_p50=Percentile $phaseGroup.Group 'triangles' 0.5}
}
$summary | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $Output
Write-Output "Summarized $($profileRows.Count) measured frames -> $Output"
