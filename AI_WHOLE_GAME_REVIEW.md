# Shiftbound: whole-game and playable-demo review

**AI-authored review by OpenAI Codex, 9 October 2026.** This is an independent assessment, not a human playtest or an approval to ship. Requested disciplines: game direction, Unity gameplay engineering, technical art, level design and QA.

**Reviewed repository:** `fnebihi10/Shiftbound-Game`, PR #1, branch `milestone-reliable-traversal`, head **`4bffa80f53dc4f733f434c0c69971c8b3b79f10d`**, “Document Milestone 1 evidence and AI project review.” The connected GitHub PR and local checkout agreed on that head. Base: `613ddf6a83b8008fc5f5650b07fe88d9a3ca3324`. GoldenRooftops is the authoritative scene; SkylineRooftops was inspected as a historical visual comparison. Unity 6000.3.25f1 / URP 17.3.0, Windows x64. No gameplay, scene or asset edits were made for this review.

Evidence labels below: **Observed** means I inspected a rendered image or controlled runtime outcome; **Code** means source/serialized asset inspection; **Test** means a check rerun against this review's fresh build; **Judgment** means an interpretation, not a measurement. A scripted route and injected keyboard inputs do not constitute human playtesting.

## A. Verdict and ratings

**Shiftbound today is a functional rooftop traversal prototype with a repaired movement/Shift/recovery foundation. It is not yet a convincing first level of a coherent finished game. My overall demo rating is 4.5/10, with medium confidence.** That rating reflects the playable presentation and demonstrated experience, rather than averaging engineering subsystems. Your approximately 4/10 assessment is compatible with the evidence, but the useful diagnosis is that presentation, teaching and depth lag behind mechanical reliability.

The current loop is: read the next rooftop, run up, jump, select the world whose platform is solid, reach shared support, repeat, recover from a checkpoint after falling, and finish at a gold frame. There is one level, three checkpoints, a timer, pause/restart and completion. Nothing inspected establishes a larger campaign, sustained progression or replay demand.

| Category | Rating / 10 | Confidence | Basis |
| --- | ---: | --- | --- |
| Core concept and identity potential | 6 | Medium | Two aligned versions of a city, a courier and momentum-preserving shifts form a workable fantasy; depth and audience appeal are unvalidated. |
| Movement foundation | 6 | Medium | Fresh motor/input tests pass, and normal controls crossed the opening lesson; comfort, physical gamepad feel and animation contact remain provisional. |
| Camera and spatial presentation | 4 | Medium-high | Forward framing works for scripted traversal; normal orbit exposes the matte edge and empty horizon. Wall behavior is not comprehensively reproduced. |
| Level design and teaching | 4 | Medium | A coherent lesson sequence and recovery exist; the route is largely repeated flat crossings, and teaching still relies heavily on explanatory text. |
| Visual execution | 3 | High for inspected views | Current foreground, courier and vegetation fall substantially below the approved concept; additional camera angles expose incomplete world construction. |
| UI and feedback | 4 | Medium-high | State and first-lesson text are readable in captured windows; a world notification overflows, and look/pause/restart discovery is missing from the main HUD. |
| Audio implementation | 2 | High for asset/code completeness | All authored cue/ambience slots in Golden are empty; generated tones provide functional cues. Listening quality is **unscored**. |
| Technical foundations and verification | 7 | Medium-high | Fresh build, scene validation and broad custom regressions pass; provenance and animation guards remain incomplete. No performance certification. |
| Evidence for a satisfying whole game | 3 | Low-medium | No sustained progression or human replay evidence; most shifting selects a required platform state. This is a validation gap, not proof the concept cannot work. |

**Investment recommendation:** fund a bounded improvement of this slice and one visual benchmark. Do not fund a level-production pipeline at the present quality bar. The work worth preserving is substantial enough that a wholesale rewrite would be hard to justify; the current experience is not strong enough that multiplying it would be justified either.

### What identity is actually demonstrated?

The strongest fantasy is a nimble courier using two versions of the same place to find a continuous route through apparently disconnected rooftops. Shared landmarks and preserved momentum can make shifting feel like spatial intuition. The warm jacket, blue backpack accents, gold route/goal marks and contemporary-versus-growth pairing give useful visual ingredients.

The demo demonstrates the **rule** of that fantasy more strongly than the **experience**. The courier has no implemented delivery objective or character motivation beyond reaching the finish. Most platforms answer “which world must I enable?” rather than “which route or movement strategy do I choose?” Midair shifts add timing, but binary platform availability alone does not establish puzzle depth. The fork is a start; its alternatives lack a clearly demonstrated difference in reward, risk or movement style.

The concept promises lush, inhabited vertical architecture, believable character motion and a coherent sense of place. The current course reads more like a test lane suspended among repeated building blocks. A stronger identity must come from the way players read and traverse the transformed city, with presentation supporting that experience.

## B. Ranked findings and corrections

Priority describes investment order: P1 means address before content expansion; P2 means address in the focused slice/benchmark. It does not imply a confirmed crash or security defect. Relative effort is comparative, not a schedule estimate.

### 1. The first Shift lesson needs proof of understanding, not another mechanical pass

**Evidence:** Observed normal W/Space traversal to the first checkpoint; Present shows a blue, non-solid bridge. Left Shift made it solid, and two normal held jumps crossed it. The explicit instruction appeared, changed with world state, dismissed at shared support and returned on checkpoint respawn. Current 1280×720 and 800×450 captures show the instruction fits. Code: [HUD lesson][hud-lesson], [guidance lifecycle][flow-guidance]. Earlier user confusion is documented in MILESTONE_1_PROGRESS; I did not independently view those supplied WhatsApp recordings, so their interpretation remains historical testimony. Test: both mandatory direct bypasses failed in every one of this review's 50 sampled attempts per lesson.

