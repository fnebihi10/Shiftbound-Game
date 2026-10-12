param([Parameter(Mandatory=$true)][string]$Csv,[Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference='Stop'
$rows=@(Import-Csv -LiteralPath $Csv)
$culture=[Globalization.CultureInfo]::InvariantCulture
function Number($value){[double]::Parse($value,$culture)}
function Percentile($values,[double]$fraction){
 if($values.Count -eq 0){return $null}
 $ordered=@($values | Sort-Object)
 $ordered[[Math]::Min($ordered.Count-1,[int][Math]::Floor(($ordered.Count-1)*$fraction))]
}
$feet=foreach($side in @('l','r')){
 $speed=[Collections.Generic.List[double]]::new()
 $distance=0.0
 for($i=1;$i -lt $rows.Count;$i++){
  $a=$rows[$i-1];$b=$rows[$i]
  $weight=if($side -eq 'l'){'left_weight'}else{'right_weight'}
  $dt=(Number $b.time_s)-(Number $a.time_s)
  if($a.grounded -ne '1' -or $b.grounded -ne '1' -or (Number $a.$weight) -lt .95 -or (Number $b.$weight) -lt .95 -or (Number $b.speed_m_s) -lt .5 -or $dt -le 0 -or $dt -gt .1){continue}
  $dx=(Number $b.($side+'x'))-(Number $a.($side+'x'))
  $dz=(Number $b.($side+'z'))-(Number $a.($side+'z'))
  $travel=[Math]::Sqrt($dx*$dx+$dz*$dz)
  $distance+=$travel;$speed.Add($travel/$dt)
 }
 [ordered]@{foot=$side;samples=$speed.Count;horizontal_speed_m_s_p50=(Percentile $speed .5);horizontal_speed_m_s_p95=(Percentile $speed .95);horizontal_speed_m_s_max=if($speed.Count){($speed|Measure-Object -Maximum).Maximum}else{$null};sampled_horizontal_travel_m=$distance}
}
[ordered]@{source_csv=[IO.Path]::GetFullPath($Csv);rows=$rows.Count;feet=$feet;scope='Consecutive grounded moving samples with IK weight >=0.95. Measures post-animation horizontal foot displacement, including anchor reacquisition. Does not prove contact quality during weight ramps, airborne motion or low-speed transitions. No animation acceptance threshold inferred.'} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $Output
