param([Parameter(Mandatory=$true)][string]$Directory,[Parameter(Mandatory=$true)][string]$Log,[Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
$all=@(Import-Csv (Join-Path $Directory 'gameplay.csv'))
# Separate startup; report the first three seconds independently.
$steady=@($all | Where-Object {[double]$_.elapsed_s -ge 3})
function Percentile($rows,$field,$p) {
 $v=@($rows | ForEach-Object {[double]::Parse($_.$field,[Globalization.CultureInfo]::InvariantCulture)} | Where-Object {$_ -gt 0} | Sort-Object)
 if($v.Count -eq 0){return $null}
 return [Math]::Round($v[[Math]::Max(0,[Math]::Ceiling($v.Count*$p)-1)],3)
}
$warningCount=@(Select-String -LiteralPath $Log -Pattern 'GfxDeviceD3D11Base::PresentFrame\(\) was called without calling').Count
$seconds=if($all.Count){[double]$all[-1].elapsed_s-[double]$all[0].elapsed_s}else{0}
$summary=[ordered]@{hardware=[IO.File]::ReadAllLines((Join-Path $Directory 'device.txt'));samples=$all.Count;duration_s=$seconds;raw_sha256=(Get-FileHash (Join-Path $Directory 'gameplay.csv') -Algorithm SHA256).Hash;warnings=$warningCount;warnings_per_minute=if($seconds -gt 0){[math]::Round($warningCount*60/$seconds,3)}else{$null};warnings_per_1000_callbacks=if($all.Count){[math]::Round($warningCount*1000/$all.Count,3)}else{$null};startup_p99_callback_ms=Percentile @($all | Where-Object {[double]$_.elapsed_s -lt 3}) 'callback_ms' .99;steady=[ordered]@{p50_callback_ms=Percentile $steady 'callback_ms' .5;p95_callback_ms=Percentile $steady 'callback_ms' .95;p99_callback_ms=Percentile $steady 'callback_ms' .99;p95_cpu_ms=Percentile $steady 'cpu_ms' .95;p95_main_ms=Percentile $steady 'main_ms' .95;p95_render_ms=Percentile $steady 'render_ms' .95;p95_gpu_ms=Percentile $steady 'gpu_ms' .95;p50_draws=Percentile $steady 'draws' .5;p50_triangles=Percentile $steady 'triangles' .5;p95_gc_bytes=Percentile $steady 'gc_bytes' .95;p95_memory_bytes=Percentile $steady 'memory_bytes' .95};shift_frames=@($all | Where-Object {[int]$_.shift_result -gt 0} | Select-Object elapsed_s,callback_ms,shift_result);limits='Windows callback timing, not Android presentation or physical input latency. Warnings lack per-event timestamps; correlation with individual stalls NOT VERIFIED.'}
$summary | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath $Output
Write-Output "Summarized $($all.Count) callbacks; warnings=$warningCount -> $Output"
