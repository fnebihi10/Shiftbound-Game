# Shiftbound

Shiftbound is a Unity 6.3 LTS / URP third-person rooftop platformer prototype. The player jumps between two versions of the same city and switches worlds to reach the goal.

**Current state:** The mechanics are playable, but the graphics and animation are an early prototype. The approved [visual target](ArtDirection/visual-target-gameview-v2.png) is concept art, not a Unity render. Compare it with the [actual Unity preview](ArtDirection/SkylineRooftops-editor-preview.png). The current scene does not yet meet the target's character, environment, lighting, or animation quality.

## Open the game

1. In Unity Hub, add the **UnityProject** folder inside this repository. Open it with **Unity 6.3 LTS (6000.3.25f1)**.
2. In Unity's Project panel, open **Assets > Shiftbound > Scenes > SkylineRooftops**. This is the latest scene and the one included in Build Profiles. Older scenes are kept for comparison.
3. Click the **Game** tab, press the **Play** button at the top, and click inside the Game view to focus input.
4. Press **Play** again to leave Play mode. Changes made during Play mode are not saved.

| Action | Keyboard / mouse | Gamepad |
| --- | --- | --- |
| Move | WASD or arrow keys | Left stick |
| Look | Hold right mouse button and move mouse | Right stick |
| Jump | Space | South button / A |
| Shift worlds | Shift | West button / X |
| Pause / resume | Escape | Start |
| Restart | R | Select / Back |

The first slice contains checkpoints, quick respawn, a blocked-shift check, a midair shift, a route puzzle, and a goal. See [GAME_DESIGN.md](GAME_DESIGN.md) for intended rules. The game currently targets Windows for testing. It does not yet have mobile touch controls or store-ready builds.

## Visual review and next work

Read [ART_DIRECTION.md](ART_DIRECTION.md) before changing the art. It separates the approved concept from the current scene and records the quality bar, gaps, and production order. A reviewer can use [CLAUDE_REVIEW.md](CLAUDE_REVIEW.md) as a concrete audit brief. The main goal is a convincing, cohesive 3D character and rooftop world that approach the approved image in a real Unity Game view, followed by movement and animation refinement. A still image alone cannot supply rigged geometry, animation, lighting, and a playable environment.

## Project layout

- `UnityProject/Assets/Shiftbound/Scenes/SkylineRooftops.unity` — latest playable scene.
- `UnityProject/Assets/Shiftbound/Scripts/` — player, camera, world switching, checkpoints, game flow, and animation scripts.
- `UnityProject/Assets/Shiftbound/Editor/` — scene generation and diagnostic tools. The build menus can regenerate scenes; save manual edits in a separate scene before using them.
- `UnityProject/Assets/Shiftbound/ThirdParty/` — selected Quaternius character and animation source assets. See [THIRD_PARTY.md](THIRD_PARTY.md).
- `ArtDirection/` — concept references and a real Unity preview, labeled separately.

Unity-generated `Library`, `Temp`, `Logs`, `Builds`, IDE files, and downloaded source archives are excluded from Git. Keep Unity `.meta` files with their assets; Unity uses them to maintain references.

## Verification

The V3 scene compiled in Unity 6.3 LTS and a Windows player build completed locally. A headless smoke run covered safe and blocked shifts, a midair shift, checkpoint respawn, and goal state. The user has run the scene in the Editor and confirmed that the visuals still fall well short of the approved concept. Full level traversal, visual quality, mobile controls, device performance, and store release requirements remain unverified.
