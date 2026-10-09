# First production benchmark — work and acceptance record

AI-authored implementation and evaluation record, 2026-10-09. Independent human art direction and playtesting remain required.

Baseline HEAD: `4bffa80f53dc4f733f434c0c69971c8b3b79f10d`, branch `milestone-reliable-traversal`. Existing untracked AI_WHOLE_GAME_REVIEW.md is preserved. GoldenRooftops remains the authored scene. No added levels or general content pipeline.

## Scope and constraints established before authoring

Opening start roof, first landing, first Overgrown bridge, visible facades/midground, far city and existing courier. Preserve all approved colliders, motor values, Shift clearance/debounce, checkpoints and the 7.5 m mandatory direct gaps. Scene changes use Unity APIs and retain original GUIDs.

The approved concept is high-end stylized realism: believable construction, warm sun/cool shadows, a related lush alternate world and an expressive courier. Existing captures reveal repetitive box facades, oversized masonry texture, overlapping HVAC layers, hard skyline limits, plastic primitive plants and incomplete motion. Generated images are environment textures only; they are not implemented 3D buildings or proof of a finished benchmark.

Proposed camera contract: retain full 360° yaw and the established -15° to 58° pitch; paired panorama skyboxes replace the finite plane. Tune normal distance/FOV only after comparison captures; keep live collision and camera-relative input. Side/rear/pitch-limit coverage must be verified in a player. Panoramas provide no explorable city or parallax.

Asset dispositions: retain licensed Poly Haven surface sources, working Quaternius rig/runtime clips and approved collision. Improve physical texture scale, authored lighting, preview presentation and existing rig motion. Replace finite sky matte, conflicting equipment shells and flat near-window treatment with a focused reusable facade/roof/equipment kit. Replace unsuitable baked-background ivy with a real alpha-cutout raster asset. A premium courier, authored stop/turn/fall/impact set and 3D rooted vegetation remain external asset requirements unless verified suitable sources are found.

Performance protocol: standalone D3D11, same actual host hardware, 1920×1080, current PC quality, VSync off, no frame cap. Warm up, inspect four fixed start-roof orbits in each world, settle 1.5 s, sample 3 s per view. No image capture/readback during timing. Record FrameTimingManager CPU/GPU timing and available rendering/GC counters; unsupported/zero timings remain unavailable, never substituted with FPS caps. Proposed p95 CPU and GPU budget is 16.7 ms for 60 FPS; this is a target until measured. Additional traversal/Shift spike measurement and human playtesting remain separate gates.

## Verdict after implementation

**The benchmark is implemented and playable, but the premium production acceptance gate is not passed.** This is a demonstrable improvement in construction, camera coverage, preview clarity, lighting and desktop rendering cost. It is not evidence of a 9/10 game. The courier, motion, vegetation, generic roof surfaces and near/far integration still visibly fall short of the approved concept. Do not expand the level or start the next milestone on the strength of the technical passes alone.

No new statement from the user was needed to start: this task authorized implementation and verification. Raising the bar means rejecting a candidate when its required gates fail, even after substantial work. The remaining blockers require better assets, rendering/latency investigation and independent human evaluation rather than more primitive decoration.

## Implemented changes and preservation

- Saved 3.0 × 3.1 m recessed-window bay and consolidated service-unit mesh/prefab assets. Visible neighbouring buildings use the same construction kit on all four faces, with top caps. Existing masses, playable geometry and landmark positions are retained.
- Retired 565 legacy opening renderers, including overlapping HVAC shells, primitive plants, fake plant route lights and redundant paving joints. Original objects/colliders are retained. The V5 marker root stays active so its runtime fallback cannot stack the old pass back into the scene.
- Opening roof UVs use a 2 m material repeat independent of slab size; existing licensed diffuse/normal sources are retained. Shared opening slabs stay concrete in both worlds. Five lit, alpha-tested hanging-ivy cards replace unsuitable opening silhouettes.
- Replaced the finite skyline plane with paired contemporary-city panorama skyboxes. Nearby buildings are meshes; the distant city is explicitly a 2D far-field texture with no parallax. An ornate first texture attempt and uniformly green floor treatment were rejected during player inspection and corrected.
- Warm directional sunlight, cool trilight ambient and authored restrained ACES/bloom grading. Existing URP PC quality remains in use; no new light count or additional rendering package.
- Camera changes from distance 8 m/FOV 66°/look-ahead 2.3 m to 7.6 m/60°/1.9 m. Full yaw, pitch limits, smoothing, collision resolution and recovery remain. Fixed-pose inspection is opt-in.
- Hatched cyan inactive collision surfaces; inactive decorative renderers are hidden. Full original material-slot arrays and initially disabled renderer states are preserved. Shift collision queries, timing and momentum are unchanged.
- World notices now fit one line without colliding with the world/timer pills; look, pause and retry controls are shown. Existing Jog/Sprint clips preserve gait phase during transitions. No new character, motion clip or low-quality substitute was introduced.

