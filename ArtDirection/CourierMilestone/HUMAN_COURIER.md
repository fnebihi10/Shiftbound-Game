# Human courier candidate — 11 October 2026

This replaces the earlier muscular, stylized authoring base with a more natural
person wearing a textured field jacket, jeans and athletic shoes. It is an
integrated review candidate, not accepted finished production art.

The anatomical, garment, shoe, hair and eye graphics come from the official
[MakeHuman CC0 system pack](https://static.makehumancommunity.org/assets/assetpacks/makehuman_system_assets.html).
The authoring tool is [MPFB 2.0.18](https://github.com/makehumancommunity/mpfb2/releases/tag/v2.0.18).
Its GPL generator stays outside the game; exported core graphics are covered by
the retained `CourierHuman/LICENSE.CC0.md`. Our authoring recipe is
`Tools/EvaluateMakeHumanCourier.py`; its sandboxed editable master is
`.validation/CourierMilestone/MPFB/CourierAnatomyMaster.blend`.

The fitted parcel pack is authored against the actual jacket, including
coat-weighted shoulder straps, rolled opening, compression straps, bound seams,
shallow zipped pocket, reflective mark and a woven canvas material. Its roughly
40 cm height and 29 cm width stay inside the shoulder/torso silhouette. A single
skinned parcel renderer replaces twelve separate detail renderers. The complete
courier has seven skinned renderers and 31,058 triangles before Unity's vertex
splitting. Samsung cost remains unmeasured.

The actor is scaled to 1.77 m and aligned using the actual soles. Foot support now
evaluates cached vertices with both foot and toe weights rather than assuming
rigid shoes. This supports deformation without per-frame mesh reads. It does
not by itself establish convincing toe push-off in every animation.

The original Jog stays default. Existing eleven authored action clips retarget
to the new Humanoid; jump refinements strengthen the leading knee, trailing heel
and changing balance without delaying input. Experimental walking/running/
sprinting remains available through `-shiftboundAuthoredGait`. Motor, collision,
Shift rules, route, camera and world composition are preserved.

Rejected iterations remain available: Human1 had incorrect eye texture and a
back-facing reflective mark; Human2 corrected them but retained a plain pack;
Human3 refined the pack; Human4 consolidates its renderer and refines jumping.
Isolated Human2/3 Editor tests failed before gameplay when Play Mode was requested
before first-use Search.index creation. The test now defers the request through
startup callbacks with all error checks still active. Human3 then passed actual
Unity Play Mode twice through the route, checkpoint recovery and retained retry.
The cause is consistent with [Unity's index creation guard](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/QuickSearch/Editor/Indexing/SearchDatabase.cs).

Comparison captures preserve the baseline prefab's own scale. Earlier Human2
comparisons incorrectly inherited the candidate avatar's different scale and
must not be used for exact size judgement. Human3 rear and Human4 front captures
correct this. Matched views use the same resolution, camera, motor and world;
fixed capture timing is visual evidence, not profiling.

Remaining courier weaknesses: generic face/hair treatment, neutral facial
expression, airborne transitions and landing anticipation, toe push-off and
plant/pivot ramps during sharp turns. Selected poses and gameplay frames have
been inspected; continuous normal-speed viewing/listening and human feel review
remain open. Compilation, route success and low measured sliding in small contact
subsets do not accept these criteria. Foreground work still waits for courier
acceptance. The full original production task remains unfinished.
