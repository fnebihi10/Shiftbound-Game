# S10+ review in progress

Historical work log. The current review checkpoint, build identities, native Samsung images, automation footage and explicit acceptance failures are in [ArtDirection/SamsungReview/README.md](ArtDirection/SamsungReview/README.md). Build24 jump/landing corrections are newer than the inspected22 footage; do not transfer older test results to24.

The owner's physical recording, `Screen_Recording_20261010-160314_Shiftbound.mp4`, was reviewed against actual ADB screenshots. The owner rejects the current phone experience as substantially worse than Windows. This is actionable negative feedback; the production milestone remains unfinished.

Verified device: Samsung SM-G975U, Android 11/API30, Snapdragon 855 platform, 4 KB pages, 60 Hz display, user-selected 1080x2280 resolution (2280x1080 landscape). USB/debugging authorized. This phone cannot establish 16 KB compatibility or the entire proposed supported device range.

## Demonstrated problems and changes

- White ivy in the owner's recording: the material lost its texture binding after importer reimport. Synchronous import, explicit material save and a non-null asset assertion corrected it. Actual iteration10/11 phone captures show green veined cutout leaves.
- Blurry controls/text: ScreenSpaceCamera UI inherited the reduced scene resolution. The shipped Canvas now uses a native ScreenSpaceOverlay. Actual phone captures verify sharp outlines and menu text.
- Frog-like airborne legs: authored ascent/descent curves now override inherited leg abduction and twist. Real motion, contact and deformation review remains necessary.
- Long wall-obscured fall: recovery threshold is now -3m, with an authoring assertion that route support is above -1m. Motor reach/collision did not change. Regression passed; physical visual recheck remains necessary.
- Mobile courier used two bone weights, versus four authored/desktop weights. Mobile quality now retains four.
- Android removed the desktop grade completely. The first attempt to restore it only at runtime produced severely clipped colors and was rejected. Build-time HDR grading variants are now retained, and the saved grade's missing component subassets are repaired. Iteration13 real-phone capture has normal lighting. No bloom on phone; scene pixel and shadow limits remain bounded.
- Offscreen evidence after the native UI change initially cleared the scene, then omitted the overlay. Those captures are invalid. The capture helper now requests the complete URP camera stack; rebuild and visual verification are still pending. Actual ADB screenshots include the shipped interface independently.

## Evidence and testing status

Local evidence is under `.validation/ProductionUpgrade/S10Plus` and exact APK copies/manifests under `AndroidIteration11`, `AndroidIteration12`, `AndroidIteration13`. These are dirty-source candidates rooted at commit324f3c3; final source must be committed and correspondence verified. Iteration13 Android source fingerprint is recorded in its manifest; do not replace it with a later working-tree fingerprint.

Windows iteration11 automated verification passed at source fingerprint3725cf85e721060d8b24d2ceecdba7ad99d60378b80154c0095a2350bfcc73a9: jump11, touch18, collision/motor, smoke, required Shift reach/teaching, synthetic phone contacts, complete route at30/60/120/144/variable and production Input System route at30/60/120/144. This does not prove physical frame rates or comfort. Latest rendering/capture changes require affected verification.

The separate permission-free `Tools/PhoneTouchProbe` companion sends Android MotionEvents and takes real screenshots. Delivery alone is **NOT** an interaction PASS. Runs were invalidated by simultaneous physical touches, saved control layout differences, initial pause, landscape cutout moving sides and an incorrectly chosen touchpad descriptor. The tool now chooses a physical touchscreen descriptor. Verify the actual layout, PLAYING state, accepted airborne Shift and menu response before accepting a run. Do not infer comfort or physical latency from this tool.

Iteration13 sustained60 test is running with `Tools/MeasureAndroidPresentation.py`: actual SurfaceFlinger desired/actual/ready timestamps, independent of Unity callbacks; PSS/thermal/battery samples; no video/readback. Repeated complete route through production Input System/motor, real checkpoints and completion menu. USB charging and synthetic input must accompany results. `AnalyzeAndroidPresentation.py` explicitly labels partial data as NOT VERIFIED. Android11 lacks FrameTimeline; estimated missed display slots are not deadline attribution. Sampling gaps must be disclosed. The earlier iteration12 run was interrupted after its failed visual review and does not count as a20-minute test.

## Remaining material gates

- Courier/art/motion: FAIL pending substantial visual/motion acceptance; no independent art approval. Courier and repetitive city still look below the approved target; technical fixes alone cannot close this gate.
- Environment: FAIL pending phone-distance coherence, panorama orbit, vegetation/contact and full-route composition review.
- Two-thumb experience: NOT VERIFIED. Owner's negative assessment remains. Uncoached five unfamiliar-player test still required; no participants/results invented.
- Android16KB: FAIL. Iteration13 every-library audit passes Burst, source-built game/swappy and source IL2CPP; libc++_shared, libmain and libunity still fail RELRO end alignment. All LOAD alignment checks pass. Static report preserves exact hashes/provenance.
- Supported official Unity6000.6.5 module inspection found aligned libunity, but libmain still fails. No vendor binaries mixed across Editor versions. Supported source linker flags cannot repair these prebuilts.
- Sustained60/30: NOT VERIFIED until full measured runs are complete and assessed. One S10+ cannot establish the lower-end support target. Lifecycle, both landscape rotations, installation/update and physical comfort require explicit results.
- Windows presentation: NOT VERIFIED. Controlled foreground ownership attempts failed; callback timings cannot settle D3D11 presentation warnings.

Google's official16KB emulator image and emulator37.2.12 are installed locally; AVD `Shiftbound16KB` is booting with WHPX and software graphics. Verify16384 page size and disable compatibility mode, then build/inspect the separate x86_64 diagnostic after the physical measurement. ARM64 remains the primary artifact. No release signing or Play publication is authorized.
