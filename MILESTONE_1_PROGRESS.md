# Milestone 1 — reliable traversal and recovery

Baseline: clean `main`, commit `613ddf6`. Implementation committed as `4030786` on `milestone-reliable-traversal`. Unity **6000.3.25f1**, URP, Windows x64. No levels, art assets, packages or production framework added. The licensing crash shown in the earlier screenshot was historical: a fresh Editor import and current builds succeeded during this task.

Follow-up to the user's Editor/player mismatch and failed first-checkpoint jumps: **the mismatch was real**. The shared courier controller referenced `__preview__` FBX subassets. Editor previews animated, but these clips were absent in the player; checking only the Idle state/name had missed that. All five states now reference actual runtime clips, and authoring/validation exclude preview clips. The first lesson also had insufficient beginner margin after the earlier gap repair: its Overgrown bridge is extended from z=13..16 to z=13..18, reducing the exit gap from 3.5 m to **1.5 m** while retaining the mandatory 7.5 m direct gap. Persistent checkpoint hints explain blue previews, Shift and held Jump. No motor strength or forgiveness was changed in this follow-up.

## Decisions and implementation

- **Support:** the old inset sphere reached approximately 0.359 m below the controller bottom. The new sweep ends at `min(groundProbeDistance, skinWidth + 0.02)`: **0.10 m** for the current controller. The radius inset and 0.04 m lift are explicitly accounted for. Walkable normal and descending motion are required. This deliberately retains contact forgiveness; it does not require literal mesh contact.
- **Jump intent:** retain 0.14 s coyote time and buffer. Held buffered input produces a full jump; releasing before execution produces a short jump using the existing 0.45 velocity multiplier. Expired input does nothing. Consumption clears the coyote window, preventing repeated ascent jumps. `JumpIntent.cs` owns this small, testable state; production `GameInput` supplies actual held state.
- **Landing:** one motor transition supplies impact speed, sequence and a small event. Ordinary post-move landings now produce squash/feedback and drive `RiggedCourierAnimator`; spawn/teleport establish support without an impact cue.
- **Gravity:** exact constant-gravity displacement preserves the nominal 1.85 m arc across render steps. A repaired semi-implicit baseline varied by approximately 13 cm between 30/144 Hz; the final fixture peak differs by less than 1 mm across the tested schedules. This measures arc consistency, not subjective polish.
- **Shift:** the CharacterController is authoritative; the probe synchronizes its pose, center, height and radius. Unit scale is validated. Every production call uses the same 0.16 s debounce, including rejected attempts. `Accepted/Blocked/Debounced/NotPlaying` results distinguish outcomes. Collision changes immediately before presentation; position and momentum are untouched. Destination penetration over 0.035 m rejects the switch. Full material arrays are preserved.
- **Retry:** all three checkpoints sit on shared roofs, so respawn **preserves the current world**, restores safe position and forward facing, clears motion/jump intent, and recovers the camera to an 18° pitch. Validation checks walkable shared support and capsule clearance in both worlds. Airborne trigger entry waits for support. Restart reloads the scene, resets time/checkpoints and starts Present.
- **Completion:** actual goal traversal stops motor velocity/intent, reframes from the stopped focus before freezing the camera, selects idle animation, and activates the existing completion UI and goal cue. Real motion capture exposed the previous frozen airborne framing losing the courier; the route test now checks viewport framing, idle state after blending, and a stationary completion camera. Existing audio remains placeholder/procedural; no listening-quality claim is made.
- **Maintenance:** refreshed the obsolete Runner prefab from the current authored courier through Unity's prefab API, retaining its GUID. It has local input and resolves the main camera at runtime. Golden remains the authoritative scene. Historical V2/V3 tools now clearly refuse missing source scenes rather than failing indirectly.

## Route evidence and correction

Earlier bypasses were reproduced with the actual controller. The original 4.5 m and 5 m direct gaps were crossed without Shift in sampled held-jump attempts. The left lookout also reaches the return route.

