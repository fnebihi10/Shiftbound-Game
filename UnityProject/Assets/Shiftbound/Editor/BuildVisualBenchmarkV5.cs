using System;
using System.Collections.Generic;
using Shiftbound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Additive, repeatable art pass for the opening 15–30 seconds. All V5 meshes are visual only.
public static class BuildVisualBenchmarkV5
{
    private const string Scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";
    private const string Folder = "Assets/Shiftbound/VisualBenchmarkV5";
    private const string RootName = "Visual Benchmark V5 - opening roofs";

    [MenuItem("Shiftbound/Build Opening Visual Benchmark V5")]
    public static void Build()
    {
        EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Shiftbound", "VisualBenchmarkV5");
        var old = GameObject.Find(RootName);
        if (old != null) UnityEngine.Object.DestroyImmediate(old);

        var root = new GameObject(RootName).transform;
        var shared = Child(root, "Shared visual construction");
        var growth = Child(root, "Overgrown visual growth");
        var art = UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();
        if (art == null) throw new InvalidOperationException("V4 RooftopWorldArt is missing.");
        var concrete = Load("Presentrooftopconcrete");
        var weathered = Load("Weatheredfacade");
        var dark = Load("Darkbluesteel");
        var ochre = Load("Weatheredochreequipment");
        var moss = Load("Overgrownrooftopconcrete");
        var leafA = Material("Muted leaf", new Color(0.15f, 0.30f, 0.12f), 0.06f);
        var leafB = Material("Sunlit leaf", new Color(0.29f, 0.43f, 0.16f), 0.07f);
        var grout = Material("Expansion joint", new Color(0.17f, 0.19f, 0.19f), 0.08f);
        var metalWear = Material("Aged zinc", new Color(0.43f, 0.46f, 0.43f), 0.34f);
        var tar = Material("Bitumen patch", new Color(0.10f, 0.12f, 0.12f), 0.07f);
        Mesh bevel = BevelBox();
        Mesh leaves = LeafCluster();

        // Shallow, open parapets preserve the route and camera sightline. Existing colliders remain authoritative.
        foreach (float z in new[] { -0.7f, 1.3f, 3.3f, 5.3f })
        foreach (float x in new[] { -4.05f, 4.05f })
        {
            Piece(shared, "Parapet weathered pier", bevel, weathered, new Vector3(x, 0.27f, z),
                new Vector3(0.28f, 0.55f, 0.30f));
            Piece(shared, "Parapet stone cap", bevel, concrete, new Vector3(x, 0.58f, z),
                new Vector3(0.39f, 0.095f, 0.42f));
        }
        foreach (float x in new[] { -4.05f, 4.05f })
        {
            Piece(shared, "Continuous roof coping", bevel, concrete, new Vector3(x, 0.40f, 2.3f),
                new Vector3(0.32f, 0.12f, 6.8f));
            Piece(shared, "Recessed parapet face", bevel, weathered, new Vector3(x, 0.12f, 2.3f),
                new Vector3(0.21f, 0.50f, 6.6f));
            Piece(shared, "Drip edge", bevel, metalWear, new Vector3(x, -0.23f, 2.3f),
                new Vector3(0.32f, 0.07f, 6.8f));
        }

        // Make the forward edge read as a constructed roof rather than a single slab.
        Piece(shared, "Start roof layered fascia", bevel, weathered, new Vector3(0f, -0.82f, 6.94f),
            new Vector3(8.35f, 0.54f, 0.24f));
        Piece(shared, "Start roof flashing", bevel, metalWear, new Vector3(0f, -0.32f, 6.96f),
            new Vector3(8.4f, 0.08f, 0.34f));
        foreach (float x in new[] { -3.35f, -1.65f, 0.05f, 1.75f, 3.45f })
            Piece(shared, "Fascia support bracket", bevel, dark, new Vector3(x, -0.69f, 7.09f),
                new Vector3(0.075f, 0.4f, 0.16f));
        foreach (float z in new[] { 7.05f, 11.95f, 16.45f })
            Piece(shared, "Landing edge metal flashing", bevel, metalWear, new Vector3(0f, -0.22f, z),
                new Vector3(z < 13f ? 5.05f : 3.05f, 0.07f, 0.13f));

        // Service equipment is built from a shared chamfered mesh, with grilles, feet and cable trays.
        foreach (float x in new[] { -2.7f, 2.7f })
        {
            var casing = Piece(shared, "HVAC chamfered service casing", bevel, x < 0f ? dark : ochre,
                new Vector3(x, 0.58f, 2.40f), new Vector3(1.36f, 1.16f, 1.46f));
            casing.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;
            Piece(shared, "HVAC raised lid", bevel, metalWear, new Vector3(x, 1.19f, 2.40f),
                new Vector3(1.48f, 0.11f, 1.57f));
            foreach (float side in new[] { -0.47f, 0.47f })
                Piece(shared, "HVAC mounting foot", bevel, dark, new Vector3(x + side, 0.05f, 2.40f),
                    new Vector3(0.18f, 0.12f, 1.12f));
            for (int i = 0; i < 6; i++)
                Piece(shared, "HVAC angled cooling blade", bevel, metalWear,
                    new Vector3(x, 0.28f + i * 0.11f, 1.60f), new Vector3(0.78f, 0.025f, 0.08f));
            Piece(shared, "HVAC utility conduit", bevel, dark,
                new Vector3(x, 0.045f, 3.48f), new Vector3(0.08f, 0.08f, 0.78f));
        }

        Piece(shared, "Cable tray near roof edge", bevel, dark, new Vector3(-3.42f, 0.055f, 4.9f),
            new Vector3(0.23f, 0.09f, 2.15f));
        for (int i = 0; i < 5; i++)
            Piece(shared, "Cable tray retaining strap", bevel, metalWear,
                new Vector3(-3.42f, 0.13f, 4.02f + i * 0.43f), new Vector3(0.27f, 0.025f, 0.05f));
        Piece(shared, "Roof drain frame", bevel, metalWear, new Vector3(3.35f, 0.016f, 5.44f),
            new Vector3(0.67f, 0.03f, 0.72f));
        Piece(shared, "Roof drain throat", bevel, tar, new Vector3(3.35f, 0.036f, 5.44f),
            new Vector3(0.53f, 0.008f, 0.56f));
        for (int i = 0; i < 5; i++)
            Piece(shared, "Roof drain grate", bevel, dark,
                new Vector3(3.10f + i * 0.125f, 0.048f, 5.44f), new Vector3(0.035f, 0.016f, 0.55f));

        // Localized wear near drainage and service points; the central running lane stays clean.
        foreach (Vector3 p in new[] { new Vector3(3.25f, 0.012f, 4.58f),
            new Vector3(-3.43f, 0.012f, 4.1f), new Vector3(2.82f, 0.012f, 0.98f) })
            Piece(shared, "Localized bitumen repair", bevel, tar, p,
                new Vector3(0.36f, 0.012f, 0.75f));
        foreach (float z in new[] { 0.15f, 2.52f, 4.88f })
            Piece(shared, "Subtle deck expansion seam", bevel, grout, new Vector3(0f, 0.017f, z),
                new Vector3(7.2f, 0.009f, 0.018f));

        var addedGrowth = new List<GameObject>();
        int seed = 27;
        foreach (Vector3 p in new[] {
            new Vector3(-3.75f, 0.08f, -0.45f), new Vector3(-3.69f, 0.08f, 0.78f),
            new Vector3(-3.78f, 0.08f, 3.17f), new Vector3(-3.70f, 0.08f, 5.64f),
            new Vector3(3.71f, 0.08f, 0.21f), new Vector3(3.76f, 0.08f, 2.97f),
            new Vector3(3.60f, 0.08f, 5.30f), new Vector3(3.30f, 0.08f, 5.58f),
            new Vector3(-2.19f, 0.08f, 9.05f), new Vector3(2.15f, 0.08f, 11.2f),
            new Vector3(-1.34f, 0.08f, 14.08f), new Vector3(1.24f, 0.08f, 15.54f) })
        {
            float scale = 0.47f + (seed % 5) * 0.10f;
            var cluster = Piece(growth, "Irregular rooted leaf cluster", leaves,
                seed % 3 == 0 ? leafB : leafA, p, Vector3.one * scale);
            cluster.transform.rotation = Quaternion.Euler(0f, seed * 43f, 0f);
            addedGrowth.Add(cluster);
            seed++;
        }
        foreach (float x in new[] { -3.79f, 3.79f })
        for (int i = 0; i < 5; i++)
        {
            float z = -0.5f + i * 1.45f + (i % 2) * 0.22f;
            addedGrowth.Add(Piece(growth, "Irregular parapet root line", bevel, moss,
                new Vector3(x, 0.014f, z), new Vector3(0.19f + i % 3 * 0.08f, 0.018f, 0.38f)));
        }
        addedGrowth.Add(Piece(growth, "Drain moss accumulation", bevel, moss,
            new Vector3(3.35f, 0.022f, 4.91f), new Vector3(0.39f, 0.02f, 0.32f)));

        var allGrowth = new List<GameObject>();
        foreach (GameObject item in art.overgrownOnly ?? Array.Empty<GameObject>())
            if (item != null) allGrowth.Add(item);
        allGrowth.AddRange(addedGrowth);
        art.overgrownOnly = allGrowth.ToArray();
        EditorUtility.SetDirty(art);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("SHIFTBOUND V5 BENCHMARK SAVED: additive visuals, " + addedGrowth.Count + " new growth groups; collision unchanged.");
    }

