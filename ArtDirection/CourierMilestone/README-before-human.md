# Courier candidate — 11 October 2026

Integrated in main GoldenRooftops. Courier production acceptance remains incomplete. The owner still prefers untouched WindowsPolished; the corrected baseline is a reproducible reference, not an accepted upgrade. No overall rating is assigned.

Current source: `39ab936ec0f3d77970ce287eb1a967174591886413193bf6e0883b05e56bb91c`. Branch main, HEAD e7af46e0, uncommitted. Windows/Android manifests match this source, Unity 6000.3.25f1. No commits, pushes, purchases or publication.

Windows: `UnityProject/Builds/WindowsCourierCandidate/Shiftbound.exe`. Android: `UnityProject/Builds/AndroidCourierCandidate/Shiftbound-QA.apk`, ARM64 IL2CPP, debug signed, QA 0.4.0/code 5. Original Jog remains the default. The adjacent `Review Authored Gait.cmd` enables the experimental replacement; its improvement is not accepted.

## Review evidence

- [Gameplay with audio](Review/gameplay-original-jog.mp4): actual wall-clock capture, synthetic production input, two full runs, fall/checkpoint recovery and retained retry.
- [Gameplay-camera comparison](Review/matched-original-jog.mp4): corrected baseline LEFT, new courier RIGHT, preserved original Jog and new airborne/recovery actions.
- [Close authored motion](Review/close-authored.mp4) and [half-speed motion](Review/close-authored-slow.mp4): original baseline LEFT, experimental gait RIGHT.
- [Experimental gait gameplay with audio](Review/gameplay-authored.mp4).

Pairs share the same world frame, motor, inputs, 1280×720 camera and profile. Close pairs use the same secondary inspection camera; the shipping camera is unchanged. Fixed 60 Hz simulation sampled at 30 Hz, no comparison audio or performance claim. Baseline is corrected c69ca256, not a claim of reproducing all unrecorded changes in the original executable. Original WindowsPolished and corrected reference builds/source remain protected.

## Implementation

Casual and Hoodie share the old 6,206-triangle geometry. The licensed CC0 Quaternius Superhero anatomical base has a usable higher-detail Humanoid rig. Blender 4.5.14 LTS authoring derives connected golden windbreaker and navy trousers, fitted hair, closed trainers, sewn straps and petrol courier pack. Source: 8,624 vertices, 16,521 triangles, 66 bones, at most four normalized influences, no unweighted vertices. Unity splits vertices at seams. This is an authored candidate, not a finished commissioned character.

Eleven baked action clips cover rest, walk/run/sprint, full/released moving and stationary jumps, falling and soft/hard recovery. Actual travel and measured imported stance distances drive the replacement gait. Jump arms now drive stationary takeoff together; moving jumps retain asymmetry. Chest lean composes with twist. Resolved vertical velocity and jump energy drive the arc. Acceleration, braking, turns, moving landings and pack response affect visual balance. Motor/input timing, camera and route are preserved.

After-animation contact correction uses actual shoe support vertices and fixed-length leg chains, holds anchors, releases unreachable contacts and adds bounded pelvis compression to retain reach while braking. Visual sole calibration is saved independently of the controller. Footsteps follow moving contact acquisition; landing uses the motor cue. Retry clears contact, pack and recovery state.

Earlier revisions were rejected for buried/floating soles, invalid runtime mesh reads and unsaved placement overrides. Iteration13–19 failures remain in `.validation/CourierMilestone`; Iteration20 fixed placement, Iteration21 support reach. Final source also refines jump arm/chest balance. Never reuse failed captures as accepted evidence.

## Verification and open work

Final actual Unity Play Mode passed two full production-input routes, fall/checkpoint recovery, retained retry and anatomical facing (`Logs/upgrade-unity-playmode.log`). Both builds passed. Final audio gameplay repeats the sequence. All 176 original WindowsPolished binary entries still match its manifest.

Final close telemetry: 735 samples; fully weighted consecutive moving subsets 36 left / 55 right. Median/p95 horizontal bone speed zero both sides; right maximum 0.822 m/s. The subset excludes contact ramps and many transitions. Sliding is not declared eliminated. CSV, logs and metrics are included.

Selected frames across gameplay and jump stages were inspected. Normal/slow movies are supplied, but this interface did not provide continuous audiovisual playback or actual listening. Do not claim they were personally watched/heard continuously. Human motion, feel, audio and phone comfort reviews remain open.

Visible unfinished quality: rounded source shoulder volume, little authored fabric detail, rigid shoes without toe deformation, plant/pivot transitions on sharp direction changes, and anticipatory falling extension. The rig supports further work and Blender is available. Dedicated garment topology/skin weights and animator-authored pivot/contact actions remain concrete work; no unavailable asset is claimed to have prevented all progress.

Android packaging, signature and LOAD alignment pass. Vendor `libmain.so` and `libc++_shared.so` fail RELRO end alignment. Source linker flags cannot fix vendor binaries; no mixed-version libraries/header patches are used. Runtime 16 KB compatibility remains unverified. [Official checks](https://developer.android.com/guide/practices/page-sizes); see Review/android-inspection.json. A newer official module is inspected separately before choosing a complete matching runtime/toolchain.

ADB lists no phone. Samsung identity/install, physical controls, sustained 60 FPS and thermal session remain unverified; recording load is not profiling.

| Area | Status | Evidence / remaining weakness |
| --- | --- | --- |
| Courier technical integration | PASS | Humanoid, complete action states, Unity routes/facing/retry, matching builds. |
| Courier production quality | FAIL | Silhouette/material/toe/pivot weaknesses; temporal human review open. |
| Foreground architecture/vegetation | FAIL | Corrected baseline protected; accepted substantial rebuild unfinished. |
| World coherence/Shift readability | NOT VERIFIED | Worlds exercised, production visual acceptance pending. |
| Camera/movement responsiveness | NOT VERIFIED | Technical traversal PASS; physical feel pending. |
| Mobile controls/interface | NOT VERIFIED | Existing controls retained; new device comfort acceptance absent. |
| Encounters/replay appeal | NOT VERIFIED | Route/recovery pass; satisfying play unestablished. |
| Audio | NOT VERIFIED | Actual mix recorded; soundscape/listening review open. |
| Recovery/retry technical state | PASS | Final Unity/player runs recover checkpoints and reset retained scene. |
| Sustained Samsung performance | NOT VERIFIED | No connected device or 15-minute session. |
| Android 16 KB compatibility | FAIL | LOAD/ZIP pass, two vendor RELRO failures; runtime also unverified. |

Next: refine courier silhouette, toe deformation and turning contacts against normal-speed footage before replacing Jog or expanding the foreground. Then resume the remaining production milestones. See [continuation](../../CONTINUATION_UPGRADE_20261011.md).
