# Android-first candidate — 9 October 2026

Baseline: clean `eaee340369e8ac6679ed3f646c2b0fa1cac500a1`, Unity 6000.3.25f1, URP 17.3.0 / Input System 1.20.0. No AGENTS.md found. Android module, bundled NDK r27c and JDK 17 installed; no ADB devices attached. GoldenRooftops is the only delivery scene. Historical reports are not current candidate evidence.

## Ranked gaps established before implementation

| Priority | Player consequence / observed evidence | Correction | Effort / dependency |
|---|---|---|---|
| 1 | Cannot play on phone: GameInput exposes keyboard/gamepad only, HUD desktop prompts | Owned multi-touch stick, held Jump, Shift, drag camera; safe-area HUD and lifecycle | Medium; physical Android phone for comfort |
| 2 | Binary checks can certify stale source; only Idle required by build validator | Fresh builds, source/binary manifest, production jump checks and all runtime motion validation | Low/medium; installed Unity |
| 3 | Backgrounding can retain jump intent; settings/progress not retained | Explicit pause on interruption, neutral-until-release input, persistent settings/checkpoint | Medium; phone interruption/update testing |
| 4 | Airborne courier remains in one wide-arm pose; current captures show stand-in silhouette | Audit licensed clips, separate takeoff/rise/fall/land without input lockout | Medium; premium courier/stop/turn commission remains |
| 5 | Audible fallback beeps and silence in ambience: saved clip fields empty | Source-backed authored cues, distinct ambience and restrained adjustable mix | Medium; sound assets/listening review |
| 6 | Foreground sparse/repetitive; panorama lacks parallax; premium opening explicitly rejected | Review small kit in motion before wider changes; credible obstacle construction | High; environment/vegetation artist |
| 7 | Android sustained pacing, thermal and latency unknown; D3D11 warnings historical | APK/AAB, named-device 20-minute soak, bounded Windows A/B diagnostics | Medium/high; phone + measurement hardware |
| 8 | Enjoyment/art approval absent | Five uncoached physical-touch players and independent art/motion review | External participants |

## Gates (all essential; no compensation across domains)

| Domain | Initial status / confidence | Acceptance evidence needed |
|---|---|---|
| Traversal/recovery | NOT VERIFIED current / medium historical | Fresh-source checks: Shift clearance/momentum/debounce, checkpoints/goal, 30/60/120/144/variable dt |
| Response/motion/camera | FAIL / high (missing touch, single air state) | Reversals, landing input, continuous orbit, wall/corner/Shift/retry recordings; physical comfort; latency method |
| Courier/environment | FAIL / high | Concept-coherent opening in normal motion, anatomy/skin/backpack, physical material scale, rooted growth, 360-degree city |
| Shift comprehension | NOT VERIFIED / low | First Shift and two-thumb midair success without coaching, non-color support cues |
| Audio/interface | FAIL / high | Authored cues + loops, phone-safe layout, event sync, listening review |
| Sustained Android performance | NOT VERIFIED / none | Named reference midrange: steady 60; named floor: stable 30; 20-minute soak with resolution/tier, distributions, memory/battery/temperature limits |
| Touch/lifecycle | NOT VERIFIED / none | Multi-touch ownership/cancellation tests then physical two-thumb, Back/background/resume/audio interruption |
| Install/update/Play | NOT VERIFIED / low | APK cold launch/update retention; IL2CPP ARM64 AAB, native 16KB checks, release signing and Console-specific testing |
| Independent players/art | NOT VERIFIED / none | Five fresh humans + independent art/motion reviewer; AI assessment never substitutes |

No supported hardware floor or reference device can be honestly established from available hardware: no phone attached. API/architecture compatibility is a packaging floor, not a performance floor. Existing Mobile URP asset is a provisional tier; do not label its settings measured.

## References and comparison rules

Approved concept: `ArtDirection/visual-target-gameview-v2.png`; current committed opening/airborne captures inspected. Compare jacket/backpack silhouette, material richness, foreground growth and corresponding landmarks at gameplay distance. Reference gameplay selected for subsequent human comparison: [Mirror's Edge Catalyst movement demonstration](https://www.ea.com/amp/news/movement-in-mirrors-edge-catalyst-the-basics) for immediate traversal response and readable route material; [Astro Bot platformer design demonstration](https://blog.playstation.com/2018/06/25/astro-bot-rescue-mission-the-dos-and-donts-of-building-a-platformer-in-ps-vr/) for landing silhouette and pose clarity. First-person and VR cameras are not direct free-orbit specifications. External video playback has not yet been reviewed in this tool session; no measured comparative claim is made.

Keep raw local logs/builds separate from committed summaries. Candidate evidence must include source fingerprint, settings, Unity version, scene and binary hashes. An APK compile is not a physical-device pass. No Google Play publication authorized.
