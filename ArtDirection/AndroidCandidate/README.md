# Android candidate evidence

Production source: `f23a3a78833bb907add9eee44be821a4e126f420`; raw Assets/Packages/ProjectSettings fingerprint `2bb476b1bfeab5a5e232f16847b8b948bf2114829b7b975ffe6dc129622e92bb`. Unity 6000.3.25f1, GoldenRooftops. See [acceptance and blockers](../../ANDROID_CANDIDATE.md) and [artifact identities](delivery.json). These files are committed review evidence; no remote upload or Play publication is claimed.

## Actual camera, HUD and audio

- [Full existing route](full-route.mp4): real elapsed time, production motor/input/camera, all three checkpoint callbacks and goal. Input is synthetic gamepad; it is not human touch play or enjoyment validation. Completion/pause IMGUI menu is outside the offscreen capture.
- [Orbit and recovery](orbit-and-recovery.mp4): continuous free orbit in Present and Overgrown, Shift, run/brake/reverse, jump, equipment approach and explicit respawn. Synthetic input; no physical controller claim.
- [16:9](phone-present-16x9.jpg), [19.5:9](phone-present-195x9.jpg), [20:9](phone-present-20x9.jpg): actual production phone Canvas in a Windows simulated layout. Cutouts, physical text size and thumb reach remain unverified.
- [Observed panorama seam](observed-panorama-seam.jpg): retained failed art evidence, visible during the Overgrown orbit. Sparse opening growth and later crude plants are also exposed in the videos.

Capture uses the shipping camera rendered into an offscreen texture, its actual camera-space phone Canvas and AudioListener PCM, encoded with original timestamps. [Route timestamps](route-times.csv) / [orbit timestamps](orbit-times.csv) document missed capture intervals; nominal capture target was 30Hz, without speeding up playback. Audio is 48kHz stereo; [route](route-audio.txt) and [orbit](orbit-audio.txt) PCM peaks show zero clipped samples. Subjective listening and gait sync remain NOT VERIFIED. Counter-Strike 2 stayed running by request. Readback/JPEG/audio recording changes load; these videos are not presentation-performance or input-to-photon evidence. Black/foreground-focus-failed recordings were rejected and are not included.

## Art attribution

Historical reviewed benchmark: [Present](../ProductionBenchmark/after-present-start.png) / [Overgrown](../ProductionBenchmark/after-overgrown-start.png). Those “after” names refer to the earlier benchmark, before this Android candidate.

Candidate controlled poses: [Present equipment](equipment-present.png) / [Overgrown equipment](equipment-overgrown.png), same saved scene/camera inspection setup used by the benchmark capture tool. These were captured before the final diagnostic-only correction; the equipment, courier and lighting assets are unchanged afterward. Construction replaces the original utility/blocked-Shift collision blocks and disables two overlapping visual shells. No route/collider change. Compare these fixed poses for art attribution; use the videos for camera/motion. This section does not establish whole-route premium acceptance.

## Correctness and Android packaging

[Verification entry-point result](verification.txt) proves source/binary correspondence before testing, 11 production jump assertions, 18 production router assertions, motor/Shift/recovery/reach cases, 10 real Input System synthetic touch cases and deterministic full routes at 30/60/120/144/variable dt plus production-input routes at 30/60/120/144. [Selected case markers](correctness-cases.txt) retain substantive outcomes. These Windows checks do not certify Android execution.

[APK inspection](android-inspection.json), [native headers](native-headers), [permissions](android-permissions.txt), [badging](android-badging.txt), [signature](android-signature.txt) and bundle inspection document the actual artifacts. The APK signs/verifies and ZIP/LOAD alignment passes. Five native libraries fail the RELRO end alignment check: `libc++_shared.so`, `libgame.so`, `libmain.so`, `libswappywrapper.so`, `libunity.so`. `libil2cpp.so` and Burst pass in this final build. No phone install/update/runtime test was available. Native page compatibility and Play preparation therefore FAIL; debug-signed bundles are QA artifacts.

