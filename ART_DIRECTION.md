# Shiftbound art direction and quality bar

## Approved target and current reality

- [Approved concept image](ArtDirection/visual-target-gameview-v2.png): a cinematic, sunlit rooftop city with lush growth, atmospheric depth, detailed materials, strong composition, and a believable young courier.
- [Actual V4 Windows-player capture](ArtDirection/GoldenRooftops-player-capture.png): a playable level with new far skyline art, textured rooftops, adjusted camera, and foliage. The near buildings and courier still have a simpler, less convincing look and movement than the concept.

The concept is a 2D image used to communicate the intended look. It is not a finished 3D scene, rigged character, or Unity screenshot. The historical V4 far city is a finite world-space matte plane; it does not cover free orbit. Matching the concept in motion requires modeled and optimized 3D assets, materials, animation, level composition, lighting, and repeated in-game evaluation. The existing project has not achieved that full quality bar.

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

## Opening production benchmark candidate — 2026-10-09

The user-approved direction is **high-end stylized realism**, with contemporary rooftop architecture, credible human motion, warm sunlight, cool shadows, purposeful growth and restrained effects. This benchmark is an implementation candidate; its assets and final appearance have not received human art approval. See [the acceptance record](PRODUCTION_BENCHMARK.md) and [asset specifications](ArtDirection/ProductionBenchmark/ASSET_SPECIFICATIONS.md).

Implemented in GoldenRooftops: a physical-scale recessed-window kit for visible buildings, consolidated service equipment, roof UVs independent of slab dimensions, warm directional light with cool trilight ambient, restrained ACES grading, lit alpha-cutout ivy and paired panorama skyboxes. Original gameplay collision remains authoritative. Inactive-world decoration is hidden; collision surfaces retain hatched cyan previews. Concrete floors remain concrete in both worlds; growth gathers at edges. The courier retains the existing licensed model and animations, with gait phase preserved between Jog and Sprint.

The camera contract is full 360-degree yaw, pitch -15 to 58 degrees, normal FOV 60 degrees, distance 7.6 m, look height 1.45 m and look-ahead 1.9 m. Collision, recovery and camera-relative input are retained. Coverage must be tested in both worlds at every supported orbit; the lowest pitch is an inspection freedom, not an ideal traversal view.

Standards for subsequent work:

- Keep corresponding building geometry and landmarks stable between worlds. Differences should describe occupation, maintenance and growth rather than a different city.
- Use metre-based material scale and recessed construction. Detail must explain construction or wear. Avoid repeated pasted-on window blocks, overlapping equipment shells and randomly scattered greenery.
- Preserve clear landing edges and the active route at 1280×720 and 800×450. Preview hatching plus transparency must distinguish proposed support from solid support without relying only on color.
- Preserve responsive traversal independently of animation. Root-motion assets must not take control of approved motor distances or jump timing without a separate gameplay decision.
- Generated background art is a far-field texture. It supplies neither modeled near assets nor true depth/parallax, and must not be used as evidence that the foreground or courier meets the concept.
- Performance claims require standalone measurements on named hardware. This desktop result establishes no Android budget.

Unresolved: premium courier model and backpack skinning; a cohesive acceleration/stop/turn/jump/fall/land animation set; authored 3D rooted vegetation; roof paving and differentiated architectural materials; panorama angular resolution and seamless spherical projection; near/mid/far art consistency; naive-player readability, gamepad hardware and subjective visual/motion approval. These remain acceptance blockers, not permission to extend the level.
