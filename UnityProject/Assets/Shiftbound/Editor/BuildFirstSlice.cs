using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Shiftbound;

public static class BuildFirstSlice
{
    private const string Base = "Assets/Shiftbound";
    private static readonly List<Material> Materials = new List<Material>();

    [MenuItem("Shiftbound/Build First Rooftop")]
    public static void Build()
    {
        EnsureFolders();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 22f;
        RenderSettings.fogEndDistance = 92f;
        RenderSettings.ambientLight = new Color(0.42f, 0.47f, 0.55f);

        Material shared = MakeMaterial("Concrete", new Color(0.26f, 0.33f, 0.42f));
        Material edge = MakeMaterial("Edge", new Color(0.09f, 0.15f, 0.22f));
        Material present = MakeMaterial("Present Platform", new Color(0.18f, 0.72f, 0.91f));
        Material altered = MakeMaterial("Overgrown Platform", new Color(0.80f, 0.61f, 0.25f));
        Material foliage = MakeMaterial("Foliage", new Color(0.23f, 0.55f, 0.35f));
        Material playerMat = MakeMaterial("Runner", new Color(1.00f, 0.90f, 0.70f));
        Material goalMat = MakeMaterial("Goal", new Color(0.97f, 0.67f, 0.24f));
        Material ghost = MakeMaterial("Ghost Preview", new Color(0.72f, 0.91f, 1f, 0.20f), true);

        var sharedRoot = new GameObject("Shared rooftop structure").transform;
        var presentRoot = new GameObject("Present world geometry").transform;
        var alteredRoot = new GameObject("Overgrown world geometry").transform;

        // Shared surfaces remain solid in either world.
        Platform(sharedRoot, "Start rooftop", new Vector3(0f, -0.5f, 3f),
            new Vector3(8f, 1f, 8f), shared, edge);
        Platform(sharedRoot, "First landing", new Vector3(0f, -0.5f, 10f),
            new Vector3(5f, 1f, 4f), shared, edge);
        Platform(sharedRoot, "Shift landing", new Vector3(0f, -0.5f, 18f),
            new Vector3(5f, 1f, 3f), shared, edge);
        Platform(sharedRoot, "Midair landing", new Vector3(0f, -0.5f, 30f),
            new Vector3(5f, 1f, 3f), shared, edge);
        Platform(sharedRoot, "Route landing", new Vector3(0f, -0.5f, 42f),
            new Vector3(5f, 1f, 3f), shared, edge);
        Platform(sharedRoot, "Goal rooftop", new Vector3(0f, -0.5f, 54f),
            new Vector3(7f, 1f, 4f), shared, edge);

        // Safe first shift, then a midair shift between world-specific surfaces.
        Platform(alteredRoot, "First alternate bridge", new Vector3(0f, -0.5f, 14.5f),
            new Vector3(3f, 1f, 3f), altered, edge);
        Platform(presentRoot, "Midair takeoff", new Vector3(0f, -0.5f, 22f),
            new Vector3(3f, 1f, 3f), present, edge);
        Platform(alteredRoot, "Midair destination", new Vector3(0f, -0.5f, 26f),
            new Vector3(3f, 1f, 3f), altered, edge);

        // The route on the right connects to the next shared landing.
        Platform(presentRoot, "Left lookout", new Vector3(-2.5f, -0.5f, 35f),
            new Vector3(3f, 1f, 3f), present, edge);
        Platform(alteredRoot, "Right route", new Vector3(2.5f, -0.5f, 35f),
            new Vector3(3f, 1f, 3f), altered, edge);
        Platform(presentRoot, "Return route", new Vector3(1f, -0.5f, 38.5f),
            new Vector3(3f, 1f, 3f), present, edge);

        // Final combined crossing.
        Platform(presentRoot, "Final present", new Vector3(-1.5f, -0.5f, 46f),
            new Vector3(3f, 1f, 3f), present, edge);
        Platform(alteredRoot, "Final alternate", new Vector3(1.5f, -0.5f, 50f),
            new Vector3(3f, 1f, 3f), altered, edge);

        Cube(sharedRoot, "Rooftop utility", new Vector3(-3.25f, 0.8f, 2.5f),
            new Vector3(1.1f, 1.6f, 2f), edge);
        Cube(sharedRoot, "Goal frame left", new Vector3(-2.5f, 1.3f, 55.2f),
            new Vector3(0.25f, 2.6f, 0.3f), goalMat);
        Cube(sharedRoot, "Goal frame right", new Vector3(2.5f, 1.3f, 55.2f),
            new Vector3(0.25f, 2.6f, 0.3f), goalMat);
        Cube(sharedRoot, "Goal frame top", new Vector3(0f, 2.5f, 55.2f),
            new Vector3(5.2f, 0.25f, 0.3f), goalMat);
        // This side wall makes blocked-switch feedback easy to test.
        Cube(alteredRoot, "Alternate wall test", new Vector3(2.5f, 1f, 3f),
            new Vector3(1.1f, 2f, 2f), altered);
        Foliage(alteredRoot, foliage, 14.5f, 2);
        Foliage(alteredRoot, foliage, 26f, 2);
        Foliage(alteredRoot, foliage, 35f, 3);
        Foliage(alteredRoot, foliage, 50f, 2);

        var sunObject = new GameObject("Sun");
        var sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 1.15f;
        sun.transform.rotation = Quaternion.Euler(42f, -36f, 0f);
        sun.shadows = LightShadows.Soft;

        var runner = new GameObject("Runner");
        runner.layer = 9;
        runner.transform.position = new Vector3(0f, 0.08f, 1f);
        var character = runner.AddComponent<CharacterController>();
        character.height = 1.9f;
        character.radius = 0.34f;
        character.center = new Vector3(0f, 0.95f, 0f);
        character.stepOffset = 0.22f;
        character.slopeLimit = 55f;
        var motor = runner.AddComponent<PlayerMotor>();
        motor.groundMask = 1;
        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Runner visual";
        visual.layer = 9;
        visual.transform.SetParent(runner.transform);
        visual.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        visual.transform.localScale = new Vector3(0.65f, 0.84f, 0.65f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.GetComponent<Renderer>().sharedMaterial = playerMat;
        motor.visual = visual.transform;
        var probeObject = new GameObject("Switch collision probe");
        probeObject.layer = 9;
        probeObject.transform.SetParent(runner.transform);
        probeObject.transform.localPosition = Vector3.zero;
        var probe = probeObject.AddComponent<CapsuleCollider>();
        probe.center = new Vector3(0f, 0.95f, 0f);
        probe.height = 1.9f;
        probe.radius = 0.34f;
        probe.isTrigger = true;

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 66f;
        camera.nearClipPlane = 0.12f;
        camera.farClipPlane = 130f;
        cameraObject.AddComponent<AudioListener>();
        var cameraRig = cameraObject.AddComponent<FollowCamera>();
        cameraRig.target = runner.transform;
        cameraRig.motor = motor;
        cameraRig.obstacleMask = 1;
        motor.view = cameraObject.transform;

        var systems = new GameObject("Game systems");
        var input = systems.AddComponent<GameInput>();
        var feedback = systems.AddComponent<FeedbackAudio>();
        var worlds = systems.AddComponent<WorldSwitcher>();
        worlds.presentRoot = presentRoot;
        worlds.alteredRoot = alteredRoot;
        worlds.playerProbe = probe;
        worlds.ghostMaterial = ghost;
        worlds.sun = sun;
        worlds.input = input;
        worlds.feedback = feedback;
        var flow = systems.AddComponent<GameFlow>();
        flow.input = input;
        flow.player = motor;
        flow.cameraRig = cameraRig;
        flow.feedback = feedback;
        flow.worlds = worlds;
        motor.input = input;
        cameraRig.input = input;

        Trigger("First checkpoint", 10f, StageTrigger.TriggerKind.Checkpoint);
        Trigger("Midair checkpoint", 30f, StageTrigger.TriggerKind.Checkpoint);
        Trigger("Final checkpoint", 42f, StageTrigger.TriggerKind.Checkpoint);
        Trigger("Goal", 54.6f, StageTrigger.TriggerKind.Goal);

        string scenePath = Base + "/Scenes/FirstRooftop.unity";
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        PrefabUtility.SaveAsPrefabAsset(runner, Base + "/Prefabs/Runner.prefab");
        AssetDatabase.SaveAssets();
        Debug.Log("Shiftbound first rooftop created: " + scenePath);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(Base)) AssetDatabase.CreateFolder("Assets", "Shiftbound");
        foreach (string name in new[] { "Scenes", "Materials", "Prefabs", "Scripts", "Editor" })
            if (!AssetDatabase.IsValidFolder(Base + "/" + name))
                AssetDatabase.CreateFolder(Base, name);
    }

