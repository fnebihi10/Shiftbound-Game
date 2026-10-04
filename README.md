# Shiftbound

## Opening Visual Benchmark V5 (source preview)

The saved `GoldenRooftops.unity` scene now contains a V5 detail layer under **Visual Benchmark V5 - opening roofs**. Reopen the scene to see non-colliding roof construction, service equipment, drainage detail, and rooted Overgrown plant clusters around the start roof and first Shift bridge. The existing collision and route rules remain authoritative. The plants are inactive in the default Present world and appear after shifting in Play mode. `OpeningBenchmarkRuntimeV5` is a fallback for scene versions without the baked layer. With a licensed Unity Editor, **Shiftbound > Build Opening Visual Benchmark V5** can replace the saved layer with more detailed chamfered meshes.

This change passed an external C# compile check, but this machine's Unity Editor reports no valid license, so V5 could not be imported, rendered, or captured. The V4 player capture below remains the latest verified game image. See [VISUAL_BENCHMARK_V5.md](VISUAL_BENCHMARK_V5.md) for ratings, verification limits, blockers, and acceptance criteria.

Slice changes and exact verification limits are recorded in [SLICE_AUDIT.md](SLICE_AUDIT.md) and [SCENE_COMPARISON.md](SCENE_COMPARISON.md). The checked-in Windows build is older than the current source. With an active Editor license, use **Shiftbound > Validate Golden Rooftops** followed by **Shiftbound > Build Windows Playable Slice** for a new build.

Shiftbound is a Unity 6.3 LTS / URP third-person rooftop platformer prototype. The player runs, jumps, and shifts between a contemporary city and its overgrown counterpart to reach the goal.

## Current playable scene

Open `UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity`. This V4 scene keeps the existing puzzles and adds paired distant city backdrops, licensed PBR rooftop and building materials, more rooftop detail and foliage, a smaller courier backpack, and adjusted camera and animation tuning. V3 `SkylineRooftops.unity` remains available for comparison.

Compare the approved [concept image](ArtDirection/visual-target-gameview-v2.png) with the [actual V4 Windows-player capture](ArtDirection/GoldenRooftops-player-capture.png). V4 is visibly closer in its skyline and materials, but its character, nearby architecture, vegetation, and overall finish are still below the concept. The distant city is a camera-facing 2D matte layer behind real playable 3D rooftops; it cannot be explored as a 3D city. This is still a prototype, not a finished store game.

## Open and test

1. In Unity Hub, add the **UnityProject** folder inside this repository. Open it with **Unity 6.3 LTS (6000.3.25f1)**.
2. In Unity's Project panel, open **Assets > Shiftbound > Scenes > GoldenRooftops**. Confirm the Hierarchy title says **GoldenRooftops**.
3. Click the **Game** tab and press the **Play** triangle. Click in the Game view to focus controls.
4. Press **Play** again to exit Play mode. Changes made while playing are not saved.

| Action | Keyboard / mouse | Gamepad |
| --- | --- | --- |
| Move | WASD or arrow keys | Left stick |
| Look | Hold right mouse button and move mouse | Right stick |
| Jump | Space | South button / A |
| Shift worlds | Shift | West button / X |
| Pause / resume | Escape | Start |
| Restart | R | Select / Back |

A local Windows test build is at `UnityProject/Builds/WindowsV4/Shiftbound.exe` when built on this machine. GitHub stores the Unity source, not the generated Windows build. The project does not yet have mobile touch controls or store-ready builds.

## Project layout

- `UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity` — latest playable scene.
- `UnityProject/Assets/Shiftbound/Scripts/` — player, camera, world switching, checkpoints, game flow, animation, and V4 backdrop/state scripts.
- `UnityProject/Assets/Shiftbound/Editor/BuildArtPassV4.cs` — V4 scene generator. It reads `SkylineRooftops` and refuses to replace the existing authored `GoldenRooftops` scene.
- `UnityProject/Assets/Shiftbound/TexturesV4/` and `MaterialsV4/` — paired city images and licensed surface materials.
- `ArtDirection/` — approved concept and real player capture, labeled separately.

Read [GAME_DESIGN.md](GAME_DESIGN.md) for mechanics, [ART_DIRECTION.md](ART_DIRECTION.md) for the production target, [CLAUDE_REVIEW.md](CLAUDE_REVIEW.md) for a reviewer brief, and [THIRD_PARTY.md](THIRD_PARTY.md) for asset sources. Keep Unity `.meta` files with their assets.

## Verification and remaining work

V4 compiled in Unity 6.3 LTS, a Windows player build completed, and an offscreen render from that player produced the linked capture. The V4 player smoke check passed blocked and safe world shifts, a midair shift, checkpoint respawn, and goal state. This does not verify that a person can comfortably traverse the full level. Mobile controls, mobile performance, polished character modeling and animation, authored environment assets, finished audio, and store requirements remain open.
