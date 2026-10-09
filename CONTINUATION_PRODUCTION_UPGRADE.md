# Resume here — production upgrade paused by owner, 9 October 2026

The owner asked to stop for sleep and continue in this chat tomorrow. This is a work checkpoint, **not the completed premium milestone**. No physical Android phone is available. Do not restart the audit, rebuild the old foundations, or lower the acceptance criteria.

## Workspace and source

- Workspace: `C:/Users/Admin/Desktop/LINDI/Shiftbound-Game`.
- Branch: `production/courier-city-upgrade`; baseline main/origin main `983ec2be6967210400c7d03c0071dc216fa03586`, originally clean. No later remote or local work was overwritten.
- No applicable AGENTS.md. Read the user's full production assignment, `PRODUCTION_UPGRADE.md`, `ANDROID_CANDIDATE.md`, `PRODUCTION_BENCHMARK.md`, `ArtDirection/AndroidCandidate/README.md`, `Tools/ANDROID_QA_PROTOCOL.md`, `ART_DIRECTION.md` and approved visual concepts.
- Unity installed: `C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe`, URP 17.3, Input System 1.20, Android SDK36/NDKr27c/JDK17.
- Unity needs the existing escalated execution path because sandbox license database access fails. Use `Invoke-CandidateChecked` from `Tools/CandidateSource.ps1`, hidden launch, explicit marker/log. Normal integration entry point is now `ProductionUpgradeAuthoring.Integrate` (Courier then City).

## Work saved

- Reauthored licensed Quaternius Superhero male anatomy into a skinned courier: fitted technical jacket, slate trousers, cyan collar/hood/seams, trail soles, swept hair, sewn delivery bag/webbing/flap/emblem. Current authoring output: **17,065 triangles, 9,131 vertices, one mesh, four material types**, valid Humanoid avatar. This is substantial geometry work, not just a material recolor. It still requires deformation/garment acceptance.
- Reproducible Blender authoring: `Tools/AuthorProductionCourier.py`; editable source `ArtDirection/ProductionUpgrade/Sources/ShiftboundCourier.blend`; exported asset under `UnityProject/Assets/Shiftbound/ProductionUpgrade/Courier`.
- Blender 4.5.14 LTS portable is under `.validation/ProductionUpgrade/Tooling/blender-4.5.14-windows-x64/blender.exe`.
- Authored mesh kit and constructed city across the existing slice: paving, flashing, drains, coping, round rails, vent casing/louvers, limestone/window/sill/mullion/service kit, grounded middle city, viaduct and street datum. Legacy decorative renderers retired; original gameplay collision signature verified unchanged on city integrations. No added level or movement reach changes.
- Opaque folded leaf/stem geometry roots growth at coping and equipment; shared city remains stable between worlds. Existing panorama shader mathematically blends wrap ends, but first reviews show poor projection/softness and it needs replacement integration.
- Reduced camera distance to 6.7 m and look-ahead to 1.5 m; preserved free orbit and grounded assistance agency.
- Shared shipped Canvas for gameplay, pause/settings and completion. Minimal control rings, Shift adjacent to Jump, held-jump slide retained, action feedback, short receding contextual instruction. Menu contact ownership, slider acquisition/commit and preference save deduplication address repeated stationary-touch writes.
- Animation blend/gait-speed refinement, chest acceleration/braking/bank layer and contact IK. Removed character scale squash on landing; regression expectation updated to preserved rig proportions while retaining impact and immediate input requirements.
- Explicit `.gitattributes` and matching C#/PowerShell UTF-8/LF canonical source hashes; binary checks remain exact SHA256. LF/CRLF equivalence checked. Build identity includes hash policy.
- Baseline Windows build/evidence retained. New output directory is `UnityProject/Builds/ProductionUpgrade/Windows`.

## Actual builds and evidence so far

Two Windows candidate builds passed. Last reviewed build is **before the latest garment coverage and lowered roof-cap correction**; do not claim its source fingerprint matches this checkpoint.

