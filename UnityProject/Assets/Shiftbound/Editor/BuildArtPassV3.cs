using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Shiftbound;

public static class BuildArtPassV3
{
    private const string Root = "Assets/Shiftbound";
    private const string Materials = Root + "/MaterialsV2";
    private const string SourceScene = Root + "/Scenes/FirstRooftop.unity";
    private const string TargetScene = Root + "/Scenes/SkylineRooftops.unity";
    private static System.Random random;

    [MenuItem("Shiftbound/Build Skyline Rooftops V3")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScene) == null)
            throw new InvalidOperationException("Historical V3 source is absent. Edit GoldenRooftops; do not regenerate authored work.");
        random = new System.Random(2046);
        if (!AssetDatabase.IsValidFolder(Materials))
            AssetDatabase.CreateFolder(Root, "MaterialsV2");
        EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
        GameObject oldArt = GameObject.Find("City art V3");
        if (oldArt != null) UnityEngine.Object.DestroyImmediate(oldArt);
        Transform art = new GameObject("City art V3").transform;

        Material concrete = Mat("Warm rooftop stone", new Color(0.72f, 0.72f, 0.67f), 0.24f, tiled: true);
        Material facade = Mat("City facades", new Color(0.55f, 0.63f, 0.64f), 0.18f, tiled: true);
        Material shadow = Mat("Deep slate", new Color(0.15f, 0.24f, 0.31f), 0.16f);
        Material trim = Mat("Pale trim", new Color(0.88f, 0.88f, 0.76f), 0.28f);
        Material darkMetal = Mat("Vents", new Color(0.10f, 0.16f, 0.20f), 0.48f);
        Material window = Mat("City glass", new Color(0.20f, 0.44f, 0.52f), 0.70f);
        Material cyan = Mat("Cyan route light", new Color(0.18f, 0.88f, 1f), 0.25f, emission: true);
        Material gold = Mat("Amber route light", new Color(1f, 0.68f, 0.19f), 0.25f, emission: true);
        Material moss = Mat("Leaf green", new Color(0.22f, 0.53f, 0.31f), 0.1f);
        Material leafLight = Mat("Fresh leaf", new Color(0.46f, 0.72f, 0.32f), 0.1f);
        Material bark = Mat("Bark", new Color(0.35f, 0.27f, 0.20f), 0.1f);
        Material jacket = Mat("Courier jacket", new Color(0.95f, 0.56f, 0.12f), 0.20f);
        Material trouser = Mat("Courier trousers", new Color(0.14f, 0.22f, 0.28f), 0.18f);
        Material skin = Mat("Courier skin", new Color(0.72f, 0.48f, 0.33f), 0.22f);
        Material hair = Mat("Courier hair", new Color(0.18f, 0.11f, 0.09f), 0.12f);
        Material scarfMat = Mat("Courier scarf", new Color(0.13f, 0.72f, 0.81f), 0.22f);
        Material softGhost = Mat("Route preview", new Color(0.55f, 0.90f, 0.97f, 0.19f), 0.1f, transparent: true);

        Transform shared = GameObject.Find("Shared rooftop structure").transform;
        Transform present = GameObject.Find("Present world geometry").transform;
        Transform altered = GameObject.Find("Overgrown world geometry").transform;
        Transform runner = GameObject.Find("Runner").transform;
        WorldSwitcher worlds = UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        GameFlow flow = UnityEngine.Object.FindFirstObjectByType<GameFlow>();
        FollowCamera follow = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        PlayerMotor motor = runner.GetComponent<PlayerMotor>();

        ReskinPlatforms(shared, concrete, shadow, trim, art, null);
        ReskinPlatforms(present, concrete, shadow, cyan, art, present);
        ReskinPlatforms(altered, concrete, shadow, gold, art, altered);
        worlds.ghostMaterial = softGhost;
        worlds.showLegacyBlockedFlash = false;
        flow.showLegacyHud = false;
        PremiumHUD hud = flow.gameObject.AddComponent<PremiumHUD>();
        hud.flow = flow;
        hud.worlds = worlds;

        // Architectural mass below shared rooftops keeps the level in one city.
        int initialCount = shared.childCount;
        for (int index = 0; index < initialCount; index++)
        {
            Transform platform = shared.GetChild(index);
            if (platform.name.Contains("edge") || platform.name.Contains("frame") ||
                platform.name.Contains("utility")) continue;
            Vector3 s = platform.localScale;
            if (s.y > 1.1f) continue;
            Box(art, platform.name + " building base",
                new Vector3(platform.position.x, -9.3f, platform.position.z),
                new Vector3(s.x + 1.3f, 18.2f, s.z + 1.3f), facade);
            Box(art, platform.name + " crown",
                new Vector3(platform.position.x, -0.4f, platform.position.z),
                new Vector3(s.x + 1.7f, 0.25f, s.z + 1.7f), trim);
        }

        AddRooftopRails(shared, trim, darkMetal);
        AddCity(art, facade, shadow, trim, window, cyan);
        AddVistaBridge(art, trim, shadow);
        AddVents(art, darkMetal, trim, moss, leafLight);
        AddOvergrowth(altered, moss, leafLight, bark);
        AddRooftopGarden(altered, moss, leafLight, bark);
        AddRouteAccent(present, cyan);
        AddRouteAccent(altered, gold);
        AddGoal(shared, gold, trim);
        MakeRiggedCourier(runner, motor, jacket, trouser, skin, hair, darkMetal);

        follow.distance = 8.0f;
        follow.lookHeight = 1.45f;
        follow.lookAhead = 2.3f;
        follow.positionSharpness = 10f;
        Camera.main.fieldOfView = 66f;
        Camera.main.farClipPlane = 245f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 68f;
        RenderSettings.fogEndDistance = 210f;
        RenderSettings.fogColor = new Color(0.65f, 0.74f, 0.79f);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.50f, 0.55f, 0.56f);
        Shader skyShader = Shader.Find("Skybox/Procedural");
        if (skyShader != null)
        {
            Material sky = MatSky(skyShader);
            RenderSettings.skybox = sky;
        }
        Light sun = worlds.sun;
        sun.transform.rotation = Quaternion.Euler(33f, -44f, 0f);
        sun.intensity = 1.1f;
        sun.shadowStrength = 0.55f;
        var atmosphere = art.gameObject.AddComponent<AtmosphereController>();
        atmosphere.worlds = worlds;
        atmosphere.presentFog = new Color(0.64f, 0.75f, 0.82f);
        atmosphere.alteredFog = new Color(0.78f, 0.73f, 0.61f);
        atmosphere.presentAmbient = new Color(0.51f, 0.57f, 0.62f);
        atmosphere.alteredAmbient = new Color(0.60f, 0.55f, 0.48f);
        AddPostProcessing(art);

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), TargetScene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(TargetScene, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Shiftbound skyline scene created: " + TargetScene);
    }

    private static Material Mat(string name, Color color, float smoothness,
        bool tiled = false, bool emission = false, bool transparent = false)
    {
        string path = Materials + "/" + name.Replace(" ", "") + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", smoothness);
        if (tiled)
        {
            Texture2D texture = StoneTexture();
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", new Vector2(2f, 2f));
        }
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.0f);
        }
        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material MatSky(Shader shader)
    {
        string path = Materials + "/RooftopSky.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader) { name = "Rooftop Sky" };
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_SkyTint", new Color(0.53f, 0.67f, 0.84f));
        material.SetColor("_GroundColor", new Color(0.59f, 0.62f, 0.60f));
        material.SetFloat("_AtmosphereThickness", 1.15f);
        material.SetFloat("_Exposure", 0.95f);
        material.SetFloat("_SunSize", 0.04f);
        return material;
    }

    private static Texture2D StoneTexture()
    {
        const string path = Materials + "/RooftopStoneTexture.asset";
        Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
        texture.name = "Rooftop stone texture";
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        var rng = new System.Random(929);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float variation = 0.88f + (float)rng.NextDouble() * 0.12f;
            bool seam = x % 32 < 2 || (y + (x / 32 % 2) * 16) % 32 < 2;
            if (seam) variation *= 0.75f;
            texture.SetPixel(x, y, new Color(variation, variation, variation, 1f));
        }
        texture.Apply();
        AssetDatabase.CreateAsset(texture, path);
        return texture;
    }

    private static GameObject Box(Transform parent, string name, Vector3 position,
        Vector3 scale, Material material, bool collider = false)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static GameObject Part(Transform parent, string name, PrimitiveType shape,
        Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(shape);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.localPosition = localPosition;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = localScale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static void ReskinPlatforms(Transform root, Material deck, Material baseMat,
        Material accent, Transform art, Transform routeRoot)
    {
        foreach (Transform child in root)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer == null) continue;
            if (child.name.Contains("edge"))
            {
                renderer.sharedMaterial = baseMat;
                continue;
            }
            if (child.name.Contains("platform") || child.name.Contains("bridge") ||
                child.name.Contains("rooftop") || child.name.Contains("landing") ||
                child.name.Contains("takeoff") || child.name.Contains("lookout") ||
                child.name.Contains("route") || child.name.Contains("alternate") ||
                child.name.Contains("present") || child.name.Contains("crossing"))
            {
                renderer.sharedMaterial = deck;
            }
        }
    }

    private static void AddRouteAccent(Transform route, Material glow)
    {
        int initialCount = route.childCount;
        for (int index = 0; index < initialCount; index++)
        {
            Transform platform = route.GetChild(index);
            if (platform.name.Contains("edge") || platform.name.Contains("wall") ||
                platform.name.Contains("Overgrowth")) continue;
            Vector3 s = platform.localScale;
            Vector3 p = platform.position;
            Box(route, platform.name + " front route light",
                new Vector3(p.x, 0.035f, p.z - s.z / 2f + 0.07f),
                new Vector3(s.x - 0.3f, 0.04f, 0.10f), glow);
            Box(route, platform.name + " left route light",
                new Vector3(p.x - s.x / 2f + 0.07f, 0.035f, p.z),
                new Vector3(0.10f, 0.04f, s.z - 0.3f), glow);
        }
    }

    private static void AddRooftopRails(Transform shared, Material trim, Material metal)
    {
        int initialCount = shared.childCount;
        for (int index = 0; index < initialCount; index++)
        {
            Transform platform = shared.GetChild(index);
            if (platform.name.Contains("edge") || !platform.name.Contains("rooftop") &&
                !platform.name.Contains("landing")) continue;
            Vector3 p = platform.position;
            Vector3 s = platform.localScale;
            for (int sign = -1; sign <= 1; sign += 2)
            {
                float x = p.x + sign * (s.x / 2f - 0.10f);
                Box(shared, platform.name + " side parapet",
                    new Vector3(x, 0.23f, p.z),
                    new Vector3(0.20f, 0.46f, s.z - 0.25f), trim, true);
                Box(shared, platform.name + " handrail",
                    new Vector3(x, 0.54f, p.z),
                    new Vector3(0.12f, 0.10f, s.z - 0.25f), metal, true);
            }
        }
    }

    private static void AddCity(Transform art, Material facade, Material shadow,
        Material trim, Material glass, Material glow)
    {
        // Low neighbouring roofs frame the route without turning it into a corridor.
        for (int side = -1; side <= 1; side += 2)
        {
            for (int index = 0; index < 8; index++)
            {
                float z = -10f + index * 11.5f;
                float width = 8f + (float)random.NextDouble() * 4f;
                float height = 14f + (float)random.NextDouble() * 11f;
                float x = side * (20f + (float)random.NextDouble() * 7f);
                float top = -2.8f + (float)random.NextDouble() * 4f;
                Box(art, "Neighbouring city block", new Vector3(x, top - height / 2f, z),
                    new Vector3(width, height, 9f), facade);
                Box(art, "Neighbouring roof trim", new Vector3(x, top, z),
                    new Vector3(width + 0.35f, 0.3f, 9.35f), trim);
                float face = x - side * (width / 2f + 0.06f);
                for (int floor = 0; floor < 3; floor++)
                {
                    float y = top - 1.9f - floor * 2.5f;
                    for (int column = 0; column < 3; column++)
                    {
                        float winZ = z - 2.5f + column * 2.5f;
                        Box(art, "Recessed window", new Vector3(face, y, winZ),
                            new Vector3(0.1f, 1.35f, 1.25f), glass);
                        Box(art, "Window ledge", new Vector3(face - side * 0.08f, y - 0.72f, winZ),
                            new Vector3(0.28f, 0.12f, 1.48f), shadow);
                    }
                }
            }
        }
        // Layered skyline sits well beyond the playable route. No colliders here.
        for (int layer = 0; layer < 2; layer++)
        {
            int count = layer == 0 ? 19 : 25;
            float spacing = layer == 0 ? 11f : 9f;
            float start = -spacing * (count - 1) / 2f;
            float depth = layer == 0 ? 108f : 151f;
            for (int i = 0; i < count; i++)
            {
                float x = start + i * spacing + ((float)random.NextDouble() - 0.5f) * 3f;
                float z = depth + (float)random.NextDouble() * 16f;
                float height = (layer == 0 ? 22f : 35f) + (float)random.NextDouble() * 30f;
                float width = 5f + (float)random.NextDouble() * 5f;
                Material body = i % 4 == 0 ? shadow : facade;
                Box(art, "Skyline tower", new Vector3(x, -18f + height / 2f, z),
                    new Vector3(width, height, 6f + (float)random.NextDouble() * 4f), body);
                Box(art, "Skyline cornice", new Vector3(x, -18f + height, z),
                    new Vector3(width + 0.6f, 0.35f, 8f), trim);
                if (i % 3 == 0)
                    Box(art, "Skyline glass face", new Vector3(x, -9f + height / 2f, z - 4.15f),
                        new Vector3(width * 0.42f, height * 0.5f, 0.09f), glass);
            }
        }
    }
    private static void AddVistaBridge(Transform art, Material stone, Material shadow)
    {
        const float z = 118f;
        Box(art, "Far city viaduct deck", new Vector3(0f, 21f, z),
            new Vector3(120f, 1.4f, 7f), stone);
        for (int i = -5; i <= 5; i++)
        {
            float x = i * 11f;
            Box(art, "Viaduct pier", new Vector3(x, 7f, z),
                new Vector3(1.6f, 27f, 6.5f), stone);
            Box(art, "Pier shadow inset", new Vector3(x + 1.1f, 13f, z - 3.2f),
                new Vector3(0.2f, 14f, 0.25f), shadow);
        }
        Box(art, "Viaduct balustrade", new Vector3(0f, 22.4f, z - 3f),
            new Vector3(120f, 1.3f, 0.6f), stone);
    }

    private static void AddRooftopGarden(Transform altered, Material moss,
        Material leaf, Material bark)
    {
        for (int i = 0; i < 11; i++)
        {
            float z = -0.7f + i * 0.72f;
            float x = i % 2 == 0 ? -3.7f : 3.7f;
            Plant(altered, new Vector3(x, 0f, z), moss, leaf);
        }
        foreach (float z in new[] { 18f, 30f, 42f, 54f })
        {
            Plant(altered, new Vector3(-2.15f, 0f, z), moss, leaf);
            Plant(altered, new Vector3(2.15f, 0f, z + 0.4f), moss, leaf);
        }
    }
    private static void AddVents(Transform art, Material metal, Material trim,
        Material moss, Material leaf)
    {
        foreach (Vector3 p in new[]
        {
            new Vector3(-2.7f, 0.55f, 2.4f),
            new Vector3(2.7f, 0.55f, 2.4f),
            new Vector3(-1.7f, 0.38f, 10f),
            new Vector3(1.8f, 0.40f, 42f)
        })
        {
            Box(art, "Rooftop ventilation unit", p,
                new Vector3(1.3f, 1.1f, 1.4f), metal, true);
            Box(art, "Vent lid", p + Vector3.up * 0.59f,
                new Vector3(1.48f, 0.12f, 1.58f), trim);
            for (int i = 0; i < 4; i++)
                Box(art, "Vent slat",
                    p + new Vector3(0f, -0.35f + i * 0.20f, -0.75f),
                    new Vector3(0.85f, 0.055f, 0.06f), trim);
            Plant(art, p + new Vector3(0.9f, -0.42f, 0.25f), moss, leaf);
        }
    }

    private static void AddOvergrowth(Transform altered, Material moss,
        Material leaf, Material bark)
    {
        int initialCount = altered.childCount;
        for (int index = 0; index < initialCount; index++)
        {
            Transform platform = altered.GetChild(index);
            if (platform.name.Contains("edge") || platform.name.Contains("wall") ||
                platform.name.Contains("Overgrowth")) continue;
            Vector3 p = platform.position;
            float width = platform.localScale.x;
            for (int i = 0; i < 3; i++)
            {
                float sign = i % 2 == 0 ? -1f : 1f;
                Plant(altered, new Vector3(p.x + sign * (width / 2f - 0.18f),
                    0f, p.z - 0.6f + i * 0.6f), moss, leaf);
            }
            if (platform.name.Contains("destination") || platform.name.Contains("alternate"))
            {
                Box(altered, "Overgrown wall fragment",
                    new Vector3(p.x + width / 2f + 0.5f, 0.6f, p.z),
                    new Vector3(0.7f, 1.4f, 1.0f), bark);
                Plant(altered, new Vector3(p.x + width / 2f + 0.3f, 1.3f, p.z),
                    moss, leaf);
            }
        }
    }

    private static void Plant(Transform parent, Vector3 p, Material moss, Material leaf)
    {
        Box(parent, "Stone planter with foliage", p + Vector3.up * 0.12f,
            new Vector3(0.48f, 0.24f, 0.48f), moss);
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "Plant stem";
        stem.transform.SetParent(parent);
        stem.transform.position = p + Vector3.up * 0.45f;
        stem.transform.localScale = new Vector3(0.055f, 0.30f, 0.055f);
        stem.GetComponent<Renderer>().sharedMaterial = moss;
        UnityEngine.Object.DestroyImmediate(stem.GetComponent<Collider>());
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 137.5f;
            float radians = angle * Mathf.Deg2Rad;
            float height = 0.39f + (i % 3) * 0.14f;
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            blade.name = "Pointed foliage leaf";
            blade.transform.SetParent(parent);
            blade.transform.position = p + new Vector3(Mathf.Cos(radians) * 0.20f,
                height, Mathf.Sin(radians) * 0.20f);
            blade.transform.rotation = Quaternion.Euler(25f + i * 7f, angle, -42f);
            blade.transform.localScale = new Vector3(0.12f, 0.39f, 0.075f);
            blade.GetComponent<Renderer>().sharedMaterial = leaf;
            UnityEngine.Object.DestroyImmediate(blade.GetComponent<Collider>());
        }
    }
    private static void AddGoal(Transform shared, Material gold, Material trim)
    {
        Box(shared, "Goal luminous header", new Vector3(0f, 2.52f, 55.2f),
            new Vector3(5.2f, 0.08f, 0.38f), gold);
        Box(shared, "Goal threshold", new Vector3(0f, 0.02f, 54.6f),
            new Vector3(3.8f, 0.06f, 0.2f), gold);
        for (int i = -2; i <= 2; i++)
            Box(shared, "Goal floor mark",
                new Vector3(i * 0.8f, 0.02f, 53.6f),
                new Vector3(0.3f, 0.04f, 0.15f), trim);
    }

    private static void MakeRiggedCourier(Transform runner, PlayerMotor motor,
        Material jacket, Material trousers, Material skin, Material hair, Material metal)
    {
        const string characterPath = InspectRiggedAssets.CharacterPath;
        const string animationPath = InspectRiggedAssets.AnimationPath;
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
        if (source == null) throw new Exception("Casual humanoid FBX is missing");
        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(characterPath)
            .OfType<Avatar>().FirstOrDefault();
        if (avatar == null || !avatar.isValid || !avatar.isHuman)
            throw new Exception("Casual humanoid avatar is invalid");

        var importer = AssetImporter.GetAtPath(animationPath) as ModelImporter;
        if (importer == null) throw new Exception("Animation library FBX is missing");
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        
        ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0
            ? importer.clipAnimations : importer.defaultClipAnimations;
        foreach (var clip in clips)
        {
            clip.loopTime = clip.name.Contains("Loop");
            clip.loopPose = clip.loopTime;
        }
        importer.clipAnimations = clips;
        importer.SaveAndReimport();

        Transform old = runner.Find("Runner visual");
        if (old != null) old.gameObject.SetActive(false);
        GameObject character = (GameObject)PrefabUtility.InstantiatePrefab(source);
        character.name = "Courier - rigged hoodie";
        character.transform.SetParent(runner, false);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.identity;
        character.transform.localScale = Vector3.one;
        motor.visual = character.transform;

        Material eye = Mat("Courier eyes", new Color(0.89f, 0.91f, 0.88f), 0.4f);
        Material shoe = Mat("Courier shoes", new Color(0.15f, 0.20f, 0.23f), 0.22f);
        foreach (SkinnedMeshRenderer renderer in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            switch (renderer.name)
            {
                case "Casual_Body": renderer.sharedMaterials = new[] { jacket, skin }; break;
                case "Casual_Feet": renderer.sharedMaterials = new[] { shoe, metal }; break;
                case "Casual_Head": renderer.sharedMaterials = new[] { skin, hair, eye, hair }; break;
                case "Casual_Legs": renderer.sharedMaterials = new[] { skin, trousers }; break;
            }
        }

        string controllerPath = Materials + "/CourierMotion.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        if (controller.layers.Length == 0) controller.AddLayer("Base Layer");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        ConfigureState(machine, "Idle", animationPath, "Idle_Loop");
        ConfigureState(machine, "Jog", animationPath, "Jog_Fwd_Loop");
        ConfigureState(machine, "Sprint", animationPath, "Sprint_Loop");
        ConfigureState(machine, "Jump", animationPath, "Jump_Loop");
        ConfigureState(machine, "Land", animationPath, "Jump_Land");
        machine.defaultState = machine.states.First(x => x.state.name == "Idle").state;
        Animator animator = character.GetComponent<Animator>();
        if (animator == null) animator = character.AddComponent<Animator>();
        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        character.AddComponent<RiggedCourierAnimator>().motor = motor;
    }

    private static void ConfigureState(AnimatorStateMachine machine, string stateName,
        string animationPath, string clipSuffix)
    {
        // An EndsWith match picks Crouch_Idle_Loop for Idle_Loop in this library.
        // Select the named clip exactly so rebuilding cannot reintroduce the crouch.
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(animationPath)
            .OfType<AnimationClip>()
            .Where(x => !x.name.StartsWith("__preview__", StringComparison.Ordinal))
            .FirstOrDefault(x => x.name == clipSuffix ||
                x.name.EndsWith("|" + clipSuffix, StringComparison.Ordinal) ||
                x.name.EndsWith("/" + clipSuffix, StringComparison.Ordinal));
        if (clip == null) throw new Exception("Missing animation clip " + clipSuffix);
        AnimatorState state = machine.states.Select(x => x.state)
            .FirstOrDefault(x => x.name == stateName);
        if (state == null) state = machine.AddState(stateName);
        state.motion = clip;
    }
    private static void MakeCourier(Transform runner, PlayerMotor motor,
        Material jacket, Material trouser, Material skin, Material hair,
        Material scarf, Material metal)
    {
        Transform old = runner.Find("Runner visual");
        if (old != null) old.gameObject.SetActive(false);
        Transform previous = runner.Find("Courier rig");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        Transform rig = new GameObject("Courier rig").transform;
        rig.SetParent(runner);
        rig.localPosition = Vector3.zero;
        motor.visual = rig;
        Part(rig, "Torso", PrimitiveType.Capsule,
            new Vector3(0f, 1.18f, 0f), new Vector3(0.62f, 0.46f, 0.36f), jacket);
        Part(rig, "Head", PrimitiveType.Sphere,
            new Vector3(0f, 1.73f, 0.08f), new Vector3(0.49f, 0.55f, 0.49f), skin);
        Part(rig, "Hair", PrimitiveType.Sphere,
            new Vector3(0f, 1.94f, 0.04f), new Vector3(0.52f, 0.28f, 0.51f), hair);
        Part(rig, "Backpack", PrimitiveType.Cube,
            new Vector3(0f, 1.20f, -0.29f), new Vector3(0.46f, 0.53f, 0.22f), metal);
        Transform scarfPivot = new GameObject("Scarf pivot").transform;
        scarfPivot.SetParent(rig);
        scarfPivot.localPosition = new Vector3(0f, 1.50f, -0.1f);
        Part(scarfPivot, "Scarf tail", PrimitiveType.Cube,
            new Vector3(0f, -0.12f, -0.23f), new Vector3(0.22f, 0.43f, 0.10f), scarf);
        Part(rig, "Scarf collar", PrimitiveType.Cylinder,
            new Vector3(0f, 1.51f, 0f), new Vector3(0.38f, 0.07f, 0.38f), scarf);
        Transform leftArm = Limb(rig, "Left arm", -0.38f, 1.42f, jacket, skin, true);
        Transform rightArm = Limb(rig, "Right arm", 0.38f, 1.42f, jacket, skin, true);
        Transform leftLeg = Limb(rig, "Left leg", -0.17f, 0.84f, trouser, metal, false);
        Transform rightLeg = Limb(rig, "Right leg", 0.17f, 0.84f, trouser, metal, false);
        var motion = rig.gameObject.AddComponent<CourierVisual>();
        motion.motor = motor;
        motion.leftArm = leftArm;
        motion.rightArm = rightArm;
        motion.leftLeg = leftLeg;
        motion.rightLeg = rightLeg;
        motion.scarf = scarfPivot;
    }

    private static Transform Limb(Transform rig, string name, float x, float y,
        Material top, Material end, bool arm)
    {
        Transform pivot = new GameObject(name + " pivot").transform;
        pivot.SetParent(rig);
        pivot.localPosition = new Vector3(x, y, 0f);
        Part(pivot, name, PrimitiveType.Capsule,
            new Vector3(0f, arm ? -0.26f : -0.32f, 0f),
            new Vector3(arm ? 0.22f : 0.25f, arm ? 0.32f : 0.37f, 0.24f), top);
        Part(pivot, arm ? "Glove" : "Shoe", PrimitiveType.Cube,
            new Vector3(0f, arm ? -0.57f : -0.69f, arm ? 0f : 0.10f),
            new Vector3(0.24f, 0.18f, arm ? 0.25f : 0.42f), end);
        return pivot;
    }

    private static void AddPostProcessing(Transform art)
    {
        string path = Materials + "/CityVolume.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);
        }
        Bloom bloom; if (!profile.TryGet(out bloom)) bloom = profile.Add<Bloom>();
        bloom.intensity.Override(0.24f);
        bloom.threshold.Override(1.1f);
        Vignette vignette; if (!profile.TryGet(out vignette)) vignette = profile.Add<Vignette>();
        vignette.intensity.Override(0.12f);
        ColorAdjustments grade; if (!profile.TryGet(out grade)) grade = profile.Add<ColorAdjustments>();
        grade.contrast.Override(10f);
        grade.saturation.Override(7f);
        var go = new GameObject("Color and bloom");
        go.transform.SetParent(art);
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.profile = profile;
    }
}
