The two required lessons now have **7.5 m direct gaps**, implemented by moving downstream roofs and their existing art through Editor APIs, without invisible blockers or reducing jump capability. The first Overgrown bridge's entry/exit gaps are now 1.0/1.5 m; the later midair intermediate jumps remain 3.5 m. The measured maximum held gap in the isolated sweep is **6.75 m**, at all tested rates (0.25 m sampling, held/tapped input, edge/coyote delay sweep). This is sampled evidence, not an exhaustive reachability proof.

| Obstacle | Intended solution | Alternatives / verification |
| --- | --- | --- |
| Start → first landing | Ordinary held jump; first checkpoint | Tap timing is tested by motor fixtures; human margins still need playtesting. |
| First landing → alternate bridge → Shift landing | Grounded Shift to Overgrown, cross intermediate bridge | Direct no-Shift crossing failed in 50/50 tested attempts after correction. |
| Present takeoff → Overgrown destination → shared midair landing | Shift while airborne; shared landing/checkpoint | Direct no-Shift crossing failed in 50/50 tested attempts after correction. |
| Route fork → return → shared route landing | Right Overgrown route or left Present lookout | Left Present route is deliberately retained as a valid alternative, not a mandatory Shift lesson. Full regression exercises the right branch; left reach is sampled separately. |
| Final Present → final Overgrown → goal | Combined movement and midair Shift | Full scripted route reaches the actual goal and checks stopped completion motion. Other mastery shortcuts are not exhaustively excluded. |

## Verification performed in this task

| Check | Result / evidence |
| --- | --- |
| Fresh Unity import | Exit 0; `Logs/milestone1-baseline.log`. |
| Editor maintenance + measured geometry repair | Saved current scene/prefab through Unity APIs; scene validation passed. |
| Current Windows x64 build | Passed; `Logs/milestone1-final-build.log`, output `UnityProject/Builds/WindowsPolished/Shiftbound.exe`. |
| Pure jump-intent checks | 11 assertions against the actual production source; `Tools/JumpIntentChecks`. |
| Standalone motor regressions | Passed at 30/60/120/144 Hz and repeating variable intervals; held/tap, release-before-buffered-landing, expiry, coyote boundaries, no ascent repeat, 0.15 m rejected/0.06 m accepted support, ceiling, edge, 25° walkable/70° rejected slopes, landing impact/squash. |
| Input/Shift/recovery regressions | Passed: virtual keyboard through production InputActions, accepted/blocked/debounced/paused Shift, nonzero position/velocity continuity, two material slots, actual checkpoint entry, supported respawn/facing/world preservation in both worlds. `Logs/milestone1-final-regression.log`. |
| Smoke | Passed; production debounce respected. `Logs/milestone1-final-smoke.log`. |
| Reach audit | Required lessons: 0/100 total sampled no-Shift bypasses. Intentional left route still reachable. `Logs/milestone1-final-reach.log`. |
| Full route | Passed at all five schedules: all 12 legs, three real checkpoint triggers and actual goal; no direct completion call. Completion viewport, blended Idle state and frozen camera checks also passed. `Logs/milestone1-final-route-{30,60,120,144,0}.log`. Rate 0 cycles 1/30, 1/120, 1/60, 1/144 s. |
| Production input in the Windows player | Rendered full routes passed at requested 30/60/120/144 FPS caps using a virtual gamepad through real `GameInput`, motor `Update`, camera-relative movement and real Shift input. Actual intervals are logged in `Logs/milestone1-production-route-*.log`; these are input/traversal tests, not performance benchmarks or human playtests. |
| Unity Play mode comparison | Complete normal-input routes passed in **both GoldenRooftops and SkylineRooftops**, using the actual Editor runtime; logs `Logs/parity-editor-{GoldenRooftops,SkylineRooftops}.log`. Both scenes have the same speed/jump/gravity/coyote/buffer and 8 m camera distance; layouts and art differ. Actual goal captures are retained beside the player captures. |
| Current D3D11 player renders | Actual Present, Overgrown and goal stills in `ArtDirection/Milestone1`; no concept images. Scripted camera frames are separate diagnostics, not human playtests or frame-rate measurements. Renderer reports AMD Radeon RX 7600; no performance profiling was performed. |

