param([Parameter(Mandatory=$true)][string]$Directory)
$ErrorActionPreference='Stop'
$results=foreach($file in Get-ChildItem -LiteralPath $Directory -Filter 'sample-*-presentation.txt' | Sort-Object Name){
    $raw=[IO.File]::ReadAllText($file.FullName)
    foreach($section in [regex]::Split($raw,'(?=layerName = )')){
        if($section -notmatch '^layerName = SurfaceView - com\.shiftboundproject\.shiftbound/') {continue}
        $hist=[regex]::Match($section,'present2present histogram is as below:\s*([^\r\n]+)').Groups[1].Value
        if(!$hist){continue}
        $bins=@([regex]::Matches($hist,'(\d+)ms=(\d+)') | ForEach-Object {[pscustomobject]@{ms=[int]$_.Groups[1].Value;count=[long]$_.Groups[2].Value}})
        $total=($bins | Measure-Object count -Sum).Sum
        if(!$total){continue}
        $quantiles=@{}
        foreach($q in @(50,95,99)){ $sum=0;foreach($bin in $bins){$sum+=$bin.count;if($sum -ge $total*$q/100){$quantiles["p$q"]=$bin.ms;break}} }
        $long=($bins | Where-Object ms -ge 32 | Measure-Object count -Sum).Sum
        [pscustomobject][ordered]@{sample=$file.BaseName;intervals=$total;histogramMs=$quantiles;intervalsAtLeast32ms=$long;percentAtLeast32ms=[math]::Round(100*$long/$total,3);bins=$bins}
    }
}
$report=[ordered]@{method='SurfaceFlinger per-app SurfaceView present2present histogram; Android bins are quantized. 16ms bucket includes 60Hz intervals; not exact 16.000ms. Aggregate includes idle/menus, recording overhead and test restarts; not sustained representative gameplay acceptance.';samples=@($results)}
[IO.File]::WriteAllText((Join-Path $Directory 'presentation-summary.json'),($report | ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
$results | Select-Object sample,intervals,percentAtLeast32ms | Select-Object -Last 3