**Player impact:** A player who reads blue geometry as a usable platform can repeatedly try an impossible direct jump and blame movement. A readable sentence does not prove that the environmental distinction is understood during play.

**Likely cause:** Full blue preview geometry occupies much of the route, including decorative geometry. Solid and preview surfaces share the same basic silhouettes; the screen communicates the explanation separately from the target.

**Correction:** Keep the forgiving bridge and explicit guidance. Establish one route-surface visual language: solid landing plane and edge treatment, preview treatment with a clearly different non-solid pattern/outline, and a brief change cue attached to the actual bridge. Reduce decorative previews near the first lesson. Present one safe grounded Shift before demanding midair timing. Give rejection a local explanation when possible.

**Priority/effort:** P1, medium across level design, technical art and UI. **Dependencies/tradeoffs:** Coordinate collision timing with presentation; preserve immediate solid-state changes. Stronger markings can compete with the art target, so test them in the visual benchmark. **Verify:** With 8 new uncoached players, at least 7 correctly explain previews and activate the bridge within two attempts. Record attempts and misinterpretations; rerun with new players after revision. This is a proposed gate, not a claimed result.

### 2. Free camera orbit exposes an unfinished world

**Evidence:** Observed held-right-mouse orbit at the start: one side view exposes a hard diagonal edge of the skyline image; a rear view replaces the city with an empty green/grey horizon. Local captures: `.validation/WholeGameReview/normal-orbit-mapped.png` and `normal-orbit-back.png`. Code: [SkylineBackdrop][backdrop] fixes a single plane's anchor and orientation in world space. Despite “camera-facing” language in README/art documents and an older comment in the class, the current implementation is a fixed world-space matte, not a panoramic skyline.

**Player impact:** A normal supported control immediately breaks the illusion of a city. The player also loses route framing when looking away; keyboard movement follows that new camera orientation.

**Likely cause:** A frontal image solution was paired with unrestricted orbit, with no complete surrounding composition.

**Correction:** Choose a camera/world contract. For free orbit, build coherent side/rear coverage using a panoramic far layer plus a small 3D/impostor midground with compatible silhouettes. If a guided camera better suits the game, prototype intentional orbit limits and look controls before committing to that direction. Merely making the plane wider cannot supply a believable rear view.

**Priority/effort:** P1 for contract, medium-high for environment solution. **Dependencies/tradeoffs:** Lock the usable orbit range and normal FOV before commissioning skyline art. Restricting orbit reduces production burden but sacrifices scouting freedom. **Verify:** Capture a full supported orbit in both worlds at start, fork and finish, across supported aspect ratios; no visible matte boundary or unintended empty horizon. Human players must still locate their next landing after looking around.

### 3. The mechanic has reliable execution but insufficient demonstrated decision depth

**Evidence:** Code and scene route: alternating world-specific platforms separated by shared platforms; a single left/right fork. Most heights are the same. The full route harness supplies platform names, required world states and scripted steering in [GameplayCompareRunner][route-runner]. Shifting changes colliders/materials while preserving the motor's momentum. There is no inspected resource, persistence puzzle or other system creating a competing reason to stay in either world.

**Player impact:** Once the rule is learned, subsequent crossings risk feeling like the same jump with a required button press. An input timing challenge can be enjoyable, but the demo has not established the variety or expressive mastery needed for a larger game.

**Likely cause:** The course was built to demonstrate functionality and enforce teaching, rather than explore several consequences of the mechanic.

**Correction:** In a disposable blockout after clarity is fixed, compare three encounter types using existing rules: a route choice with an obvious safe/fast tradeoff; a shift timed to preserve a chosen airborne trajectory; and a transformed view that reveals a route around aligned landmarks. Test whether players plan differently and voluntarily retry. Do not introduce a new movement power merely to conceal shallow encounters.

**Priority/effort:** P1 validation, medium. **Dependencies/tradeoffs:** Needs readable previews and stable movement/camera first. More combinatorial geometry increases clearance and shortcut-testing burden. **Verify:** Players describe at least three different decisions; both fork routes attract use; at least half of a small test cohort voluntarily retry without being instructed. If only compulsory toggling survives, consider a shorter time-trial game or rethink encounter rules before production.

### 4. Foreground art lacks a coherent asset and construction language

**Evidence:** Observed current Present/Overgrown renders and normal captures: identical nearby facades, flat roof caps, blue window blocks, oversized stone texture features, box equipment, literal paving grids and repeated bright primitive plants. Layered equipment reads as stacked simple volumes. The approved concept instead uses varied parapets, convincing facade depth, grounded vegetation, richer material transitions and consistent sun/shadow relationships. [V4 authoring][v4-art] and [V5 fallback][v5-art] show the layered procedural construction; the saved scene contains V3, V4 and V5 art roots. This is evidence of accumulated layers, not proof every prop is duplicated.

**Player impact:** The detail hierarchy puts a rich illustrative background behind simplistic near objects. The camera repeatedly shows the least convincing assets at the most scrutinized distances. Added textures have not made the construction believable.

**Likely cause:** Independent passes added detail to primitive shapes without replacing the underlying kit, setting consistent physical scale or reconciling overlapping styles.

