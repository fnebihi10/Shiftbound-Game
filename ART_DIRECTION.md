# Shiftbound art direction and quality bar

## Approved target and current reality

- [Approved concept image](ArtDirection/visual-target-gameview-v2.png): a cinematic, sunlit rooftop city with lush growth, atmospheric depth, detailed materials, strong composition, and a believable young courier.
- [Actual V3 Unity preview](ArtDirection/SkylineRooftops-editor-preview.png): procedural, repeated tower and rooftop geometry, flat surfaces, simple lighting and sky, and a stock low-poly character.

The concept is a 2D illustration generated to communicate the intended look. It is not a 3D asset, a screenshot of a finished level, or a texture that can simply be placed behind gameplay. Matching it in motion requires designed and optimized 3D assets, materials, animation, scene composition, lighting, and repeated in-Editor evaluation. The existing scene has not achieved that.

## Target for the first production-quality area

Build one short playable rooftop section at high quality before extending the whole route. Keep the world-switch mechanic and aligned landmarks. Use the concept as a visual brief, while checking every decision in the actual Unity Game view at gameplay camera distance.

1. **Character:** A human-proportioned courier inspired by the concept: recognizable hair silhouette, jacket/hoodie, backpack, trousers, and shoes. The model needs clean rigging, believable skinning, purposeful colors and materials, and visible detail at the normal camera distance. Replacing a model alone is insufficient without good animation.
2. **Movement:** A cohesive idle, acceleration, run, stop, turn, jump, fall, and land set with sensible blending and timing. Feet should contact the ground consistently. The camera should give the player room while keeping landing surfaces readable.
3. **Near rooftops:** Hand-authored arrangement and varied silhouettes. Use proper parapets, vents, pipes, rooftop equipment, worn surface variation, rails, banners, and localized plants. Remove obvious repetition and empty uniform slabs.
4. **Far city:** Layered skyline with varied building masses, bridges/viaducts, foliage, and atmospheric perspective. Background geometry must feel integrated with the playable space rather than like a wall of identical towers. Avoid costly detail where it cannot be seen.
5. **Lighting and materials:** One coherent time of day, a plausible sky, warm sunlight, cool shadows, tuned exposure, readable material response, and restrained post-processing. Overgrown and present worlds need distinct visual identity while sharing recognizable landmarks.
6. **Interface:** Compact, readable HUD that does not overlap or distract from the scene. It must work at both desktop and mobile aspect ratios.
7. **Performance:** Profile on a target midrange Android device before declaring the scene suitable for Play Store. Pick texture, geometry, transparency, shadow, and post-processing budgets from device measurements, not a desktop screenshot.

## Production order

1. Make a small art benchmark scene with one rooftop, the character, one opposing building, and a slice of skyline. Match its **in-game screenshot** to the concept's composition and feeling.
2. Choose an asset route: create original models/textures, license compatible production assets, or commission character/environment work. Record provenance and redistribution rights for every asset.
3. Integrate and tune character rig, materials, animation, and movement at the actual gameplay scale.
4. Replace blockout geometry in the full level using the benchmark's visual language. Keep collisions and switch rules reliable.
5. Add sound, effects, world-state lighting, UI, and performance passes. Test on real devices and with players.

## Review gate

A candidate art pass should be judged with an actual Unity Game-view capture from the start rooftop and a short gameplay video. Compare both against the approved concept and the current Unity preview. A still render alone cannot show movement quality, readability, or performance.
