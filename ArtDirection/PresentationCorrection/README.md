# Presentation correction — 11 October 2026

The owner rejected the previous candidate after comparing actual play: WindowsPolished 6/10, upgrade 4/10. These are the owner's assessments, not an agent rating. Source `84c74f6b` fixed backwards facing but did not establish acceptable presentation. That candidate remains preserved under `.validation/BackwardsFix` and is superseded for presentation.

The comparison video was inspected in temporal samples covering Editor play and the preferred executable. The repeated facade detail crowded the near architecture. The modified jog weakened the running pose. The Editor also logged Mobile_RPAsset at 1101×506, uncapped, while the preferred executable uses the PC rendering profile. This profile difference does not invalidate the owner's review or prove the cause of the perceived smoothness difference.

GoldenRooftops now restores 1,512 original renderer references, meshes, materials and shadow settings from the preserved reference scene. The rejected facade/roof/shrub kit is removed from the playable scene, original roof texture treatment and deck materials restored. Its authoring assets and rejected source are preserved separately. Removal refuses any kit containing collision. Unsupported spherical growth proxies remain disabled. Gameplay transforms, collision, route, camera and user scene edits were not replaced wholesale. Original complete jog replaces the scaled-muscle clip; speed blending now uses walk/jog/sprint reference speeds 1.5/3/7 m/s instead of overcranking sprint against 5.2 m/s. Facing correction, contact correction, input ownership and retained retry remain.

Editor Play Mode defaults to the PC rendering profile and a 60 cap even with Android selected as the build target. `Shiftbound > Preview > Samsung Rendering Profile` explicitly enables the Mobile profile for rendering inspection. This preference affects the Editor only. Actual Android still uses the existing Mobile profile and 30/60 setting. Neither preview is evidence of Samsung frame pacing.

[Matched images](Matched) cover opening, exposed, close, rear and below views in both worlds at 1280×720. These are Editor renders without HUD; static humanoid sampling has a known facing difference from runtime, so use gameplay footage for facing. The preferred executable and previous evidence are untouched.

This revision corrects a rejected direction. It does not complete the requested major production upgrade.

| Area | Status | Evidence and remaining weakness |
|---|---|---|
| Courier and animation | FAIL | Original jog restored; backwards-facing regression checked. Original model/rig and static airborne poses still lack production quality. |
| Foreground architecture/vegetation | FAIL | Approved foreground recovered in matched views; major asset upgrade rejected. Close foliage and generic buildings remain baseline weaknesses. |
| World coherence and Shift | NOT VERIFIED | Matched worlds restore composition; route rules checked. Owner review of this revision pending. |
| Camera/movement responsiveness | NOT VERIFIED | Original motor/camera retained and automated route checks; satisfying physical play not established. |
| Mobile controls/interface | NOT VERIFIED | Synthetic routing checks cannot establish two-thumb comfort or visual finish. |
| Encounter quality/replay | NOT VERIFIED | Existing route, recovery and retry functional checks; production quality remains open. |
| Audio | NOT VERIFIED | Gameplay recording contains actual game audio; no listening acceptance claimed. |
| Recovery/retry | PASS | Technical state reset and checkpoint recovery in actual Unity Play Mode and candidate recording; perceived hitch acceptance remains separate. |
| Sustained Samsung performance | NOT VERIFIED | No ADB device detected during this revision. Desktop callbacks cannot establish presented FPS or thermal performance. |
| Android compatibility | FAIL | Vendor libmain.so and libc++_shared.so RELRO compatibility remain unresolved; runtime on 16 KB device not verified. |

Current branch is main, HEAD e7af46e0; changes remain uncommitted. Artifact identities, fresh gameplay and technical checks are recorded alongside this file after verification. No independent quality approval is claimed.

Final corresponding source: `c69ca2565508e48ee972307e86ef749091b9313c1e128aefb4bc5b21c1d8a69c`. [Build identities](build-identities.json) match current source, Windows and Android. Outputs: `.validation/PresentationRegression/FinalWindows/Shiftbound.exe` and `FinalAndroid/Shiftbound-QA.apk`; APK SHA256 `90e6bba37e593c5bba53141cb2523d7efe6b075b43101453f9b132d54dd825d3`. Portable Windows archive: `Shiftbound-PresentationCorrection-Windows.zip`; source archive: `Sourcec69ca256.zip`, under that same ignored root. QA 0.4.0/code 5, existing package/signing unchanged.

[Final gameplay with game audio](gameplay.mp4) records two completed runs, actual falls/checkpoint recovery twice and one retained retry. [Gameplay log](gameplay.log) verifies body facing on both runs: 1,302 samples, minimum dot 0.826. [Unity Play Mode log](unity-playmode.log) independently verifies two runs and recovery/reset; 1,446 facing samples, minimum dot 0.826. [Movement samples](movement-grid.png) and [quarter-speed excerpt](movement-slow.mp4) were inspected as temporal images. Human normal-speed play and listening acceptance remain open. All input is synthetic; capture/readback rate is not performance evidence.

The intermediate `35abf31e` revision passed the complete automated suite, including 11 jump-intent and 18 touch assertions plus 30/60/120/144 production/facing routes. Its source/builds/recording/tests remain preserved in the ignored root. Final source differs only in `PresentationCorrection.cs` and GoldenRooftops, removing the already-disabled visual kit; runtime scripts are identical. Final scene was independently checked in Unity Play Mode and recorded gameplay after removal. [Verification scope](verification.json) distinguishes those results.

[Desktop callback diagnostic](windows-callbacks.json): Ryzen 5600X/RX 7600, PC profile, D3D11, 1280×720, 60 cap, 721 callbacks over 12.075 s. Steady p50/p95/p99 16.664/16.812/16.942 ms. Startup p99 92.921 ms remains a weakness. GPU/render/GC counters unavailable; these are callbacks, not presented frames. Removing the kit reduced p95 Unity allocated-memory counter from 165,024,846 to 111,175,054 bytes in corresponding route checks. APK reduced from 69,037,718 to 56,931,174 bytes. This does not prove device frame pacing.

[Fresh Android inspection](android-inspection.json): all LOAD alignments, ZIP alignment and signing pass. Vendor libmain.so and libc++_shared.so RELRO fail; 16 KB runtime verification and installation/update remain NOT VERIFIED. ADB listed no device throughout this revision. Exact dependency remains a supported Unity Android runtime/module with corrected vendor binaries, followed by actual 16 KB runtime testing.

Next: assess this correction against the untouched executable in physical play. Refine actual courier stride/contact/airborne action and source-backed foreground assets before claiming an upgrade. Do not invoke the old `UpgradeProduction.ApplyEnvironment` or `Integrate`: they regenerate the rejected kit. Preserve the recovered visual baseline during that work.