**Correction:** Author one small reusable facade/parapet/roof/HVAC kit with agreed dimensions, edge profiles, material density and wear placement. Replace redundant layers in the benchmark rather than stacking another pass. Establish where equipment sits, drains connect and growth roots. Keep the gameplay landing outlines intact. Use material breakup and contact lighting after the underlying shapes work.

**Priority/effort:** P1 benchmark, high art effort. **Dependencies/tradeoffs:** Camera contract and target platform first. A more restrained stylized foreground can be coherent at lower cost, but it needs a matching skyline; the concept-quality foreground requires a larger asset investment. **Verify:** Owner reviews actual normal-camera footage of the same short section in both worlds. Inspect every used orbit angle; no conspicuous texture stretching, unsupported props, contradictory scales or duplicate equipment. Extra decals and bloom are not acceptance criteria.

### 5. Courier motion and silhouette do not carry the game's fantasy

**Evidence:** Observed slim limbs, simple clothing surfaces, small hair shape and box-like backpack relative to the concept's expressive courier. Sampled fresh movement frames show running/jumping and an upright completion pose; they do not establish continuous foot contact or clean skinning. Code: [RiggedCourierAnimator][animation] selects five discrete states, uses one Jump state for all airborne motion, and changes global playback speed with locomotion speed. It has no dedicated descent/turn/stop set or foot-phase synchronization. Footsteps are emitted by distance travelled in [PlayerMotor][motor-feedback], rather than animation contacts. Current controller contains non-null references for all five states; I found no evidence that the repaired Editor-only clip defect is currently recurring.

**Player impact:** The avatar is always central, so weak silhouette, generic poses and disconnected contact cues undermine the perceived quality of every movement action.

**Likely cause:** A rigged placeholder was adapted to gameplay with minimal state selection and accessories, rather than a cohesive character/motion set.

**Correction:** First tune motion on the existing rig: actual-speed blends, reversal/stop behavior, distinct ascent/descent presentation, and impact-sensitive landings. Synchronize footsteps and takeoff with verified animation events or contact-phase logic. Then refine/license a character matching the approved silhouette, with a courier-specific backpack and clean shoulders/hips. Keep root-motion ownership explicit.

**Priority/effort:** P1 movement/animation, medium; final character high. **Dependencies/tradeoffs:** Set motor feel and normal screen size before final animation/character work. Better clips cannot correct bad skinning, and a premium model cannot correct timing mismatch. **Verify:** Record starts, low-speed walking, full-speed runs, stops, 180° reversals, tap/hold jumps, ledge falls and hard landings. Review in real time and slow motion; no obvious sustained foot sliding, ground penetration or backpack/body intersection. Require valid player-runtime clips in all requested states.

### 6. Movement tuning is consistent but not yet proven comfortable

**Evidence:** Scene uses 7.5 m/s maximum speed, 42 m/s² ground acceleration/braking, 19 m/s² air acceleration, 1.85 m jump height, 28 m/s² gravity, 0.45 release multiplier, and 0.14 s buffer/coyote time. [Motor source][motor] and [scene values][motor-scene]. The same MoveTowards acceleration handles starting, stopping and reversing. Ideal continuous calculations imply about 0.18 s to full ground speed, 0.67 m to brake from it, and 0.36 s to reverse direction; these are **derived values**, not measured human outcomes. Visual facing uses velocity and interpolated turning. Normal inputs crossed the opening route; fixtures passed jump, support, slope, ceiling, buffering and coyote cases.

**Player impact:** Rapid full reversals and strong airborne correction may make the courier feel nimble or weightless; the current discrete animations offer limited evidence of weight. Camera-relative movement means rotating while holding forward steers the desired velocity during a jump. Unintended steering can be mistaken for unreliable jumping.

**Likely cause:** One responsiveness configuration has served multiple contexts, with no documented human tuning comparison.

**Correction:** Compare a small set of coordinated acceleration/braking/reversal/air-control and camera settings on this route. Preserve existing forgiveness unless evidence shows a problem. Define whether airborne camera rotation should steer immediately; make that rule learnable. Reassess jump cut alongside animation and landing visibility, rather than changing jump range in isolation.

**Priority/effort:** P1, medium. **Dependencies/tradeoffs:** Range changes invalidate mandatory gaps and beginner margins; rerun reach audits and re-test the route after tuning. More weight increases precision demands. **Verify:** Keyboard and physical gamepad participants land ordinary gaps predictably, stop on intended roofs and recover from imperfect takeoffs. Log fall locations and attributed causes. Separate missed inputs from misunderstood destinations and camera steering.

### 7. UI omits control discovery and has a reproduced notification layout defect

**Evidence:** Observed main HUD lists move/jump/Shift but omits held-right-mouse look, pause and restart. Escape paused; R restarted and restored Present/start framing. At 1280×720, “OVERGROWN WORLD” wrapped beyond the upper notification panel; capture `normal-leftshift-mapped.png`. Code: [notification rectangle][hud-notice] gives a 240×38 label with a 23 px centered style. Main controls and pause/completion buttons are in [PremiumHUD][hud]. At 800×450 the first lesson fits, but the persistent 13 px text scaled by the 0.72 minimum is small. Gamepad prompts use A/X and update flags, without a platform-specific glyph set or shown Start/Back guidance.

**Player impact:** Discoverable camera and recovery controls are essential for a traversal game. A broken state banner makes an otherwise functioning action look unfinished. Controller players get less actionable pause/retry text than the keyboard bindings support.

**Likely cause:** Prototype IMGUI fixed rectangles and independently scaled text, plus an incomplete prompt hierarchy.