    private static Transform Child(Transform parent, string name)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent);
        return child;
    }

    private static Material Load(string name)
    {
        var result = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shiftbound/MaterialsV4/" + name + ".mat");
        if (result == null) throw new InvalidOperationException("Missing V4 material: " + name);
        return result;
    }

    private static Material Material(string name, Color color, float smoothness)
    {
        string path = Folder + "/" + name.Replace(" ", "") + ".mat";
        var result = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (result == null)
        {
            result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(result, path);
        }
        result.SetColor("_BaseColor", color);
        result.SetFloat("_Smoothness", smoothness);
        result.enableInstancing = true;
        EditorUtility.SetDirty(result);
        return result;
    }

    private static GameObject Piece(Transform parent, string name, Mesh mesh, Material material,
        Vector3 position, Vector3 scale)
    {
        var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = scale;
        item.GetComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = item.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = true;
        return item;
    }

    private static Mesh BevelBox()
    {
        string path = Folder + "/ChamferedModule.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        float[] heights = { -0.5f, -0.43f, 0.43f, 0.5f };
        float[] widths = { 0.43f, 0.5f, 0.5f, 0.43f };
        for (int band = 0; band < 3; band++)
        for (int side = 0; side < 4; side++)
        {
            Vector2 a = Corner(side, widths[band]);
            Vector2 b = Corner((side + 1) % 4, widths[band]);
            Vector2 c = Corner((side + 1) % 4, widths[band + 1]);
            Vector2 d = Corner(side, widths[band + 1]);
            Quad(vertices, triangles, uv,
                new Vector3(a.x, heights[band], a.y), new Vector3(d.x, heights[band + 1], d.y),
                new Vector3(c.x, heights[band + 1], c.y), new Vector3(b.x, heights[band], b.y));
        }
        Quad(vertices, triangles, uv, new Vector3(-0.43f, 0.5f, -0.43f),
            new Vector3(-0.43f, 0.5f, 0.43f), new Vector3(0.43f, 0.5f, 0.43f),
            new Vector3(0.43f, 0.5f, -0.43f));
        Quad(vertices, triangles, uv, new Vector3(-0.43f, -0.5f, 0.43f),
            new Vector3(-0.43f, -0.5f, -0.43f), new Vector3(0.43f, -0.5f, -0.43f),
            new Vector3(0.43f, -0.5f, 0.43f));
        var mesh = new Mesh { name = "Reusable chamfered rooftop module" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, uv);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Vector2 Corner(int i, float width)
    {
        switch (i) { case 0: return new Vector2(-width, -width);
            case 1: return new Vector2(width, -width);
            case 2: return new Vector2(width, width);
            default: return new Vector2(-width, width); }
    }

    private static Mesh LeafCluster()
    {
        string path = Folder + "/RootedLeafCluster.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        for (int i = 0; i < 9; i++)
        {
            float angle = i * 2.39996f;
            Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 tangent = new Vector3(-radial.z, 0f, radial.x);
            float height = 0.32f + (i % 3) * 0.10f;
            Vector3 basePoint = radial * 0.07f;
            Vector3 mid = radial * (0.25f + i % 2 * 0.06f) + Vector3.up * (height * 0.7f);
            Vector3 tip = radial * (0.40f + i % 3 * 0.05f) + Vector3.up * height;
            int n = vertices.Count;
            vertices.Add(basePoint); vertices.Add(mid - tangent * 0.105f);
            vertices.Add(tip); vertices.Add(mid + tangent * 0.105f);
            uv.Add(Vector2.zero); uv.Add(Vector2.up); uv.Add(Vector2.one); uv.Add(Vector2.right);
            vertices.AddRange(new[] { vertices[n], vertices[n + 1], vertices[n + 2], vertices[n + 3] });
            uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
            triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3,
                n + 6, n + 5, n + 4, n + 7, n + 6, n + 4 });
        }
        var mesh = new Mesh { name = "Reusable rooted leaf cluster" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, uv);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static void Quad(List<Vector3> vertices, List<int> triangles, List<Vector2> uv,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int n = vertices.Count;
        vertices.AddRange(new[] { a, b, c, d });
        uv.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
        triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
    }
}
