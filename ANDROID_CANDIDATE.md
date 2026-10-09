# Android-first candidate — 9 October 2026

Baseline: clean `eaee340369e8ac6679ed3f646c2b0fa1cac500a1`, Unity 6000.3.25f1, URP 17.3.0 / Input System 1.20.0. No AGENTS.md found. Android module, bundled NDK r27c and JDK 17 installed; no ADB devices attached. GoldenRooftops is the only delivery scene. Historical reports are not current candidate evidence.

## Ranked gaps established before implementation

| Priority | Player consequence / observed evidence | Correction | Effort / dependency |
|---|---|---|---|
| 1 | Cannot play on phone: GameInput exposes keyboard/gamepad only, HUD desktop prompts | Owned multi-touch stick, held Jump, Shift, drag camera; safe-area HUD and lifecycle | Medium; physical Android phone for comfort |
| 2 | Binary checks can certify stale source; only Idle required by build validator | Fresh builds, source/binary manifest, production jump checks and all runtime motion validation | Low/medium; installed Unity |
| 3 | Backgrounding can retain jump intent; settings/progress not retained | Explicit pause on interruption, neutral-until-release input, persistent settings/checkpoint | Medium; phone interruption/update testing |
| 4 | Airborne courier remains in one wide-arm pose; current captures show stand-in silhouette | Audit licensed clips, separate takeoff/rise/fall/land without input lockout | Medium; premium courier/stop/turn commission remains |
| 5 | Audible fallback beeps and silence in ambience: saved clip fields empty | Source-backed authored cues, distinct ambience and restrained adjustable mix | Medium; sound assets/listening review |
| 6 | Foreground sparse/repetitive; panorama lacks parallax; premium opening explicitly rejected | Review small kit in motion before wider changes; credible obstacle construction | High; environment/vegetation artist |
| 7 | Android sustained pacing, thermal and latency unknown; D3D11 warnings historical | APK/AAB, named-device 20-minute soak, bounded Windows A/B diagnostics | Medium/high; phone + measurement hardware |
| 8 | Enjoyment/art approval absent | Five uncoached physical-touch players and independent art/motion review | External participants |

## Gates (all essential; no compensation across domains)

| Domain | Initial status / confidence | Acceptance evidence needed |
|---|---|---|
| Traversal/recovery | NOT VERIFIED current / medium historical | Fresh-source checks: Shift clearance/momentum/debounce, checkpoints/goal, 30/60/120/144/variable dt |
| Response/motion/camera | FAIL / high (missing touch, single air state) | Reversals, landing input, continuous orbit, wall/corner/Shift/retry recordings; physical comfort; latency method |
| Courier/environment | FAIL / high | Concept-coherent opening in normal motion, anatomy/skin/backpack, physical material scale, rooted growth, 360-degree city |
| Shift comprehension | NOT VERIFIED / low | First Shift and two-thumb midair success without coaching, non-color support cues |
| Audio/interface | FAIL / high | Authored cues + loops, phone-safe layout, event sync, listening review |
| Sustained Android performance | NOT VERIFIED / none | Named reference midrange: steady 60; named floor: stable 30; 20-minute soak with resolution/tier, distributions, memory/battery/temperature limits |
| Touch/lifecycle | NOT VERIFIED / none | Multi-touch ownership/cancellation tests then physical two-thumb, Back/background/resume/audio interruption |
| Install/update/Play | NOT VERIFIED / low | APK cold launch/update retention; IL2CPP ARM64 AAB, native 16KB checks, release signing and Console-specific testing |
| Independent players/art | NOT VERIFIED / none | Five fresh humans + independent art/motion reviewer; AI assessment never substitutes |

No supported hardware floor or reference device can be honestly established from available hardware: no phone attached. API/architecture compatibility is a packaging floor, not a performance floor. Existing Mobile URP asset is a provisional tier; do not label its settings measured.

## References and comparison rules