**Correction:** Fix notification sizing/wrapping now; give the first screen and pause overlay complete relevant controls, including controller alternatives. Make menus navigable and focused with a physical gamepad. Use content-driven safe-area layout and last-meaningful-device prompts when replacing the prototype UI. Keep world state more prominent than a speed timer during learning.

**Priority/effort:** P1 for defect/discovery, low-medium. **Dependencies/tradeoffs:** Declare a supported minimum window size and platform/controller set; do not promise mobile readability through scaling alone. **Verify:** Capture every notice, lesson, pause and completion state at 1280×720 and 800×450; no clipping or collisions. Finish/retry/resume without a mouse on the chosen controller. New players discover look and restart without external README instructions.

### 8. Lighting and sound are incomplete authored systems

**Evidence:** Code/scene: [global Volume][volume-scene] has `sharedProfile: {fileID: 0}`; `MaterialsV2/CityVolume.asset` has `components: []`. Enabling camera post-processing therefore does not supply the authored bloom/grade/vignette implied by the historical V3 generator. This is a confirmed saved configuration gap. Current frames show broad bright facade surfaces and weak-looking grounding relative to the concept; their exact lighting causes require targeted rendering inspection. [Golden audio slots][audio-scene] are all null, including both ambience loops; [FeedbackAudio][audio-code] synthesizes sine-tone fallbacks. I did not listen to playback.

**Player impact:** Inadequate contact and material response flatten the foreground. Functional tones can signal state changes, but this implementation does not establish city atmosphere, physical footfalls or a satisfying Shift signature.

**Likely cause:** Saved authored configuration/assets did not follow the breadth of historical tooling and intended presentation.

**Correction:** Use a deliberately authored and validated Volume profile as part of the benchmark; first balance light direction, ambient contribution, contact shadows and material scale. Author distinct Shift/rejection/goal cues and a small footsteps/landing set, then two related ambience beds. Make physical contact and world change lead the mix, without masking decisions.

**Priority/effort:** P2, medium across technical art/audio. **Dependencies/tradeoffs:** Lighting must match skyline direction and target rendering budget. Foliage transparency, shadows and post effects cost GPU time. Audio needs continuous real-time listening and synchronization review. **Verify:** Saved scene reopens with the intended profile; a fresh player visibly matches the accepted benchmark. Human listeners distinguish successful/rejected Shift, jump/landing and goal without confusion; cue timing matches the visible event. Do not claim sound quality from clip-slot inspection.

### 9. Verification can provide stronger assurances than it actually establishes

**Evidence:** Independently confirmed the earlier AI_REVIEW hypotheses. [VerifyMilestone1][verify-script] builds only with `-Build`, otherwise accepts the existing executable with no source fingerprint; it omits the source JumpIntent command. The default build uses `-nographics`. In this review even graphics-enabled **batchmode** failed licensing outside the sandbox; a graphics-enabled launch **without `-batchmode`**, with `-quit -executeMethod SliceDelivery.BuildWindows`, succeeded. The prior review's batch fallback is not a reproducible guarantee today. [Animation validator][animation-validator] requires upright Idle and rejects preview clips it encounters, but does not require every requested state and non-null motion.

**Player/team impact:** A stale binary can hide source failure, and future animation regressions can escape the build gate. A developer may confuse a license/environment failure with a project defect.

**Likely cause:** Verification grew around a known local player and narrowly repaired historical defects.

**Correction:** Record build commit/source fingerprint and tool versions; distinguish current-source verification from existing-binary testing. Include source assertions. Require every animation state/runtime motion. Expose graphics/headless options and document the actual successful command with environmental caveats.

**Priority/effort:** P2, low-medium engineering. **Dependencies/tradeoffs:** A fresh-build gate costs iteration time; retain clearly labeled quick binary diagnostics. **Verify:** A changed source fingerprint prevents a stale certification; a missing Jump/land/run clip fails validation in an isolated test fixture; a clean checkout on a second licensed installation builds using the documented path.

### 10. Content authoring and runtime costs need a production decision

**Evidence:** Golden contains 1,491 serialized MeshRenderer entries; this is a file count, **not runtime draw calls**. Many details are individually authored primitive objects. [WorldSwitcher][switcher] caches world descendants, checks destination colliders individually by temporarily enabling them for penetration queries, and swaps every world-specific renderer's material arrays. [V5 fallback][v5-art] locates a named root and constructs a fallback when absent. Maintenance and route tests rely on object names and baseline coordinates. The scene validator expects exactly three checkpoints/one goal. These are acceptable prototype constraints, but not an established scalable content workflow.

**Player/team impact:** Added detail may increase submission, shadow and transparency costs. More levels can magnify fragile authoring assumptions and world-state synchronization errors.

**Likely cause:** The project evolved through scene generators and focused one-scene repair tools.

**Correction:** After visual/mechanic gates, define reusable rooftop/encounter prefabs with explicit anchors and world-membership rules. Retain authored collision separate from decoration. Profile before consolidating meshes or adding a broader framework. Avoid rewriting working systems merely to anticipate scale.

**Priority/effort:** P2 after validation, medium. **Dependencies/tradeoffs:** Combining geometry can improve submission while worsening culling/iteration. Broader dynamic-world support needs explicit invalidation of cached sets. **Verify:** A second small encounter can be authored without hardcoded-name repairs; validation catches missing references and unsafe spawns; compare measured CPU/GPU/rendering costs in both worlds and on Shift.

## Complete route, teaching and pacing

The following dimensions come from saved platform transforms under the shared, Present and Overgrown roots. They describe collision-support slabs, not all visible decoration. For this route the slab tops are at y=0; all slabs below have y center -0.5 and height 1. Dimensions are x width × z depth, in Unity units interpreted as metres. Scene evidence includes [start][start-scene], [bridge][bridge-scene] and [midair destination][midair-scene]. The route runner verifies the named main route; alternative reach has separate samples.

