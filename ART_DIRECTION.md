# Shiftbound art direction and quality bar

## Approved target and current reality

- [Approved concept image](ArtDirection/visual-target-gameview-v2.png): a cinematic, sunlit rooftop city with lush growth, atmospheric depth, detailed materials, strong composition, and a believable young courier.
- [Actual V4 Windows-player capture](ArtDirection/GoldenRooftops-player-capture.png): a playable level with new far skyline art, textured rooftops, adjusted camera, and foliage. The near buildings and courier still have a simpler, less convincing look and movement than the concept.

The concept is a 2D image used to communicate the intended look. It is not a finished 3D scene, rigged character, or Unity screenshot. The V4 far city is a camera-facing matte layer. Matching the concept in motion requires modeled and optimized 3D assets, materials, animation, level composition, lighting, and repeated in-game evaluation. The existing V4 scene has not achieved that full quality bar.

## Production target for one excellent playable rooftop section

1. **Character:** Create or license a courier with human proportions, a distinctive hair silhouette, jacket, backpack, trousers, shoes, clean skinning, detailed materials, and strong readability at gameplay camera distance. The current Quaternius character is a useful rigged stand-in, but its shape and detail are not the final target.
2. **Motion:** Tune idle, acceleration, run, stop, turn, jump, fall, and landing as one cohesive set. Check feet against the ground and hands against the backpack and body. Evaluate in a gameplay video, not a still.
3. **Near environment:** Replace repetitive boxes with authored building facades, varied rooftop edges, vents, pipes, rails, drains, banners, wear, and irregular plant growth. Preserve level collision and world-switch safety.
4. **Far city:** Keep atmospheric depth and varied silhouettes. The V4 matte improves the current forward view but needs a proper panoramic/impostor solution for free camera turns and future levels.
5. **Lighting and materials:** Balance warm sunlight, cool shadows, sky, exposure, contact shadow, readable surfaces, and restrained post effects. Make the present and overgrown worlds clearly related but distinct.
6. **Interface and performance:** Refine the HUD for desktop and mobile aspect ratios. Profile on a midrange Android device before setting texture, geometry, transparency, shadow, and post-effect budgets for a Play Store target.

## Next milestone

Build a small art benchmark with one start rooftop, the courier, nearby facades, and a city slice. Use actual Game-view captures and a short running/jumping video to compare it with the concept. The visual gate is a credible character and foreground from the normal camera distance, plus stable performance. Only then extend the quality across the whole level.

The current V4 pass is an incremental step and a useful baseline, not the end of art production.
