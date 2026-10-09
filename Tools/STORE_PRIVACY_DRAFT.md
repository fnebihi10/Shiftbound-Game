# Shiftbound store and privacy preparation — QA candidate

Draft for owner review, not a published declaration. No Play Console access, release signing ownership, developer contact, target audience or content-rating questionnaire was supplied.

## Facts checked in this candidate

- One offline rooftop traversal scene; Present/Overgrown Shift, touch controls, local settings and checkpoint retention.
- Application identifier `com.shiftboundproject.shiftbound`, version `0.2.0`, versionCode `2`; owner must confirm the permanent identifier before first distribution.
- Android API26 minimum, API36 target, ARM64 IL2CPP, OpenGLES3, landscape rotations. These establish proposed compatibility, not measured device support.
- Final merged APK permission report has no INTERNET permission. It contains the app-private `DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION`.
- UnityConnect, analytics, purchasing, ads, performance reporting and cloud crash reporting are disabled in saved settings. No analytics, ads, account, purchasing or outbound networking calls were found in Shiftbound gameplay source. Built-in engine modules remain installed; module presence alone is not active collection.
- Settings and the shared checkpoint are saved locally with PlayerPrefs. Restart/completion clears checkpoint progress; uninstall/clear app data removes local settings/progress.
- The opt-in QA recorder writes device/render metadata and gameplay timing to app-local files. Nothing uploads them. A tester manually exporting these files shares device/model/OS/GPU information; explain that when requesting logs.
- CC0 audio/animation attribution and original asset provenance are in `THIRD_PARTY.md`. No asset purchase or subscription was made.

## Proposed store copy

“Run, jump and Shift between two versions of a rooftop city. Read the changing platforms, keep your momentum and find your way to the goal.”

Do not advertise a frame rate, broad device compatibility, premium art acceptance, campaign length or quality rating until supported by evidence. This candidate has one short route. No monetization or accounts are included.

## Owner decisions and remaining evidence

Supply developer/support/privacy contact and the intended audience; complete content rating from actual game content. Prepare a public privacy-policy URL describing the final app's local storage, optional QA exports and absence/presence of external collection after a final dependency and device traffic audit. Data Safety answers must match that final audit and any later services. Do not submit this draft as a completed Console form.

Use actual phone screenshots and gameplay for the listing. Current Windows phone-layout captures are review evidence, not device screenshots. Confirm adaptive icon/store artwork and accessibility/readability on physical phones. Verify the device catalog, internal-track split installation and same-signature update retention.

Release remains blocked by failing native 16KB RELRO checks, absent physical-device testing, signing/Console access and independent player/art review. No publication is authorized.