| Encounter | Authored platforms (center x,z; width×depth) | What it teaches/tests; assessment |
| --- | --- | --- |
| Start → first landing | Shared start (0,3; 8×8), landing (0,10; 5×4) | Ordinary jump across a 1 m edge gap. Safe roof permits rehearsal, but surrounding previews/props already introduce visual complexity. First checkpoint gives recovery and Shift instructions. |
| First alternate bridge → Shift landing | Overgrown bridge (0,15.5; 3×5), shared landing (0,21; 5×3) | Grounded Shift, then 1 m entry and 1.5 m exit gaps. Direct shared-to-shared gap is 7.5 m. Good beginner margin and recovery on first landing. The explicit guidance is useful; comprehension still needs humans. |
| Midair takeoff → destination → landing | Present (0,25; 3×3), Overgrown (0,29; 3×3), shared (0,35.5; 5×3) | Change back to Present to use takeoff, shift away after leaving it, then cross 3.5 m to shared support. First true airborne state timing lesson. Present-to-shared bypass is 7.5 m. Higher execution burden than the first bridge; ensure the new takeoff state and destination are visible together. Midair landing supplies checkpoint 2. |
| Fork and return | Overgrown right (2.5,40.5; 3×3), Present left (-2.5,40.5; 3×3), Present return (1,44; 3×3), shared landing (0,47.5; 5×3) | Spatial choice and diagonal movement; right branch requires world changes, left is a legitimate Present alternative. Most fresh left-to-return samples succeed for some timings. That is intended mastery, not a defect to block. No demonstrated incentive makes the two routes meaningfully different beyond route availability. Shared landing supplies checkpoint 3. |
| Final combined crossing → goal | Present (-1.5,51.5; 3×3), Overgrown (1.5,55.5; 3×3), shared goal (0,59.5; 7×4) | Diagonal jump plus midair Shift, then shared goal. A reasonable combination test, but mainly repeats existing verbs. Goal callbacks, stopped motion, idle blend and framing pass. Gold frame gives a visible destination; the finish lacks a stronger authored payoff. |

The current progression is introduction → airborne timing → branch → combined test. Preserve that skeleton. The weak point is not the number of checkpoints: it is repeated same-height slabs, similar visual rhythm and unclear reasons to choose a route. Add a moment to survey a transformed landmark and an intentional recovery/reset beat between hard crossings before adding more distance.

No invisible blocker is needed to suppress the documented direct bypasses. The fresh audit measured a **sampled** held maximum gap of 6.75 m and tapped maximum of 3.75 m at 0.25 m spacing, including tested coyote delays. The two 7.5 m lesson bypasses failed 50/50 attempts each. This is not an exhaustive proof against diagonal routes, decorative supports, different takeoff histories or all camera/device behavior. The final crossing and every incidental art surface have not been exhaustively shortcut-audited. A shortcut after instruction can be a reward for mastery; a shortcut before the rule is learned can erase teaching.

### Confirmed defects versus reproduction risks

| Confirmed issue | Trigger and consequence | Source evidence | Severity |
| --- | --- | --- | --- |
| Skyline boundary and missing rear coverage | Use supported camera orbit at the start; city illusion breaks into a visible image edge/empty horizon. Reproduced in this build. | SkylineBackdrop.cs:31–60; local orbit captures | P2, presentation defect; high confidence |
| World notification overflows | Press left Shift at 1280×720; “OVERGROWN WORLD” wraps outside its intended panel. Reproduced in this build. | PremiumHUD.cs:132–137; local Shift capture | P2, UI defect; high confidence |
| Missing authored post-processing configuration | Open/build saved Golden; its global Volume contributes no saved profile, and the historical CityVolume asset has no components. No claimed post effect follows merely from the camera flag. | GoldenRooftops.unity:95692–95697; MaterialsV2/CityVolume.asset:15 | P3, configuration gap; high confidence |
| Stale binary can receive current-looking certification | Change source, then omit `-Build`; verifier still runs the old executable and prints success if its checks pass. Established from control flow, without deliberately breaking source. | VerifyMilestone1.ps1:25–40 | P2, tooling defect; high confidence |
| Missing animation states/motions are not required by validation | Remove a requested non-Idle state or clear its motion in a future change; this validation loop has no requirement rejecting the absence. No destructive mutation performed. | SliceDelivery.cs:86–99 | P2, regression guard defect; high confidence in guard gap |

Batch licensing failure is an observed environmental constraint, not a gameplay defect. Dense camera-overlap saturation, pause/resume held-input timing, incidental shortcuts and foot-contact errors remain reproduction risks. No confirmed P0/P1 runtime failure was found in the paths exercised.

## Movement, camera and technical details that matter

**Jump and grounding:** JumpIntent preserves held/released input across the buffer, expires intent at its boundary and consumes coyote opportunity. The motor shortens ascent on release and integrates constant-gravity displacement. Fresh fixture peaks were about 1.910 m held and 0.434 m tapped in world coordinates; those include the fixture's initial/support offset and should not be compared directly with the nominal height as a defect. The support sweep is limited to controller skin plus 0.02 m (0.10 m here), checks walkable normals, and is used while descending. This is sensible forgiving contact, not exact mesh foot contact. Steep slopes, ceiling impacts and repeated ascent-jump prevention have actual regression coverage.