The authoring and refinement each compared every existing collider's identity, transform, scale and serialized fields before/after and refused to save on a change. Both passed. Motor source, safe Shift clearance/debounce, checkpoint and goal logic are unchanged. Existing blocked-Shift test wall and original rooftop utility collider remain; their crude visible presentation remains an art issue. This preservation check establishes unchanged collision data, not enjoyable gameplay or complete camera correctness.

## Actual evidence

[Open the comparison gallery](ArtDirection/ProductionBenchmark/index.html). It includes fixed-pose standalone renders and separate OS screen captures with the HUD. It identifies their origin and limitations.

| Evidence | Location | What it establishes |
|---|---|---|
| Before normal-control gameplay | [.validation/ProductionBenchmark/BeforeFootage/gameplay-25s.avi](.validation/ProductionBenchmark/BeforeFootage/gameplay-25s.avi) | 500 actual client frames, 20 fps, 25 s; unchanged visual baseline. |
| After normal-control gameplay | [.validation/ProductionBenchmark/AfterFootage/gameplay-25s.avi](.validation/ProductionBenchmark/AfterFootage/gameplay-25s.avi) | Opening jump, Shift, bridge crossing, shared landing, mouse orbit and recovery in the real player. |
| Small-window gameplay | [.validation/ProductionBenchmark/SmallFootage/gameplay-25s.avi](.validation/ProductionBenchmark/SmallFootage/gameplay-25s.avi) | Same normal-input sequence at 800×450, including readable first-Shift state changes. |
| Paired fixed views | `.validation/ProductionBenchmark/{BeforeCaptures,AfterCaptures,SmallCaptures}/` | 22 actual camera renders per set: eight yaw positions in both worlds, pitch -15°/58° and first landing. Camera/motor held for controlled inspection; these are not playtest screenshots. |
| Curated comparisons/HUD captures | `ArtDirection/ProductionBenchmark/` | Stored PNG/JPEG evidence suitable for reviewing the report without rerunning Unity. |
| Timing audit | `ArtDirection/ProductionBenchmark/{before,after,small}-capture-times.csv` | Real screen-capture sampling timestamps. After clip's final frame completed at 24.979 s; maximum completion delay relative to schedule was 33 ms. This is not the game frame rate. |

Videos are silent MJPEG AVI, about 101 MB before and 98 MB after, kept in the ignored local evidence directory. Use a compatible local video player; these files are not claimed to be uploaded or hosted on GitHub. No audio quality/listening approval was performed. The real game received Windows keyboard/mouse input; the operator was an automated script, not a human tester.

## Standalone performance evidence

Actual host: **AMD Ryzen 5 5600X, AMD Radeon RX 7600, 32,657 MB system RAM, Windows 11 build 26300, Unity 6000.3.25f1, D3D11, release player, PC quality, 1920×1080, VSync off, uncapped**. Same stationary start-roof protocol before/after: yaw 0/90/180/270 in both worlds, 1.5 s settle then 3 s per view. Baseline 18,667 unique measured frames; candidate 22,295. No screenshots, gameplay recorder or regression jobs ran during sampling.

| View | CPU frame p95 before → after (ms) | GPU p95 before → after (ms) | Draw calls median before → after | Triangles median before → after |
|---|---:|---:|---:|---:|
| Present forward | 2.139 → 1.460 | 1.795 → 1.263 | 2,772 → 848 | 322,831 → 490,171 |
| Present right | 1.299 → 1.127 | 0.791 → 0.943 | 856 → 312 | 80,653 → 241,085 |
| Present rear | 1.198 → 1.153 | 0.842 → 0.771 | 717 → 242 | 87,457 → 112,047 |
| Present left | 1.456 → 1.180 | 1.190 → 0.942 | 1,344 → 479 | 141,599 → 276,763 |
| Overgrown forward | 2.383 → 1.609 | 1.838 → 1.549 | 5,510 → 1,956 | 884,727 → 716,949 |
| Overgrown right | 1.330 → 1.177 | 0.790 → 0.939 | 1,214 → 326 | 155,629 → 241,147 |
| Overgrown rear | 1.261 → 1.128 | 0.754 → 0.745 | 1,081 → 250 | 170,397 → 112,087 |
| Overgrown left | 1.679 → 1.190 | 1.461 → 0.949 | 3,030 → 523 | 475,059 → 277,045 |