- Last reviewed build manifest fingerprint: `cacbf95a41c31e518683ec0ae6a49a85dd50e31319f30d6da2993bd7c2d4f270`; build time 2026-10-09T21:46:19Z; revision metadata records baseline because it was built from dirty branch work. Final delivery must commit source before final build.
- `.validation/ProductionUpgrade/BeforeOrbit`: fresh baseline gameplay/audio/video, ~25 s, synthetic production input; NOT performance measurement.
- `.validation/ProductionUpgrade/ReviewOrbit1` and `ReviewOrbit2`: actual candidate gameplay renders, audio and videos, ~27 s. Orbit2 includes actual shipped pause panel, movement/jump/recovery. Readback overhead means NOT performance evidence.
- `.validation/ProductionUpgrade/ReviewFixed1` and `ReviewFixed2`: both worlds, yaws and pitch limits, 1280x720, actual Canvas. ReviewFixed2 successfully logged `SHIFTBOUND BENCHMARK CAPTURES COMPLETE`; invocation mistakenly expected `...PASSED`, so helper returned failure despite completed capture. Use correct marker next time.
- `.validation/ProductionUpgrade/CourierReview`: offline pose renders. Sampling via direct clip or Animator.Update still mostly produced standing poses. **These do not establish deformation/animation acceptance.** Capture runtime motions or use AnimationMode sampling correctly.
- First full regression: 11 jump-intent assertions PASS; 18 touch-router assertions PASS. Traversal stopped at the old `post-move landing squash at30Hz` expectation. Source expectation was intentionally fixed, but the suite has **not yet been rerun to completion**.
- Logs: `Logs/upgrade-courier-refine.log`, `upgrade-city-refine.log`, `upgrade-courier-motion-review.log`, `candidate-Windows-build.log`, `upgrade-checkpoint-integration.log`.
- Final pause integration **PASS**: `ProductionUpgradeAuthoring.Integrate` compiled, reimported the latest courier, saved the corrected city and preserved the collision signature. This integrated scene has not yet been rebuilt/captured; that is the first resume action.

## Resume with these concrete corrections

1. Build the integrated checkpoint; visually verify the **lowered route-building crown** exposes paving. Previous flat roof was caused by the membrane cap reaching y=.08 over the paving at y=.017. Current structure max is roof-.75, keeping cap below tiles. Review equivalent views in both worlds and at phone size.
2. Inspect new garment coverage; previous screenshots showed exposed shoulder triangles/chest intersections. Current jacket selection/hidden-body cut now extends to 1.555/1.56 m with larger coverage/inflation and neck exclusion. Do not call this accepted until actual stride, air, land and orbit show good deformation. The anatomy-derived jacket still risks reading too tight/muscular; tailor it if needed.
3. **Major remaining motion defect:** runtime rise/descent has overly spread arms; current local-Z arm correction did not solve it. Reauthor consistent Humanoid muscle curves for Takeoff/Rise/Fall/Land from licensed source clips, with bent elbows close to bag. Remove bad local rotation workaround. `CourierMotionPolish.compression` currently calculated but unused; implement meaningful proportion-preserving landing absorption/IK or remove it. Keep movement/jump/Shift immediate. Calibrate gait by runtime foot contact, not arbitrary state count.
4. Integrate the saved new original distant matte `ArtDirection/ProductionUpgrade/DistantCity-v1.png`, using real equirectangular mapping and a small horizon band; existing compressed source stretches sky/city and looks pixelated. Retain seam continuity. New matte is generated but **not yet imported/shipped**. Main near/middle kit still needs stronger tiered silhouettes, construction and less repetitive windows. Huge sparse rectangular towers over a checkerboard street datum remain visibly unfinished in reverse/recovery views. Improve composition/construction, not generic detail scattering.
5. Foliage shader receives shadows but lacks a ShadowCaster pass; add matched wind/cast pass and review leaf volume/contact/color (currently overly lime in Overgrown). Maintain landing silhouettes; no transparent stacking. Source mesh kit has duplicated degenerate triangle fans in Bevel/Leaf; use explicit triangles if optimizing.
6. Finish UI cleanup: obsolete PhoneHUDCanvas unused; PremiumHUD currently exits early after allocating old styles, old PhoneControls LegacyMenu drawing methods remain dead. Keep serialized field compatibility/FormatTime. Desktop menu sensitivity slider currently edits TouchSensitivity; correctly target MouseSensitivity on desktop and label/read value accordingly. Test stationary pause touches, acquired drag/release/cancel, secondary touches, completion inert rows, neutral resume, all layout extrema/aspects/safe areas/rotations.
7. Implement phone rendering tiers from the written budgets: bounded 720p/540p internal resolutions, HDR/bloom off on phone, shadow35m1024/22m512, one cascade, justified MSAA. Current URP phone settings still predate the budget. Record actual draw/triangle/memory counters; no phone acceptance inferred from PC.
8. Review actual audio mix from route captures. Current clips are source-backed; timing/mix refinement and audibility under simultaneous footsteps/landing/shift remain needed.
9. Rerun meaningful delivered-player regressions and full route at30/60/120/144, update expectations only for intentional changes, add worthwhile menu/preview regressions, and capture route through completion/air Shift. `Tools/VerifyMilestone1.ps1 -ExistingBinary -Player <newplayer>` supports isolated output. Source fingerprint must match first.

