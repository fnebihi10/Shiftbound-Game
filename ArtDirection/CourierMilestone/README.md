Latest integrated candidate: [jump revision and acceptance record](JumpReview/README.md), 12 October 2026. Earlier evidence retained below.

Current integrated update: [contact and Editor opening review](ContactReview/README.md), 12 October 2026. Earlier motion review remains preserved below.

Current integrated motion revision: source ca637172. Read [HumanMotionReview/README.md](HumanMotionReview/README.md) for current builds, evidence and acceptance. The source349493c8 record below is preserved history. Courier production milestone remains unfinished.

# Integrated human courier — 11 October 2026

GoldenRooftops contains the more natural human courier described in [the authoring record](HUMAN_COURIER.md). Production acceptance remains incomplete. The owner still prefers protected WindowsPolished; corrected baseline c69ca256 is a reproducible reference, not an accepted production upgrade. No rating or zero-defect claim is assigned.

Branch main, HEAD e7af46e0, uncommitted. Corresponding source: 349493c8b5f1d39ca3f07dc607762bb603dc14cd2ef3e1b3c86b2c56b8d5249b. Unity6000.3.25f1. Archive: .validation/CourierMilestone/Source-349493c8.zip.

Windows: UnityProject/Builds/WindowsHumanCourierCandidate/Shiftbound.exe. Android: UnityProject/Builds/AndroidHumanCourierCandidate/Shiftbound-QA.apk, ARM64 IL2CPP/GLES3, debug signed, QA0.4.0/code5. APK SHA256: 394795204ab27287cde69b00c3b5dd7c41584e0ac9ba3c690348b477f6e6a47b. Both manifests match current source. All176 WindowsPolished binary entries checked unchanged. Original executable, corrected c69 source/builds and earlier candidates preserved. No purchases, publishing, commits or pushes.

Original Jog stays default. Review Authored Gait.cmd beside the Windows executable enables the experimental replacement; its improvement is not accepted. New actor: textured clothing, deforming shoes with foot/toe support, fitted parcel kit, retargeted action clips. Jump balance/knee/heel refinements preserve physical traversal and input. Seven skinned renderers /31058 triangles; Samsung rendering cost unmeasured.

## Evidence

HumanReview contains:

- gameplay-original-jog.mp4: final source, normal wall-clock gameplay, actual audio, two routes, movement prelude, camera, Shift, falling/checkpoint recovery, completion and retained retry. Synthetic production input.
- matched-original-jog.mp4: final courier versus corrected baseline, same motor/camera/world/resolution1280×720 and simulated movement. Baseline left.
- close-authored.mp4 / close-authored-slow.mp4: isolated Human4 experimental gait/actions, same asset/motion authoring as main, secondary close camera. Source93cdfe1332918fd62af9d80d56494d0293f6fa90ac0b5c08f22ddce6c3a46e44.
- rear-pack-comparison.mp4: intermediate Human3 rear attachment inspection, before renderer consolidation/final jump refinement; not final-source footage.
- Source/build identities, actual Unity D3D12 log, player logs, contact measurements and native checks.

Fixed comparisons sample60Hz simulation at30Hz, without audio; they do not measure pacing. Normal gameplay supplies actual sound. Selected poses/frames inspected. Continuous normal-speed audiovisual playback/listening was unavailable through this interface; human motion, audio and physical control judgement remain open.

A first integrated matched recording failed when its wall-clock recovery deadline expired during slow fixed readback. Partial evidence is quarantined under MatchedJog-failed-wallclock, not accepted. Final source changes only that diagnostic to gameplay time plus detailed failure reporting from integrated6acc051c. Final complete matched route, including actual recovery, passed. Gameplay behavior unchanged.

## Verification and acceptance

Integrated6acc passed actual Unity D3D11 Play Mode; final3494 passed D3D12, matching the owner's backend: two routes, facing, falling/checkpoint recovery and retained retry. Final normal/audio and matched recording also pass. Full6acc suite passed11 jump assertions,18 touch assertions, smoke/reach/phone checks, direct routes at30/60/120/144/jitter and production input/facing at30/60/120/144. Final-only diagnostic change checked in final sequences; earlier suite scope recorded honestly.

Final audio gameplay:1944 post-animation samples. Fully weighted consecutive grounded moving subsets76 left/123 right; horizontal foot-speed p95 .059m/s left /zero right. This excludes contact ramps and many transitions; sliding not declared eliminated.

Scene comparison preserves6179 serialized blocks. Actor reference changes; Unity additionally serialized default URP Sun data during inspection. Existing camera, geometry, Light transform/color/intensity, motor and encounters not intentionally changed. Rejected foreground kit not reintroduced.

| Area | Status | Evidence / remaining weakness |
| --- | --- | --- |
| Courier technical integration | PASS | Humanoid, fitted actor, actual sole support, action states, corresponding builds, real Unity/player routes. |
| Courier production quality | FAIL | Generic face/hair, airborne transitions/landing anticipation, toe push-off, sharp-turn plant/pivot ramps; human temporal review open. |
| Foreground architecture/vegetation | FAIL | Corrected reference protected; accepted substantial rebuild unfinished. |
| World coherence/Shift readability | NOT VERIFIED | Both worlds exercised; production visual acceptance pending. |
| Camera/movement responsiveness | NOT VERIFIED | Technical routes/facing pass; human feel/comfort open. |
| Mobile controls/interface | NOT VERIFIED | Routing checks pass; physical multitouch/gesture comfort and visual acceptance absent. |
| Encounters/replay appeal | NOT VERIFIED | Completion/recovery exercised; satisfying play/mastery unestablished. |
| Audio | NOT VERIFIED | Actual mix supplied; listening/soundscape review open. |
| Recovery/retry technical state | PASS | Final Editor/player falls/checkpoints, retained reset, two completions. |
| Sustained Samsung performance | NOT VERIFIED | No final device session; no presented-frame60FPS/thermal claim. |
| Android16KB compatibility | FAIL | LOAD/ZIP/signature pass; vendor main/C++ RELRO ends fail. Runtime unverified. |

Native checks follow [Android's documented RELRO criterion](https://developer.android.com/guide/practices/page-sizes?hl=en). OfficialUnity6000.3.26 module ARM64 libmain variants inspected read-only retain the failed boundary; earlier Review has that inspection. No runtime migration, mixed vendor binaries or header patch. Dependency: supported conformant runtime/toolchain, then actual16KB runtime verification. Samsung performance is a separate gate.

Next: refine toe push-off, sharp-turn contacts and airborne/recovery motion using the new character. Obtain human normal-speed judgement before accepting the courier or replacing Jog, then continue the foreground and outstanding milestones. Samsung testing reserved for final meaningful integrated candidate as requested. See [continuation](../../CONTINUATION_UPGRADE_20261011.md). Earlier Superhero-based work in README-before-human.md / Review is superseded as main.