These are FrameTimingManager readings and valid ProfilerRecorder rendering counters, not inferred FPS. Draw/triangle counts cover rendered passes, not unique scene objects or model triangles. The replacement reduces draw overhead while increasing geometry in several views; it is not a universal GPU improvement. Each recorded phase is below the proposed 16.7 ms component budget on this desktop, but that is a limited diagnostic pass.

**Performance approval remains provisional.** The final profile emitted 83 D3D11 presentation warnings about missing presentation waits; normal-control recordings emitted 3 before and 50 after. Cause and latency impact have not been isolated. Do not attribute the warning to a particular new feature without reproduction, or infer low input latency from low CPU/GPU times. The profile script also adds timing/CSV bookkeeping overhead, not separately measured. Release GC-allocation recorder was unavailable; there is no GC claim. No Unity Profiler deep capture, thermal soak, mobile hardware, traversal/Shift-spike distribution or input-to-photon measurement was completed.

Full per-view summaries and raw CSV SHA-256 are stored in [before-profile-summary.json](ArtDirection/ProductionBenchmark/before-profile-summary.json) and [after-profile-summary.json](ArtDirection/ProductionBenchmark/after-profile-summary.json). Raw frames/hardware are in `.validation/ProductionBenchmark/{BeforeProfile,AfterProfile}/`.

## Acceptance gates

| Gate | Result | Evidence and limit |
|---|---|---|
| Implemented GoldenRooftops and reproducible Windows build | PASS | Authoring, refinement, final and normal delivery builds succeeded; scene validation passed. Unity APIs authored the scene and saved referenced assets. |
| Preserve approved traversal/Shift/checkpoints | PASS, automated | Collision contracts unchanged; 11 production-source jump-intent assertions; smoke, regression and reach audit; complete direct-step routes at 30/60/120/144/variable dt; rendered routes through production GameInput/Update/camera at 30/60/120/144 FPS. Each full route reached all three checkpoints and the goal, with stopped completion motion. Automated passes establish reliability, not player enjoyment. |
| Normal keyboard/mouse play remains possible | PASS, exercised | Real OS input in both recorded window sizes; jumping, Shift bridge, orbit and recovery observed. No systematic responsiveness measurement or human approval. |
| 20–30 s real gameplay benchmark and before/after | PASS | Three 25 s clips with real frame timestamps and accompanying comparisons. Silent sampled screen recordings, not engine performance captures. |
| Identical Present/Overgrown inspection poses | PASS | Eight yaw views plus pitch limits and first lesson at each size. World collider state can affect camera collision where geometry differs; start inspection is on safe shared support. |
| Supported skyline coverage | PASS for coverage; FAIL for premium finish | Fixed rear/side/pitch renders no longer reveal a finite matte rectangle or empty rear horizon. Panorama angular detail, spherical projection/seam quality and near/far integration remain below the target; discrete views are not exhaustive continuous motion approval. |
| Active-route readability at 1280×720 and 800×450 | PASS for inspected clipping/state; HUMAN GATE OPEN | Preview hatching, solid bridge, world pills, controls and lesson text visible at both sizes; notice no longer wraps. Small text and overlapping visual attention need naive-player assessment. |
| Convincing foreground construction/materials | PARTIAL / NOT ACCEPTED | Recessed windows, coherent kit dimensions and roof UVs improve construction. Generic concrete, repeated flat roof caps, residual utility/test wall and visible legacy far props still look prototype-like. |
| Believable courier motion and premium silhouette | FAIL | Existing low-detail model, backpack and wide-arm jump pose remain plainly below concept. Gait-phase correction does not supply missing acceleration/turn/stop/fall/impact assets. |
| Clear, coherent and lush Overgrown counterpart | PARTIAL / NOT ACCEPTED | Same near geometry and corresponding city art; improved edge ivy and solid bridge communication. Planar foliage and legacy visible midground vegetation still do not establish authored natural growth. |
| Standalone render budget | DIAGNOSTIC PASS; FINAL GATE OPEN | Actual per-view timing/counters recorded on named desktop. Presentation warnings, traversal spikes, latency and target-device measurements remain unresolved. |
| Audio, physical gamepad and independent human playtesting | NOT VERIFIED | Silent capture and virtual-input diagnostics do not establish these gates. |

## Remaining work ranked by impact

