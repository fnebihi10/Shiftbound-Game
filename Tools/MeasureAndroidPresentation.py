"""ADB-only physical rendering measurement. No screen capture or input injection.

Android 11 SurfaceFlinger FrameTracker prints desired, actual, frame-ready ns.
Reference: android.googlesource.com/platform/frameworks/native/+/master/services/surfaceflinger/FrameTracker.cpp
Observed presentation intervals are independent of Unity callback intervals.
This is not FrameTimeline deadline attribution or physical input-to-photon.
"""
import argparse, csv, hashlib, json, pathlib, statistics, subprocess, time

parser = argparse.ArgumentParser()
parser.add_argument('--adb', required=True)
parser.add_argument('--serial', required=True)
parser.add_argument('--apk', required=True)
parser.add_argument('--output', required=True)
parser.add_argument('--seconds', type=int, default=1200)
parser.add_argument('--fps', type=int, choices=(30, 60), default=60)
args = parser.parse_args()
output = pathlib.Path(args.output); output.mkdir(parents=True, exist_ok=True)
package = 'com.shiftboundproject.shiftbound'
def adb(*command):
    result = subprocess.run([args.adb, '-s', args.serial, *command], capture_output=True,
                            text=True, encoding='utf-8', errors='replace', timeout=15,
                            creationflags=subprocess.CREATE_NO_WINDOW)
    if result.returncode: raise RuntimeError(result.stderr.strip() or result.stdout.strip())
    return result.stdout
def shell(command): return adb('shell', command)
identity = json.loads(pathlib.Path(args.apk+'.manifest.json').read_text(encoding='utf-8-sig'))
apk_sha = hashlib.file_digest(open(args.apk, 'rb'), 'sha256').hexdigest()
recorded = [b['sha256'] for b in identity['binaries'] if b['path'].endswith('.apk')]
if recorded != [apk_sha]: raise RuntimeError('Measured APK differs from recorded build')
installed_paths = shell('pm path '+package).strip().splitlines()
if len(installed_paths) != 1 or not installed_paths[0].startswith('package:/data/app/'):
    raise RuntimeError('Expected one installed base APK')
installed_path = installed_paths[0][len('package:'):]
if any(c in installed_path for c in "'\"\n\r; ") or not installed_path.endswith('/base.apk'):
    raise RuntimeError('Unexpected installed APK path')
installed_sha = shell("sha256sum '"+installed_path+"'").split()[0]
if installed_sha != apk_sha: raise RuntimeError('Installed APK differs from measured build')
(output/'identity.json').write_text(json.dumps(identity, indent=2), encoding='utf-8')
device = {key: shell('getprop '+prop).strip() for key,prop in (
    ('model','ro.product.model'),('android','ro.build.version.release'),('api','ro.build.version.sdk'))}
device['page_size'] = shell('getconf PAGE_SIZE').strip()
device['display'] = shell('wm size').strip()
device.update(apk_sha256=apk_sha, installed_apk_sha256=installed_sha, target_fps=args.fps, planned_seconds=args.seconds,
              conditions='USB connected/charging; automated repeated complete route; no video/readback; 1 s ADB presentation sampling and 30 s mem/thermal sampling. Synthetic input is not physical responsiveness.')
