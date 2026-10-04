using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Shiftbound
{
    // Runtime fallback while the V5 authoring pass has not been baked into the scene.
    // Visual meshes never own gameplay collision. The Editor menu can bake the same benchmark.
    public sealed class OpeningBenchmarkRuntimeV5 : MonoBehaviour
    {
        private const string RootName = "Visual Benchmark V5 - opening roofs";
        private readonly List<GameObject> growth = new List<GameObject>();
        private Mesh bevel;
        private Mesh leaf;
        private Material stone;
        private Material steel;
        private Material zinc;
        private Material leafDark;
        private Material leafLight;
        private Material patch;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (SceneManager.GetActiveScene().name != "GoldenRooftops" || GameObject.Find(RootName) != null)
                return;
            new GameObject(RootName).AddComponent<OpeningBenchmarkRuntimeV5>();
        }

        private void Awake()
        {
            var art = FindFirstObjectByType<RooftopWorldArt>();
            if (art == null || art.worlds == null) { Destroy(gameObject); return; }
            bevel = MakeBevel();
            leaf = MakeLeafCluster();
            stone = Existing("Presentrooftopconcrete");
            steel = Existing("Darkbluesteel");
            zinc = NewMaterial("V5 aged zinc", new Color(0.42f, 0.47f, 0.45f), 0.35f);
            leafDark = NewMaterial("V5 deep leaves", new Color(0.15f, 0.29f, 0.12f), 0.06f);
            leafLight = NewMaterial("V5 sunlit leaves", new Color(0.29f, 0.43f, 0.16f), 0.07f);
            patch = NewMaterial("V5 bitumen repair", new Color(0.11f, 0.13f, 0.13f), 0.06f);
            if (stone == null || steel == null) { Destroy(gameObject); return; }

            var solids = new GameObject("Visual construction").transform;
            solids.SetParent(transform);
            var plants = new GameObject("Overgrown rooted plants").transform;
            plants.SetParent(transform);
            foreach (float x in new[] { -4.05f, 4.05f })
            {
                Add(solids, "Roof parapet wall", bevel, stone, new Vector3(x, 0.13f, 2.3f),
                    new Vector3(0.23f, 0.50f, 6.6f));
                Add(solids, "Roof coping", bevel, stone, new Vector3(x, 0.43f, 2.3f),
                    new Vector3(0.34f, 0.11f, 6.8f));
                Add(solids, "Roof drip edge", bevel, zinc, new Vector3(x, -0.25f, 2.3f),
                    new Vector3(0.32f, 0.065f, 6.8f));
                for (int i = 0; i < 4; i++)
                    Add(solids, "Parapet stone pier", bevel, stone,
                        new Vector3(x, 0.24f, -0.7f + i * 2f),
                        new Vector3(0.32f, 0.49f, 0.34f));
            }
            Add(solids, "Layered roof fascia", bevel, stone, new Vector3(0f, -0.81f, 6.95f),
                new Vector3(8.35f, 0.53f, 0.23f));
            Add(solids, "Metal roof flashing", bevel, zinc, new Vector3(0f, -0.34f, 6.95f),
                new Vector3(8.4f, 0.08f, 0.32f));
            for (int i = 0; i < 5; i++)
                Add(solids, "Fascia support", bevel, steel,
                    new Vector3(-3.35f + i * 1.7f, -0.69f, 7.09f),
                    new Vector3(0.075f, 0.38f, 0.15f));
            foreach (float x in new[] { -2.7f, 2.7f })
            {
                Add(solids, "Chamfered HVAC housing", bevel, steel,
                    new Vector3(x, 0.57f, 2.4f), new Vector3(1.36f, 1.15f, 1.45f));
                Add(solids, "HVAC zinc lid", bevel, zinc,
                    new Vector3(x, 1.18f, 2.4f), new Vector3(1.49f, 0.11f, 1.56f));
                for (int i = 0; i < 5; i++)
                    Add(solids, "HVAC cooling blade", bevel, zinc,
                        new Vector3(x, 0.28f + i * 0.12f, 1.61f),
                        new Vector3(0.79f, 0.025f, 0.075f));
            }
            Add(solids, "Grated roof drain", bevel, zinc, new Vector3(3.35f, 0.019f, 5.44f),
                new Vector3(0.7f, 0.03f, 0.73f));
            Add(solids, "Drain throat", bevel, patch, new Vector3(3.35f, 0.039f, 5.44f),
                new Vector3(0.56f, 0.008f, 0.58f));
            for (int i = 0; i < 5; i++)
                Add(solids, "Drain grate bar", bevel, steel,
                    new Vector3(3.10f + i * 0.125f, 0.05f, 5.44f),
                    new Vector3(0.035f, 0.016f, 0.55f));
            foreach (Vector3 p in new[] { new Vector3(3.24f, 0.012f, 4.6f),
                new Vector3(-3.42f, 0.012f, 4.1f) })
                Add(solids, "Localized surface repair", bevel, patch, p,
                    new Vector3(0.37f, 0.014f, 0.71f));

            int seed = 17;
            foreach (Vector3 p in new[] { new Vector3(-3.72f, 0.08f, 0.17f),
                new Vector3(-3.74f, 0.08f, 2.21f), new Vector3(-3.68f, 0.08f, 5.34f),
                new Vector3(3.74f, 0.08f, 0.95f), new Vector3(3.68f, 0.08f, 3.35f),
                new Vector3(3.33f, 0.08f, 5.51f), new Vector3(-2.11f, 0.08f, 9.12f),
                new Vector3(2.06f, 0.08f, 11.18f), new Vector3(-1.31f, 0.08f, 14.21f),
                new Vector3(1.24f, 0.08f, 15.48f) })
            {
                var item = Add(plants, "Rooted leaf cluster", leaf,
                    seed % 3 == 0 ? leafLight : leafDark, p, Vector3.one * (0.44f + seed % 4 * 0.1f));
                item.transform.rotation = Quaternion.Euler(0f, seed * 47f, 0f);
                growth.Add(item);
                seed++;
            }
            var existing = new List<GameObject>(art.overgrownOnly ?? new GameObject[0]);
            existing.AddRange(growth);
            art.overgrownOnly = existing.ToArray();
            foreach (GameObject item in growth) item.SetActive(art.worlds.IsAltered);
        }

        private void OnDestroy()
        {
            if (bevel != null) Destroy(bevel);
            if (leaf != null) Destroy(leaf);
            if (zinc != null) Destroy(zinc);
            if (leafDark != null) Destroy(leafDark);
            if (leafLight != null) Destroy(leafLight);
            if (patch != null) Destroy(patch);
        }

        private Material Existing(string name)
        {
            foreach (Renderer item in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (Material material in item.sharedMaterials)
                    if (material != null && material.name == name) return material;
            return null;
        }

        private static Material NewMaterial(string name, Color color, float smoothness)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;
            return material;
        }

        private static GameObject Add(Transform parent, string name, Mesh mesh, Material material,
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
            return item;
        }

        private static Mesh MakeBevel()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uv = new List<Vector2>();
            float[] h = { -0.5f, -0.43f, 0.43f, 0.5f };
            float[] w = { 0.43f, 0.5f, 0.5f, 0.43f };
            for (int band = 0; band < 3; band++)
            for (int side = 0; side < 4; side++)
            {
                Vector2 a = Corner(side, w[band]);
                Vector2 b = Corner((side + 1) % 4, w[band]);
                Vector2 c = Corner((side + 1) % 4, w[band + 1]);
                Vector2 d = Corner(side, w[band + 1]);
                Quad(vertices, triangles, uv, new Vector3(a.x, h[band], a.y),
                    new Vector3(d.x, h[band + 1], d.y), new Vector3(c.x, h[band + 1], c.y),
                    new Vector3(b.x, h[band], b.y));
            }
            Quad(vertices, triangles, uv, new Vector3(-0.43f, 0.5f, -0.43f),
                new Vector3(-0.43f, 0.5f, 0.43f), new Vector3(0.43f, 0.5f, 0.43f),
                new Vector3(0.43f, 0.5f, -0.43f));
            Quad(vertices, triangles, uv, new Vector3(-0.43f, -0.5f, 0.43f),
                new Vector3(-0.43f, -0.5f, -0.43f), new Vector3(0.43f, -0.5f, -0.43f),
                new Vector3(0.43f, -0.5f, 0.43f));
            var mesh = new Mesh { name = "V5 chamfered module" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, uv);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        private static Vector2 Corner(int i, float w)
        {
            switch (i) { case 0: return new Vector2(-w, -w);
                case 1: return new Vector2(w, -w);
                case 2: return new Vector2(w, w);
                default: return new Vector2(-w, w); }
        }

        private static Mesh MakeLeafCluster()
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                float a = i * 2.39996f;
                Vector3 radial = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 tangent = new Vector3(-radial.z, 0f, radial.x);
                float height = 0.32f + i % 3 * 0.1f;
                int n = vertices.Count;
                vertices.Add(radial * 0.07f);
                vertices.Add(radial * 0.28f + Vector3.up * (height * 0.7f) - tangent * 0.1f);
                vertices.Add(radial * (0.4f + i % 3 * 0.05f) + Vector3.up * height);
                vertices.Add(radial * 0.28f + Vector3.up * (height * 0.7f) + tangent * 0.1f);
                vertices.AddRange(new[] { vertices[n], vertices[n + 1], vertices[n + 2], vertices[n + 3] });
                triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3,
                    n + 6, n + 5, n + 4, n + 7, n + 6, n + 4 });
            }
            var mesh = new Mesh { name = "V5 rooted leaves" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }

        private static void Quad(List<Vector3> vertices, List<int> triangles, List<Vector2> uv,
            Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int n = vertices.Count;
            vertices.AddRange(new[] { a, b, c, d });
            uv.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
            triangles.AddRange(new[] { n, n + 1, n + 2, n, n + 2, n + 3 });
        }
    }
}
