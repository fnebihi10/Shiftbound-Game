# Opening Visual Benchmark V5

## Scope and baseline

The benchmark covers the start roof, first jump, first landing and first alternate bridge, approximately the opening 15–30 seconds of normal play. The latest verified in-game image is the archived [V4 Windows-player capture](ArtDirection/GoldenRooftops-player-capture.png); the [visual target](ArtDirection/visual-target-gameview-v2.png) is concept art. The current V4 scene has 1,393 MeshRenderers by the previous scene audit. The actual V5 runtime result has **not** been captured or profiled.

These baseline ratings reflect the repository, prior deterministic test reports, scene data, and the archived V4 capture. They are provisional where human play or profiling is missing.

| Area | V4 baseline / 10 | Evidence and limit |
| --- | ---: | --- |
| Gameplay mechanics | 6 | Core jump, Shift, checkpoints and goal exist; bypass and human feel unverified. |
| Movement feel | 7 | Coyote time, jump buffer, actual velocity and fixed-step checks; no human verdict. |
| Camera | 6 | Collision and tuned forward framing; no free-look playtest. |
| World switching | 7 | Blocked, grounded and airborne smoke cases passed in earlier builds. |
| Level design | 5 | Teach/practice/challenge path exists; first Shift bypass remains possible by geometry. |
| Code architecture | 6 | Small comprehensible systems; UI/audio and art generation remain prototype-like. |
| Reliability/testing | 6 | Useful validator, smoke and comparison paths; no continuous human traversal. |
| Character quality | 3 | Rigged CC0 stand-in with simple backpack and outfit silhouette. |
| Animation quality | 3 | Idle/Jog/Sprint/Jump/Land; turns, fall and foot contact need review. |
| Foreground environment | 3 | Repeated primitives and thin trim in the actual V4 capture. |
| Distant environment | 5 | Attractive world-anchored matte; finite angular coverage and little parallax. |
| Materials | 5 | Licensed concrete albedo and normals, but mostly uniform response. |
| Vegetation | 3 | Repeated leaf shapes and flat ivy cards. |
| Lighting | 5 | Directional sun, fog and world palette; no calibrated capture review. |
| Post-processing | 4 | Camera enables it; committed volume includes legacy/template components. |
| UI | 3 | IMGUI main HUD and basic pause/completion. |
| Audio | 2 | Synthesized placeholder cues. |
| Performance readiness | 2 | Renderer count known; no presented frame-time or GPU profile. |
| Mobile readiness | 1 | Separate mobile URP asset exists; touch and device validation absent. |
| Production/release readiness | 2 | Playable source and Windows build path; art, device and release gates open. |
| Overall vertical-slice quality | 4 | Coherent playable prototype; foreground and courier well below target. |

## Changes implemented in source

- `OpeningBenchmarkRuntimeV5.cs` constructs visual-only roof coping, layered fascia, parapet piers, chamfered service housings, cooling blades, a grated drain, localized repair marks and irregular leaf clusters. It runs only in Golden Rooftops when no baked V5 root exists. It reuses two procedural meshes and disables their shadow casting. The Overgrown plants join `RooftopWorldArt`'s existing world state list. No gameplay collider is added.
- `BuildVisualBenchmarkV5.cs` provides an idempotent Editor command to bake a richer benchmark into the existing scene. It preserves the V4 scene as its source and replaces only its own V5 root on rerun. Its resulting scene still requires licensed Editor validation and a capture.
- `FeedbackAudio.cs` accepts licensed authored cues and two looping world ambiences, crossfades them on Shift, and retains synthesized sounds as fallbacks. `PlayerMotor.cs` now triggers a jump cue and distinguishes hard landings by fall speed. No production audio files have been added.
- Project product/company fields and template application identifiers were replaced with provisional Shiftbound values. Confirm an owned identifier and publisher name before distribution.

## Verification record

