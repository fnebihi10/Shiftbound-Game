# Production upgrade sources and provenance

Work in progress. See `../../CONTINUATION_PRODUCTION_UPGRADE.md` for review defects and unfinished acceptance.

The replacement courier uses the **Quaternius Universal Base Characters Standard** CC0 humanoid anatomy/rig already present in this repository (`UnityProject/Assets/Shiftbound/ThirdParty/Quaternius/Character/Superhero_Male_FullBody.fbx`) and CC0 hairstyle from the existing downloaded standard source archive. Existing Quaternius and source texture licensing is retained in `THIRD_PARTY.md` and the third-party directories. Clothing, textile atlas, bag, trim, footwear additions and skin-weight integration are authored in `Tools/AuthorProductionCourier.py` with Blender4.5.14LTS. Editable source is `Sources/ShiftboundCourier.blend`. No purchased assets or runtime cloth.

Constructed environment geometry and foliage are authored by `ProductionMeshKit.cs` and `ProductionCityAuthoring.cs`. Shared stone/concrete texture sources and their existing provenance remain in the repository. The new geometry adds scenery depth without adding a playable level or changing route colliders.

`DistantCity-v1.png` is an original image generated9October2026 using the built-in imagegen tool. It is a **pending integration candidate**, not a gameplay capture. Prompt: horizontally wrapping2:1 distant warm-limestone city matte viewed from35m, miniature horizon band, soft pale-blue sky/cream clouds/warm afternoon, mist below, no nearby objects/people/text/interface; complements actual geometry within200m. Image generation is not 3D character authoring or evidence of gameplay quality.

Review evidence and tool downloads are local under `.validation/ProductionUpgrade`, excluded from Git. Baseline build is retained, and separate upgrade build output is under `UnityProject/Builds/ProductionUpgrade`.
