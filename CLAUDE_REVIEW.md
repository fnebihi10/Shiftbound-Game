# Review brief for Claude or another collaborator

Please inspect this Unity project as a working prototype. The user wants the **second concept image** at `ArtDirection/visual-target-gameview-v2.png` to define the visual direction. `ArtDirection/GoldenRooftops-player-capture.png` is an actual V4 Windows-player render. Do not describe the concept as implemented or assume the distant matte is an explorable 3D city.

Start with `README.md`, `GAME_DESIGN.md`, `ART_DIRECTION.md`, and `THIRD_PARTY.md`. Inspect the latest `UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity`, runtime scripts, editor generator, packages, and third-party assets. Older scenes are prototypes kept for comparison.

Please report:

1. The largest remaining differences between the actual V4 player image and the approved concept, ranked by visual impact and implementation cost.
2. A realistic production plan for **one excellent playable rooftop section**: character model, rig and animation, environment assets, materials, lighting, skyline, UI, and sound.
3. Which gameplay code is sound, which parts need correction, and what player-feel or switch-safety tests are still missing.
4. A practical asset workflow with clear licensing and Android performance limits. Distinguish code-produced work from 3D art work. A 2D concept cannot automatically become equivalent 3D gameplay.
5. The smallest next milestone with acceptance criteria based on a real Game-view capture and short gameplay video.

If you change scenes or code, preserve existing work and verify compilation. State precisely what you tested and what you could not test. Do not call the game store ready until it runs and is evaluated on target devices.
