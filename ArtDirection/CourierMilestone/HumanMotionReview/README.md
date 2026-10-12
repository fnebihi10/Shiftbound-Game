Integrated anatomical motion revision

GoldenRooftops source `ca6371724296650a9bd0029588ca003314000af2b58e450e78b7784c7efbfdd2`, Unity 6000.3.25f1. Windows: `UnityProject/Builds/WindowsHumanMotionCandidate/Shiftbound.exe`. Android: `UnityProject/Builds/AndroidHumanMotionCandidate/Shiftbound-QA.apk`, SHA256 `27b03ce645b9e22132b2d529420d8c41d3f0ac085264b76b547d4cca094f444c`. Corresponding source archive: `.validation/CourierMilestone/Source-ca637172.zip`. Earlier integrated source 349493c8 and both references remain preserved.

Eleven action clips now use the anatomical courier's own rig rather than the older Hero rig. Shoe-weight geometry informs authored sole placement. Forefoot articulation and continuous push-off pitch are present. Jump refinement adds immediate visual compression, leading knee and trailing heel lift, unfolding through descent. Physics, input timing, jump height/distance and preferred original Jog remain unchanged. Authored gait is still an opt-in experiment. Only two actor components changed in the integrated scene; 6,182 other serialized blocks are unchanged.

Evidence here:

- `gameplay-with-audio.mp4`: integrated ca637172, actual elapsed-time gameplay, synthetic production input, two routes, fall/checkpoint recovery, retained retry and completion. Recorded game mix included. Recording overhead excludes performance claims.
- `matched-original-jog.mp4`: integrated ca637172 against corrected c69ca256, baseline left/candidate right, same 1280×720 framing/world/motor/input. Fixed simulation 60 Hz sampled at 30 Hz; silent visual comparison.
- `close-authored.mp4`, `close-authored-slow.mp4`: isolated source 6516a5eed15dbbfbfbf696820cca38cf6bd0e51547abd2090804ddf1518432b7, same inspected action assets subsequently copied selectively into main, experimental gait and close camera. Half-speed version is for inspection, not frame pacing.
- `scene-diff.json`, `android-inspection.json`: selective integration and produced artifact checks.

Actual Unity D3D12 Play Mode passes two routes, facing, fall/recovery and retained retry (`Logs/courier-motion7-unity-d3d12.log`). The invocation initially expected `PLAYMODE` rather than the actual `PLAY MODE` log marker: wrapper failure is a reporting typo, not a failed Editor run. Do not rerun an unchanged scene to hide this history. Current-source regression suite passes 11 jump and 18 touch assertions, 13 technical checks including production routes/facing at 30/60/120/144 and direct jitter (`Logs/courier-motion7-regression-suite.log`).

Selected close/gameplay frames were inspected. Continuous audiovisual listening/playback is unavailable in this tool interface; normal/slow footage is supplied for human temporal judgement. Imported action, technical route and build acceptance do not establish production quality.

| Area | Result | Remaining weakness / evidence |
|---|---|---|
| Courier production | FAIL | Neutral face/hair/material finish; pivot contacts and landing transitions still need refinement and temporal review. Jump now has clearer articulation, not yet independently accepted. |
| Foreground architecture/vegetation | FAIL | Major production milestone unfinished; preserve recovered world while courier work continues. |
| World coherence / Shift readability | NOT VERIFIED | Baseline preserved; no new production acceptance from route scripts. |
| Camera and movement quality | NOT VERIFIED | Technical traversal/facing passes; physical landing judgement remains open. |
| Mobile controls/UI | NOT VERIFIED | Router checks pass; Samsung owner feedback useful but not complete comfort/UI acceptance. |
| Encounter quality | NOT VERIFIED | Existing route passes; mastery/fairness/replay judgement remains open. |
| Audio quality | NOT VERIFIED | Game mix recorded; audible aesthetic review remains open. |
| Recovery/retry | PASS | Technical retained reset, checkpoints, failure recovery and completion verified; human pacing judgement remains open. |
| Sustained Samsung 60 FPS | NOT VERIFIED | 15-minute mixed diagnostic on 349493c8 includes idle/capture/restarts, not representative sustained gameplay. See ../SAMSUNG_SESSION_20261011.md. New ca637172 installed and embedded identity confirmed. |
| Android compatibility | FAIL | LOAD alignment, ZIP alignment and signature pass. Vendor main/C++ RELRO fails. 4 KB Samsung startup passes; real 16 KB runtime unverified. |

Next focused action: planted-foot yaw during abrupt turns and landing-to-run contact ramps. Keep main ca637172 preserved while evaluating the change in the isolated Work project. Then finish the remaining courier presentation weaknesses before expanding foreground work. No self-rating or zero-flaw claim.
