# Review brief for Claude or another collaborator

Please inspect this Unity project as a working prototype, then give a candid, actionable review. The user wants the **second concept image** in `ArtDirection/visual-target-gameview-v2.png` to define the art direction. `ArtDirection/SkylineRooftops-editor-preview.png` shows the actual current Unity scene. Do not describe the concept as implemented.

Start with `README.md`, `GAME_DESIGN.md`, and `ART_DIRECTION.md`. Inspect the latest `UnityProject/Assets/Shiftbound/Scenes/SkylineRooftops.unity`, relevant scripts and editor generators, Unity package versions, and third-party license notes. Older scenes are historical prototypes.

Please report:

1. The biggest visual differences between the current Game view and approved concept, in priority order.
2. A realistic production plan for **one excellent playable rooftop section**: character sourcing/modeling, rig and animation, environment assets, material and lighting setup, skyline, UI, and sound.
3. Which existing mechanics and code are sound, which need correction, and where player feel or world-switch safety needs testing.
4. A specific asset pipeline with licenses and mobile performance constraints. Distinguish assets you can produce in code from assets needing 3D art work. Avoid claiming a 2D reference can automatically become matching 3D gameplay.
5. The smallest next milestone that can be validated in Unity with a real in-game capture and short gameplay video, plus acceptance criteria.

If you change code or scenes, preserve existing work and verify compilation. Say plainly what you tested and what you could not test. Do not call the game store ready until it runs and is evaluated on target devices.