1. **Courier and motion assets — high effort, external dependency.** Obtain/commission a Humanoid courier and cohesive in-place movement set matching the approved silhouette. The current frame `after-jump-720.jpg` shows why phase tuning alone cannot meet the bar. Verify foot contacts, takeoff/fall/landing continuity, backpack clearance and response at the exact gameplay camera. Do not change motor timing to conceal bad animation.
2. **Foreground art acceptance — medium/high effort, artist dependency.** Review and replace rejected kit portions with authored paving, coping, believable service equipment and materials. The retained utility block and blocked-Shift wall need coherent visual construction matching their unchanged collision, not hiding an obstacle. Validate construction from forward, rear, low and wall-proximity views.
3. **Rooted 3D vegetation — medium/high effort, external dependency.** Replace planar ivy and visible primitive far growth with attached stems and varied volumes, with LOD and alpha-overdraw budgets. Preserve route markers and landing silhouettes. No random clutter pass.
4. **Panorama/integration — medium effort, art dependency.** Replace the 1774×887 generated candidate with properly registered, seamless spherical art at adequate angular resolution; match shadow/sun direction, scale and near/mid/far architecture in continuous orbit. Greater texture size alone will not add parallax or fix wrong projection.
5. **Presentation latency and human gate — engineering + playtesting.** Reproduce the D3D11 warnings with post-processing on/off, timing instrumentation on/off and controlled presentation settings; use a named target device, standalone profiler trace and input-to-photon method. Then conduct fresh-player instruction, gap-judgment and world-state testing at both sizes. No higher rating before independent art, motion and playtesting evidence.

Detailed required asset dimensions, compatibility and licensing constraints are in [ASSET_SPECIFICATIONS.md](ArtDirection/ProductionBenchmark/ASSET_SPECIFICATIONS.md). These are the next dependencies, not a request to add another level. Postpone expansion, collectible/replay systems, a general content pipeline, additional effects and decorative density until this benchmark passes.

## Reproduction and provenance

Review branch: `milestone-reliable-traversal`. The baseline hash above identifies the source before this benchmark; the implementation and evidence are recorded alongside this report in Git. No unrelated user changes were overwritten; the pre-existing AI review is retained. `SkylineRooftops` and original third-party assets are unchanged. Generated texture provenance is recorded in THIRD_PARTY.md. Final local production build is `UnityProject/Builds/WindowsPolished/Shiftbound.exe`; local baseline/candidate comparison builds are in `.validation/ProductionBenchmark/{BeforePlayer,AfterPlayer}/`. Builds, raw logs and AVI recordings remain ignored local artifacts; committed screenshots, measurement summaries, verification markers and reproduction tools are available for remote review.

Final scene, build assembly/scene binary and video fingerprints are stored in [artifact-manifest.json](ArtDirection/ProductionBenchmark/artifact-manifest.json). Final regression success markers and log hashes are in [automated-verification.json](ArtDirection/ProductionBenchmark/automated-verification.json). Local logs are under `Logs/production-*.log`. The manifest's baseline HEAD is not the post-benchmark commit: use the containing Git commit to identify the published source snapshot.

Build with Unity 6000.3.25f1 and `-executeMethod SliceDelivery.BuildWindows`. Here the non-batch Editor launch with `-quit -force-d3d11` worked; batch/headless Editor licensing had failed in the earlier review. `SHIFTBOUND_BUILD_DIR` selects an isolated output. Do not rerun the historical scene generators. The initial benchmark authoring method refuses to stack onto an already authored candidate; edit existing kit assets/scene from this point.

Standalone diagnostic flags: `-shiftboundBenchmarkCaptures <directory>` or `-shiftboundProfile <directory>`; specify D3D11 and window size. The profile disables gameplay motion and samples controlled views; normal play has no probe unless explicitly requested. `Tools/CaptureProductionBenchmark.ps1 -Player <absolute exe> -Output <absolute directory>` records ordinary input and screen pixels, aborting if focus is lost. A failed initial focus attempt was corrected and replaced by complete recordings; it is not part of the evidence.

Read/inspected: HEAD/working tree, approved concept and previous actual player captures, README.md, GAME_DESIGN.md, ART_DIRECTION.md, MILESTONE_1_PROGRESS.md, THIRD_PARTY.md, AI_REVIEW.md and the latest whole-game AI review; scene, V3/V4/V5 authoring/runtime code, materials, licensed character/animation assets, camera, input, motor, Shift, flow/HUD, URP settings and test harnesses. Observations above distinguish code contracts, automated results, actual renders and subjective visual judgments. No human enjoyment, professional art sign-off or audio claim is made.
