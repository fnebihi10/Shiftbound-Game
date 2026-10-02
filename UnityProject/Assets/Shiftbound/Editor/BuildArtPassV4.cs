using System;
using System.Collections.Generic;
using System.Linq;
using Shiftbound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class BuildArtPassV4
{
    private const string Root = "Assets/Shiftbound";
    private const string Textures = Root + "/TexturesV4";
    private const string Materials = Root + "/MaterialsV4";
    private const string Source = Root + "/Scenes/SkylineRooftops.unity";
    private const string Target = Root + "/Scenes/GoldenRooftops.unity";

    [MenuItem("Shiftbound/Build Golden Rooftops V4")]
    public static void Build()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Target) != null)
            throw new InvalidOperationException("GoldenRooftops already exists. Duplicate or move the authored scene before regenerating V4.");
        if (!AssetDatabase.IsValidFolder(Materials)) AssetDatabase.CreateFolder(Root, "MaterialsV4");
        EditorSceneManager.OpenScene(Source, OpenSceneMode.Single);
        Transform v3 = GameObject.Find("City art V3").transform;
        Transform shared = GameObject.Find("Shared rooftop structure").transform;
        Transform present = GameObject.Find("Present world geometry").transform;
        Transform overgrown = GameObject.Find("Overgrown world geometry").transform;
        Transform runner = GameObject.Find("Runner").transform;
        WorldSwitcher worlds = UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        FollowCamera cameraRig = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        Camera camera = Camera.main;

        RemoveFlatSkyline(v3);
        Transform art = new GameObject("Art V4 - rooftop and distant skyline").transform;
        Material presentDeck = Surface("Present rooftop concrete", "concrete_floor_01", 2.4f, 0.14f);
        Material overgrownDeck = Surface("Overgrown rooftop concrete", "concrete_moss", 2.4f, 0.12f);
        Material facade = Surface("Weathered facade", "concrete_brick_wall_001", 2f, 0.16f);
        Material ochre = Solid("Weathered ochre equipment", new Color(0.68f, 0.36f, 0.09f), 0.28f);
        Material steel = Solid("Dark blue steel", new Color(0.10f, 0.16f, 0.20f), 0.38f);
        Material metalEdge = Solid("Sunlit metal trim", new Color(0.58f, 0.55f, 0.46f), 0.48f);
        Material gold = Solid("Warm brass handrail", new Color(0.91f, 0.51f, 0.11f), 0.55f);
        Material leaf = Solid("Foliage green", new Color(0.19f, 0.41f, 0.18f), 0.1f);
        Material seam = Solid("Worn stone joints", new Color(0.29f, 0.27f, 0.24f), 0.04f);
        Material bag = Solid("Courier backpack", new Color(0.15f, 0.18f, 0.19f), 0.28f);
        Material scarf = Solid("Courier blue scarf", new Color(0.07f, 0.47f, 0.61f), 0.25f);
        Material ivy = IvyMaterial();

        // The prototype skyline consisted of almost identical blocks. Keep nearby real 3D
        // buildings, but let the layered matte establish the distant city and horizon.
        foreach (Transform child in v3)
            if (child.name == "Neighbouring city block")
                child.GetComponent<Renderer>().sharedMaterial = facade;
        foreach (Transform child in v3)
        {
            if (child.name == "Neighbouring city block" || child.name == "Neighbouring roof trim" ||
                child.name == "Recessed window" || child.name == "Window ledge")
                child.position += Vector3.down * 2.4f;
        }

        List<Renderer> sharedDecks = Decks(shared);
        foreach (Renderer deck in sharedDecks) deck.sharedMaterial = presentDeck;
        foreach (Renderer deck in Decks(present)) deck.sharedMaterial = presentDeck;
        foreach (Renderer deck in Decks(overgrown)) deck.sharedMaterial = presentDeck;

        var artState = art.gameObject.AddComponent<RooftopWorldArt>();
        artState.worlds = worlds;
        artState.sharedDecks = sharedDecks.ToArray();
        artState.presentDeck = presentDeck;
        artState.overgrownDeck = presentDeck; // Shared rooftop remains stone; growth gathers at its edges.
        List<GameObject> onlyOvergrown = new List<GameObject>();

        AddSkyline(art, worlds, camera);
        AddStartRoofEquipment(art, ochre, steel, metalEdge, gold);
        AddForegroundArchitecture(art, steel, gold, metalEdge);
        AddPavingJoints(art, seam);
        AddVines(art, ivy, overgrownDeck, onlyOvergrown);
        AddCourierAccents(runner, bag, metalEdge, scarf);
        artState.overgrownOnly = onlyOvergrown.ToArray();

        cameraRig.distance = 8f;
        cameraRig.lookHeight = 1.45f;
        cameraRig.lookAhead = 2.3f;
        camera.fieldOfView = 66f;
        camera.farClipPlane = 245f;
        RenderSettings.fogStartDistance = 120f;
        RenderSettings.fogEndDistance = 235f;
        RenderSettings.ambientLight = new Color(0.75f, 0.73f, 0.68f);
        var atmosphere = v3.GetComponent<AtmosphereController>();
        if (atmosphere != null)
        {
            atmosphere.presentAmbient = new Color(0.67f, 0.72f, 0.75f);
            atmosphere.alteredAmbient = new Color(0.75f, 0.73f, 0.68f);
        }
        worlds.sun.intensity = 1.25f;
        worlds.sun.shadowStrength = 0.7f;

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Target);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Target, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("Shiftbound Golden Rooftops V4 created: " + Target);
    }

    private static void RemoveFlatSkyline(Transform v3)
    {
        string[] prefixes = { "Skyline tower", "Skyline cornice", "Skyline glass face",
            "Far city viaduct", "Viaduct pier", "Pier shadow inset", "Viaduct balustrade" };
        for (int i = v3.childCount - 1; i >= 0; i--)
        {
            Transform child = v3.GetChild(i);
            if (prefixes.Any(prefix => child.name.StartsWith(prefix, StringComparison.Ordinal)))
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
    }

    private static List<Renderer> Decks(Transform parent)
    {
        var result = new List<Renderer>();
        foreach (Transform child in parent)
        {
            if (child.GetComponent<BoxCollider>() == null) continue;
            Vector3 size = child.localScale;
            if (size.x < 1.5f || size.z < 1.5f || size.y > 1.1f) continue;
            if (child.name.Contains("edge") || child.name.Contains("rail") ||
                child.name.Contains("parapet")) continue;
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null) result.Add(renderer);
        }
        return result;
    }

    private static Texture2D Texture(string file, bool normal = false)
    {
        string path = Textures + "/" + file;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new Exception("Missing V4 texture: " + path);
        bool changed = false;
        if (normal && importer.textureType != TextureImporterType.NormalMap)
        {
            importer.textureType = TextureImporterType.NormalMap;
            changed = true;
        }
        if (normal && !importer.flipGreenChannel)
        {
            importer.flipGreenChannel = true; // Poly Haven source is DirectX; Unity expects +Y.
            changed = true;
        }
        if (importer.maxTextureSize != 2048) { importer.maxTextureSize = 2048; changed = true; }
        if (importer.wrapMode != TextureWrapMode.Repeat && !file.StartsWith("Skyline_"))
        { importer.wrapMode = TextureWrapMode.Repeat; changed = true; }
        if (changed) importer.SaveAndReimport();
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) throw new Exception("Could not import texture: " + path);
        return texture;
    }

    private static Material Surface(string name, string source, float repeat, float smoothness)
    {
        string path = Materials + "/" + name.Replace(" ", "") + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", source == "concrete_moss" ? Color.white :
            source == "concrete_brick_wall_001" ? new Color(1.18f, 1.18f, 1.16f) :
            new Color(1.03f, 1.08f, 1.13f));
        material.SetTexture("_BaseMap", Texture(source + "_Diffuse.jpg"));
        material.SetTexture("_BumpMap", Texture(source + "_nor_dx.jpg", true));
        material.SetFloat("_BumpScale", 0.75f);
        material.EnableKeyword("_NORMALMAP");
        material.SetTextureScale("_BaseMap", Vector2.one * repeat);
        material.SetTextureScale("_BumpMap", Vector2.one * repeat);
        material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material Solid(string name, Color color, float smoothness)
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
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material Unlit(string name, Texture2D texture, bool alphaCutout)
    {
        string path = Materials + "/" + name.Replace(" ", "") + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetTexture("_BaseMap", texture);
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Cull", 0f);
        if (alphaCutout)
        {
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.45f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.AlphaTest;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material IvyMaterial()
    {
        return Unlit("Hanging ivy cutout", Texture("Ivy_Hanging.png"), true);
    }

    private static void AddSkyline(Transform parent, WorldSwitcher worlds, Camera camera)
    {
        Material present = Unlit("Distant city present", Texture("Skyline_Present.png"), false);
        Material altered = Unlit("Distant city overgrown", Texture("Skyline_Overgrown.png"), false);
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Distant city matte - visual backdrop only";
        quad.transform.SetParent(parent);
        UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.GetComponent<MeshRenderer>().sharedMaterial = present;
        var backdrop = quad.AddComponent<SkylineBackdrop>();
        backdrop.viewer = camera;
        backdrop.worlds = worlds;
        backdrop.presentMaterial = present;
        backdrop.overgrownMaterial = altered;
        backdrop.distance = 110f;
    }

    private static GameObject Box(Transform parent, string name, Vector3 position,
        Vector3 size, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = size;
        item.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static void AddStartRoofEquipment(Transform art, Material ochre,
        Material steel, Material trim, Material brass)
    {
        foreach (int sign in new[] { -1, 1 })
        {
            float x = sign * 2.7f;
            Material shell = sign > 0 ? ochre : steel;
            // Non-colliding shells wrap the V3 ventilation collider and give it a stronger silhouette.
            Box(art, "HVAC weathered enclosure", new Vector3(x, 0.55f, 2.4f),
                new Vector3(1.34f, 1.12f, 1.44f), shell);
            Box(art, "HVAC overhanging lid", new Vector3(x, 1.16f, 2.4f),
                new Vector3(1.52f, 0.11f, 1.56f), trim);
            Box(art, "HVAC inset grille", new Vector3(x, 0.52f, 1.664f),
                new Vector3(0.93f, 0.59f, 0.035f), steel);
            for (int i = 0; i < 5; i++)
                Box(art, "HVAC louver", new Vector3(x, 0.28f + i * 0.12f, 1.633f),
                    new Vector3(0.79f, 0.025f, 0.04f), trim);
            for (int ix = -1; ix <= 1; ix += 2)
            for (int iy = -1; iy <= 1; iy += 2)
                Box(art, "HVAC fastener", new Vector3(x + ix * 0.56f,
                    0.55f + iy * 0.45f, 1.64f), new Vector3(0.065f, 0.065f, 0.035f), trim);
        }
        for (int sign = -1; sign <= 1; sign += 2)
        {
            float x = sign * 4.3f;
            Box(art, "Rooftop safety rail", new Vector3(x, 0.71f, 2.1f),
                new Vector3(0.09f, 0.10f, 7.4f), brass);
            for (int i = 0; i < 4; i++)
                Box(art, "Safety rail upright", new Vector3(x, 0.43f, -1.2f + i * 2.2f),
                    new Vector3(0.09f, 0.58f, 0.09f), steel);
        }
    }

    private static void AddForegroundArchitecture(Transform art, Material steel,
        Material brass, Material trim)
    {
        // Pipes and roof-edge brackets provide scale and parallax near the gameplay route.
        foreach (int sign in new[] { -1, 1 })
        {
            float x = sign * 4.7f;
            Box(art, "Roof edge service pipe", new Vector3(x, -0.3f, 2.2f),
                new Vector3(0.12f, 0.12f, 8.1f), brass);
            for (int i = 0; i < 5; i++)
            {
                Box(art, "Service pipe bracket", new Vector3(x, -0.25f, -1.1f + i * 1.7f),
                    new Vector3(0.25f, 0.22f, 0.11f), steel);
            }
        }
        Box(art, "Start rooftop front weathered fascia", new Vector3(0f, -0.62f, 6.4f),
            new Vector3(9.7f, 0.5f, 0.15f), trim);
    }


    private static void AddPavingJoints(Transform art, Material seam)
    {
        // Irregular thin joints break up the start deck at the same scale as the concept's slabs.
        for (int i = 0; i < 6; i++)
        {
            float z = -0.55f + i * 1.28f;
            Box(art, "Stone slab transverse joint", new Vector3(0f, 0.008f, z),
                new Vector3(7.6f, 0.012f, 0.018f), seam);
        }
        for (int i = 0; i < 5; i++)
        {
            float x = -3.1f + i * 1.55f;
            Box(art, "Stone slab staggered joint", new Vector3(x, 0.009f,
                i % 2 == 0 ? 2.45f : 3.1f),
                new Vector3(0.018f, 0.012f, 6.9f), seam);
        }
    }    private static void AddVines(Transform art, Material ivy, Material leaf,
        List<GameObject> onlyOvergrown)
    {
        Vector3[] positions =
        {
            new Vector3(3.18f, 0.8f, 1.58f),
            new Vector3(-3.13f, 0.82f, 1.58f),
            new Vector3(-4.25f, 0.33f, 5.8f),
            new Vector3(4.23f, 0.28f, 5.7f),
            new Vector3(-2.7f, 0.23f, 8.3f),
            new Vector3(2.8f, 0.2f, 11.2f),
            new Vector3(-2.5f, 0.2f, 17.5f),
            new Vector3(2.5f, 0.2f, 29.2f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Quad);
            foliage.name = "Overgrown ivy curtain";
            foliage.transform.SetParent(art);
            foliage.transform.position = positions[i];
            foliage.transform.rotation = Quaternion.Euler(0f, i % 2 == 0 ? 0f : 20f, 0f);
            foliage.transform.localScale = new Vector3(i < 4 ? 1.15f : 0.9f,
                i < 4 ? 1.8f : 1.3f, 1f);
            foliage.GetComponent<Renderer>().sharedMaterial = ivy;
            UnityEngine.Object.DestroyImmediate(foliage.GetComponent<Collider>());
            onlyOvergrown.Add(foliage);
        }
        // Larger hanging silhouettes tie nearby real buildings into the painted skyline.
        foreach (Vector3 p in new[]
        {
            new Vector3(-14f, -0.1f, 6f), new Vector3(14f, -0.1f, 8f),
            new Vector3(-14f, -0.4f, 19f), new Vector3(14f, -0.5f, 23f),
            new Vector3(-13f, -0.7f, 35f), new Vector3(13f, -0.7f, 38f)
        })
        {
            GameObject curtain = GameObject.CreatePrimitive(PrimitiveType.Quad);
            curtain.name = "Building facade ivy";
            curtain.transform.SetParent(art);
            curtain.transform.position = p;
            curtain.transform.localScale = new Vector3(2.6f, 3.8f, 1f);
            curtain.GetComponent<Renderer>().sharedMaterial = ivy;
            UnityEngine.Object.DestroyImmediate(curtain.GetComponent<Collider>());
            onlyOvergrown.Add(curtain);
        }
        for (int i = 0; i < 18; i++)
        {
            float z = -1f + (i % 9) * 0.92f;
            float x = i < 9 ? -3.8f : 3.8f;
            GameObject patch = Box(art, "Overgrown roof edge moss",
                new Vector3(x + (i % 3 - 1) * 0.12f, 0.012f, z),
                new Vector3(0.45f + (i % 3) * 0.16f, 0.025f, 0.27f), leaf);
            onlyOvergrown.Add(patch);
        }
    }

    private static void AddCourierAccents(Transform runner, Material bag,
        Material trim, Material scarf)
    {
        Transform courier = runner.Find("Courier - rigged hoodie");
        if (courier == null) return;
        Animator animator = courier.GetComponent<Animator>();
        if (animator == null || animator.avatar == null || !animator.avatar.isHuman) return;
        Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        if (spine == null) return;
        Vector3 backpackWorld = courier.TransformPoint(new Vector3(0f, 1.40f, -0.10f));
        Quaternion backpackRotation = courier.rotation;
        Transform pack = new GameObject("Courier travel backpack").transform;
        pack.SetParent(spine);
        pack.position = backpackWorld;
        pack.rotation = backpackRotation;
        Box(pack, "Backpack body", pack.TransformPoint(new Vector3(0f, 0f, -0.05f)),
            new Vector3(0.23f, 0.27f, 0.10f), bag);
        Box(pack, "Backpack flap", pack.TransformPoint(new Vector3(0f, 0.085f, -0.095f)),
            new Vector3(0.22f, 0.06f, 0.025f), trim);
        Box(pack, "Backpack emblem", pack.TransformPoint(new Vector3(0f, 0.03f, -0.11f)),
            new Vector3(0.06f, 0.06f, 0.012f), scarf);
        foreach (int sign in new[] { -1, 1 })
            Box(pack, "Backpack strap", pack.TransformPoint(new Vector3(sign * 0.095f, 0.04f, 0.05f)),
                new Vector3(0.037f, 0.26f, 0.035f), bag);
    }

    [MenuItem("Shiftbound/Capture Golden Rooftops V4")]
    public static void Capture()
    {
        EditorSceneManager.OpenScene(Target, OpenSceneMode.Single);
        Camera camera = Camera.main;
        var follow = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        var worlds = UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        var backdrop = UnityEngine.Object.FindFirstObjectByType<SkylineBackdrop>();
        if (camera == null || follow == null || worlds == null || backdrop == null)
            throw new Exception("V4 capture scene components missing");
        follow.Snap();
        camera.aspect = 16f / 9f;
        foreach (Renderer item in worlds.presentRoot.GetComponentsInChildren<Renderer>(true))
            item.enabled = false;
        foreach (Renderer item in worlds.alteredRoot.GetComponentsInChildren<Renderer>(true))
            item.enabled = true;
        var art = UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();
        foreach (GameObject item in art.overgrownOnly) if (item != null) item.SetActive(true);
        backdrop.presentMaterial = backdrop.overgrownMaterial;
        backdrop.Refresh();
        var courier = GameObject.Find("Courier - rigged hoodie");
        if (courier != null)
        {
            Animator animator = courier.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Play("Base Layer.Sprint", 0, 0.14f);
                animator.Update(0.12f);
                foreach (var skin in courier.GetComponentsInChildren<SkinnedMeshRenderer>())
                    Debug.Log("V4 COURIER " + skin.name + " materials: " +
                        string.Join(", ", skin.sharedMaterials.Select(m => m == null ? "null" : m.name)));
            }
        }
        RenderTexture target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        RenderTexture.active = target;
        camera.Render();
        Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        image.Apply();
        string output = System.IO.Path.GetFullPath(System.IO.Path.Combine(
            Application.dataPath, "../../ArtDirection/GoldenRooftops-editor-preview.png"));
        System.IO.File.WriteAllBytes(output, image.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(target);
        UnityEngine.Object.DestroyImmediate(image);
        Debug.Log("SHIFTBOUND V4 CAPTURE: " + output);
    }}
