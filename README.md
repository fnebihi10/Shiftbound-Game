# Shiftbound

The [AI project review](AI_REVIEW.md) records code findings, current verification, visual gaps and the next acceptance gate for Milestone 1.

Current implementation: [Milestone 1 — reliable traversal and recovery](MILESTONE_1_PROGRESS.md). Unity import, a current Windows build, input/physics regressions and the complete scripted route succeeded in this task. Run `Tools/VerifyMilestone1.ps1 -Build` to reproduce checks (see the graphics-enabled fallback in [AI_REVIEW.md](AI_REVIEW.md) if headless licensing fails); the local player is `UnityProject/Builds/WindowsPolished/Shiftbound.exe`. Actual current [Present](ArtDirection/Milestone1/present.png) and [Overgrown](ArtDirection/Milestone1/overgrown.png) renders are available. Human playtesting and audiovisual polish remain open.

## Opening Visual Benchmark V5 (source preview)

The saved `GoldenRooftops.unity` scene now contains a V5 detail layer under **Visual Benchmark V5 - opening roofs**. Reopen the scene to see non-colliding roof construction, service equipment, drainage detail, and rooted Overgrown plant clusters around the start roof and first Shift bridge. The existing collision and route rules remain authoritative. The plants are inactive in the default Present world and appear after shifting in Play mode. `OpeningBenchmarkRuntimeV5` is a fallback for scene versions without the baked layer. With a licensed Unity Editor, **Shiftbound > Build Opening Visual Benchmark V5** can replace the saved layer with more detailed chamfered meshes.

The original V5 attempt was blocked by licensing. A fresh licensed import, build and real player capture succeeded during Milestone 1; earlier V4/V5 audit documents remain historical records. See the milestone progress document above for current evidence. The visual target remains unfinished.

Earlier slice changes are recorded in [SLICE_AUDIT.md](SLICE_AUDIT.md) and [SCENE_COMPARISON.md](SCENE_COMPARISON.md). Generated builds are local artifacts. Use **Shiftbound > Validate Golden Rooftops** followed by **Shiftbound > Build Windows Playable Slice** for a current build.

Shiftbound is a Unity 6.3 LTS / URP third-person rooftop platformer prototype. The player runs, jumps, and shifts between a contemporary city and its overgrown counterpart to reach the goal.

## Current playable scene

Open `UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity`. The authored scene retains the V4/V5 art layers and now includes Milestone 1 traversal, checkpoint and Shift-lesson repairs. V3 `SkylineRooftops.unity` remains available for historical comparison.

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

A current local Windows test build is at `UnityProject/Builds/WindowsPolished/Shiftbound.exe`. GitHub stores the Unity source, not the generated Windows build. The project does not yet have mobile touch controls or store-ready builds.

## Project layout

- `UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity` — latest playable scene.
- `UnityProject/Assets/Shiftbound/Scripts/` — player, camera, world switching, checkpoints, game flow, animation, and V4 backdrop/state scripts.
- `UnityProject/Assets/Shiftbound/Editor/BuildArtPassV4.cs` — V4 scene generator. It reads `SkylineRooftops` and refuses to replace the existing authored `GoldenRooftops` scene.
- `UnityProject/Assets/Shiftbound/TexturesV4/` and `MaterialsV4/` — paired city images and licensed surface materials.
- `ArtDirection/` — approved concept and real player capture, labeled separately.

Read [GAME_DESIGN.md](GAME_DESIGN.md) for mechanics, [ART_DIRECTION.md](ART_DIRECTION.md) for the production target, [CLAUDE_REVIEW.md](CLAUDE_REVIEW.md) for a reviewer brief, and [THIRD_PARTY.md](THIRD_PARTY.md) for asset sources. Keep Unity `.meta` files with their assets.

## Verification and remaining work

V4 compiled in Unity 6.3 LTS, a Windows player build completed, and an offscreen render from that player produced the linked capture. The V4 player smoke check passed blocked and safe world shifts, a midair shift, checkpoint respawn, and goal state. This does not verify that a person can comfortably traverse the full level. Mobile controls, mobile performance, polished character modeling and animation, authored environment assets, finished audio, and store requirements remain open.