(output/'conditions.json').write_text(json.dumps(device,indent=2),encoding='utf-8')
layers = shell('dumpsys SurfaceFlinger --list').splitlines()
layer = next((line for line in layers if line.startswith('SurfaceView - '+package+'/')),None)
if layer is None or any(c in layer for c in "'\"\n\r"): raise RuntimeError('No unambiguous game surface')
seen=set(); intervals=[]; previous=None; gaps=0; errors=0; periods=[]
start=time.monotonic(); deadline=start+args.seconds; next_stats=start
game_pid=shell('pidof '+package).strip()
if not game_pid.isdigit():raise RuntimeError('Expected one running game process')
route_count=0;last_route_time=start;workload_failed=False
with (output/'presentations.csv').open('w',newline='') as file, (output/'polls.csv').open('w',newline='') as audit, (output/'device-samples.txt').open('w',encoding='utf-8') as stats:
    frames=csv.writer(file);frames.writerow(['elapsed_s','desired_ns','actual_ns','ready_ns','display_interval_ms','sampling_boundary'])
    polls=csv.writer(audit);polls.writerow(['elapsed_s','poll_ms','valid_buffer_frames','new_frames','error'])
    while time.monotonic()<deadline and not(output/'stop.request').exists():
        polled=time.monotonic();elapsed=polled-start
        try:
            raw=shell("dumpsys SurfaceFlinger --latency '"+layer+"'")
            rows=[]
            for line in raw.splitlines():
                fields=line.split()
                if len(fields)==1 and fields[0].isdigit():periods.append(int(fields[0]))
                if len(fields)!=3 or not all(f.isdigit() for f in fields):continue
                row=tuple(map(int,fields))
                if 0<row[1]<9223372036854775807:rows.append(row)
            rows.sort(key=lambda r:r[1])
            new=[r for r in rows if r[1] not in seen]
            boundary=bool(previous and new and rows and rows[0][1]>previous)
            if boundary:gaps+=1
            for index,row in enumerate(new):
                seen.add(row[1]); interval=None
                if previous and row[1]>previous and not(boundary and index==0):
                    interval=(row[1]-previous)/1e6;intervals.append(interval)
                frames.writerow([round(elapsed,3),*row,interval,boundary and index==0])
                previous=max(previous or 0,row[1])
            polls.writerow([round(elapsed,3),round((time.monotonic()-polled)*1000,3),len(rows),len(new),''])
            file.flush();audit.flush()
            if time.monotonic()>=next_stats:
                # RuntimeInitialize AfterSceneLoad does not run after an ordinary
                # scene restart. Require ongoing completion evidence, not an idle
                # scene that happens to continue presenting at the target rate.
                if shell('pidof '+package).strip()!=game_pid:
                    raise RuntimeError('Game process changed during measurement')
                workload=adb('logcat','-d','--pid='+game_pid,'-s','Unity:I')
                (output/'workload.log').write_text(workload,encoding='utf-8')
                count=workload.count('SHIFTBOUND FULL ROUTE PASSED:')
                if count>route_count:last_route_time=time.monotonic()
                route_count=count
                if time.monotonic()-last_route_time>90:
                    workload_failed=True
                    raise RuntimeError('No route completion for 90 s; repeated gameplay workload stalled')
                # Only the game's process plus global thermal/charging conditions.
                stats.write('\nELAPSED_S='+str(round(elapsed,3))+'\n'+shell('dumpsys meminfo '+package))
                stats.write(shell('dumpsys thermalservice'))
                battery=shell('dumpsys battery')
                stats.write('\n'.join(l for l in battery.splitlines() if any(k in l for k in ('USB powered:', 'level:', 'temperature:', 'status:', 'voltage:')))+'\n')
                stats.flush();next_stats=time.monotonic()+30
        except (RuntimeError,subprocess.TimeoutExpired) as error:
            errors+=1;polls.writerow([round(elapsed,3),'','','',str(error)]);audit.flush()
            if workload_failed:break
        time.sleep(max(0,polled+1-time.monotonic()))
def percentile(values,p):
    ordered=sorted(values)
    if not ordered:return None
    index=(len(ordered)-1)*p;lower=int(index);upper=min(lower+1,len(ordered)-1)
    return ordered[lower]+(ordered[upper]-ordered[lower])*(index-lower)
target=1000/args.fps
missed=sum(max(0,round(v/target)-1) for v in intervals)
summary=dict(measured_seconds=round(time.monotonic()-start,3),observed_frames=len(seen),interval_count=len(intervals),
    route_completions=route_count,workload_verified=not workload_failed and route_count>=max(2,args.seconds//90),
    planned_seconds=args.seconds,completed_duration=time.monotonic()-start>=args.seconds,stopped_by_request=(output/'stop.request').exists(),
    refresh_ns=statistics.median(periods) if periods else None,display_interval_ms={
    'p50':percentile(intervals,.5),'p95':percentile(intervals,.95),'p99':percentile(intervals,.99),'max':max(intervals) if intervals else None},
    estimated_missed_target_slots=missed,missed_slot_fraction=missed/(len(intervals)+missed) if intervals else None,
    intervals_over_50ms=sum(v>50.5 for v in intervals),intervals_over_100ms=sum(v>100 for v in intervals),
    sampling_gaps=gaps,adb_errors=errors,
    limitation='Observed display intervals only. Estimated missed slots are not FrameTimeline deadline attribution. Sampling gaps are excluded from interval statistics; coverage must be reviewed. USB charging and automated input must accompany conclusions.')
(output/'summary.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print(json.dumps(summary))
