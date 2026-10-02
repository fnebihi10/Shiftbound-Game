# Shiftbound

Current branch work and exact verification limits are recorded in [SLICE_AUDIT.md](SLICE_AUDIT.md). The checked-in Windows build is older than the current source. With an active Editor license, use **Shiftbound > Validate Golden Rooftops** followed by **Shiftbound > Build Windows Playable Slice** for a new build.

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
- `UnityProject/Assets/Shiftbound/Editor/BuildArtPassV4.cs` — V4 scene generator. It creates `GoldenRooftops` from V3 and will overwrite manual edits to that generated scene; save a new scene before running it after manual work.
- `UnityProject/Assets/Shiftbound/TexturesV4/` and `MaterialsV4/` — paired city images and licensed surface materials.
- `ArtDirection/` — approved concept and real player capture, labeled separately.

Read [GAME_DESIGN.md](GAME_DESIGN.md) for mechanics, [ART_DIRECTION.md](ART_DIRECTION.md) for the production target, [CLAUDE_REVIEW.md](CLAUDE_REVIEW.md) for a reviewer brief, and [THIRD_PARTY.md](THIRD_PARTY.md) for asset sources. Keep Unity `.meta` files with their assets.

## Verification and remaining work

V4 compiled in Unity 6.3 LTS, a Windows player build completed, and an offscreen render from that player produced the linked capture. The V4 player smoke check passed blocked and safe world shifts, a midair shift, checkpoint respawn, and goal state. This does not verify that a person can comfortably traverse the full level. Mobile controls, mobile performance, polished character modeling and animation, authored environment assets, finished audio, and store requirements remain open.
