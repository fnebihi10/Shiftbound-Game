"""Analyze an existing device run; partial data never establishes sustained PASS."""
import argparse, csv, json, pathlib, re, math
parser=argparse.ArgumentParser()
parser.add_argument('directory')
args=parser.parse_args()
root=pathlib.Path(args.directory)
conditions=json.loads((root/'conditions.json').read_text())
with (root/'presentations.csv').open() as stream:frames=list(csv.DictReader(stream))
with (root/'polls.csv').open() as stream:polls=list(csv.DictReader(stream))
intervals=sorted(float(f['display_interval_ms']) for f in frames if f.get('display_interval_ms'))
def percentile(p):
    if not intervals:return None
    n=(len(intervals)-1)*p;l=int(n);h=min(l+1,len(intervals)-1)
    return round(intervals[l]+(intervals[h]-intervals[l])*(n-l),4)
raw=(root/'device-samples.txt').read_text()
pss=[int(v)/1024 for v in re.findall(r'TOTAL PSS:\s*(\d+)',raw)]
if not pss:pss=[int(v)/1024 for v in re.findall(r'^\s*TOTAL\s+(\d+)\s',raw,re.M)]
battery=[int(v)/10 for v in re.findall(r'^\s*temperature:\s*(\d+)',raw,re.M)]
thermals=[int(v) for v in re.findall(r'Thermal Status:\s*(\d+)',raw)]
elapsed=float(polls[-1]['elapsed_s']) if polls else 0
missed=sum(max(0,round(v*conditions['target_fps']/1000)-1) for v in intervals)
summary=json.loads((root/'summary.json').read_text()) if(root/'summary.json').exists() else {}
complete=summary.get('completed_duration',False) and summary.get('workload_verified',False)
report=dict(duration_s=elapsed,sustained_status='MEASURED; evaluate acceptance and workload coverage' if complete else 'NOT VERIFIED',
    intervals=len(intervals),p50_ms=percentile(.5),p95_ms=percentile(.95),p99_ms=percentile(.99),
    max_ms=max(intervals) if intervals else None,over_100ms=sum(v>100 for v in intervals),
    estimated_missed_slot_fraction=missed/(len(intervals)+missed) if intervals else None,
    pss_mb_range=[min(pss),max(pss)] if pss else None,
    battery_c_range=[min(battery),max(battery)] if battery else None,max_thermal_status=max(thermals) if thermals else None,
    sampling_gaps=sum(f['sampling_boundary']=='True' for f in frames),polls_without_new_frames=sum(p['new_frames']=='0' for p in polls),errors=sum(bool(p['error']) for p in polls),
    conditions=conditions['conditions'])
report['route_completions']=summary.get('route_completions')
report['workload_verified']=summary.get('workload_verified',False)
report['minutes']=[]
for minute in range(math.ceil(elapsed/60)):
    values=sorted(float(f['display_interval_ms']) for f in frames if f.get('display_interval_ms') and minute*60<=float(f['elapsed_s'])<(minute+1)*60)
    def value_at(p):
        if not values:return None
        n=(len(values)-1)*p;l=int(n);h=min(l+1,len(values)-1)
        return round(values[l]+(values[h]-values[l])*(n-l),4)
    report['minutes'].append(dict(minute=minute+1,intervals=len(values),p50_ms=value_at(.5),p95_ms=value_at(.95),p99_ms=value_at(.99),over_50ms=sum(v>50.5 for v in values)))
print(json.dumps(report,indent=2))
