# Backwards-running correction — 11 October 2026

**SUPERSEDED FOR PRESENTATION:** the owner subsequently rejected this 84c74f6b candidate as worse overall than WindowsPolished. Facing repair remains valid; overall quality acceptance was not established. See [the presentation correction](../PresentationCorrection/README.md) for current corresponding builds, evidence and unresolved criteria.

The owner's manual video exposed a real regression in the previous candidate. I introduced a 180-degree local rotation using an imported chest bone axis as evidence of model facing. That inference was wrong for the evaluated Humanoid runtime. Route completion tests did run, but did not verify anatomical facing; the prior visual verification was insufficient. The previous 731a0001 candidate is rejected.

## Repair and evidence

`GoldenRooftops` now uses identity local rotation for the courier under its motor heading. `UpgradeProduction.ApplyCourier` also uses identity, so authoring cannot reintroduce the wrong rotation. The pack remains attached to the rig. Movement/input timings are unchanged; owner-saved scene changes were retained.

`CourierFacingRegression` derives the anatomical front from animated upper-arm positions and the hips/head axis. It compares that front against resolved velocity when the motor heading has settled. It does not mistake the motor's own heading or an imported bone axis for visual facing. The check is opt-in and is used by Unity Play Mode verification, production-input route checks and production recordings.

- `reproduction.log`: original rotation reproduces the failure in actual Unity Play Mode; body/travel dot -0.896 at 3.05 m/s. The Editor check exits with failure.
- `unity-playmode.log`: repaired scene, two full production-input routes including reversals, jumps, actual falling/checkpoint recovery, completion and retained retry. Anatomical facing passes on 1,453 evaluated frames; minimum body/travel dot 0.843.
- `repaired-gameplay.mp4`: fresh continuous Windows camera/HUD/game audio recording with both routes, recovery and retry. Anatomical facing passes on 1,281 evaluated frames, minimum dot 0.847. These are software input tests, not physical touch or sustained performance evidence.
- `movement-grid.png`, `run-and-jump-slow.mp4`: temporal inspection material. The courier shows its back/pack while moving away from the camera and turns toward actual travel. This fixes the reported orientation failure; it does not accept overall animation quality.
- `before-after-facing.png`: rejected source on the left, repaired source on the right; both are moving away from the camera at run-clock 4.8 seconds. The incorrect front-facing model is replaced by the correctly rear-facing model and pack.

## Replacement artifacts

Source: main/e7af46e plus uncommitted changes; canonical fingerprint `84c74f6bb04571ef455bd3d9995054f9e5aade35f3f758772b7c8319fd51ce7e`. Full manifests live beside the artifacts.

- Windows: `.validation/BackwardsFix/Windows/Shiftbound.exe`.
- Android: `.validation/BackwardsFix/Android/Shiftbound-QA.apk`.
- Portable Windows: `.validation/BackwardsFix/Shiftbound-Windows-FacingFix.zip`; source snapshot: `.validation/BackwardsFix/Source84c74f6b.zip`.
- `build-identities.json`: Windows, Android and current source match. APK SHA256 `7ff53733de8d762e482c3f0bcf3b4f9d70018dd3606a72ef4b867c6b1efa32d7`.
- `android-inspection.json`: fresh APK LOAD/ZIP/signature checks pass; the same supplied libmain/C++ runtime RELRO checks fail. Runtime on 16 KB and physical Samsung testing remain NOT VERIFIED.
- `verification.json`: final current-source checks passed. Anatomical facing and actual routes pass at 30/60/120/144 Hz, with minimum alignment 0.895/0.901/0.906/0.900. Existing motor/Shift regression, smoke, reach, phone-router checks and direct-step jitter routes also pass. Jump intent 11 assertions and touch ownership 18. This is specific technical evidence, not production quality acceptance.

The production milestones remain incomplete as recorded in the earlier acceptance table. Samsung remains disconnected. Native vendor RELRO compatibility remains an open failure; this orientation repair does not resolve it. No commits or pushes were made.