## Android/runtime and performance status

- No adb device connected. No physical phone, five-player study or independent reviewer available. These remain **NOT VERIFIED**; prepare exact build/procedure after available production work.
- Current official Android page-size documentation (checked9Oct2026) explicitly requires RELRO end `(VirtAddr+MemSiz)%0x4000==0`; baseline five failures are real. Project linker flags do not repair vendor prebuilts. References: https://developer.android.com/guide/practices/page-sizes and official Unity release/dependency docs.
- Unity6000.3.26f1 released8Oct2026, supported AGP9.1.1/Gradle9.3.1; no release-note claim that RELRO was fixed. Downloaded signed official Android module to `.validation/ProductionUpgrade/Tooling/UnityAndroid-6000.3.26f1.exe`, signature verified Unity Technologies SF. Existing Editor was **not upgraded** and libraries were **not mixed**.
- Attempted extraction with bundled and full official7-Zip26.04: custom NSIS archive unsupported. Extraction outputs `UnityAndroid-6000.3.26f1/[0]` and `UnityAndroid26Files/[0]` are installer overlays, not runtime libs. One inspection-only overlay had flags modified for parser experiment; never ship/trust it as a native replacement. Original signed installer untouched.
- Silent isolated module installation hung without output files; owned installerPID21352 stopped at pause, exit2. Do not leave it running. Isolated destination `UnityAndroid26Installed` contains no native artifacts.
- Next useful route: get official release API module links and inspect Linux Android support tar.xz (same vendor Android runtime) or perform a supported full isolated Editor/module installation. If latest supported runtime also fails, record exact vendor-library provenance and requested fixed runtime; do not hex-patch/relink prebuilts or claim compatibility.
- Inspector `Tools/InspectAndroidArtifact.ps1`: every LOAD/RELRO, ZIP alignment, signature, identity/API/ABI. Bundletool `.validation/AndroidTooling/bundletool-all-1.18.3.jar`. ffmpeg `.validation/AndroidTooling/ffmpeg.exe`.
- Android emulator is absent. A verified supported16KB launch/gameplay environment still needs setup. Physical20-minute performance/touch remains unavailable independently of emulator.
- Windows warnings still need controlled foreground investigation. `Tools/CaptureProductionBenchmark.ps1 -ObserveOnly -NoVideo` verifies foreground ownership; current `ProfileWindowsGameplay.ps1` defaults old build and hidden window, so extend Player/foreground path rather than reuse its hidden callback data as presentation evidence. Preserve user's unrelated workloads. Record actual CPU/GPU availability and limitations.

## Delivery gates at pause

| Gate | Status | Reason |
|---|---|---|
| Courier and coherent motion | FAIL | Outfit/deformation needs review; wide-arm air posture remains a major defect |
| Coherent playable environment | FAIL | Latest roof correction unreviewed; middle/far composition and foliage contact need work |
| Excellent two-thumb play | NOT VERIFIED | Implemented UI/input improvements; full new-build regressions and physical comfort pending |
| Android compatibility and sustained smoothness | FAIL | Baseline vendor RELRO failures unresolved; rebuilt Android artifact absent |
| Physical16KB install/lifecycle,20min60/30,5 unfamiliar players,independent artistic review | NOT VERIFIED | Hardware/participants/reviewer unavailable |

Do not award a numerical quality rating or describe this checkpoint as the completed production milestone. Complete available work, build/inspect frequently, keep exact artifact identities, and then report real remaining dependencies.
