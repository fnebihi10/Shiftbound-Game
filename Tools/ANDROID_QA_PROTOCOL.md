# Physical Android QA and release handoff

Use the candidate APK whose SHA256 and source fingerprint match the build manifest. No phones or human participants were available when this protocol was prepared. Empty result rows are not passes.

## Install and play

With USB debugging enabled and the phone authorized, use bundled `SDK/platform-tools/adb.exe devices -l`, then `adb -s SERIAL install -r UnityProject/Builds/AndroidCandidate/Shiftbound-QA.apk`. Cold launch from the app icon in landscape. Left disc moves; hold JUMP for distance, release early for a short jump; tap SHIFT to change support; drag the free right area to orbit. For midair Shift with two thumbs, keep JUMP held and slide that thumb into the center of SHIFT. This explicit chord fires once per hold; lift to release Jump. The camera holds heading while airborne. Pause button or Android Back opens settings. Adjust size, horizontal inset, height, touch sensitivity and camera follow. Pause before backgrounding; interruptions also pause. Resume explicitly after lifting fingers. Restart clears the saved checkpoint; ordinary relaunch resumes its shared checkpoint.

Debug QA signing is for local testing. Updates require the same signing key. Release signing or Play App Signing can require uninstalling the debug build, which destroys local progress; never count that as an update-retention test.

## Five fresh players (uncoached)

Hand each person the phone on the starting roof and say only: “Reach the rooftop goal. You can stop at any time.” Do not explain Shift, jump hold, route or camera before observing. Record consent separately from anonymous results; avoid personal information in repository results.

| Anonymous ID / phone / build | Instruction understood | First Shift: attempts/time | Avoidable deaths + location/cause | Completion time / stopped | Camera discomfort 0–4 + description | Voluntary replay? |
|---|---|---|---|---|---|---|
| P1 | | | | | | |
| P2 | | | | | | |
| P3 | | | | | | |
| P4 | | | | | | |
| P5 | | | | | | |

Observe ordinary two-thumb play at the mandatory midair Shift and final combination. Count occasions requiring a third finger, camera/control confusion, involuntary jumps/shifts and landing-target occlusion. Ask what the previews mean after the attempt. Afterward ask whether they want another run, then offer settings adjustments and retest. Separate coached retests from first attempts. Essential usability fails if the required interaction cannot be performed comfortably with two thumbs or recurring input conflicts cause falls.

Independent art/motion reviewer: compare the approved concept, chosen gameplay references and candidate normal-speed video at shipping camera distance. Inspect silhouette/anatomy/skin/backpack; start/stop/reverse/air/land continuity; actual feet displacement; physical material scale and construction; rooted growth depth; continuous 360-degree corresponding landmarks. Ask for the three worst defects and whether the opening should propagate. Keep reviewer words and AI interpretation separate.

## Functional/device matrix

Record phone model, SoC/GPU, RAM, Android/build/security version, display refresh, cutout, app/versionCode/signature, graphics API, quality, actual render dimensions, battery %, charger state and ambient temperature. Test 16:9, 19.5:9 and 20:9 layouts; simulator/synthetic layouts only establish geometry, not thumb comfort.

Perform: held/released/buffered jump with movement; stick + jump + Shift; orbit + stick; pointer leaving bounds; second contact on occupied control; cancelled touches; Back while airborne; notification/audio interruption; Home/app switch; lock/unlock; 5-minute background; process kill/cold restart; retry and completion. Lift all controls before explicit resume. No involuntary actions allowed. Test a physical controller separately, including deadzones/inversion. Cold launch after force-stop and update with same-key `adb install -r`; confirm volume/layout/inversion and saved checkpoint retained. Check both landscape rotations and safe areas.

## Twenty-minute thermal run

Use actual repeated full-route play, Shift, near-foliage orbit, collisions, falls/retry and completion. Start unplugged at a recorded battery level/temperature and fixed brightness; record charging/ambient changes. Separate cold launch/loading from steady play. Run 60-cap and 30-cap tiers separately. Never describe alternating 60/30 as sustained 60.

In the pause menu, tap the PAUSED title five times to reveal the QA row. Tap that row to begin recording, resume for the run, then pause and tap it again to flush CSV/metadata. This local diagnostic does not transmit data. Pull CSV/metadata after the run from the app's persistent data path. Capture Android presentation evidence with Perfetto/FrameTimeline or vendor tooling; CPU callback intervals cannot certify missed presentation deadlines. Capture `dumpsys meminfo PACKAGE`, `dumpsys battery`, `dumpsys thermalservice` at start and 1-minute intervals where accessible. Report p50/p95/p99 by minute and whole steady interval, deadline misses, Shift-correlated spikes, CPU/main/render/GPU (unsupported counters explicitly absent), GC/allocation, memory, temperature/throttle indicators and battery delta. Compare identical instrumentation off/on runs to measure overhead. Screen recording changes load: record it as a separate condition. Do not infer input-to-photon from the CSV; use a high-speed camera simultaneously filming physical finger/button and screen, with documented sampling/error, if available.

## Play Console remaining steps

Checked 9 October 2026: [target API requirements](https://support.google.com/googleplay/android-developer/answer/11926878) require API36 for new phone apps/updates. [16KB guidance](https://developer.android.com/guide/practices/page-sizes) requires native LOAD/RELRO and package alignment checks plus runtime testing. [Personal-account testing](https://support.google.com/googleplay/android-developer/answer/14151465) applies to qualifying personal accounts: 12 opted-in testers for 14 consecutive days before production access application. Account type/date and actual Console requirements remain unknown; five usability players are a separate study.

Create/store an upload keystore outside source control. Set `SHIFTBOUND_RELEASE_SIGNING=1`, `SHIFTBOUND_KEYSTORE`, `SHIFTBOUND_KEY_ALIAS`, `SHIFTBOUND_KEYSTORE_PASSWORD`, `SHIFTBOUND_KEY_PASSWORD` in the local process environment, then `Tools/BuildCandidate.ps1 -Target Bundle`. Never echo credentials or commit the keystore. Current application ID is `com.shiftboundproject.shiftbound`, candidate version 0.2.0/code 2, Android 8+/ARM64/GLES3 compatibility proposal. Increment versionCode for every Console upload. APK/AAB QA uses debug signing unless release variables are explicitly supplied.

Before any publication: verify AAB-generated split installation with bundletool, signing certificate, native libraries, supported device catalog, Console prelaunch report and actual internal distribution/update. Owner must confirm package identity, signing ownership, developer verification and production access. Fill content rating/target audience, screenshots captured on device, support contact, privacy policy and Data Safety from inspected final manifest/dependencies/observed traffic. Source currently implements local settings/checkpoint persistence without ads, accounts or monetization; do not infer “no collection” from that alone—inspect Unity services, merged permissions and runtime traffic. Remove unused service packages if justified by that audit. No publication has been authorized.