These are custom standalone PhysX/Input System regressions and source assertions, not a claim that a Unity Test Framework suite was run. Logs/builds/raw recordings are local ignored artifacts. Historical logs are not included as new test results.

Diagnostic failures are retained in `Logs/parity-*.log`: the initial accelerated normal-input harness advanced gameplay faster than unscaled debounce time; the initial batch Editor harness lacked real frame pacing and later stalled on `WaitForEndOfFrame`, which the batch Editor does not emit. These harness defects were fixed with real elapsed-time pacing, test-only cloned background-input settings, and explicit camera rendering. Final successful Editor runs exited normally. No user's Editor was forcibly closed; only an owned, stalled batch diagnostic was stopped.

## Supported workflow and exact commands

Edit **Assets/Shiftbound/Scenes/GoldenRooftops.unity directly** in Unity 6000.3.25f1. Preserve GUIDs and existing V3/V4/V5 layers. Do not regenerate it from Skyline or historical generators. Use **Shiftbound > Validate Golden Rooftops**, then **Shiftbound > Build Windows Playable Slice**. **Milestone 1 > Maintain Current Golden Scene** refreshes supported checkpoint anchors/probe/prefab; inspect deliberate anchor edits before rerunning it. The measured geometry tool is guarded and idempotent for this baseline; it is not a general level generator.

From the repository root in PowerShell, close any Editor using this project, then run:

```powershell
dotnet run --project Tools/JumpIntentChecks/JumpIntentChecks.csproj
.\Tools\VerifyMilestone1.ps1 -Build
& .\UnityProject\Builds\WindowsPolished\Shiftbound.exe
```

The verifier checks process exit codes and required pass markers, and fails on compiler/regression errors. Omit `-Build` only when intentionally testing an already-current local player. Run the executable without diagnostic flags for normal input, or double-click `Play-Current-Shiftbound.cmd` at the repository root. Testing the saved Golden scene directly in Unity remains supported. WASD/arrows move; Space jumps; Shift switches; hold right mouse to look; Escape pauses; R restarts. Gamepad bindings remain unchanged.

For a scripted rendered route (silent, excludes IMGUI HUD):

```powershell
& .\UnityProject\Builds\WindowsPolished\Shiftbound.exe -batchmode -force-d3d11 -shiftboundCompareRun ArtDirection/Milestone1/goal.png -shiftboundProductionInput -shiftboundStepRate 60 -shiftboundMotionFrames .validation/milestone1-motion -logFile Logs/milestone1-motion.log
.\Tools\PackMotionCapture.ps1 -Frames .validation/milestone1-motion -Output .validation/milestone1-route.avi
```

## User-video follow-up — 2026-10-08

Inspected the supplied 32-second WhatsApp recording using Windows video decoding and extracted frames. The recorded failed attempts after the first checkpoint remain in **Present**; the next blue roof is an inactive Overgrown preview. This is evidence of a failed teaching/communication gate, not proof that Jump input failed. The footage does not reveal physical key presses or establish whether a Shift key was attempted. The direct Present crossing is intentionally unavailable; the intended solution is Shift on the shared checkpoint roof, then jump via the solid Overgrown bridge.

Added a prominent contextual keyboard/gamepad instruction at that checkpoint: make the bridge solid before jumping. It persists through notifications, changes to held-jump crossing guidance when Overgrown is active, dismisses at the shared landing's leading edge, and reappears on retry. Its endpoint is an authored scene reference saved through the Editor API; other checkpoints clear it. No movement, collision, world-switch timing, or geometry changes in this follow-up. Full-route regressions now assert guidance before the exit and dismissal afterward. Human understanding and small-window legibility still need confirmation; scripted traversal cannot establish either.

