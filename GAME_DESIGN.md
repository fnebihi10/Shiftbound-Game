# Shiftbound - game design, first playable slice

Status: SkylineRooftops V3 scene and Windows test build generated. Automated checks cover core world switching and flow; the user has opened and played the scene in Unity and found that the graphics remain far below the approved concept. Full interactive route and Android testing are pending. Working title, tuning values, and visuals may change after player tests.

## Player experience

Shiftbound is a responsive third-person platform game for short sessions. Players cross one contemporary rooftop that has two overlapping versions: a clean, illuminated present and an overgrown alternate state. Shared landmarks stay aligned. Shifting changes which route surfaces are solid, letting players solve movement puzzles in the same physical place.

The first slice teaches the controls without a long tutorial. A new player should reach the goal in roughly 2-4 minutes; a learned route should take about 60-90 seconds. Fast retry is part of the feel.

## Movement and camera

- Camera-relative movement and responsive turning.
- Variable jump height, coyote time, and a short jump input buffer.
- Reliable ground checks and clear takeoff and landing feedback.
- Third-person camera that keeps landing spots visible and moves in front of walls rather than clipping through them.
- Keyboard and gamepad through Unity's Input System for the Editor prototype. Add touch input before calling the Android version playable.
- Expose movement, jump, camera, and feedback values in the Inspector for tuning.

## World-switch rules

- A single Shift action swaps the active world on the ground or in midair.
- Shared structure and landmarks remain solid in both worlds. World-specific route surfaces collide only in their active world. Inactive route surfaces receive a subtle visual preview and never obstruct movement.
- A successful shift preserves player position and velocity.
- Before changing collision state, test the player's full collision shape against destination-world geometry. If the destination is blocked, keep the current world and provide immediate visible and audible rejection feedback.
- A valid midair shift may leave the player above empty space. The player can then fall and respawn at the latest checkpoint. A switch must never embed the player in a wall.
- Use static platforms for this first slice. Moving-platform behavior is outside this prototype.
- A short input debounce may prevent accidental double shifts, but it must not make the mechanic feel delayed.

## First level sequence

| Area | Purpose |
| --- | --- |
| Safe starting rooftop | Learn movement, camera, and jump with no fall hazard. |
| First gap and checkpoint | Test jumping; recover quickly. |
| Obvious alternate bridge | Show both routes, teach a safe grounded Shift. |
| Midair bridge swap | Require one readable Shift during a jump. |
| Small route puzzle | Choose between two visible routes; prevent a simple repeated-switch pattern. |
| Final combined crossing | Combine jump timing, route reading, and a midair Shift. |
| Goal rooftop | Show completion, time, retry, and next-step feedback. |

Use frequent checkpoints and a quick respawn. Obstacles should teach one idea, let the player practice it, then combine it with movement.

## Visual and audio direction

- Present world: clear contemporary rooftop materials, clean geometric silhouettes, cool lighting.
- Alternate world: the same rooftop with overgrowth, warmer accent lighting, changed surface shapes, and organic silhouettes.
- Keep important landmarks aligned so the place feels transformed rather than replaced.
- Make active surfaces readable through shape and material as well as color. Preview inactive surfaces near the route without visual clutter.
- Use restrained switch effects, original or properly licensed sound, distinct footsteps and landing feedback, and a clear goal reveal.
- Favor stable frame rate and visual coherence over expensive effects.

## Slice scope

Include one level, movement, camera, shifting, checkpoints, fast respawn, pause, restart, a compact controls display, and a completion screen. Build layout from simple geometry first, then make one focused visual and audio pass.

Exclude additional levels, level editor, accounts, online sharing, leaderboards, ads, purchases, and a large game framework. Those can be evaluated after testing the slice.

## Build and verification order

1. Inspect the installed Unity version, project template, render pipeline, and packages.
2. Create the project in this folder and establish source control.
3. Implement movement and camera. Test in an empty room.
4. Implement safe switching and blocked-switch feedback. Test on the ground and midair.
5. Block out the full first level with checkpoints and goal.
6. Add pause, restart, controls display, and completion screen.
7. Polish readability, animation cues, sound, lighting, and performance.
8. Verify compilation and play the whole path. Build and test Android when modules and a test device are available.

After each increment, record what changed, required Editor steps, and exactly what was tested. Do not report Unity compilation or gameplay as tested until those checks actually run.

## Player validation

First ask 5-10 people to play without coaching. Watch for confusion about controls, active surfaces, and Shift timing. Observe whether anyone voluntarily replays to improve a run. Revise the mechanic and level before creating a large content set if the first slice is unclear or not enjoyable.



