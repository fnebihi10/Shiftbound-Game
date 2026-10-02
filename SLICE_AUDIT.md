# Shiftbound playable slice audit

Baseline: `main` at `13ee1039b90b9e897a62ea831491d7da2ed4083b` (clean before work). Development branch: `polish-playable-slice`. Target scene: `UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity`.

## Observed and confirmed

| Priority | Evidence | Finding | Change |
| --- | --- | --- | --- |
| P0 | `ArtDirection/GoldenRooftops-player-capture.png` beside `ArtDirection/visual-target-gameview-v2.png` | The actual V4 courier is much smaller and less detailed; nearby rooftops read as repeated boxes and polygonal plants. The concept is an art target, not implemented 3D. | No visual parity claim. The skyline is now fixed in world space, normal orientation corrected, and camera post effects enabled. Authored character and environment work remains. |
| P0 | `PlayerMotor.cs` pre-move ground sphere and coyote refresh | The former ground test could count side contacts and refresh jump eligibility during ascent. | Walkable sphere cast near the feet, descending-only ground acceptance, post-move contact update, actual displacement velocity, deterministic step entry point. |
| P0 | `FollowCamera.cs` previous smoothed placement after sphere cast | The final smoothed position could be inside geometry; `Snap` ignored collision. | Collision now resolves at the final position and on snap, including overlap penetration. |
| P1 | `WorldSwitcher.cs` former `sharedMaterial` assignment | Renderers with multiple material slots lost their original secondary slots after a shift. | All material arrays are cached and restored, with matching ghost slots. |
| P1 | `WorldSwitcher.cs` and `AtmosphereController.cs` formerly both set fog or sun state | World presentation fought between immediate and blended writes. | `AtmosphereController` owns fog, ambient and sun blending; switching owns collision and route material state. |
| P1 | `PremiumHUD.cs` former `elapsed.ToString("00:00.0")` | Numeric formatting did not calculate minutes and seconds. | Explicit `mm:ss.t` calculation, device prompts, clickable pause and retry. |
| P1 | `SkylineBackdrop.cs` former camera position/rotation copy | The city moved with every camera turn. | World-anchored matte impostor. This is stable from the main route, not a full 360-degree panoramic city. |
| P2 | `TexturesV4/*_nor_dx.jpg.meta` | DirectX normal maps had green-channel flip disabled. | All three normal imports now flip green for Unity's tangent convention. |

## Code and level risks that still need play verification

- `BuildFirstSlice.cs` places the first shared landing through z=12 and the shift landing from z=16.5, leaving a 4.5 m gap. `PlayerMotor.cs` defaults imply ideal level-ground flight time `2 sqrt(2 × 1.85 / 28) = 0.727 s` and about 5.45 m at 7.5 m/s. A normal jump may bypass the first shift. The present midair takeoff ends at z=23.5 and the shared landing begins at z=28.5, another gap within that ideal reach. These are geometric risks, not completed jump outcomes; platform geometry was not moved without a playthrough.
- The scene YAML contains 1,393 MeshRenderer components and 65 BoxColliders. Submission, foliage overdraw, shadow cost and memory have not been profiled. The 4× PC MSAA setting is a quality choice, not a measured optimization.
- A stationary skyline quad has finite angular coverage and will look flat from side views. It needs panoramic art, layered impostors or modeled silhouettes after gameplay camera evaluation.
- `RiggedCourierAnimator.cs` still uses the existing Idle, Jog, Sprint, Jump and Land clips. Actual-speed matching is improved, but separate falling, stopping and turning motion, feet and backpack clearance need captured motion review.
- `FeedbackAudio.cs` still uses synthesized tones. Footstep and landing cues were added, but final authored sound and mix require listening in the game.
- `PremiumHUD.cs` uses IMGUI. It scales by height and has clickable controls, but small/wide aspect ratios and gamepad navigation need live UX testing.

## Verification record

- Read the five root guidance files, V4 scene, generators, input, runtime scripts, material imports, URP PC asset and project quality settings.
- Compiled all runtime and Editor C# sources using `dotnet build` against the installed Unity 6000.3.25f1 managed assemblies: zero warnings and errors. This is a syntax/type check, not a Unity Editor import or player build.
- A later Unity batch Editor run passed `SliceDelivery.ValidateScene`. Both retained scenes built successfully as separate Windows players. Their headless smoke checks passed, including jump stepping at 30, 60, 120 and 144 simulated steps/s and material restoration.
- The two Windows players completed the same fixed-step opening path through the first overgrown bridge. See [SCENE_COMPARISON.md](SCENE_COMPARISON.md) for captures and exact limits. No full-level human playthrough, continuous video, presented frame-time comparison or GPU profile exists.

## Manual verification still required

1. Open `UnityProject` with Unity 6000.3.25f1. Open `Assets/Shiftbound/Scenes/GoldenRooftops.unity`.
2. Run **Shiftbound > Validate Golden Rooftops**. Run the scene in Play mode and traverse start, first gap, alternate bridge, midair crossing, route choice, final crossing and goal using the controls in `README.md`. Test wall/corner shifts, floor contact, airborne and rapid shifts, camera walls and respawn.
3. Run **Shiftbound > Build Windows Playable Slice**. The default output is `UnityProject/Builds/WindowsPolished/Shiftbound.exe`; set `SHIFTBOUND_BUILD_DIR` for a different output directory.
4. Run the new executable with `-shiftboundSmoke -logFile <absolute-log-path>` and require `SHIFTBOUND SMOKE PASSED` in the log. Run with `-shiftboundCapture <absolute-png-path>` for a still. Record a continuous full traversal and both worlds separately.
5. Profile a standalone 1920×1080 PC build on declared CPU/GPU, PC quality, VSync off, 4× MSAA. Record CPU and GPU frame-time p50/p95/p99/max, draw submissions, shadow and transparency passes, GC allocations, switching spikes and memory before claiming 60 FPS. Compare screenshots against the approved concept and the archived V4 capture.

The existing `UnityProject/Builds/Windows/Shiftbound.exe` predates these source edits. Do not present it as the polished build.
