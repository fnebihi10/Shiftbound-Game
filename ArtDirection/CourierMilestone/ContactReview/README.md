# Courier contact and Editor opening review — 12 October 2026

This is an integrated improvement, not courier milestone acceptance. Original Jog, jump physics, camera settings, lighting and the approved world remain unchanged. The original WindowsPolished executable and corrected c69 baseline remain protected.

Integrated source: `4a2d28839d48d77384b348709b5c3f978f000058e715fecce220afb4bdcd881d`, main HEAD e7af46e0, uncommitted. Corresponding Unity 6000.3.25f1 candidates:

- `UnityProject/Builds/WindowsCourierContactCandidate/Shiftbound.exe`.
- `UnityProject/Builds/AndroidCourierContactCandidate/Shiftbound-QA.apk`; SHA256 `ab9f05317e89d11a04d1b3969e57a9cdaebcf1ad7fdcbf24b27a4d15c3a99398`. QA debug signed, ARM64/GLES3, 0.4.0/code5.
- Preserved source `.validation/CourierMilestone/Source-4a2d2883.zip`, with source fingerprint checked before and after archiving. Previous ca637 source and both candidates retained.

Foot planting now preserves an acquired anatomical foot heading, releasing larger pivots before twisting the leg. Current shoe pitch informs pelvis reach before contact gains full weight. Supported shoes no longer sink into the roof when horizontal contact fades. These are visual corrections; they do not delay input or change collision, jump intent, speed or jump distance.

The saved Editor camera used to be at the origin, showing giant legs and the roof outside Play Mode. `GameplayOpeningPreview.AuthorAndBuildWindows` stores the existing FollowCamera opening pose. Only camera Transform block 449586988 changes: 6,185 scene blocks unchanged, no objects added or removed. Runtime camera parameters remain unchanged. The temporary collision probe is removed before saving. `editor-opening.png` shows the corrected stopped Game view.

Evidence:

- `gameplay-with-audio.mp4`: integrated 4a2 source, continuous production-input movement, repeated route, Shift, fall/checkpoint recovery, retained retry and completion. Actual captured game mix and elapsed-time frames; readback load makes this unsuitable for performance claims. Raw `.validation/CourierMilestone/IntegratedContact11/Gameplay`.
- `matched-original-jog.mp4`: integrated candidate right, restored original visual runner left; identical world, motor, input, camera and 1280×720 captures, fixed simulation 60 Hz sampled at 30 Hz. Silent, not frame-pacing evidence. Raw `IntegratedContact11/MatchedJog`.
- `stopping-turning.mp4` and `stopping-turning-slow.mp4`: isolated same-human original-Jog contact comparison, old contacts left, revised contacts right. The first 140 pairs have exact equality of every recorded time, speed, grounded state, motor position and camera position. `comparison-identity.json` records both sources. This isolates contact changes; it is not a new world/baseline acceptance comparison.
- `contact-metrics.json`: isolated full-route sample results. Fully weighted left shoe maximum clearance falls from 52.84 mm to 4.53 mm; p95 becomes zero. Right shoe full-weight penetration becomes zero. Earlier fading-contact penetration of several centimetres falls below 1 mm in the reviewed stop. Counts differ because turn plants now release; these measurements do not establish artistic acceptance.
- `unity-d3d12.log`: actual integrated Unity Play Mode passes two routes, facing, actual fall/checkpoint recovery and retained retry. First launch omitted route flags and was stopped; failed setup log remains `Logs/courier-contact11-unity-d3d12.log`. It contributes no acceptance evidence.
- `regression.log`: current-source suite PASS: 11 jump-intent assertions, 18 touch-router assertions and 13 player checks across regression/smoke/reach/mobile routing, direct 30/60/120/144/jitter traversal and production-input facing at 30/60/120/144 Hz. This verifies technical behavior, not physical comfort or artistic quality.
- `android-inspection.json`: LOAD and ZIP-P16 alignment/signature pass; vendor `libmain.so` and `libc++_shared.so` RELRO end alignment still FAIL. Actual 16 KB runtime not verified. No vendor patch or compatibility claim.

Selected rendered frames and poses were inspected. Continuous audiovisual playback/listening is unavailable through the tool interface; the movies are supplied for human normal-speed, slow-motion and audio review. Physical comfort and integrated candidate Samsung performance remain NOT VERIFIED: ADB lists no device. Earlier mixed 15-minute session applies to source3494, not this source, and does not establish sustained representative 60 FPS.

Acceptance remains: courier FAIL (jump symmetry/transitions, generic face/hair/material finish and human temporal review outstanding); foreground FAIL; world coherence, camera feel, mobile interface/physical controls, encounter appeal, audio and sustained Samsung performance NOT VERIFIED; technical recovery/retry PASS; Android 16 KB compatibility FAIL. No overall rating or zero-defect claim.

Next: inspect the isolated Motion13 running takeoff, which increases leading/trailing leg and opposing arm distinction. Motion12 completed a full route but close inspection found its arm/knee opposition was reversed; it is not promoted. Motion13 corrects that relationship. Promote only after matched gameplay demonstrates improvement. Finish courier transitions before expanding foreground work. Do not run the rejected `UpgradeProduction.ApplyEnvironment` recipe.