**Stopping/turning/landing:** Movement velocity and actual displacement are separately exposed, which is useful for collision-aware animation. Facing follows intended horizontal velocity, however, so constrained motion can still ask the visual to turn as if it were moving. A landing sequence drives one brief squash and the Land animation; harder impacts select another cue, but the current default cues remain synthesized. There is no demonstrated anticipatory landing/edge balance presentation. Pause/resume with buttons already held, unusual hitches and buffered landing during world change deserve targeted reproduction rather than being declared current defects.

**Camera:** Golden actually uses an 8 m distance, 1.45 m look height, 2.3 m yaw-relative look-ahead, 66° vertical FOV and sharpness 10; do not evaluate it using FollowCamera's different class defaults. [Scene camera][camera-scene]. Position smoothing uses exponential interpolation; collision is resolved before and after smoothing. Focus adds 0.05 seconds of actual velocity, including vertical velocity, so jump/fall motion affects framing. The forward view keeps the course visible but makes the courier less dominant than in the approved concept and compresses repeated landings into a distant central sequence. Wider FOV or more distance alone will not improve depth judgment.

The [collision resolver][camera-resolver] casts a sphere, then pushes out of up to 16 overlaps in one pass. Dense overlap saturation, a focus starting inside geometry, near-plane clipping at very small distances and secondary penetrations after a push are **risks requiring reproduction**, not defects established by this review. There is no inspected character/obstacle fade system for occlusion. Validate wall proximity, side occlusion and landing visibility with an actual walk/orbit test matrix. Respawn restored forward framing in the normal test; completion framing/idle passed in full-route automation. Completion menu usability with a physical controller remains open.

**Architecture:** The execution order is deliberate: flow before motor, motor before world updates, animation after movement, camera last. GameInput supplies central Input System actions. Motor, world switching, flow, stage triggers and presentation are small enough to tune independently. Golden references and checkpoint shared support/clearance passed fresh validation; the Runner prefab and five-state controller were inspected. The build explicitly selects Golden; historical scene generators are not the supported way to extend its authored route. No confirmed P0/P1 runtime blocker was found in exercised paths. That is not an exhaustive correctness guarantee.

**Performance:** No profiler session or performance result is claimed. The first explicit measurement target should be this Windows machine: **Ryzen 5 5600X + Radeon RX 7600**, D3D11 player, 1920×1080, current documented quality settings, a proposed 60 FPS target. Confirm installed RAM, OS, driver and build mode in the profile record. Measure CPU main/render threads, GPU frame time, p95/p99 frame times, Shift spikes, batches/set-pass counts, visible triangles, shadow passes, transparent overdraw, texture residency, allocations and restart behavior. Compare both worlds, a full orbit and the densest vegetation view. Proposed gate: p95 CPU/GPU frame time each within 16.7 ms during representative play, with investigated traversal/Shift hitches; this is a target, not today's measurement. This machine is a baseline, not proof of low-end support. Choose and name a second minimum-spec PC before claiming a supported PC range. Android requires a separate platform decision, physical device target, touch design and measured budget; it is not validated by these Windows caps.

## C. What to preserve

- **Position/momentum-preserving Shift and capsule clearance.** It supports continuity and potentially expressive airborne routes without embedding the player. Accepted/blocked/debounced/not-playing results are a useful contract, and fresh tests exercise it.
- **Shared support and checkpoint recovery.** All three anchors are validated for both worlds; preserving world state avoids silently undoing the player's choice. Normal fall recovery restored pose, camera and first-lesson guidance.
- **Jump intent extraction and motor observability.** A small production-source state machine, actual velocity and landing sequence provide reliable tuning/debugging hooks. Keep buffer/coyote forgiveness while testing feel.
- **The repaired lesson skeleton.** Grounded lesson, airborne lesson, intentional fork and final combination are worth iterating. Keep beginner bridge margins and legitimate later alternatives; avoid turning mastery routes into invisible walls.
- **Related-world landmarks and warm courier/gold accents.** These can carry a recognizable visual system if the foreground and backdrop share a coherent style. Preserve alignment as environments are rebuilt.
- **Golden as the authored source of truth, plus provenance and opt-in diagnostics.** The project records concept/generated/third-party/player evidence separately, retains GUIDs, and keeps tests off the normal play path. Improve guards rather than throwing away this discipline.

## D. Three successive milestones

All numeric gates below are proposed acceptance targets. Small cohort results are directional, not statistically representative. No precise delivery estimates are warranted without staffing, asset scope and platform decisions.

### Milestone 1 — Make this demo clear and enjoyable

**Engineering:** Fix notification overflow and complete control discovery. Tune braking/reversal/air control jointly with camera framing. Strengthen source/build provenance and required animation-state validation. Retain shared spawn safety and current normal controls.

**Level design:** Reduce first-lesson visual competition; establish unmistakable preview/solid landing cues. Review airborne lesson pacing and both fork routes. Mark takeoff and destination information without obscuring the scene. Audit shortcuts according to whether they defeat teaching or express mastery.

**Art:** Make a restrained readability pass on route surfaces, edges and world changes. Do not expand decorative density yet.

**Animation:** Integrate ascent/descent, speed-appropriate locomotion and impact blends on the existing verified rig. Check stops and reversals in actual gameplay footage.

**Audio:** Supply a minimal authored Shift/rejection and landing/contact set; verify synchronization by listening. Full ambience production can follow.

