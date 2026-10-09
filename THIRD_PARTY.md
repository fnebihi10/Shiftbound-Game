# Third-party and generated visual assets

## Quaternius

Selected character models, textures, and animations under `UnityProject/Assets/Shiftbound/ThirdParty/Quaternius/` came from [Universal Base Characters](https://quaternius.com/packs/universalbasecharacters.html), [Ultimate Modular Men Pack](https://quaternius.com/packs/ultimatemodularcharacters.html), and [Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html). Source license files in the project identify CC0 1.0 for the base character and animation packs; the modular character pack's official page identifies it as CC0.

## Poly Haven

The V4 surface textures in `UnityProject/Assets/Shiftbound/TexturesV4/` include diffuse and normal maps from [Concrete Floor 01](https://polyhaven.com/a/concrete_floor_01), [Concrete Moss](https://polyhaven.com/a/concrete_moss), and [Concrete Brick Wall 001](https://polyhaven.com/a/concrete_brick_wall_001). These are listed as CC0 on the official asset pages. The source downloads were verified against Poly Haven API MD5 checksums.

## Original generated concept and background images

`ArtDirection/visual-target-gameview-v2.png` is generated concept art. `Skyline_Present.png`, `Skyline_Overgrown.png`, and `Ivy_Hanging.png` in `TexturesV4/` were generated for this project with the built-in image-generation tool. Their prompts are recorded in [ArtDirection/GENERATED_ASSETS.md](ArtDirection/GENERATED_ASSETS.md). The city images are distant visual mattes; the ivy image is an alpha-cutout foliage card. They are not evidence of implemented 3D geometry. `ArtDirection/GoldenRooftops-player-capture.png` is an actual rendered frame from the V4 Windows player.

Preserve the provenance and license records if assets are replaced or redistributed.

## Opening production benchmark candidate

`ProductionBenchmark/Textures/CityPresent.png`, `CityOvergrown.png` and `IvyCutout.png` were generated/edited with the built-in image-generation tool on 2026-10-09. The final city pair uses the approved concept as a style reference, then an edit of the same city to remove roof forests and facade ivy for Present. An initial overly ornate city pair was rejected during actual-player inspection and replaced. The ivy is an edit of the existing project ivy, requesting a transparent background and neutral lighting; the resulting PNG contains actual alpha-zero background pixels. These are raster runtime texture assets, not 3D models, animations or gameplay captures. See [the specifications and limitations](ArtDirection/ProductionBenchmark/ASSET_SPECIFICATIONS.md).

The facade/window and consolidated service-unit meshes were authored in this repository for this benchmark. Their materials reuse the existing licensed Poly Haven surface maps; no external asset license has been inferred for an unavailable premium courier or animation set.

