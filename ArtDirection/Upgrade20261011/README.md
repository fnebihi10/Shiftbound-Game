# Integrated upgrade evidence — 11 October 2026

**REJECTED CANDIDATE:** the owner's manual recording exposed backwards running. The added 180-degree visual rotation was incorrect at Humanoid runtime. The automated passes below verified mechanics and did not establish facing or visual acceptance. Do not use these old builds as the repaired candidate. See `../BackwardsFix/README.md` for the reproduction, correction, new facing regression and replacement builds.

GoldenRooftops contains the candidate. The preferred WindowsPolished executable and the restoration reference remain untouched. Main is based on `e7af46e0d6ee366a8413171348b0105825564200`; all upgrade changes are uncommitted. No overall quality rating is assigned.

## Review material

- `Matched/`: five matched camera views in Present and Overgrown, rendered from the preserved scene and candidate with the same Editor procedure. These are static Editor renders without HUD. Opening, exposed, close, rear and below views are included.
- `before-orbit.mp4`: actual preferred reference executable, orbit with game audio.
- `matched-review.png`: reference on the left, candidate on the right; opening Present above and below-route Overgrown below. Full-resolution views remain in `Matched/`.
- `.validation/ProductionUpgrade/Upgrade20261011/Reference/`: full original Assets/Packages/ProjectSettings snapshot and reference capture outputs.
- `.validation/ProductionUpgrade/Upgrade20261011/Windows/Shiftbound.exe`: candidate; adjacent manifest identifies source and all output binaries.
- `.validation/ProductionUpgrade/Upgrade20261011/Android/Shiftbound-QA.apk`: local debug QA candidate; adjacent manifest must match Windows source before review.
- `Logs/upgrade-unity-playmode.log`: actual Unity Editor Play Mode, production GameInput/motor/camera, two full routes, actual fall/checkpoint recovery, retained retry and completion. PASS marker required, not merely compilation.
- `build-identities.json`: Windows, Android and current source all match `731a000106663427099248e204e218ad2da0b384055a8cfcdea3243f221f48ef`. Unity 6000.3.25f1; local QA 0.4.0/code 5. Full per-file manifests sit beside each artifact.
- `android-inspection.json`: inspection of the final APK, SHA256 `6767ee0dfa7fd325ca650539b953e6337854db63fa95048124a921684a9f0f80`. All seven libraries pass LOAD alignment, ZIP alignment and signature pass. `libmain.so` and `libc++_shared.so` fail RELRO end alignment. Runtime on 16 KB remains NOT VERIFIED.
- `candidate-gameplay.mp4`: 37.7 seconds of continuous actual camera/HUD/game mix, software Input System input, two complete routes with camera movement, acceleration/braking/reversal, stationary and moving jumps, falling, checkpoint recovery, completion and retained retry. Recording frame/readback load is not performance evidence. Raw frames, WAV, contact CSV, launch and identity are in ignored `VerifiedProduction/`.
- `stationary-jump-slow.mp4`, `moving-jump-recovery-slow.mp4`, `motion-review.png`: selected temporal inspection material. Frames were inspected; audible listening and normal-speed human gameplay review remain outstanding.
- `verification.json`: all 14 recorded checks passed, including Unity Editor Play Mode, motor/Shift regression, smoke, reach audit, synthetic phone UI, five direct-step rates (including jitter), four production-input rates. Jump intent 11 assertions, touch routing 18. Recorded recovery twice, reset once, route twice, no logged gameplay exception.
- `contact-summary.json`: 1,936 post-animation samples; 31 consecutive moving/grounded fully weighted foot samples. Left p95 displacement speed 0.090 m/s, right 0; earlier full-weight anchor jumps removed. Full-weight sample count fell after release correction. Weight ramps and complete stride/contact quality are not accepted by this metric; animation remains FAIL.
- `windows-profile.json`: isolated Windows diagnostic, Ryzen 5600X/RX 7600, 1280×720, render scale 1, 60 cap, 12.1 seconds. Steady callback p50/p95/p99 16.662/16.817/16.913 ms; startup p99 93.543 ms; main-thread p95 0.66 ms; GPU/render/draw/GC counters unavailable. These are hidden-window callbacks, not compositor deadlines, sustained thermal results or Samsung performance.
- Ignored `Source731a0001.zip`: complete Assets/Packages/ProjectSettings snapshot, all 1,142 source files verified against manifest; `source-archive-verification.json` records its checksum. Ignored `Shiftbound-Windows-Upgrade.zip` contains the full portable Windows folder and manifest.

The character keeps the licensed original humanoid mesh and rig. Its heading, fitted equipment, materials, gait, airborne poses, transitions, landing recovery, contacts and sound ownership were reworked. The existing traversal motor timings and route collision are retained. Foreground additions are authored meshes; skyline and lighting retain the approved baseline. Input ownership, settings and persistence remain in their existing systems.

## Acceptance scope

| Area | Production acceptance | Evidence and remaining weakness |
|---|---|---|
| Courier quality and animation | FAIL | Meaningful integrated changes and continuous sequence tooling; original mesh topology/deformation remains. Final temporal contact inspection and independent gameplay review required. |
| Foreground architecture and vegetation | FAIL | Matched views show rebuilt roofs/facades/drainage and attached folded foliage. Small kit remains repetitive; varied authored equipment and convincing close vegetation still need refinement. |
| World coherence and Shift readability | NOT VERIFIED | Warm skyline protected; inactive previews restricted to collision surfaces. Synthetic traversal establishes rule execution, not first-player understanding. |
| Camera and movement responsiveness | NOT VERIFIED | Physics/input timing retained; routes and immediate continuation exercised. Human landing/camera evaluation remains outstanding. |
| Mobile controls and interface | NOT VERIFIED | Canvas play/pause/settings/completion and safe-area layout, existing multi-touch ownership retained. Actual phone fit, slide comfort and separate-button use require physical play. |
| Existing encounter quality and replay appeal | NOT VERIFIED | No added levels; timed hints, recovery and repeated completion integrated. Mastery, fairness and voluntary replay need player review. |
| Audio | NOT VERIFIED | Footstep ownership now follows contact; complete game mix recorded by capture tooling. Listening review cannot be established with image inspection. |
| Recovery and retry flow | PASS | Unity Play Mode completes two routes with actual checkpoint recovery and a retained-scene restart; checks every trigger rearmed, Present, clock, completion, checkpoint and input state. Broader lifecycle verification on phone remains separate. |
| Sustained Samsung performance | NOT VERIFIED | No device listed by ADB during current checks; no sustained frame-pacing/thermal result. |
| Android 16 KB compatibility | FAIL | Final artifact fails RELRO end alignment in vendor libmain and C++ runtime, despite passing LOAD/ZIP/signature checks. Exact dependency is a supported Unity Android runtime/module with aligned ARM64 libmain and C++ runtime, then actual 16 KB runtime verification. |

Compilation and automated route passes are technical evidence. They do not accept the three production milestones or establish a 9–10/10 experience. Failed areas stay open.