**Human playtesting/acceptance:** Eight new uncoached players across keyboard and a named physical gamepad; at least 7 understand the first bridge within two attempts and at least 6 finish without coaching within the design's proposed 4-minute novice target. Record deaths by encounter and cause, control discovery and camera complaints. At least half voluntarily choose another run; if they do not, investigate before declaring enjoyment achieved. Both routes and every checkpoint are manually exercised in both worlds. HUD notices/lessons/pause/completion fit 1280×720 and 800×450. Supported camera views show useful landings, and no reproduced wall intrusion remains. Source assertions, fresh-build validation and existing route/recovery regressions pass. A distinct test cohort should confirm revisions when the first gate fails.

### Milestone 2 — Bring one short section to a convincing visual standard

**Engineering/technical art:** Lock camera orbit/world contract; restore an authored Volume profile; measure the RX 7600 baseline before defining detailed asset budgets. Keep decoration and gameplay support clearly separated.

**Art:** Rebuild the start, first bridge and nearby facade/midground as one coherent kit. Resolve equipment layers, physical scale, texture density, edge construction and plant rooting. Establish city coverage for supported views in both worlds. Refine or replace the courier once normal screen size is approved.

**Animation:** Inspect rig deformation and contact through this section; pair takeoff, descent and landings with the approved motion language.

**Level design:** Preserve the accepted lesson and collision margins; provide a composed destination and readable Shift transformation. Do not make an attractive screenshot that changes or obscures the crossing.

**Audio:** Complete this section's two related ambience beds, contact set and Shift signature, with a deliberate mix.

**Human playtesting/acceptance:** Owner approves a 20–30 second **real player recording**, HUD visible, plus orbit captures in both worlds against the concept. The section must look coherent from every supported view, with no matte boundary, conspicuous prop overlap or material-scale mismatch. Slow-motion review finds no obvious sustained sliding/penetration or major skinning issue. At least 7/8 fresh players identify usable destinations and the active world correctly; readability must survive the art pass. Meet the declared frame-time budget in representative play on the chosen baseline and record the measurements. Acceptance is an in-motion quality gate, not a still-image resemblance score.

### Milestone 3 — Validate the foundation for a larger game

**Engineering:** Build the smallest reusable encounter/anchor validation workflow demonstrated by a second blockout. Test both routes, different input devices and the declared minimum-spec PC. Use profiling evidence to decide batching/LOD/streaming needs.

**Level design/game direction:** Create three mechanically distinct encounters using current verbs and assemble a short progression through them. Establish a risk/reward reason to choose a fork and a learning-to-mastery curve. Decide whether the product is primarily precision traversal, spatial puzzle traversal or time-trial mastery based on play evidence.

**Art:** Extend only the approved kit far enough to test variety and production cost. Check transformed landmark consistency; document the effort of producing one new encounter rather than extrapolating from a screenshot.

**Animation:** Demonstrate the accepted motion set across diagonal approaches, narrow landings, different elevations and longer drops.

**Audio:** Extend the same feedback language without new contradictory cues; test recognizability during harder combined actions.

**Human playtesting/acceptance:** Use 10–12 players, with returning players separated from new ones. They can explain three different encounter decisions; no encounter requires coaching for most players; no required lesson has a reproduced ordinary bypass. At least half voluntarily replay, and returning players improve through learned strategies rather than avoiding bad camera/UI. Record route choices, abandonment and reasons for replay/refusal. A second encounter is authored and validated without coordinate/name-specific repair code. A second licensed clean-checkout build works. Agree minimum PC hardware and verify the visual benchmark there before content commitments. If decision variety or voluntary replay remains weak, revise the mechanic/format rather than commission additional levels.

## E. Next five actions

1. **Run an uncoached baseline test of the exact current build.** Use the opening/Shift sequence and a full route, keyboard plus physical gamepad. Capture screen/input and comments; establish whether failures are rule, control, camera or timing problems. This supplies the missing evidence for the highest-cost choices.
2. **Repair lesson and HUD communication as one bounded pass.** Fix the reproduced wrapped notice; add look/pause/retry discovery; reduce decorative preview noise and attach the solid-state cue to the first bridge. Confirm understanding with new players.
3. **Lock and tune the movement/camera contract.** Compare a few coordinated settings; resolve landing visibility, air steering and allowed orbit. Fix skyline coverage for that contract. Rerun reach/recovery tests whenever ranges change.
4. **Produce one cohesive opening art/animation/audio benchmark.** Replace inconsistent foreground construction, resolve character motion and material/contact lighting, and judge actual play footage in both worlds. Preserve accepted collision and lesson margins.
5. **Test three distinct encounter decisions before greenlighting expansion.** Use cheap blockouts and voluntary replay/route-choice evidence to decide the product format. Establish reproducible build/animation guards and measured hardware budgets as gates to further production.

Postpone additional finished levels, an explorable city, moving platforms, extra powers, leaderboards, monetization, mobile/store launch work and a large architecture rewrite until these actions establish clarity, coherent visual production and sustained player interest. A larger asset count is not a substitute for those results.

## F. Evidence, reproduction and limitations

### Read and inspected

Required documents read in full: README.md, GAME_DESIGN.md, ART_DIRECTION.md, MILESTONE_1_PROGRESS.md, THIRD_PARTY.md and AI_REVIEW.md. Previous ratings/reviews were treated as hypotheses. Also inspected ArtDirection/Milestone1/README.md, current Golden scene serialization, historical Skyline comparison evidence, Runner prefab, CourierMotion controller, GhostPreview and V4 materials, CityVolume asset, project/package/build settings, and runtime/editor/QA code cited above.

