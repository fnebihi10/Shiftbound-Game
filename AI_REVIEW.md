# AI project review — 2026-10-09

Reviewer: OpenAI Codex in the local coding session, published at the owner's request. This is an AI-authored assessment, not a human approval or a completed GitHub bot review. Implementation reviewed: `4030786`, based on `613ddf6`.

## Findings

### P2 — The verification command can certify an outdated player

[`Tools/VerifyMilestone1.ps1`](Tools/VerifyMilestone1.ps1), lines 25–38, builds only when `-Build` is supplied. Otherwise it runs whatever executable is already in `WindowsPolished` and prints a milestone verification success message without establishing that it corresponds to the current source. Editing a script after the last build, including introducing a compile error, does not invalidate these binary tests. It also never executes `Tools/JumpIntentChecks`, so the exact input-memory boundary assertions require a separate command.

Recommendation: distinguish verification of an existing binary from verification of current source; make the latter rebuild or validate a recorded source fingerprint. Include the source-level jump-intent command in the complete verification path. Existing binary verification can remain useful when its provenance is stated.

### P2 — The documented headless build is not reproducible in this environment

[`Tools/VerifyMilestone1.ps1`](Tools/VerifyMilestone1.ps1), line 26, always builds with `-nographics`. A fresh attempt in this review failed on Package Manager IPC inside the sandbox, then failed outside it with exit 198 and `com.unity.editor.headless` / no valid license. An otherwise equivalent `-batchmode -quit -force-d3d11` build outside the sandbox passed, including scene validation.

Recommendation: expose a build graphics option and document the tested graphics-enabled fallback for this Windows setup. Do not treat this as a compilation failure or assume every Unity installation has the same licensing restriction. From the repository root, run the Editor build below, then `Tools/VerifyMilestone1.ps1` against that newly built player:

```powershell
& 'C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe' -batchmode -quit -force-d3d11 -projectPath "$PWD/UnityProject" -executeMethod SliceDelivery.BuildWindows -logFile "$PWD/Logs/github-review-build.log"
```

### P2 — Animation validation does not require all five runtime states

[`UnityProject/Assets/Shiftbound/Editor/SliceDelivery.cs`](UnityProject/Assets/Shiftbound/Editor/SliceDelivery.cs), lines 86–99, rejects preview clips and validates upright Idle, but permits missing Jog, Sprint, Jump or Land states and permits null motions on those states. Removing Jump or clearing its motion therefore bypasses this part of the validation even though `RiggedCourierAnimator` still requests it. The route harness checks the final Idle clip; it does not assert a valid clip in every movement state.

Recommendation: require all five expected states and their intended non-preview runtime clips before building. Verify playback during movement as well as completion. The current checked-in controller has references for all five states; this finding concerns the incomplete regression guard, not a claim that those current references are broken. The validator gap was established by code inspection; no destructive controller mutation was performed.

## Gameplay and architecture assessment

The extracted `JumpIntent` has a narrow responsibility and tests production source directly. The motor accounts explicitly for support tolerance, preserves short versus held buffered jumps, consumes the coyote window, and integrates constant-gravity displacement. World switching uses the controller capsule, retains material arrays and shares a debounce contract for accepted and blocked attempts. Checkpoint validation checks shared support and clearance in both worlds. The route harness exercises real checkpoint and goal callbacks, with an additional mode using production input and camera updates.

These are useful foundations for the current rooftop prototype. This review found no confirmed P0/P1 defect in the inspected paths. That is not an exhaustive safety or reachability proof. Fixed-size camera collision queries, free-look near walls, pause/resume input timing and physical device behavior deserve manual or targeted follow-up coverage. The route driver supplies scripted steering and cannot establish player comfort or lesson comprehension.

## Visual assessment

Compared the existing [approved concept](ArtDirection/visual-target-gameview-v2.png) with the actual [Present](ArtDirection/Milestone1/present.png) and [Overgrown](ArtDirection/Milestone1/overgrown.png) captures. These captures omit the IMGUI HUD, so they cannot verify the new instruction panel's legibility.

| Remaining gap | Visual impact | Relative effort | Next action |
| --- | --- | --- | --- |
| Repetitive foreground facades, flat roof edges and simple equipment | High | Medium to high | Author a small modular facade/roof/prop kit for the opening section; preserve traversal colliders. |
| Courier silhouette, clothing detail and motion quality | High | High | Refine or replace the character and inspect skinning, feet, turns and jump/landing blends in video. |
| Sparse, stylized foreground plants compared with the lush concept | High | Medium to high | Add varied rooted clusters and deliberate foliage placement with a measured transparency budget. |
| Contact lighting and foreground/background consistency | Medium to high | Medium | Tune material scale, contact shadows and exposure around the real gameplay camera. |
| Distant city uses a camera-facing matte | Medium in these forward stills; other angles unverified | High for a spatial replacement | Inspect a complete orbit before choosing a panoramic/impostor or modeled solution. |

The current renders do not meet the concept's character and foreground quality. A 2D skyline image does not establish an explorable city. Retain asset provenance records in `THIRD_PARTY.md`; this review inspected those records but did not re-audit external licenses. Mobile performance, touch controls and device builds remain unverified.

## Verification

- `dotnet run --project Tools/JumpIntentChecks/JumpIntentChecks.csproj`: passed, 11 assertions against production source.
- Fresh Unity 6000.3.25f1 Windows build with graphics enabled: passed, including `SliceDelivery.ValidateScene`; local log `Logs/github-review-build.log`.
- `git diff --check`: passed before publication.
- Player regression, smoke, reach audit, deterministic routes at 30/60/120/144 Hz and variable intervals, and rendered production-input routes at 30/60/120/144 FPS caps: all passed again against the completed fresh build using `Tools/VerifyMilestone1.ps1`. Requested caps are not performance measurements.

Local logs and generated players remain ignored artifacts. Historical tests described in `MILESTONE_1_PROGRESS.md` are distinct from checks rerun for this review. No human playtest, physical controller/keyboard test, audio listening assessment, profiler session or mobile validation was performed here.

## Smallest next milestone

Complete one opening rooftop and its first Shift lesson before extending content. Acceptance: a first-time player understands when to make the blue bridge solid; an actual HUD-inclusive capture remains readable at the supported minimum window size; keyboard and gamepad users traverse, fall, respawn and finish; a short real gameplay video shows credible character motion and collision-safe free-look; all existing source/build/traversal checks pass. Establish hardware and measured frame-time targets before making performance claims.
