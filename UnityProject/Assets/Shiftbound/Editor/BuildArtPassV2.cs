using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Shiftbound;

public static class BuildArtPassV2
{
    private const string Root = "Assets/Shiftbound";
    private const string Materials = Root + "/MaterialsV2";
    private const string SourceScene = Root + "/Scenes/FirstRooftop.unity";
    private const string TargetScene = Root + "/Scenes/CityRooftops.unity";
    private static System.Random random;

    [MenuItem("Shiftbound/Build City Rooftops V2")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScene) == null)
            throw new InvalidOperationException("Historical V2 source is absent. Edit GoldenRooftops; do not regenerate authored work.");
        random = new System.Random(2046);
        if (!AssetDatabase.IsValidFolder(Materials))
            AssetDatabase.CreateFolder(Root, "MaterialsV2");
        EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
        GameObject oldArt = GameObject.Find("City art V2");
        if (oldArt != null) UnityEngine.Object.DestroyImmediate(oldArt);
        Transform art = new GameObject("City art V2").transform;

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
        AddVents(art, darkMetal, trim, moss, leafLight);
        AddOvergrowth(altered, moss, leafLight, bark);
        AddRouteAccent(present, cyan);
        AddRouteAccent(altered, gold);
        AddGoal(shared, gold, trim);
        MakeCourier(runner, motor, jacket, trouser, skin, hair, scarfMat, darkMetal);

        follow.distance = 4.9f;
        follow.lookHeight = 1.25f;
        follow.lookAhead = 1.35f;
        follow.positionSharpness = 10f;
        Camera.main.fieldOfView = 61f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 34f;
        RenderSettings.fogEndDistance = 125f;
        RenderSettings.fogColor = new Color(0.58f, 0.70f, 0.76f);
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
        sun.intensity = 1.35f;
        sun.shadowStrength = 0.65f;
        art.gameObject.AddComponent<AtmosphereController>().worlds = worlds;
        AddPostProcessing(art);

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), TargetScene);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(TargetScene, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Shiftbound redesigned scene created: " + TargetScene);
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
        material.SetFloat("_Exposure", 1.25f);
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
        for (int side = -1; side <= 1; side += 2)
        {
            for (int index = 0; index < 9; index++)
            {
                float z = -9f + index * 9.5f;
                float width = 6f + (float)random.NextDouble() * 4f;
                float height = 18f + (float)random.NextDouble() * 15f;
                float x = side * (10f + (float)random.NextDouble() * 4f);
                float top = 0.5f + (float)random.NextDouble() * 4f;
                float centerY = top - height / 2f;
                Box(art, "Near city building", new Vector3(x, centerY, z),
                    new Vector3(width, height, 7f), facade);
                Box(art, "City roof lip", new Vector3(x, top, z),
                    new Vector3(width + 0.45f, 0.28f, 7.5f), trim);
                float innerFace = x - side * (width / 2f + 0.045f);
                for (int floor = 0; floor < 4; floor++)
                {
                    float y = top - 2.2f - floor * 2.6f;
                    for (int column = 0; column < 3; column++)
                    {
                        float winZ = z - 2.25f + column * 2.25f;
                        Box(art, "Window", new Vector3(innerFace, y, winZ),
                            new Vector3(0.10f, 1.2f, 1.1f), glass);
                        Box(art, "Window sill", new Vector3(innerFace - side * 0.02f, y - 0.68f, winZ),
                            new Vector3(0.16f, 0.09f, 1.25f), shadow);
                    }
                }
                if (index % 3 == 0)
                    Box(art, "City light strip",
                        new Vector3(innerFace - side * 0.04f, top - 0.7f, z),
                        new Vector3(0.14f, 0.08f, 6.8f), glow);
            }
        }
        for (int i = 0; i < 15; i++)
        {
            float x = -65f + i * 9f;
            float z = 82f + (float)random.NextDouble() * 22f;
            float height = 25f + (float)random.NextDouble() * 45f;
            Box(art, "Distant skyline tower",
                new Vector3(x, -13f + height / 2f, z),
                new Vector3(6f + (float)random.NextDouble() * 6f, height, 7f), facade);
            Box(art, "Distant skyline cap",
                new Vector3(x, -12.8f + height, z),
                new Vector3(7.2f, 0.5f, 7.8f), trim);
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
        GameObject pot = Box(parent, "Planter", p + Vector3.up * 0.15f,
            new Vector3(0.48f, 0.3f, 0.48f), moss);
        for (int i = 0; i < 3; i++)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Leaf cluster";
            orb.transform.SetParent(parent);
            orb.transform.position = p + new Vector3((i - 1) * 0.22f, 0.47f + i % 2 * 0.12f, 0f);
            orb.transform.localScale = new Vector3(0.38f, 0.42f, 0.32f);
            orb.GetComponent<Renderer>().sharedMaterial = leaf;
            UnityEngine.Object.DestroyImmediate(orb.GetComponent<Collider>());
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