Follow-up verification: Editor authoring/scene validation passed (`Logs/jump-video-lesson-authoring.log`); final Windows build passed (`Logs/jump-video-verified-build.log`); 11 source jump-intent assertions and standalone motor/input/Shift/recovery/smoke/reach checks passed. Full deterministic routes passed at 30/60/120/144 Hz and variable intervals (`Logs/milestone1-final-route-*.log`). Final rendered normal-input routes passed at all four requested caps (`Logs/jump-video-production-{30,60,120,144}.log`), including the real checkpoint, guidance lifecycle, bridge and actual goal. `git diff --check` passed. The initial normal-input run failed because the new guidance assertion preceded the physics checkpoint callback; the test now waits for that real callback (bounded to three physics ticks), without direct checkpoint calls. This failure is retained in `Logs/milestone1-production-route-30.log`; final pass logs are separate. No new HUD screenshot or human playtest was captured; instruction legibility/understanding and physical Shift key use remain manual checks.

Run the freshly rebuilt `UnityProject/Builds/WindowsPolished/Shiftbound.exe`, or open the saved Golden scene in Unity. At the first checkpoint, press Shift while on shared support, confirm **OVERGROWN** and a solid bridge, then cross it in two smaller jumps. Confirm prompt dismissal at the shared landing and restoration when retrying from that checkpoint. Next bounded work is improving the preview/solid visual distinction if this lesson still confuses a human player, before advancing movement/camera tuning.

Second recording (8 seconds, `WhatsApp Video 2026-10-08 at 20.25.34.mp4`) shows the updated guidance and Present-world failed crossings. The user confirmed they were using Space alone and had not pressed Shift. This attempt therefore does not reproduce a Shift rejection or a failed Overgrown bridge jump. Explained the separate controls: press/release Shift on the shared checkpoint roof, confirm Overgrown, then jump via the bridge with Space. No production movement, scene geometry or input-binding changes were needed for this report.

Added missing keyboard-specific Shift coverage to the opt-in regression: actual checkpoint support, left and right Shift with W+Space, normal WorldSwitcher.Update, and exactly one accepted toggle while holding the key past cooldown. Fresh Windows build and all standalone traversal regressions passed (`Logs/jump-video-keyboard-build.log`, `Logs/jump-video-keyboard-regression.log`). This uses a virtual keyboard through the production Input System; physical keyboard/OS behavior is not proven. No new quality rating or human successful crossing is claimed.

## Open quality gates and next tranche

No current compile/build blocker remains. The deterministic route still isolates motor steps; additional production-input routes now retain motor Update, live camera and camera-relative controls. Both use scripted steering and held jumps, so neither proves human timing, fun, free-look comfort or complete physical gamepad behavior. Sampled rendered frames cannot establish full foot-contact/blending quality, every camera wall interaction or audio synchronization. No profiler session, hardware target or FPS improvement is claimed.

Manual gate before adding content: play the complete route with keyboard and gamepad; inspect camera at roof edges/walls and during Shift; tap/hold/buffer jumps; fall after each checkpoint in both worlds; verify respawn framing; finish and confirm idle/camera/UI/goal sound; pause/restart; inspect art alignment at relocated roof boundaries. The missing runtime animation/standing-pose defect is repaired, but character motion remains basic, vegetation primitive and nearby construction/materials inconsistent. Golden's photographic backdrop and foreground still clash; Skyline is a legitimate visual comparison, not a lesser version by assertion.

**Next bounded milestone:** tune acceleration, braking, reversals, air control and landing framing together in this existing route, then improve actual-speed locomotion, turning/falling and impact blending using verified available clips. Follow with foot-contact/surface audio; coordinated immediate Shift presentation; one approved 20–30 s visual benchmark; extend that quality through the existing slice; finally settings/navigation/accessibility and measured standalone optimization. Resolve old/V4/V5 equipment overlap, texture response, lit vegetation, Volume configuration and skyline coverage during the benchmark, with exact asset requirements. No additional levels before these gates.
