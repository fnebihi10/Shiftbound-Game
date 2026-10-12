# Integrated courier jump revision — 12 October 2026

Current GoldenRooftops source: `d4a8a3d56b4e77759dca5301d19d8a9a7311d7a8ecb3be6e9b052a2433cd454b`, main HEAD e7af46e0, uncommitted. Courier milestone and the overall production upgrade remain incomplete.

Corresponding Unity 6000.3.25f1 candidates:

- `UnityProject/Builds/WindowsCourierJumpCandidate/Shiftbound.exe`.
- `UnityProject/Builds/AndroidCourierJumpCandidate/Shiftbound-QA.apk`; SHA256 `524fccac25b3f60193e0ef0b8dab62853975895002a5ebb9c78b94492a3250a4`. QA0.4.0/code5, debug signed, ARM64/GLES3.
- `.validation/CourierMilestone/Source-d4a8a3d5.zip`, 107,209,133 bytes; source fingerprint verified before/after archiving. Previous source4a2, ca637, corrected c69 and original WindowsPolished remain preserved. No commit or push.

Running jumps now use a more distinct leading knee/trailing leg and opposite arm drive, with mild torso balance. The limbs unfold into descent, preserving immediate takeoff and continuing run on landing. Original Jog, jump distance/height, collision, camera settings and world remain unchanged. The earlier Motion12 same-side arm/knee revision was rejected after close inspection; corrected Motion13 is integrated. Only the authored action FBX/metadata change from the verified contact candidate. Contact and Editor-opening improvements from [ContactReview](../ContactReview/README.md) are retained.

Evidence:

- `jump-before-after.mp4` and `jump-before-after-slow.mp4`: previous own-rig actions left, revised actions right. 368 frame pairs have exact equality of recorded time, speed, grounded state, motor position and camera position. Both 1280×720, same original jog/input/motor/world, fixed60 simulation sampled30; half-speed second movie. Isolated sources and exact fields are in `comparison-identity.json`. The same revised action FBX was selectively copied into the main integrated source. Raw `HumanContact11JumpOnly` and `HumanMotion13JumpOnly` under `.validation/CourierMilestone`.
- First full/prelude comparison failed exact matching at frame45 after a pause; it was rejected as matched evidence. Focused recapture removed the pause prelude and all368 pairs matched. Full route/recovery logs are retained separately, not passed off as exact before/after pairs.
- `apex-close.jpg`: inspected revised moving-jump apex. Takeoff, ascent, descent and immediate running landing selected frames were also inspected. This supports a visible pose improvement, not complete temporal quality acceptance.
- `gameplay-with-audio.mp4`: integrated d4a source, uninterrupted actual-time gameplay and captured game mix. Two production-input routes, camera use, Shift, fall/checkpoint recovery, retained retry and completion. Raw `IntegratedMotion13/Gameplay`. Heavy readback means it is not performance evidence.
- `matched-original-jog.mp4`: final integrated candidate right, protected restored visual courier left; same gameplay motor, world, 1280×720 resolution and preferred gameplay camera for each pair. Fixed simulation, silent. Raw `IntegratedMotion13/MatchedJog`.
- `rear-attachment.mp4`: final integrated candidate right and restored reference left from the same rear close camera during the route. Selected takeoff, apex and running landing frames inspected for bag fit and attachment. Raw `IntegratedMotion13/Rear`; technical full-route/facing PASS. This does not guarantee perfect attachment in every frame.
- `unity-d3d12.log`: actual Unity Play Mode PASS, two production-input routes/facing, fall/checkpoint recovery and retained retry.
- `regression.log`: final current-source PASS, 11 jump-intent and18 touch-router assertions, plus13 player checks including direct30/60/120/144/jitter and production-input/facing30/60/120/144. Human comfort and performance remain separate gates.
- `android-inspection.json`: LOAD and ZIP-P16 alignment plus signing checks PASS. Unity vendor `libmain.so` and `libc++_shared.so` RELRO ends FAIL; actual16 KB runtime and current artifact Samsung install/run NOT VERIFIED.

The tool interface supports rendered-frame/pose inspection, but continuous video playback and listening are unavailable. Human normal/slow-speed motion and sound judgement remain outstanding. The phone is disconnected. Earlier Samsung feedback and mixed15-minute diagnostics apply to prior candidates, not this final source. Synthetic input and scripts do not establish physical comfort, sustained60 FPS or overall quality.

| Area | Status | Remaining criterion |
| --- | --- | --- |
| Courier and complete movement presentation | FAIL | More convincing face/hair/material finish; human temporal review of jump transitions, turns, foot contacts and land-into-run; remaining brief contact reach gap. |
| Foreground architecture/vegetation | FAIL | Major production rebuild remains outstanding; preserve approved composition. |
| World coherence / Shift readability | NOT VERIFIED | Technical route passes; visual/encounter acceptance still required. |
| Camera and responsiveness | NOT VERIFIED | Input behavior and actual route pass; human feel and difficult landing/collision review open. |
| Mobile UI / physical controls | NOT VERIFIED | Technical routing retained; physical comfort and current Samsung review open. |
| Encounter quality / replay appeal | NOT VERIFIED | Existing route completes; production anticipation/mastery/fairness review open. |
| Audio | NOT VERIFIED | Actual mix recorded; listening and soundscape acceptance open. |
| Recovery / retry state | PASS | Actual fall/checkpoint and retained-scene retry verified across repeated route. |
| Sustained Samsung performance | NOT VERIFIED | Current source not device-tested; earlier mixed session insufficient. |
| Android16 KB compatibility | FAIL | Conformant vendor runtime/toolchain and genuine16 KB runtime test required. |

Next highest-impact work remains courier transition/deformation and human gameplay review before the foreground milestone expands. Do not claim zero defects, a9–10 rating or finished production art. Do not run the rejected `UpgradeProduction.ApplyEnvironment` recipe.
