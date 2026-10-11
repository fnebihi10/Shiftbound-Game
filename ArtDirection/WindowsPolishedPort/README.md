# Current game review: WindowsPolished restoration

Review `main`, not the superseded build35 screenshots. The owner preferred the older WindowsPolished executable and requested its presentation and gameplay in Unity GoldenRooftops and on Samsung. This checkpoint restores that baseline; it does not complete the larger production upgrade or establish a rating.

The previous production work is preserved in the commit history through `821ee36` and in a local ignored backup. The original WindowsPolished executable is untouched. No levels were added. [Restoration implementation and checks](../../WINDOWS_POLISHED_ANDROID.md).

## Look at the delivered game

| Evidence | What it establishes |
|---|---|
| [Samsung Present](samsung-present.png), [Samsung Overgrown](samsung-overgrown.png) | Actual Samsung S10+ screenshots, 2280x1080, original opening position; visually inspected. |
| [Samsung motion footage](samsung-automated-motion.mp4) | 11.97 seconds of actual device rendering with synthetic movement, held stationary jump/landing and world change. No physical two-thumb evaluation or performance acceptance. |
| [Desktop Present](desktop-restored-present.png), [Desktop Overgrown](desktop-restored-overgrown.png) | Restored game-camera captures, 1280x720, UI excluded; aspect differs from Samsung. |
| [Preferred executable reference](../SamsungReview/desktop-polished-reference.png) | Preserved older executable. Restored desktop opening closely reproduces its skyline, palette, courier and framing. |
| [Runtime render state](render-state-startup.txt) | First-frame mobile settings; not a sustained performance measurement. |
| [Build identity](build.json), [artifact checks](artifact.json) | Installed source/APK fingerprints, same-key update and remaining native compatibility failures. |

Installed APK: QA 0.3.1/code4, package `com.shiftboundproject.shiftbound`, Unity 6000.3.25f1. Local runnable artifact: `.validation/ProductionUpgrade/WindowsPolishedAndroid/Android/Shiftbound-QA.apk` (not committed). APK SHA256: `872cb7420201950a38fd341c41c629e4d6888c9611a0e3028a1b6545274bc622`.

Installed source fingerprint: `c6caaf42dcc1d0188040773616bdab67ee0b1ec469e2fda0b9e376e0e18ef589`. Published source after Unity metadata migration: `3c3efffc163b8fd3b670b5f39f45b0aaffb92a5a9acafe7c56627b5ca6132dd4`. Only `Mobile_RPAsset.k_AssetPreviousVersion` changed from 12 to 13 after the build; rendering values and gameplay are unchanged. The build manifest's base `821ee36` alone does not reproduce the APK.

Samsung uses GLES3, Mobile_RPAsset, scale1, 2xMSAA, four-bone skinning and original colour grading. Startup snapshot is 2280x1080 before the deferred 1920x910 resolution request; later game buffer allocations were 910x1920 (rotated). Physical captures remain 2280x1080. No measured sustained 60 FPS claim follows from these settings or recording frame rate.

## Honest acceptance status

| Category | Status and scope |
|---|---|
| Samsung visual quality | NOT VERIFIED against production target. PASS only for reproducing the preferred treatment in the inspected opening views. |
| Courier and motion | NOT VERIFIED against production target. Original courier/clips restored; brief automated motion inspected. Foot contact, deformation and complete traversal still need evaluation. |
| Two-thumb interaction and camera | NOT VERIFIED. Synthetic input cannot establish comfort. |
| Sustained Samsung performance | NOT VERIFIED. No 20-minute measurement of this candidate. |
| Lifecycle, recovery and persistence | NOT VERIFIED for full acceptance. PASS for same-key update without data clear and loading existing non-default settings. |
| Android artifact compatibility | FAIL. Fresh artifact has vendor RELRO failures in libc++_shared.so and libmain.so; signing, ZIP and LOAD checks pass. AAB-generated APK and separate 16 KB runtime unverified. |
| Independent human/art evaluation | NOT VERIFIED. Owner's approximate 6/10 for the preferred executable is personal feedback, not an independent score for this port. |

## Suggested independent AI review

Inspect current main source, the native Samsung screenshots and the actual video before judging. Explain visible strengths and defects in materials, city depth, courier, animation, camera and readability. Separate direct observations from hypotheses. Give any image/motion score with its limited evidence and confidence; do not infer finger comfort, sustained performance or Android compatibility from screenshots. Identify the highest-impact next production changes. Do not average failed or unverified major categories into overall acceptance.