Approved concept: `ArtDirection/visual-target-gameview-v2.png`; current committed opening/airborne captures inspected. Compare jacket/backpack silhouette, material richness, foreground growth and corresponding landmarks at gameplay distance. Reference gameplay selected for subsequent human comparison: [Mirror's Edge Catalyst movement demonstration](https://www.ea.com/amp/news/movement-in-mirrors-edge-catalyst-the-basics) for immediate traversal response and readable route material; [Astro Bot platformer design demonstration](https://blog.playstation.com/2018/06/25/astro-bot-rescue-mission-the-dos-and-donts-of-building-a-platformer-in-ps-vr/) for landing silhouette and pose clarity. First-person and VR cameras are not direct free-orbit specifications. External video playback has not yet been reviewed in this tool session; no measured comparative claim is made.

Keep raw local logs/builds separate from committed summaries. Candidate evidence must include source fingerprint, settings, Unity version, scene and binary hashes. An APK compile is not a physical-device pass. No Google Play publication authorized.

## Implemented candidate

Branch `candidate/android-first-slice`; implementation `105e4d6`, background diagnostic correction `f23a3a7`. The latter only changes the opt-in synthetic input harness; shipping focus/interruption behavior remains explicit pause and neutral input. Final build identity and artifact hashes are in `ArtDirection/AndroidCandidate/delivery.json` and adjacent manifests. Later documentation/evidence commits do not change production assets.

Real EnhancedTouch contacts now reach the existing gameplay input facade: owned left stick, held/released Jump, Shift and manual camera orbit. Sliding a held Jump thumb into the inner Shift target is an explicit once-per-hold chord for the two-thumb midair encounter. It preserves Jump hold and movement; it never transfers that contact to camera control. Separate Shift touches still work. Cancellation, pause, interruption and retry clear intents; resume waits for neutral controls. Landscape safe areas, adjustable control size/inset/height, camera sensitivity/inversion/follow, volume and 30/60 caps persist. Checkpoint retention uses shared anchors; it does not save elapsed time or exact airborne state.

Motor reach/physics remain unchanged. Visual interpolation is exponential; animation uses actual displacement, continuous gait phase and separate takeoff/rise/fall/land motions. Eight valid Humanoid runtime states are required at build time. This is not complete premium motion coverage: braking/turn clips, foot-contact correction, backpack clearance and anatomy still require work. Footsteps follow displacement, not authored foot-contact phase.

Saved AudioClip assignments now contain CC0 cues and original ambience loops; synthesized fallback tones are removed. Cues have independent sources/gains and follow actual gameplay events. The original equipment mesh replaces two conspicuous collision blocks with panels, louvers, hinges and coping; overlapping decorative shells are disabled. Collider bounds, route coordinates and mandatory Shift lessons are preserved. No new level or decorative generator pass was added. The rest of the art route remains below acceptance.

Android builds use the installed SDK36 / NDK r27c / OpenJDK17, IL2CPP ARM64, API36 target/API26 minimum, GLES3 and both landscape rotations. Frame pacing is enabled. Mobile URP remains a provisional configuration: render scale .8, one 1024px sun-shadow cascade, 50m shadow distance, no additional-light shadows, SRP batching. Benchmark textures have Android 2048px import ceilings with automatic compression. These are saved settings, **not measured budgets or a supported-device claim**. Texture residency/compression artifacts, geometry/overdraw/LOD costs, HDR/post-processing, sustained 60/30 tiers and the device floor must be resolved on actual hardware.

## Final acceptance record

All nine domains remain essential. Confidence describes the evidence, not a numerical quality rating.

| Domain | Candidate gate / confidence | Worst remaining defect; evidence needed to close |
|---|---|---|
| Traversal/recovery | **NOT VERIFIED Android**; automated checks PASS, high confidence in tested cases | No on-phone route/recovery run. Production jump intent, ownership, motor reversals/landing input/reset, Shift safety, reach/mandatory lessons, checkpoint/goal and full-route frame-rate checks are summarized in `verification.txt`; install and repeat on named devices. |
| Responsiveness/motion/camera | **FAIL** / high for motion gaps; physical comfort low | Stand-in anatomy and extended-arm falling pose; no authored stop/turn set. Real-time orbit/route evidence exposes continuity and camera behavior; independent motion review, ordinary touch camera/wall play and physical controller comparison remain open. Software synthetic event timing is separate; input-to-photon NOT VERIFIED. |
| Courier/environment coherence | **FAIL** / high | Sparse/planar opening overgrowth, repeated facades, aggregate roofs and soft panorama mismatch; the Overgrown orbit exposes a visible wrap seam, and later route still contains crude plants. Controlled before/after equipment views and continuous orbit do not pass premium art. Requires an accepted source-backed courier/rooftop/vegetation kit and limited midground composition review before propagation. |
| Shift comprehension/encounters | **NOT VERIFIED** / low | Synthetic full route and held-Jump slide pass mechanics; they cannot prove comprehension or two-thumb comfort at the mandatory midair lesson. Five fresh players must show first Shift success, avoidable deaths and understandable inactive hatching/shape/sound cues. |
| Audio/interface | **NOT VERIFIED** / medium for technical checks | Authored cues/ambience and HUD are present; captured mixes have no clipped samples. Subjective listening, gait/contact sync, actual phone text size/reach, pause/completion UI and interruption mix behavior remain unreviewed. Layout captures are Windows simulations. |
| Sustained Android performance | **NOT VERIFIED** / none | No named phone, GPU timings, presentation deadlines, thermal soak or battery measurement. Desktop callback diagnostics cannot pass this gate. Select reference/floor from available hardware; run the documented 20-minute 60/30-cap study with render dimensions, temperature and instrumentation limits. |
| Touch/phone UI/lifecycle | **NOT VERIFIED physical Android** / medium source, none physical | Ten production synthetic touch cases plus 18 router assertions cover ownership/chord/cancel/neutral resume/retry. Actual Back/Home/lock/audio interruption/cutouts/update and demanding two-thumb Shift need device QA. Do not infer lifecycle reliability from Windows calls to Suspend. |
| Install/update/Play | **FAIL** / high native inspection, none device/Console | QA APK and AAB compile/sign/package; native RELRO failures prevent native 16KB acceptance. Final failing filenames/hashes are in `android-inspection.json`. No cold install, same-key update, release signing, real split installation or Play internal-track test. |
| Independent player/art review | **NOT VERIFIED** / none | No humans recruited or impersonated. Five blank player rows and independent art/motion protocol are supplied in `Tools/ANDROID_QA_PROTOCOL.md`; autonomous route completion is reliability evidence only. |

## Evidence and bounded Windows diagnostic

`ArtDirection/AndroidCandidate/README.md` indexes committed real-time HUD/audio videos, simulated landscape layouts, comparable equipment poses, checks, identities and static Android inspection. Historical baseline captures remain in `ArtDirection/ProductionBenchmark/`; they were inspected but their old executable has no current-source manifest. Do not present those captures as this candidate.

Windows diagnostic conditions use RX7600 / Ryzen5600X / 32GB / driver32.0.31041.1004, D3D11, windowed 1280x720, 240Hz display, VSync0, PC scale1 and 60 cap (plus a 30-cap comparison). Counter-Strike 2 remained running by user request. The inactive window can skip rendering: render/GPU/draw counters are unavailable, and its ScreenCapture recording was black and rejected. Timing on/off, post on/off and attempted recording runs therefore only establish callback/harness behavior. Warning counts are normalized by duration/callback count in the JSON summaries; zero warnings here does not demonstrate that historical presentation warnings are harmless or fixed. A comparable foreground presentation baseline/candidate A/B and individual warning/stall correlation remain NOT VERIFIED. No warnings were suppressed and no speculative driver/render fix was applied.

The valid background videos use an explicit shipping-camera RenderTexture, production camera-space phone Canvas, actual AudioListener PCM and measured capture timestamps. They retain elapsed time (no time compression). Readback/encoding adds load and misses capture intervals, so they are visual/audio review evidence, not presented-frame performance or input-to-photon measurement. Shipping pause/completion menus still use IMGUI and are not included in offscreen captures.

## Exact external handoff and next step

1. **First: connect an authorized USB-debugging ARM64 Android phone.** Record its actual model/SoC/OS/refresh/cutout and perform installation, ordinary two-thumb mandatory Shift and Back/background/cancel tests before choosing a performance floor. A second lower-end device and a 16KB device are needed to close those separate gates. No emulator result will replace them.
2. **Resolve native 16KB support with Unity's vendor runtime/module.** APK/AAB LOAD and ZIP/page configuration can pass while RELRO fails. Follow the current [official native-page checks](https://developer.android.com/guide/practices/page-sizes), obtain actual replacement binaries that pass and test them; an untested Editor version or a project-only linker flag is not a fix for prebuilt `libunity`/`libmain`/Swappy/C++ libraries. Keep the QA artifacts separate from a releasable build.
3. **Commission the opening art kit against the existing asset specifications.** Best concrete path is one jacket/backpack Humanoid courier plus in-place stop/turn/air/landing clips, reusable constructed paving/coping/drains/equipment and rooted ivy with source files/LODs. Require commercial redistribution rights and a written quote; cost and an artist are not yet available. Approve the source-backed opening in motion before extending the kit. CC0 Quaternius clips integrated here are a technical improvement, not a premium substitute. No purchase was authorized or made.
4. Supply five fresh phone players and an independent art/motion reviewer. Supply permanent package identity, external upload keystore/signing ownership, developer contact and Play Console access for release preparation. Account-specific testing, distributed split updates and store/privacy submission remain pending. No publication is authorized.

The weakest immediate playable-slice gate is physical Android interaction/install reliability. The most definite release failure is native 16KB compatibility. Passing either will not compensate for failed premium art/motion acceptance.