    private static Material MakeMaterial(string name, Color color, bool transparent = false)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new System.InvalidOperationException("URP Lit shader not found.");
        var material = new Material(shader) { name = name };
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", 0.34f);
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
        string path = Base + "/Materials/" + name.Replace(" ", "") + ".mat";
        AssetDatabase.CreateAsset(material, path);
        Materials.Add(material);
        return material;
    }

    private static void Platform(Transform parent, string name, Vector3 position,
        Vector3 scale, Material deck, Material edge)
    {
        Cube(parent, name, position, scale, deck);
        Cube(parent, name + " edge", position + new Vector3(0f, -0.39f, 0f),
            new Vector3(scale.x + 0.14f, 0.16f, scale.z + 0.14f), edge);
    }

    private static void Cube(Transform parent, string name, Vector3 position,
        Vector3 scale, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void Foliage(Transform parent, Material material, float z, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Overgrowth silhouette";
            sphere.transform.SetParent(parent);
            sphere.transform.position = new Vector3(i % 2 == 0 ? -1.65f : 1.65f, 0.38f, z + i * 0.4f);
            sphere.transform.localScale = new Vector3(0.68f, 0.95f, 0.68f);
            Object.DestroyImmediate(sphere.GetComponent<Collider>());
            sphere.GetComponent<Renderer>().sharedMaterial = material;
        }
    }

    private static void Trigger(string name, float z, StageTrigger.TriggerKind kind)
    {
        var item = new GameObject(name);
        item.transform.position = new Vector3(0f, 1.2f, z);
        var collider = item.AddComponent<BoxCollider>();
        collider.size = new Vector3(4f, 2.4f, 0.8f);
        collider.isTrigger = true;
        item.AddComponent<StageTrigger>().kind = kind;
    }
}