Visual evidence viewed: approved `ArtDirection/visual-target-gameview-v2.png` (**generated concept**); `ArtDirection/Milestone1/present.png` and `overgrown.png` (**existing actual player renders, no HUD**); `ArtDirection/Comparison/skyline-compare.png` (**historical gameplay comparison**). The concept's rich skyline/vegetation is not proof of implemented 3D assets. License/provenance records were inspected; external asset licenses were not re-audited.

### Fresh work executed for this review

- GitHub PR fetch verified the current head against local HEAD. Initial direct Git network access was unavailable; the authenticated connector supplied PR metadata.
- `dotnet run --project Tools/JumpIntentChecks/JumpIntentChecks.csproj`: **11 assertions passed** against production source.
- A fresh graphics-enabled Unity build without batchmode: **scene validation and Windows build passed**, exit 0, `Logs/whole-game-review-build-interactive.log`. Exact successful invocation:

```powershell
Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.25f1/Editor/Unity.exe' -ArgumentList '-quit -force-d3d11 -projectPath "C:/Users/Admin/Desktop/LINDI/Shiftbound-Game/UnityProject" -executeMethod SliceDelivery.BuildWindows -logFile "C:/Users/Admin/Desktop/LINDI/Shiftbound-Game/Logs/whole-game-review-build-interactive.log"' -WindowStyle Hidden
```

- Fresh player `-shiftboundRegression`, `-shiftboundSmoke` and `-shiftboundReachAudit -shiftboundRequireShiftLessons`: **passed**, separate `Logs/whole-game-review-{regression,smoke,reach}.log`.
- Full deterministic route at 30/60/120/144 Hz and variable steps: **all passed**, `Logs/whole-game-review-route-{30,60,120,144,0}.log`.
- Rendered production GameInput/motor Update/live-camera routes at 30/60/120/144 caps: **all passed**, `Logs/whole-game-review-production-{30,60,120,144}.log`. These caps are requested diagnostic settings, not measured performance claims.
- A fresh rendered production-input route produced **191 camera frames**, `Logs/whole-game-review-motion.log`; packaged using `Tools/PackMotionCapture.ps1` as `.validation/WholeGameReview/current-route.avi`. Silent, sampled/scripted, excludes IMGUI HUD, and uses nominal packaging intervals; unsuitable for timing/performance claims. Selected route/completion frames were visually inspected, not a complete continuous listening/motion audit.
- Normal player, no diagnostic flags: controlled **OS-injected scan-code keyboard/mouse inputs** exercised W, Space, left Shift, right-mouse orbit, Escape and R. Observed opening crossing, first bridge crossing, guidance lifecycle, a deliberate fall, Overgrown checkpoint recovery, pause and full restart. Captured HUD-inclusive 1280×720 frames and an 800×450 checkpoint frame under `.validation/WholeGameReview/`. This is a limited functional/visual session, not a human manual full-level completion.
- Tracked gameplay/scene/asset files remained unchanged; review report and local diagnostic/evidence artifacts are separate. Local logs/builds/captures are ignored repository artifacts and are not GitHub attachments.

### Failures and limits

The first sandbox Editor attempt failed local Package Manager IPC; approved external batchmode reached IPC but failed license entitlement with exit 198. The non-batch graphics-enabled Editor build succeeded. This is an environment/build-mode limitation, not a compile defect. An initial reviewer-written route invocation had malformed argument construction and launched no valid route test; it was stopped and excluded. The corrected named route logs above supply the results. Early window captures/input attempts and splash-screen frames were discarded as evidence of controls; mapped scan-code inputs and ready gameplay captures supplied the observations.

I have **not performed a human playtest**, physical keyboard/gamepad validation, audio listening audit, full wall/occlusion matrix, exhaustive shortcut search, continuous skinning/foot-contact review, Unity Test Framework suite, profiler session, second-machine build, Android/touch/device validation or sustained whole-game progression test. No claim of enjoyment, commercial viability or measured FPS follows from passing checks. Skyline was a source/image comparison, not freshly rebuilt or played. Previous WhatsApp video conclusions are documented history, not independently replayed evidence in this review.

The strongest conclusions are the verified build/traversal foundations, the foreground-versus-concept gap, the reproduced skyline orbit break and notification overflow. Movement comfort, lesson comprehension after the prompt repair, gamepad menus, audiovisual quality and long-term mechanic depth remain provisional and should determine the next investment gate.

[motor]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/PlayerMotor.cs#L53
[motor-feedback]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/PlayerMotor.cs#L112
[motor-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L37618
[camera-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L29365
[camera-resolver]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/FollowCamera.cs#L88
[backdrop]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/SkylineBackdrop.cs#L31
[hud]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/PremiumHUD.cs#L87
[hud-lesson]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/PremiumHUD.cs#L111
[hud-notice]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/PremiumHUD.cs#L132
[flow-guidance]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/GameFlow.cs#L21
[route-runner]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/GameplayCompareRunner.cs#L67
[switcher]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/WorldSwitcher.cs#L94
[animation]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/RiggedCourierAnimator.cs#L33
[animation-validator]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Editor/SliceDelivery.cs#L86
[verify-script]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/Tools/VerifyMilestone1.ps1#L25
[v4-art]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Editor/BuildArtPassV4.cs#L38
[v5-art]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/OpeningBenchmarkRuntimeV5.cs#L43
[volume-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L95692
[audio-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L105301
[audio-code]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scripts/FeedbackAudio.cs#L34
[start-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L85484
[bridge-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L12514
[midair-scene]: https://github.com/fnebihi10/Shiftbound-Game/blob/4bffa80f53dc4f733f434c0c69971c8b3b79f10d/UnityProject/Assets/Shiftbound/Scenes/GoldenRooftops.unity#L126805
