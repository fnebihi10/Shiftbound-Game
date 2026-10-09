using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Shiftbound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static partial class ProductionBenchmarkDelivery
{
    private const string Folder = "Assets/Shiftbound/ProductionBenchmark";
    private const string Scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";
    private static Material stone, frame, glass, steel, zinc, ochre;

    [MenuItem("Shiftbound/Apply Opening Production Benchmark")]
    public static void ApplyAndBuild()
    {
        EditorSceneManager.OpenScene(Scene, OpenSceneMode.Single);
        string collisionBefore = CollisionContract();
        if (GameObject.Find("Production benchmark - authored opening") != null)
            throw new InvalidOperationException("Benchmark already authored. Edit the existing assets; do not stack another pass.");
        foreach (string directory in new[] { "Materials", "Meshes", "Prefabs" })
            if (!AssetDatabase.IsValidFolder(Folder + "/" + directory)) AssetDatabase.CreateFolder(Folder, directory);
        AssetDatabase.Refresh();
        stone = Surface("Facade limestone", "Weatheredfacade", new Color(0.83f, 0.84f, 0.82f), 0.12f);
        Material deck = Surface("Roof concrete", "Presentrooftopconcrete", new Color(0.83f, 0.85f, 0.87f), 0.12f);
        Material mossDeck = Surface("Roof restrained moss", "Overgrownrooftopconcrete", new Color(0.72f, 0.76f, 0.68f), 0.1f);
        frame = Solid("Painted window frame", new Color(0.22f, 0.27f, 0.28f), 0.35f, 0.15f);
        glass = Solid("Recessed cool glass", new Color(0.12f, 0.23f, 0.28f), 0.7f, 0.15f);
        steel = Solid("Equipment graphite", new Color(0.13f, 0.18f, 0.20f), 0.35f, 0.45f);
        zinc = Solid("Aged zinc flashing", new Color(0.44f, 0.47f, 0.46f), 0.4f, 0.65f);
        ochre = Solid("Equipment ochre", new Color(0.66f, 0.38f, 0.13f), 0.3f, 0.2f);
        var root = new GameObject("Production benchmark - authored opening").transform;
        WorldSwitcher worlds = UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        RooftopWorldArt art = UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();

        // Replace the matte plane, retaining it disabled for historical comparison.
        foreach (SkylineBackdrop old in UnityEngine.Object.FindObjectsByType<SkylineBackdrop>(FindObjectsSortMode.None))
        { old.enabled = false; old.GetComponent<Renderer>().enabled = false; }
        var panorama = root.gameObject.AddComponent<WorldPanorama>();
        panorama.worlds = worlds;
        panorama.present = Panorama("City panorama present", "CityPresent.png");
        panorama.overgrown = Panorama("City panorama overgrown", "CityOvergrown.png");
        RenderSettings.skybox = panorama.present;

        // Consolidate previous overlapping opening passes. Never remove/resize collision.
        var legacyV5 = GameObject.Find("Visual Benchmark V5 - opening roofs");
        if (legacyV5 != null)
            foreach (Renderer r in legacyV5.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        Renderer[] originalRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int retired = 0;
        foreach (Renderer r in originalRenderers)
        {
            if (r.transform.position.z > 23f || r.transform.IsChildOf(worlds.playerProbe.transform.parent)) continue;
            string name = r.name.ToLowerInvariant();
            bool obsolete = name.Contains("hvac") || name.Contains("vent") || name.Contains("stone slab") ||
                name.Contains("plant stem") || name.Contains("foliage") || name.Contains("leaf") ||
                name.Contains("planter") || name.Contains("moss") || name.Contains("ivy") || name.Contains("front weathered fascia");
            if (obsolete && r.GetComponent<SkinnedMeshRenderer>() == null) { r.enabled = false; retired++; }
        }

        // Physical metre UVs on the original meshes: the collider/transform contract is untouched.
        foreach (Renderer r in originalRenderers)
        {
            if (r.transform.position.z > 23f) continue;
            BoxCollider box = r.GetComponent<BoxCollider>();
            if (box == null || box.isTrigger || r.transform.localScale.y > 1.1f ||
                r.transform.localScale.x < 1.5f || r.transform.localScale.z < 1.5f) continue;
            var filter = r.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) continue;
            Mesh mesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
            mesh.name = r.name + " metre UV";
            Vector3[] vertices = mesh.vertices, normals = mesh.normals;
            Vector2[] uv = mesh.uv;
            Vector3 size = r.transform.lossyScale;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 p = Vector3.Scale(vertices[i], size);
                Vector3 n = normals[i];
                uv[i] = Mathf.Abs(n.y) > 0.5f ? new Vector2(p.x, p.z) :
                    Mathf.Abs(n.x) > 0.5f ? new Vector2(p.z, p.y) : new Vector2(p.x, p.y);
                uv[i] *= 0.5f; // One 2 m texture repeat, independent of deck dimensions.
            }
            mesh.uv = uv;
            AssetDatabase.CreateAsset(mesh, Folder + "/Meshes/" + Safe(r.name) + "-deck.asset");
            filter.sharedMesh = mesh;
            r.sharedMaterial = deck;
        }
        art.presentDeck = deck;
        art.overgrownDeck = mossDeck;
        // Shared decks outside the benchmark keep their existing material assignment.
        art.sharedDecks = art.sharedDecks.Where(r => r != null && r.transform.position.z <= 23f).ToArray();

        var kit = new MeshAuthor();
        WindowBay(kit, 3f, 3.1f, Matrix4x4.identity);
        Mesh bay = SaveMesh(kit, "WindowBay-3x3.1m");
        var bayObject = Piece(root, "Window bay specification", bay, new[] { stone, frame, glass }, Vector3.zero);
        PrefabUtility.SaveAsPrefabAsset(bayObject, Folder + "/Prefabs/WindowBay.prefab");
        UnityEngine.Object.DestroyImmediate(bayObject);

        int buildings = 0;
        foreach (Renderer original in originalRenderers)
        {
            if ((original.name != "Neighbouring city block" && !original.name.EndsWith("building base", StringComparison.Ordinal)) ||
                original.transform.position.z > 30f) continue;
            Vector3 size = original.transform.lossyScale;
            Vector3 center = original.bounds.center;
            MeshAuthor building = new MeshAuthor();
            int floors = Mathf.Max(1, Mathf.RoundToInt(size.y / 3.1f));
            for (int side = 0; side < 4; side++)
            {
                float width = side % 2 == 0 ? size.x : size.z;
                float depth = side % 2 == 0 ? size.z : size.x;
                int columns = Mathf.Max(1, Mathf.RoundToInt(width / 3f));
                float bayWidth = width / columns, floorHeight = size.y / floors;
                Quaternion rotation = Quaternion.Euler(0, side * 90, 0);
                for (int floor = 0; floor < floors; floor++)
                for (int column = 0; column < columns; column++)
                {
                    Vector3 local = new Vector3(-width / 2 + bayWidth * (column + 0.5f),
                        -size.y / 2 + floorHeight * (floor + 0.5f), depth / 2);
                    WindowBay(building, bayWidth, floorHeight, Matrix4x4.TRS(rotation * local, rotation, Vector3.one));
                }
            }
            building.Quad(new Vector3(-size.x/2, size.y/2, -size.z/2), new Vector3(-size.x/2, size.y/2, size.z/2),
                new Vector3(size.x/2, size.y/2, size.z/2), new Vector3(size.x/2, size.y/2, -size.z/2), 0);
            Mesh authored = SaveMesh(building, "Building-" + buildings);
            Piece(root, "Constructed facade " + buildings, authored, new[] { stone, frame, glass }, center);
            original.enabled = false;
            buildings++;
        }
        // Surface-mounted window slabs are replaced by apertures/reveals in the building mesh.
        foreach (Renderer original in originalRenderers)
            if (original.transform.position.z <= 30f && (original.name == "Recessed window" || original.name == "Window ledge"))
                original.enabled = false;

        MeshAuthor service = new MeshAuthor();
        // One coherent asset, with an actual open front grille recess and perimeter casing.
        service.Bevel(new Vector3(0, 0.55f, 0.1f), new Vector3(1.30f, 1.1f, 1.2f), 0.035f, 0);
        service.Box(new Vector3(-0.58f,0.55f,-0.60f),new Vector3(0.14f,1.1f,0.2f),0);
        service.Box(new Vector3(0.58f,0.55f,-0.60f),new Vector3(0.14f,1.1f,0.2f),0);
        service.Box(new Vector3(0,0.98f,-0.60f),new Vector3(1.02f,0.24f,0.2f),0);
        service.Box(new Vector3(0,0.12f,-0.60f),new Vector3(1.02f,0.24f,0.2f),0);
        service.Bevel(new Vector3(0,1.13f,0),new Vector3(1.44f,0.12f,1.50f),0.015f,1);
        for (int i = 0; i < 6; i++)
            service.Box(new Vector3(0,0.31f+i*0.106f,-0.58f),new Vector3(1.01f,0.033f,0.12f),1,
                Quaternion.Euler(-25,0,0));
        Mesh serviceMesh = SaveMesh(service, "ServiceUnit-1.3m");
        var prefabUnit = Piece(root,"Service unit specification",serviceMesh,new[]{steel,zinc,glass},Vector3.zero);
        PrefabUtility.SaveAsPrefabAsset(prefabUnit,Folder+"/Prefabs/ServiceUnit.prefab");
        UnityEngine.Object.DestroyImmediate(prefabUnit);
        Piece(root,"Graphite service unit",serviceMesh,new[]{steel,zinc,glass},new Vector3(-2.7f,0,2.4f));
        Piece(root,"Ochre service unit",serviceMesh,new[]{ochre,zinc,glass},new Vector3(2.7f,0,2.4f));
        var landingUnit = Piece(root,"Landing service unit",serviceMesh,new[]{steel,zinc,glass},new Vector3(-1.7f,0,10f));
        landingUnit.transform.localScale = new Vector3(0.85f,0.7f,0.85f);

        // Reuse the authored hanging-ivy texture, alpha-tested and lit; no invented 3D foliage.
        Material ivy = Solid("Lit ivy cutout", Color.white, 0.12f, 0);
        ivy.SetTexture("_BaseMap", ImportTexture("IvyCutout.png", true));
        ivy.SetFloat("_AlphaClip", 1); ivy.SetFloat("_Cutoff",0.45f); ivy.SetFloat("_Cull",0);
        ivy.EnableKeyword("_ALPHATEST_ON"); ivy.renderQueue = 2450;
        var growth = new List<GameObject>(art.overgrownOnly ?? Array.Empty<GameObject>());
        var ivyMesh = new MeshAuthor();
        ivyMesh.Quad(new Vector3(-0.5f,-0.5f,0),new Vector3(0.5f,-0.5f,0),new Vector3(0.5f,0.5f,0),new Vector3(-0.5f,0.5f,0),0,true);
        Mesh curtain = SaveMesh(ivyMesh,"IvyCurtain");
        foreach (Vector3 position in new[] {new Vector3(-3.30f,0.47f,1.69f),new Vector3(3.28f,0.47f,1.69f),
            new Vector3(-2.10f,-0.55f,7.08f),new Vector3(2.65f,-0.55f,7.08f),new Vector3(-1.62f,-0.55f,12.08f)})
        {
            var plant = Piece(root,"Rooted hanging ivy",curtain,new[]{ivy},position);
            plant.transform.localScale = new Vector3(0.95f,1.45f,1);
            growth.Add(plant); plant.SetActive(false);
        }
        art.overgrownOnly = growth.ToArray();

        Material preview = new Material(Shader.Find("Shiftbound/Benchmark Route Preview"));
        AssetDatabase.CreateAsset(preview, Folder+"/Materials/Route preview.mat");
        worlds.ghostMaterial = preview;
        worlds.previewCollisionSurfacesOnly = true;
        worlds.presentLight = new Color(1f,0.91f,0.76f);
        worlds.alteredLight = new Color(1f,0.92f,0.80f);
        worlds.sun.intensity = 1.65f; worlds.sun.shadowStrength = 0.85f;
        worlds.sun.transform.rotation = Quaternion.Euler(35f,-44f,0);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.43f,0.53f,0.65f);
        RenderSettings.ambientEquatorColor = new Color(0.26f,0.32f,0.39f);
        RenderSettings.ambientGroundColor = new Color(0.12f,0.15f,0.18f);
        var atmosphere = UnityEngine.Object.FindFirstObjectByType<AtmosphereController>();
        atmosphere.presentAmbient = new Color(0.43f,0.53f,0.65f);
        atmosphere.alteredAmbient = new Color(0.44f,0.53f,0.59f);
        atmosphere.presentFog = new Color(0.64f,0.70f,0.73f);
        atmosphere.alteredFog = new Color(0.64f,0.70f,0.67f);
        RenderSettings.fogStartDistance=70f; RenderSettings.fogEndDistance=235f;
        FollowCamera camera = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        camera.distance=7.6f; camera.lookAhead=1.9f;
        Camera.main.fieldOfView=60f;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
        var bloom=profile.Add<Bloom>(); bloom.intensity.Override(0.08f); bloom.threshold.Override(1.3f);
        var grade=profile.Add<ColorAdjustments>(); grade.contrast.Override(3f); grade.saturation.Override(-4f);
        AssetDatabase.CreateAsset(profile,Folder+"/Materials/Opening grade.asset");
        foreach (Volume old in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None)) old.enabled=false;
        var volume=root.gameObject.AddComponent<Volume>(); volume.isGlobal=true; volume.sharedProfile=profile;
        Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing=true;

        if (collisionBefore != CollisionContract()) throw new InvalidOperationException("Benchmark changed gameplay collision. Scene will not be saved.");
        System.IO.Directory.CreateDirectory("Logs");
        System.IO.File.WriteAllText("Logs/production-collision-contract.txt", collisionBefore);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("SHIFTBOUND PRODUCTION AUTHORING PASSED: collision unchanged; buildings="+buildings+" retired opening renderers="+retired);
        SliceDelivery.BuildWindows();
    }

    public static void RefineAndBuild()
    {
        EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
        string before=CollisionContract();
        Transform root=GameObject.Find("Production benchmark - authored opening").transform;
        stone=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Facade limestone.mat");
        frame=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Painted window frame.mat");
        glass=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Materials/Recessed cool glass.mat");
        var art=UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();
        art.overgrownDeck=art.presentDeck; // Growth gathers at edges, not as a full-floor green filter.
        foreach(Material panorama in new[]{root.GetComponent<WorldPanorama>().present,root.GetComponent<WorldPanorama>().overgrown})
        { panorama.SetFloat("_Rotation",90f); panorama.SetFloat("_Exposure",1f); EditorUtility.SetDirty(panorama); }
        int count=11;
        Renderer[] originals=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach(Renderer r in originals)
        {
            if(r.transform.position.z<=23f && r.name.StartsWith("Overgrowth silhouette",StringComparison.Ordinal))r.enabled=false;
            if(r.name=="Alternate wall test")r.sharedMaterial=stone;
            if(r.name!="Neighbouring city block" || !r.enabled)continue;
            Vector3 size=r.transform.lossyScale;
            MeshAuthor building=new MeshAuthor();
            int floors=Mathf.Max(1,Mathf.RoundToInt(size.y/3.1f));
            for(int side=0;side<4;side++)
            {
                float width=side%2==0?size.x:size.z,depth=side%2==0?size.z:size.x;
                int columns=Mathf.Max(1,Mathf.RoundToInt(width/3f));
                float bayWidth=width/columns,floorHeight=size.y/floors;
                Quaternion rotation=Quaternion.Euler(0,side*90,0);
                for(int floor=0;floor<floors;floor++)for(int column=0;column<columns;column++)
                {
                    Vector3 p=new Vector3(-width/2+bayWidth*(column+0.5f),-size.y/2+floorHeight*(floor+0.5f),depth/2);
                    WindowBay(building,bayWidth,floorHeight,Matrix4x4.TRS(rotation*p,rotation,Vector3.one));
                }
            }
            building.Quad(new Vector3(-size.x/2,size.y/2,-size.z/2),new Vector3(-size.x/2,size.y/2,size.z/2),
                new Vector3(size.x/2,size.y/2,size.z/2),new Vector3(size.x/2,size.y/2,-size.z/2),0);
            Piece(root,"Constructed facade "+count,SaveMesh(building,"Building-"+count),new[]{stone,frame,glass},r.bounds.center);
            r.enabled=false;count++;
        }
        foreach(Renderer r in originals)
            if(r.name=="Recessed window" || r.name=="Window ledge")r.enabled=false;
        // The original lighting/shadow direction is unchanged; all sky views use the same rotation.
        if(before!=CollisionContract())throw new InvalidOperationException("Refinement altered collision.");
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();Debug.Log("SHIFTBOUND BENCHMARK REFINEMENT PASSED: collision unchanged; far facades aligned with kit.");
        SliceDelivery.BuildWindows();
    }

    private static string CollisionContract() => string.Join("\n",UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None)
        .OrderBy(c=>c.GetInstanceID()).Select(c=>GlobalObjectId.GetGlobalObjectIdSlow(c)+"|"+c.name+"|"+c.transform.position.ToString("F6")+
        "|"+c.transform.rotation.ToString("F6")+"|"+c.transform.lossyScale.ToString("F6")+"|"+EditorJsonUtility.ToJson(c)));
    private static string Safe(string text) => new string(text.Select(c=>char.IsLetterOrDigit(c)?c:'-').ToArray());
    private static Texture2D ImportTexture(string file,bool alpha)
    {
        string path=Folder+"/Textures/"+file;
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.maxTextureSize=alpha?2048:4096; importer.alphaIsTransparency=alpha;
        importer.wrapMode=alpha?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
        importer.mipmapEnabled=true; importer.textureCompression=TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    private static Material Panorama(string name,string file)
    {
        var material=new Material(Shader.Find("Skybox/Panoramic"));
        material.SetTexture("_MainTex",ImportTexture(file,false)); material.SetFloat("_Exposure",0.85f);
        material.SetFloat("_Mapping",1); material.SetFloat("_ImageType",0);
        AssetDatabase.CreateAsset(material,Folder+"/Materials/"+name+".mat"); return material;
    }
    private static Material Surface(string name,string existing,Color tint,float smoothness)
    {
        var original=AssetDatabase.LoadAssetAtPath<Material>("Assets/Shiftbound/MaterialsV4/"+existing+".mat");
        var material=new Material(original); material.name=name;
        material.SetColor("_BaseColor",tint); material.SetFloat("_Smoothness",smoothness); material.SetFloat("_BumpScale",0.4f);
        material.SetTextureScale("_BaseMap",Vector2.one); material.SetTextureScale("_BumpMap",Vector2.one);
        AssetDatabase.CreateAsset(material,Folder+"/Materials/"+name+".mat"); return material;
    }
    private static Material Solid(string name,Color color,float smoothness,float metallic)
    {
        var material=new Material(Shader.Find("Universal Render Pipeline/Lit")); material.name=name;
        material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",smoothness); material.SetFloat("_Metallic",metallic);
        AssetDatabase.CreateAsset(material,Folder+"/Materials/"+name+".mat"); return material;
    }
    private static Mesh SaveMesh(MeshAuthor source,string name)
    {
        Mesh mesh=source.Create(name); AssetDatabase.CreateAsset(mesh,Folder+"/Meshes/"+name+".asset"); return mesh;
    }
    private static GameObject Piece(Transform parent,string name,Mesh mesh,Material[] materials,Vector3 position)
    {
        var item=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); item.transform.SetParent(parent);
        item.transform.position=position; item.GetComponent<MeshFilter>().sharedMesh=mesh;
        item.GetComponent<MeshRenderer>().sharedMaterials=materials; return item;
    }
    private static void WindowBay(MeshAuthor mesh,float width,float height,Matrix4x4 transform)
    {
        // Actual aperture, 180 mm deep reveal, sill and mullion; no window pasted onto a wall box.
        float x=0.68f,y=0.87f;
        mesh.Panel(-width/2,-height/2,-x,height/2,0,0,transform);
        mesh.Panel(x,-height/2,width/2,height/2,0,0,transform);
        mesh.Panel(-x,-height/2,x,-y,0,0,transform);
        mesh.Panel(-x,y,x,height/2,0,0,transform);
        mesh.Quad(transform.MultiplyPoint3x4(new Vector3(-x,-y,0)),transform.MultiplyPoint3x4(new Vector3(-x,-y,-0.18f)),
            transform.MultiplyPoint3x4(new Vector3(-x,y,-0.18f)),transform.MultiplyPoint3x4(new Vector3(-x,y,0)),0);
        mesh.Quad(transform.MultiplyPoint3x4(new Vector3(x,-y,-0.18f)),transform.MultiplyPoint3x4(new Vector3(x,-y,0)),
            transform.MultiplyPoint3x4(new Vector3(x,y,0)),transform.MultiplyPoint3x4(new Vector3(x,y,-0.18f)),0);
        mesh.Panel(-x,-y,x,y,-0.18f,2,transform);
        foreach (float side in new[]{-1f,1f})
        {
            mesh.Box(new Vector3(side*x,0,-0.09f),new Vector3(0.065f,1.8f,0.18f),1,Quaternion.identity,transform);
            mesh.Box(new Vector3(0,side*y,-0.09f),new Vector3(1.4f,0.065f,0.18f),1,Quaternion.identity,transform);
        }
        mesh.Box(new Vector3(0,0,-0.12f),new Vector3(0.04f,1.68f,0.10f),1,Quaternion.identity,transform);
        mesh.Box(new Vector3(0,-0.94f,0.025f),new Vector3(1.55f,0.10f,0.32f),0,Quaternion.identity,transform);
    }

    private sealed class MeshAuthor
    {
        private readonly List<Vector3> positions=new List<Vector3>();
        private readonly List<Vector2> uv=new List<Vector2>();
        private readonly List<int>[] indices={new List<int>(),new List<int>(),new List<int>()};
        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,int slot,bool normalized=false)
        {
            int offset=positions.Count; positions.AddRange(new[]{a,b,c,d});
            float width=normalized?1f:Vector3.Distance(a,b)*0.5f,height=normalized?1f:Vector3.Distance(b,c)*0.5f;
            uv.AddRange(new[]{Vector2.zero,new Vector2(width,0),new Vector2(width,height),new Vector2(0,height)});
            indices[slot].AddRange(new[]{offset,offset+1,offset+2,offset,offset+2,offset+3});
        }
        public void Panel(float left,float bottom,float right,float top,float depth,int slot,Matrix4x4 transform)
        {
            Quad(transform.MultiplyPoint3x4(new Vector3(left,bottom,depth)),transform.MultiplyPoint3x4(new Vector3(right,bottom,depth)),
                transform.MultiplyPoint3x4(new Vector3(right,top,depth)),transform.MultiplyPoint3x4(new Vector3(left,top,depth)),slot);
        }
        public void Box(Vector3 center,Vector3 size,int slot,Quaternion rotation=default,Matrix4x4 transform=default)
        {
            if(rotation==default)rotation=Quaternion.identity;
            if(transform==default)transform=Matrix4x4.identity;
            Matrix4x4 matrix=transform*Matrix4x4.TRS(center,rotation,size);
            Vector3 P(float x,float y,float z)=>matrix.MultiplyPoint3x4(new Vector3(x,y,z)*0.5f);
            Quad(P(-1,-1,1),P(1,-1,1),P(1,1,1),P(-1,1,1),slot);
            Quad(P(1,-1,-1),P(-1,-1,-1),P(-1,1,-1),P(1,1,-1),slot);
            Quad(P(-1,-1,-1),P(-1,-1,1),P(-1,1,1),P(-1,1,-1),slot);
            Quad(P(1,-1,1),P(1,-1,-1),P(1,1,-1),P(1,1,1),slot);
            Quad(P(-1,1,1),P(1,1,1),P(1,1,-1),P(-1,1,-1),slot);
            Quad(P(-1,-1,-1),P(1,-1,-1),P(1,-1,1),P(-1,-1,1),slot);
        }
        public void Bevel(Vector3 center,Vector3 size,float radius,int slot)
        {
            // Chamfered rectangular extrusion with physical bevel widths (not stretched unit cubes).
            float x=size.x/2,z=size.z/2,y=size.y/2;
            Vector3[] ring={new Vector3(-x+radius,0,-z),new Vector3(x-radius,0,-z),new Vector3(x,0,-z+radius),
                new Vector3(x,0,z-radius),new Vector3(x-radius,0,z),new Vector3(-x+radius,0,z),
                new Vector3(-x,0,z-radius),new Vector3(-x,0,-z+radius)};
            for(int i=0;i<8;i++)
            {
                int j=(i+1)%8;
                Quad(center+ring[j]+Vector3.down*y,center+ring[i]+Vector3.down*y,
                    center+ring[i]+Vector3.up*y,center+ring[j]+Vector3.up*y,slot);
                Quad(center+Vector3.up*y,center+ring[j]+Vector3.up*y,center+ring[i]+Vector3.up*y,center+Vector3.up*y,slot);
                Quad(center+Vector3.down*y,center+ring[i]+Vector3.down*y,center+ring[j]+Vector3.down*y,center+Vector3.down*y,slot);
            }
        }
        public Mesh Create(string name)
        {
            var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(positions);mesh.SetUVs(0,uv);mesh.subMeshCount=3;
            for(int i=0;i<3;i++)mesh.SetTriangles(indices[i],i);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
    }
}