## Windows diagnostic limits

All conditions: source above, RX7600 driver32.0.31041.1004, Ryzen5600X/32GB, D3D11, hidden inactive window, 1280×720, PC render scale1, VSync0, display240Hz. CS2 remained running. First three seconds are separated as startup. Warning rate is per callback count/duration, not presented frames.

| Condition | Steady p50 / p95 / p99 callback ms | Presentation warnings |
|---|---|---|
| [60 cap / timing on](FinalDiagnosticTimingOn.json) | 16.667 / 16.804 / 16.993 | 0 |
| [60 / FrameTiming off](FinalDiagnosticNoTiming.json) | 16.670 / 16.803 / 17.068 | 0 |
| [60 / post off](FinalDiagnosticNoPost.json) | 16.666 / 16.806 / 16.911 | 0 |
| [60 / attempted recording](FinalDiagnosticRecorded.json) | See JSON; black capture rejected | 0 |
| [30 cap](FinalDiagnostic30.json) | 33.340 / 34.657 / 37.607 | 0 |

The inactive window can skip drawing: render/GPU/draw counters were unavailable, and normal ScreenCapture returned black. GC allocation was unavailable; memory counters and main/CPU timings are in JSON. Callback similarity with FrameTiming on/off does not quantify render/GPU instrumentation overhead. Recording adds readback load but did not establish equivalent presentation. These conditions cannot explain historical warnings, establish smooth rendering, or pass any Android performance gate. No suppressions or speculative rendering fix were applied. A foreground, repeatable presentation A/B with warning timestamps and no competing workload is still needed.

[Software input probe](software-input.json): seven synthetic EnhancedTouch event timestamps reached PlayerMotor.Update; p50 16.6402ms, p95/p99 27.6108ms. Small Windows diagnostic only, with competing load. This excludes physical scan, display and photon delay. No hardware latency method or named Android performance result was available.

## Reproduce and device handoff

Install the same Unity Editor/modules, activate its license, then run `Tools/BuildCandidate.ps1 -Target Windows`, `-Target Apk` and `-Target Bundle`. Builds use the working batchmode/D3D11 Editor path, fresh source manifests, IL2CPP ARM64/API36 and local debug signing. Build output is under `UnityProject/Builds/`; raw captures/logs remain ignored under `.validation/` and `Logs/`.

With .NET10 installed, restore `Tools/JumpIntentChecks/JumpIntentChecks.csproj` and `Tools/TouchInputChecks/TouchInputChecks.csproj` against an empty local package source (neither uses NuGet packages), then run `Tools/VerifyMilestone1.ps1` (fresh build default), or `-ExistingBinary` (strict source/binary hashes). The touch project references the installed Unity CoreModule; override `UnityEditorRoot` when installed elsewhere.

`Tools/RecordExperience.ps1 -Output ABSOLUTE_DIR -Scenario Orbit -Seconds 25` or `-Scenario Route -Seconds 90` captures actual offscreen HUD/audio and checks current-source correspondence. `Tools/ProfileWindowsGameplay.ps1` reproduces the bounded Windows diagnostic, with `-NoTiming`, `-NoPost`, `-Rate 30` or `-Record`; empty hidden-window capture is explicitly rejected.

Use `Tools/InspectAndroidArtifact.ps1` and `Tools/InspectAndroidBundle.ps1` to rerun native/package/signature checks. Obtain bundletool from [Google's official instructions](https://developer.android.com/tools/bundletool); the checked tool SHA is recorded with bundle inspection. A failed native check must remain a failure. Follow [physical QA, five-player study and Play handoff](../../Tools/ANDROID_QA_PROTOCOL.md) and [store/privacy draft](../../Tools/STORE_PRIVACY_DRAFT.md). No signing credentials are stored in source.
