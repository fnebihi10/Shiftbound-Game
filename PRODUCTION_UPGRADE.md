# Production upgrade — 9 October 2026

Baseline `983ec2be6967210400c7d03c0071dc216fa03586`; clean checkout, fetched main identical. Work branch `production/courier-city-upgrade`. No applicable AGENTS.md found. Existing evidence and a fresh player orbit inspected before changes. No added level. Original builds/evidence retained.

## Target and implementation

An agile courier crossing a believable warm limestone city, with cooler industrial Present roofs and rooted, layered Overgrown growth. Yellow technical outerwear, slate trousers, cyan textile accent and fitted delivery bag establish the silhouette. Architectural materials, foreground construction and real middle-distance buildings must belong to the same city. Effects and interface must leave the landing visible.

The strongest unfinished cues are the old angular shorts character, mismatched air poses, aggregate roof expanses, planar growth, repeated uncapped city boxes, distant panorama scale/seam, and competing instruction panels. Replace/re-author these together. Use licensed humanoid anatomy as a modeling/rig foundation, author clothing and equipment with continuous surfaces and appropriate skin weights; evaluate actual deformations. Build a reusable paving/coping/drain/equipment kit and rooted stem/leaf volumes. Extend coherent treatment across existing roofs only after reviewing the opening. Simplify contextual guidance, add integrated action status, and render pause/completion through the same shipped Canvas as gameplay.

Representative benchmark: start roof, first solid/preview bridge, first checkpoint and required airborne Shift. Review forward, side, rear, low/high orbit, walking/sprinting/braking/reversing, takeoff/descent/running landing, recovery and menus in both worlds at 1280×720 and phone-scale 800×450. Keep equivalent baseline captures. Input remains responsive while animation blends; physics/reach changes require explicit measured justification.

## Candidate device and rendering budgets

Proposed validation devices (performance support remains NOT VERIFIED): Snapdragon 778G / Adreno 642L / 6 GB RAM for 60 FPS; Snapdragon 665 / Adreno 610 / 4 GB for 30 FPS. ARM64, Android 8/API26+, GLES3, landscape both rotations. 60 tier: 1280×720 equivalent maximum internal render, one shadow cascade/1024 map/35 m, no extra shadow lights, no HDR/bloom on phone. 30 tier: 960×540 equivalent, 512 shadow/22 m. Target steady CPU and GPU p95 < target interval, p99 < 1.5× interval; <1% missed presentation deadlines, no repeated >100 ms gameplay hitches, stable post-warmup memory <750 MB PSS, no severe thermal state. Measure 20 minutes separately per tier with FrameTimeline and device telemetry. These are acceptance targets, not measured results.

Asset limits: courier ≤30k triangles, ≤4 opaque material slots, ≤2k maps, no runtime cloth; visible environment ≤350k unique triangles and ≤250 submitted draws as an initial target; foliage opaque shaped leaves or alpha cutout, bounded density/LOD, no transparent canopy stacking. Record real counters before accepting these budgets.

## Acceptance

1. Courier: clear authored outfit/bag, correct human rig, no conspicuous skin/clothing/bag intersections in captured motions, grounded feet and continuous gait/air/landing at motor speeds. Independent motion/art approval remains separate.
2. Environment: coherent near/middle/far scale and materials, no panorama wrap seam in continuous orbit, constructed roofs/equipment, physically attached volumetric growth, clear active support at phone size throughout existing route.
3. Interaction: owned simultaneous touches, preserved held jump through deliberate Shift gesture, no accidental actions on cancel/resume, contextual teaching recedes after demonstrated success, feedback distinguishes accepted/blocked/cooldown, actual shipped pause/settings/completion included in evidence, settings persisted without writes per stationary contact.
4. Android: every native LOAD/RELRO + ZIP and bundle-generated APK inspected; verified 16KB runtime launch/gameplay; physical install/touch/lifecycle and 20-minute 60/30 runs. Windows foreground rendering diagnostics do not substitute for Android presentation.

Use PASS, FAIL or NOT VERIFIED for every gate. No numeric self-rating. Retain source fingerprint and artifact hashes, provenance/licenses and continuation notes. No purchases, publication or contact with others.

## Verified dependencies

Owner reports no physical phone available. Physical touch comfort, install/update/lifecycle, latency, thermal/performance and five unfamiliar-player study remain NOT VERIFIED; complete all available authoring/build/visual/regression work. No independent reviewer available. Installed Unity is 6000.3.25f1/NDK r27c. Current official Android guidance explicitly requires `(RELRO VirtAddr + MemSiz) % 16384 == 0`; baseline five failures are real static failures. Trace binaries to supported vendor/runtime sources before choosing a fix. Project linker flags cannot repair prebuilt vendor libraries.

Official compatibility reference: https://developer.android.com/guide/practices/page-sizes (checked 9 October 2026).