| Type | Result |
| --- | --- |
| Automatically verified | Runtime and Editor C# project builds passed with zero warnings/errors against the installed Unity 6000.3.25f1 assemblies. Repository state and prior smoke/comparison reports were inspected. |
| Through Unity Editor | **Unavailable now.** Sandboxed startup failed at Package Manager IPC; elevated startup reached licensing but exited with `No valid Unity Editor license found` (return code 198). |
| Through standalone build | **Not run for V5.** The existing local Windows build predates these changes. |
| Visually verified | **V4 only.** The archived player capture was viewed; no V5 Game-view/player image exists. |
| Not yet verified | V5 shader/material appearance, baked scene, audio mix, full route, Shift bypass, GPU/CPU frame times, mobile device behavior, human comfort/readability. |

The runtime fallback exists so a future licensed build can include the benchmark without running an Editor menu. The Editor bake has more detail than the runtime fallback; select one implementation for final production after visual and performance review. Its generated meshes and runtime materials are small, but the additional renderer submissions are **not** an optimization claim.

## Before and expected V5 difference

The V4 capture shows a textured floor and distant skyline, with boxy service equipment, very regular leaf forms, exposed razor-like edges and little roof construction. V5 source adds near-scale roof assemblies and grounded vegetation across the first traversal segment. Their appearance in a rendered player is still unverified, so no updated screenshot or visual quality score is claimed from them. The character, matte skyline, main IMGUI HUD and basic animation set remain the largest visible gaps.

## Updated assessment

Scores describe **verified quality**, so the unrendered V5 geometry does not raise a visual score yet.

| Area | Current / 10 | Why |
| --- | ---: | --- |
| Gameplay | 6 | Movement/Shift unchanged; jump/landing audio hooks added, unplayed. |
| Code | 6 | Additive systems compile; visual generator and fallback need Editor review. |
| Testing | 6 | Existing regression paths retained; V5 could not be run. |
| Character | 3 | No authored replacement or motion review. |
| Animation | 3 | No new clips. |
| Environment | 3 | V5 source adds detail, but no verified rendered result. |
| Materials | 5 | Existing PBR sources retained; new materials await rendered review. |
| Lighting | 5 | Existing setup unchanged. |
| Vegetation | 3 | New rooted clusters implemented, visual quality unverified. |
| Skyline | 5 | Existing matte unchanged. |
| UI | 3 | Main HUD remains IMGUI. |
| Audio | 2 | Replaceable cue/ambience slots added; actual assets and mix absent. |
| Performance readiness | 2 | No V5 standalone profile. |
| Mobile readiness | 1 | No touch or device test. |
| Release readiness | 2 | Unity license, authored content and release validation open. |
| Overall vertical slice | 4 | Only V4 has verified player evidence. |

## Remaining blockers, ranked

| Work | Visual impact | Gameplay impact | Effort |
| --- | --- | --- | --- |
| Licensed Unity import, bake/capture and profile of V5 | High | High validation value | Low once license works |
| Authored courier model, materials, rig cleanup and motion set | Very high | Medium | High |
| First Shift bypass test and smallest geometry correction if reproduced | Medium | High | Low–medium |
| Artist-authored facade/roof kit and believable vegetation atlas | Very high | Medium readability | High |
| Native game UI and licensed sound/ambience mix | Medium | Medium | Medium |
| Mid-distance 3D silhouettes and side-angle matte coverage | High | Low | Medium |
| PC standalone frame profile and representative Android touch/device pass | Indirect | High reliability | Medium |

## Smallest next milestone and acceptance gate

With a valid Editor entitlement, bake V5, validate Golden Rooftops, build Windows, run `-shiftboundSmoke`, capture **actual** Present and Overgrown player frames at start and the first bridge, and record a continuous 20-second human playthrough. In that pass, verify the first Shift cannot be bypassed from normal takeoff and landing points, the parapets never hide targets, no new visual object blocks the controller or camera, world growth toggles correctly, and the player remains readable. Profile the same build at 1920×1080 on a stated PC, recording CPU/GPU p50 and p95 frame times, draw submissions, GC, and Shift spikes. Review the captures against the concept and archived V4 image before extending V5 elsewhere.
