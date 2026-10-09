# Actual Milestone 1 player captures

`present.png`, `overgrown.png` and `goal.png` were rendered from the current Windows x64 player using D3D11 at 1280×720. They use the existing game assets, lighting and gameplay camera. These are actual game renders, not concept images, and exclude the IMGUI HUD.

`editor-Golden-goal.png` and `editor-Skyline-goal.png` capture the actual Unity Play-mode goal state after each complete production-input route. The corrected shared controller uses real animation clips rather than Editor-only previews.

The local `.validation/milestone1-route.avi` packages scripted gameplay-camera frames at 20 frames/s (one frame per three updates from a production-input run capped at 60 FPS, plus completion frames). Actual source frame intervals vary; playback is for inspection, not timing analysis. It is silent. It does not prove human comfort, audio quality or measured performance. See `MILESTONE_1_PROGRESS.md` for commands, tests and remaining gates.
