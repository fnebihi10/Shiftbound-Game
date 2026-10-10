using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Shiftbound;

public static partial class ProductionUpgradeAuthoring
{
    static Material paving,limestone,metal,glass,roofDark,leaf;
    static Material[] facadeRenders;
    static Material[] BuildingPalette(Bounds b)
    {
        int style=Mathf.Abs(Mathf.RoundToInt(b.center.x*3+b.center.z))%facadeRenders.Length;
        return new[]{facadeRenders[style],metal,glass,roofDark,limestone};
    }
    static int meshIndex;
    static System.Random random;
    static string CollisionSignature()=>string.Join("\n",UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None)
        .OrderBy(c=>GlobalObjectId.GetGlobalObjectIdSlow(c).ToString()).Select(c=>GlobalObjectId.GetGlobalObjectIdSlow(c)+"|"+EditorJsonUtility.ToJson(c)+"|"+c.transform.position.ToString("F6")+"|"+c.transform.rotation.ToString("F6")+"|"+c.transform.lossyScale.ToString("F6")));
    static GameObject Piece(Transform parent,string name,ProductionMeshKit kit,Material[] materials)
    {
        Mesh mesh=kit.Create(name);string path=Root+"/Meshes/City-"+(meshIndex++)+".asset";
        var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(old!=null){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
        var o=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(parent,false);
        o.GetComponent<MeshFilter>().sharedMesh=mesh;var r=o.GetComponent<MeshRenderer>();r.sharedMaterials=materials;r.shadowCastingMode=ShadowCastingMode.On;
        GameObjectUtility.SetStaticEditorFlags(o,StaticEditorFlags.BatchingStatic);
        return o;
    }
    static float Range(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
    public static void City()
    {
        Folders();EditorSceneManager.OpenScene(Scene);string before=CollisionSignature();meshIndex=0;random=new System.Random(7813);
        var existing=GameObject.Find("Production city - constructed route");
        if(existing!=null)UnityEngine.Object.DestroyImmediate(existing);
        var root=new GameObject("Production city - constructed route").transform;
        var growth=new GameObject("Rooted alternate growth").transform;growth.SetParent(root,false);
        var motor=UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();var worlds=UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        var art=UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();
        foreach(string name in new[]{"Production city - Present surfaces","Production city - Overgrown surfaces"})
        {var old=GameObject.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old);}
        var presentDecor=new GameObject("Production city - Present surfaces").transform;presentDecor.SetParent(worlds.presentRoot,false);
        var alteredDecor=new GameObject("Production city - Overgrown surfaces").transform;alteredDecor.SetParent(worlds.alteredRoot,false);
        paving=Material("Warm stone paving",new Color(.74f,.74f,.68f),.18f);
        string stonePath=Root+"/Surfaces/LimestonePaving-v1.png";
        var stone=AssetDatabase.LoadAssetAtPath<Texture2D>(stonePath);
        if(stone==null)throw new Exception("Missing authored quiet limestone surface");
        var stoneImporter=(TextureImporter)AssetImporter.GetAtPath(stonePath);stoneImporter.maxTextureSize=1024;stoneImporter.mipmapEnabled=true;stoneImporter.wrapMode=TextureWrapMode.Repeat;
        var stoneAndroid=stoneImporter.GetPlatformTextureSettings("Android");stoneAndroid.overridden=true;stoneAndroid.format=TextureImporterFormat.ASTC_6x6;stoneAndroid.maxTextureSize=1024;stoneImporter.SetPlatformTextureSettings(stoneAndroid);stoneImporter.SaveAndReimport();
        paving.SetTexture("_BaseMap",stone);paving.SetTexture("_BumpMap",null);paving.DisableKeyword("_NORMALMAP");
        limestone=Material("Limestone coping",new Color(.95f,.94f,.84f),.16f);
        limestone.SetTexture("_BaseMap",stone);limestone.SetTexture("_BumpMap",null);limestone.DisableKeyword("_NORMALMAP");
        metal=Material("Weathered zinc",new Color(.22f,.32f,.36f),.32f,.25f);
        glass=Material("Blue recessed glazing",new Color(.25f,.39f,.45f),.65f,.15f);
        roofDark=Material("Roof membrane",new Color(.21f,.26f,.27f),.16f);
        // Mineral render belongs on wall fields; rough paving belongs on decks.
        // Shared variants give city blocks a restrained architectural palette.
        facadeRenders=new[]{
            Material("Ivory mineral render",new Color(.88f,.84f,.71f),.12f),
            Material("Warm terracotta render",new Color(.82f,.37f,.20f),.10f),
            Material("Cool limestone render",new Color(.32f,.62f,.60f),.13f)};
        leaf=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Rooted leaves.mat");
        if(leaf==null){leaf=new Material(Shader.Find("Shiftbound/Rooted foliage"));AssetDatabase.CreateAsset(leaf,Root+"/Materials/Rooted leaves.mat");}
        string ivyPath=Root+"/Surfaces/IvyLeaf-v1.png";
        var ivyImporter=(TextureImporter)AssetImporter.GetAtPath(ivyPath);ivyImporter.maxTextureSize=512;ivyImporter.mipmapEnabled=true;ivyImporter.alphaIsTransparency=true;ivyImporter.wrapMode=TextureWrapMode.Clamp;
        var ivyAndroid=ivyImporter.GetPlatformTextureSettings("Android");ivyAndroid.overridden=true;ivyAndroid.format=TextureImporterFormat.ASTC_6x6;ivyAndroid.maxTextureSize=512;ivyImporter.SetPlatformTextureSettings(ivyAndroid);ivyImporter.SaveAndReimport();
        AssetDatabase.ImportAsset(ivyPath,ImportAssetOptions.ForceSynchronousImport);
        var ivy=AssetDatabase.LoadAssetAtPath<Texture2D>(ivyPath);
        if(ivy==null)throw new Exception("Ivy texture import has no Texture2D");
        leaf.SetTexture("_LeafMap",ivy);EditorUtility.SetDirty(leaf);AssetDatabase.SaveAssetIfDirty(leaf);
        // Cache authored building masses before retiring legacy decoration.
        Renderer[] originals=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var buildings=originals.Where(r=>r.enabled&&r.name.StartsWith("Constructed facade",StringComparison.Ordinal)).Select(r=>r.bounds).ToArray();
        if(buildings.Length==0)buildings=originals.Where(r=>r.name.StartsWith("Constructed facade",StringComparison.Ordinal)).Select(r=>r.bounds).ToArray();
        int retired=0;
        foreach(var r in originals)
        {
            if(r.transform.IsChildOf(motor.visual))continue;
            if(r.GetComponent<Collider>()==null){r.enabled=false;retired++;}
        }
        foreach(var backdrop in UnityEngine.Object.FindObjectsByType<SkylineBackdrop>(FindObjectsSortMode.None))backdrop.enabled=false;
        var decks=originals.Where(r=>r.GetComponent<BoxCollider>()!=null&&!r.GetComponent<BoxCollider>().isTrigger&&!r.name.EndsWith("edge",StringComparison.Ordinal)&&r.bounds.size.y<=1.1f&&r.bounds.size.x>1.5f&&r.bounds.size.z>1.5f).ToArray();
        var palette=new[]{paving,limestone,metal,roofDark};
        int roofCount=0;
        foreach(var r in decks)
        {
            Bounds b=r.bounds;r.sharedMaterial=roofDark;
            Transform owner=r.transform.IsChildOf(worlds.presentRoot)?presentDecor:r.transform.IsChildOf(worlds.alteredRoot)?alteredDecor:root;
            var kit=new ProductionMeshKit();
            int nx=Mathf.CeilToInt(b.size.x/.92f),nz=Mathf.CeilToInt(b.size.z/1.16f);
            float sx=b.size.x/nx,sz=b.size.z/nz;
            for(int iz=0;iz<nz;iz++)for(int ix=0;ix<nx;ix++)
            {
                Vector3 center=new Vector3(b.min.x+(ix+.5f)*sx,b.max.y+.004f,b.min.z+(iz+.5f)*sz);
                kit.Bevel(center,new Vector3(sx-.015f,.027f,sz-.015f),.008f,0);
            }
            // Flashing/drip edge under slab, continuous all around, preserving gaps.
            foreach(float side in new[]{-1f,1f})
            {
                kit.Bevel(new Vector3(b.center.x+side*(b.extents.x-.045f),b.max.y-.10f,b.center.z),new Vector3(.09f,.16f,b.size.z),.018f,1);
                kit.Bevel(new Vector3(b.center.x,b.max.y-.10f,b.center.z+side*(b.extents.z-.045f)),new Vector3(b.size.x,.16f,.09f),.018f,1);
                // Service drain set into the paving edge, clear of the center line.
                Vector3 drain=new Vector3(b.center.x+side*(b.extents.x-.2f),b.max.y+.022f,b.center.z);
                for(int n=0;n<6;n++)kit.Box(drain+Vector3.forward*(n*.035f-.08f),new Vector3(.21f,.014f,.016f),2);
            }
            // One sparse maintenance strip communicates scale without clutter.
            if(b.size.x>=5)
                kit.Box(new Vector3(b.max.x-.50f,b.max.y+.023f,b.min.z+.45f),new Vector3(.13f,.015f,.50f),2);
            var surface=Piece(owner,"Constructed paving "+r.name,kit,palette);surface.AddComponent<WorldSurfaceSkin>().collisionSurface=r.GetComponent<Collider>();r.enabled=false;roofCount++;
            if(owner==root)
            {
                // The crown stays beneath the walkable paving, including its
                // raised membrane cap. It must never cover the surface kit.
                Bounds structure=new Bounds();structure.SetMinMax(new Vector3(b.min.x-.125f,-29,b.min.z-.125f),new Vector3(b.max.x+.125f,b.max.y-.38f,b.max.z+.125f));
                var shell=new ProductionMeshKit(5);Building(shell,structure,false,true);var building=Piece(root,"Route building "+r.name,shell,BuildingPalette(structure));building.AddComponent<CameraArchitectureVolume>().bounds=structure;
            }
            var vines=new ProductionMeshKit(1);
            // Growth originates at coping/drainage, turns over the edge and hangs
            // below the slab; clear middle and landing edges retain silhouette.
            for(int side=-1;side<=1;side+=2)
            {
                float x=b.center.x+side*(b.extents.x-.12f);
                for(int cluster=0;cluster<(b.size.z>6?4:2);cluster++)
                {
                    float z=Mathf.Lerp(b.min.z+.55f,b.max.z-.45f,(cluster+.3f)/(b.size.z>6?4:2));
                    IvyMass(vines,new Vector3(x,b.max.y+.03f,z),side,Range(1.2f,2.1f),b.size.x>=5?1f:.72f);
                }
            }
            if(owner!=presentDecor)Piece(growth,"Rooted coping growth "+r.name,vines,new[]{leaf});
        }
        // Coping and pipe cross sections replace stretched square rail rendering.
        foreach(var r in originals.Where(r=>r.GetComponent<Collider>()!=null&&(r.name.Contains("parapet")||r.name.Contains("handrail"))))
        {
            Bounds b=r.bounds;var kit=new ProductionMeshKit();
            if(r.name.Contains("handrail"))
            {
                if(b.size.z>b.size.x)kit.Tube(new Vector3(b.center.x,b.center.y,b.min.z),new Vector3(b.center.x,b.center.y,b.max.z),.046f,2,sides:10);
                else kit.Tube(new Vector3(b.min.x,b.center.y,b.center.z),new Vector3(b.max.x,b.center.y,b.center.z),.046f,2,sides:10);
            }
            else
            {
                kit.Bevel(b.center,b.size,.028f,1);kit.Bevel(b.center+Vector3.up*(b.extents.y-.005f),new Vector3(b.size.x+.045f,.065f,b.size.z+.045f),.014f,1);
            }
            var railPalette=(Material[])palette.Clone();
            railPalette[2]=Material("Amber route metal",new Color(.96f,.52f,.10f),.32f,.15f);
            Piece(root,"Coping construction "+r.name,kit,railPalette);r.enabled=false;
        }
        // Re-author the three existing equipment/collision objects as service
        // cabinets with an actual raised cap, hinges, louver and panel recesses.
        foreach(var r in originals.Where(r=>r.GetComponent<BoxCollider>()!=null&&!r.GetComponent<BoxCollider>().isTrigger&&(r.name=="Rooftop utility"||r.name=="Alternate wall test"||r.name=="Rooftop ventilation unit")))
        {
            Bounds b=r.bounds;var kit=new ProductionMeshKit();
            kit.Bevel(b.center,b.size,.035f,0);kit.Bevel(b.center+Vector3.up*b.extents.y,new Vector3(b.size.x+.09f,.075f,b.size.z+.12f),.018f,2);
            float floor=decks.Where(d=>d.bounds.min.x<=b.center.x&&d.bounds.max.x>=b.center.x&&d.bounds.min.z<=b.center.z&&d.bounds.max.z>=b.center.z&&d.bounds.max.y<=b.min.y+.02f).Select(d=>d.bounds.max.y+.018f).DefaultIfEmpty(b.min.y-.12f).Max();
            float mountHeight=Mathf.Max(.025f,b.min.y-floor+.015f);
            foreach(float x in new[]{-.38f,.38f})foreach(float depth in new[]{-.34f,.34f})
                kit.Bevel(new Vector3(b.center.x+x*b.size.x,floor+mountHeight*.5f,b.center.z+depth*b.size.z),new Vector3(.13f,mountHeight,.13f),.01f,2);
            float z=b.min.z-.008f;kit.Box(new Vector3(b.center.x,b.center.y,z),new Vector3(b.size.x*.78f,b.size.y*.83f,.014f),3);
            for(int n=0;n<9;n++)kit.Box(new Vector3(b.center.x,b.min.y+.20f+n*.066f,z-.015f),new Vector3(b.size.x*.7f,.026f,.04f),2);
            foreach(float side in new[]{-1f,1f})
            {kit.Box(new Vector3(b.center.x+side*b.size.x*.40f,b.center.y,z-.03f),new Vector3(.025f,b.size.y*.86f,.026f),2);kit.Box(new Vector3(b.center.x+side*b.size.x*.28f,b.center.y+.12f,z-.035f),new Vector3(.025f,.14f,.055f),2);}
            Transform owner=r.transform.IsChildOf(worlds.alteredRoot)?alteredDecor:root;
            var servicePalette=(Material[])palette.Clone();
            servicePalette[0]=r.name=="Rooftop ventilation unit"?
                Material("Blue service enamel",new Color(.12f,.46f,.55f),.28f,.08f):
                Material("Warm service enamel",new Color(.96f,.52f,.10f),.27f,.08f);
            var service=Piece(owner,"Constructed service "+r.name,kit,servicePalette);service.AddComponent<WorldSurfaceSkin>().collisionSurface=r.GetComponent<Collider>();r.enabled=false;
            var vines=new ProductionMeshKit(1);
            for(int branch=0;branch<5;branch++)
            {
                Vector3 start=new Vector3(b.max.x-.10f-branch*.15f,b.max.y+.06f,b.min.z-.04f);
                Vine(vines,start,new Vector3(-.06f,-b.size.y*(.46f+branch*.11f),-.025f),1.05f,28);
            }
            Piece(growth,"Attached equipment ivy "+r.name,vines,new[]{leaf});
        }
        int index=0;
        foreach(Bounds b in buildings.Where(b=>Mathf.Abs(b.center.x)>8))
        {
            Bounds grounded=b;grounded.SetMinMax(new Vector3(b.min.x,-29,b.min.z),b.max);
            var kit=new ProductionMeshKit(5);Building(kit,grounded,false);var building=Piece(root,"Limestone city block "+index,kit,BuildingPalette(grounded));building.AddComponent<CameraArchitectureVolume>().bounds=grounded;
            var plants=new ProductionMeshKit(1);
            if(index%2==0)
            {
                // Canopy grows from the roof drainage edge, then descends the
                // wall. Larger crowns read as volumes at gameplay distance.
                int clusters=Mathf.Max(3,Mathf.FloorToInt(b.size.z/1.45f));
                for(int n=0;n<clusters;n++)
                    IvyMass(plants,new Vector3(b.min.x+.04f,b.max.y+.04f,Mathf.Lerp(b.min.z+.6f,b.max.z-.6f,(n+.5f)/clusters)),-1,Range(2.4f,5.2f),1.65f);
            }
            if(index%2==0)Piece(growth,"Rooted city facade "+index,plants,new[]{leaf});index++;
            if(Mathf.Abs(b.center.x)<28 && b.center.z>=-12 && b.center.z<80 && index%2==0)
            {
                // Roof gardens belong to adjacent scenery, outside the playable
                // slabs. A visible supporting structure grounds the climbing
                // canopy and breaks the repeated flat roof silhouette.
                Vector3 garden=new Vector3(b.center.x,b.max.y+.35f,b.center.z);
                var terrace=new ProductionMeshKit();
                terrace.Bevel(garden,new Vector3(3.8f,.16f,3.2f),.04f,0);
                foreach(float x in new[]{-1.55f,1.55f})foreach(float z in new[]{-1.25f,1.25f})
                {
                    Vector3 basePoint=garden+new Vector3(x,.05f,z);
                    terrace.Bevel(basePoint+Vector3.up*.18f,new Vector3(.65f,.36f,.65f),.035f,0);
                    terrace.Box(basePoint+Vector3.up*1.22f,new Vector3(.09f,2.4f,.09f),1);
                    // The root wraps its actual support from planter to rafters.
                    var climber=new ProductionMeshKit(1);
                    Vine(climber,basePoint+Vector3.up*.35f,new Vector3(0,2.05f,0),1.8f,28);
                    IvyMass(climber,basePoint+Vector3.up*2.45f,Mathf.Sign(x),1.3f,1.6f);
                    Piece(growth,"Garden support climber "+index+" "+x+" "+z,climber,new[]{leaf});
                }
                foreach(float z in new[]{-1.25f,1.25f})
                    terrace.Box(garden+new Vector3(0,2.5f,z),new Vector3(3.55f,.12f,.13f),1);
                var canopy=new ProductionMeshKit(1);
                for(int beam=0;beam<9;beam++)
                {
                    float x=-1.55f+beam*.3875f;
                    terrace.Box(garden+new Vector3(x,2.57f,0),new Vector3(.10f,.10f,2.9f),1);
                    if(beam%2==0)Vine(canopy,garden+new Vector3(x,2.64f,-1.25f),new Vector3(0,0,2.5f),2.1f,25);
                }
                Piece(root,"Constructed roof garden "+index,terrace,new[]{limestone,metal,glass,roofDark});
                Piece(growth,"Supported garden canopy "+index,canopy,new[]{leaf});
            }
        }
        // Composed middle/far ring: stable geometry in both worlds. The viaduct
        // remains the skyline landmark. This is scenery, no added playable area.
        for(int block=0;block<24;block++)
        {
            float a=block*Mathf.PI*2/24;float radius=block%3==0?82:Range(105,145);
            Vector3 center=new Vector3(Mathf.Sin(a)*radius,-12,30+Mathf.Cos(a)*radius);
            float height=block%6==0?48:Range(25,37);center.y=-28+height*.5f;
            var kit=new ProductionMeshKit(5);Building(kit,new Bounds(center,new Vector3(Range(10,17),height,Range(9,15))),true);
            // Related wings make a city block with a courtyard and stepped roof,
            // rather than an isolated tower on an empty grid.
            for(int wing=0;wing<2;wing++)
            {
                float h=height*(wing==0?.57f:.72f);
                Vector3 offset=Quaternion.Euler(0,block*15,0)*new Vector3(wing==0?-13:11,0,wing==0?5:-9);
                Vector3 c=center+offset;c.y=-29+h*.5f;
                Building(kit,new Bounds(c,new Vector3(wing==0?12:9,h,12)),true);
            }
            Piece(root,"Middle distance block "+block,kit,BuildingPalette(new Bounds(center,Vector3.one)));
        }
        Viaduct(root);
        // A constructed street datum connects foundations in recovery/orbit views.
        var streets=new ProductionMeshKit();streets.Box(new Vector3(0,-29.15f,30),new Vector3(400,.2f,400),3);
        for(int lane=-6;lane<=6;lane++)
        {
            streets.Box(new Vector3(lane*26,-29.02f,30),new Vector3(6,.025f,340),2);
            streets.Box(new Vector3(0,-29.01f,lane*26+30),new Vector3(340,.025f,6),2);
        }
        Piece(root,"City street and foundation datum",streets,palette);
        // Use the registered existing image pair only for genuinely far scenery.
        foreach(var pano in UnityEngine.Object.FindObjectsByType<WorldPanorama>(FindObjectsSortMode.None))
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Atmosphere/DistantCity-v1.png");
            if(texture==null)throw new Exception("Missing authored distant city matte");
            var ti=(TextureImporter)AssetImporter.GetAtPath(Root+"/Atmosphere/DistantCity-v1.png");ti.wrapMode=TextureWrapMode.Repeat;ti.maxTextureSize=2048;ti.mipmapEnabled=true;
            var android=ti.GetPlatformTextureSettings("Android");android.overridden=true;android.format=TextureImporterFormat.ASTC_6x6;android.maxTextureSize=2048;ti.SetPlatformTextureSettings(android);ti.SaveAndReimport();
            foreach(var m in new[]{pano.present,pano.overgrown}){m.shader=Shader.Find("Shiftbound/Registered far city");m.SetTexture("_MainTex",texture);m.SetColor("_Tint",m==pano.present?Color.white:new Color(.95f,1,.92f));EditorUtility.SetDirty(m);}
            RenderSettings.skybox=pano.present;
        }
        art.sharedDecks=Array.Empty<Renderer>();art.overgrownOnly=new[]{growth.gameObject};growth.gameObject.SetActive(false);
        Camera.main.farClipPlane=240;
        // All authored support in this slice is above -1m. Below -3m the
        // courier cannot recover onto it; return promptly before an occluded
        // city-wall fall view. Jump/collision physics and reach are unchanged.
        if(decks.Any(d=>d.bounds.max.y < -1))throw new Exception("Fast recovery requires re-evaluating lower route support");
        UnityEngine.Object.FindFirstObjectByType<GameFlow>().fallHeight=-3;
        var follow=UnityEngine.Object.FindFirstObjectByType<FollowCamera>();follow.distance=6.7f;follow.lookAhead=1.5f;follow.lookHeight=1.35f;
        follow.mobileDiagnosticPipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        worlds.sun.intensity=1.8f;worlds.sun.shadowStrength=.68f;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        foreach(var atmosphere in UnityEngine.Object.FindObjectsByType<AtmosphereController>(FindObjectsSortMode.None)){atmosphere.presentFog=new Color(.62f,.74f,.85f);atmosphere.alteredFog=new Color(.63f,.75f,.64f);atmosphere.presentAmbient=new Color(.67f,.76f,.85f);atmosphere.alteredAmbient=new Color(.70f,.78f,.66f);}
        RenderSettings.fogStartDistance=48;RenderSettings.fogEndDistance=205;
        leaf.SetFloat("_Wind",.026f);
        if(leaf.GetTexture("_LeafMap")==null)throw new Exception("Ivy material lost its veined texture");
        if(before!=CollisionSignature())throw new Exception("Production city altered existing collision contract");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        Debug.Log("SHIFTBOUND PRODUCTION CITY PASSED: roofs="+roofCount+" retired renderers="+retired+" authored meshes="+meshIndex+" collision unchanged");
    }
    static void Building(ProductionMeshKit kit,Bounds b,bool distant,bool route=false)
    {
        // Near walls have real openings/reveals. A recessed core closes the
        // interior at bounded cost; the outer facade is built around each pane.
        kit.Box(b.center,distant?b.size:b.size-new Vector3(.34f,0,.34f),0);
        // Setback crown and physical coping establish a different roof silhouette.
        kit.Bevel(new Vector3(b.center.x,b.max.y+.13f,b.center.z),new Vector3(b.size.x+.24f,.25f,b.size.z+.24f),.055f,4);
        kit.Bevel(new Vector3(b.center.x,b.max.y+.22f,b.center.z),new Vector3(b.size.x-.4f,.12f,b.size.z-.4f),.025f,3);
        int floors=Mathf.Max(1,Mathf.FloorToInt(b.size.y/3.05f));
        int style=Mathf.Abs(Mathf.RoundToInt(b.center.x*3+b.center.z))%3;
        for(int side=0;side<4;side++)
        {
            float width=side%2==0?b.size.x:b.size.z,depth=side%2==0?b.size.z:b.size.x;
            int cols=Mathf.Max(1,Mathf.FloorToInt(width/2.65f));Quaternion rot=Quaternion.Euler(0,side*90,0);
            Vector3 P(float x,float y,float z)=>b.center+rot*new Vector3(x,y,z);
            for(int floor=0;floor<floors;floor++)for(int col=0;col<cols;col++)
            {
                float x=-width*.5f+(col+.5f)*width/cols,y=-b.extents.y+(floor+.53f)*b.size.y/floors;
                // An opaque recessed reveal, small sill, thin frame and split pane.
                float z=depth*.5f+.012f;float w=style==1?1.60f:col%3==0?1.42f:1.28f,h=floor==0?2.1f:style==2?1.95f:1.80f;
                float l=x-w*.5f-.075f,r=x+w*.5f+.075f,lo=y-h*.5f-.075f,hi=y+h*.5f+.075f;
                float recess=distant?z+.005f:z-.10f;
                if(!distant)
                {
                    float cellL=-width*.5f+col*width/cols,cellR=cellL+width/cols;
                    float cellB=-b.extents.y+floor*b.size.y/floors,cellT=cellB+b.size.y/floors;
                    void Wall(float a,float c,float d,float e)=>kit.Quad(P(a,d,z),P(c,d,z),P(c,e,z),P(a,e,z),0);
                    Wall(cellL,l,cellB,cellT);Wall(r,cellR,cellB,cellT);Wall(l,r,cellB,lo);Wall(l,r,hi,cellT);
                    kit.Quad(P(l,lo,z),P(l,lo,recess),P(l,hi,recess),P(l,hi,z),0);
                    kit.Quad(P(r,lo,recess),P(r,lo,z),P(r,hi,z),P(r,hi,recess),0);
                    kit.Quad(P(l,hi,z),P(l,hi,recess),P(r,hi,recess),P(r,hi,z),4);
                }
                kit.Quad(P(l,lo,recess),P(r,lo,recess),P(r,hi,recess),P(l,hi,recess),1);
                kit.Quad(P(x-w*.5f,y-h*.5f,recess+.012f),P(x+w*.5f,y-h*.5f,recess+.012f),P(x+w*.5f,y+h*.5f,recess+.012f),P(x-w*.5f,y+h*.5f,recess+.012f),2);
                if(!distant)
                {
                    Vector3 sill=P(x,y-h*.5f-.09f,z+.10f);Vector3 sillSize=side%2==0?new Vector3(w+.21f,.10f,.24f):new Vector3(.24f,.10f,w+.21f);kit.Bevel(sill,sillSize,.014f,4);
                    // Pale lintels and jambs surround recessed glass. These share
                    // a material with the coping, rather than adding per-bay draws.
                    kit.Box(P(x,y+h*.5f+.10f,z+.04f),side%2==0?new Vector3(w+.28f,.14f,.16f):new Vector3(.16f,.14f,w+.28f),4);
                    if(style==0)foreach(float sign in new[]{-1f,1f})
                        kit.Box(P(x+sign*(w*.5f+.10f),y,z+.03f),new Vector3(.12f,h+.12f,.12f),4);
                    kit.Tube(P(x,y-h*.5f,recess+.025f),P(x,y+h*.5f,recess+.025f),.018f,1,sides:4);
                    // Deep stone lintels and paired shutters give the residential
                    // bays construction and distinguish them from office glazing.
                    if(style==2)
                    {
                        foreach(float sign in new[]{-1f,1f})
                            kit.Quad(P(x+sign*(w*.5f+.06f),y-h*.5f,z+.07f),P(x+sign*(w*.5f+.34f),y-h*.5f,z+.07f),P(x+sign*(w*.5f+.34f),y+h*.5f,z+.07f),P(x+sign*(w*.5f+.06f),y+h*.5f,z+.07f),1);
                    }
                    if(floor==floors-2&&col%2==0)
                    {
                        Vector3 balcony=P(x,y-h*.5f-.10f,z+.30f);
                        kit.Bevel(balcony,side%2==0?new Vector3(w+.42f,.14f,.7f):new Vector3(.7f,.14f,w+.42f),.025f,4);
                        kit.Tube(P(x-w*.6f,y-h*.5f+.62f,z+.6f),P(x+w*.6f,y-h*.5f+.62f,z+.6f),.026f,1,sides:5);
                        for(int rail=0;rail<6;rail++)
                        {float rx=x-w*.6f+rail*w*1.2f/5;kit.Tube(P(rx,y-h*.5f,z+.6f),P(rx,y-h*.5f+.62f,z+.6f),.018f,1,sides:4);}
                    }
                }
            }
            // Real corner drainage/service pipes and a mid-height belt course.
            if(!distant)kit.Tube(P(-width*.5f+.12f,-b.extents.y,depth*.5f+.09f),P(-width*.5f+.12f,b.extents.y,depth*.5f+.09f),.06f,1,sides:8);
            if(!distant&&style==1)
                for(int pier=0;pier<=cols;pier++)
                {
                    float x=-width*.5f+pier*width/cols;
                    kit.Box(P(x,0,depth*.5f+.055f),side%2==0?new Vector3(.18f,b.size.y,.18f):new Vector3(.18f,b.size.y,.18f),0);
                }
        }
        // Continuous floor belts, raised plinth and stone roof coping relate the
        // facade to its construction and break the stamped-window appearance.
        for(int floor=1;floor<floors;floor+=distant?3:2)
            kit.Bevel(new Vector3(b.center.x,b.min.y+floor*b.size.y/floors,b.center.z),new Vector3(b.size.x+.20f,.16f,b.size.z+.20f),.025f,4);
        if(!distant)
        {
            if(!route)
                foreach(float sign in new[]{-1f,1f})
                {
                    kit.Bevel(new Vector3(b.center.x+sign*(b.extents.x-.06f),b.max.y+.47f,b.center.z),new Vector3(.18f,.64f,b.size.z),.025f,0);
                    kit.Bevel(new Vector3(b.center.x,b.max.y+.47f,b.center.z+sign*(b.extents.z-.06f)),new Vector3(b.size.x,.64f,.18f),.025f,0);
                }
            Vector3 service=new Vector3(b.center.x+.2f,b.max.y+.55f,b.center.z+.6f);
            kit.Bevel(service,new Vector3(1.1f,.8f,1.5f),.05f,1);
            for(int i=0;i<5;i++)kit.Box(service+new Vector3(0,-.20f+i*.1f,-.77f),new Vector3(.85f,.025f,.06f),3);
        }
        else
        {
            // A roof pavilion, belt course and coping give the middle city actual
            // construction and tiered silhouettes under orbit, at bounded cost.
            Vector3 pavilion=new Vector3(b.center.x,b.max.y+1.3f,b.center.z);
            kit.Bevel(pavilion,new Vector3(b.size.x*.56f,2.4f,b.size.z*.60f),.12f,0);
            kit.Bevel(pavilion+Vector3.up*1.27f,new Vector3(b.size.x*.58f,.22f,b.size.z*.62f),.07f,1);
            for(int floor=1;floor<floors;floor++)
                kit.Box(new Vector3(b.center.x,b.min.y+floor*b.size.y/floors,b.center.z),new Vector3(b.size.x+.14f,.08f,b.size.z+.14f),0);
        }
    }
    static void IvyMass(ProductionMeshKit kit,Vector3 root,float outward,float length,float scale)
    {
        // A shared root colonises the coping, then branches into a hanging mat.
        // All branches stay attached to the same wall; the clear landing center
        // is deliberately left open. Larger top leaves form the older canopy.
        for(int branch=0;branch<4;branch++)
        {
            float spread=(branch-1.5f)*.16f*scale;
            Vector3 start=root+new Vector3(-outward*.10f,0,spread);
            Vine(kit,start,new Vector3(outward*.15f,-length*(.66f+branch*.10f),spread*.35f),scale,26);
        }
        for(int branch=0;branch<3;branch++)
            Vine(kit,root+Vector3.forward*(branch-1)*.16f*scale,new Vector3(-outward*.42f,.025f,(branch-1)*.18f),scale*.85f,13);
    }
    static void Vine(ProductionMeshKit kit,Vector3 root,Vector3 direction,float scale,int leaves)
    {
        Vector3 previous=root;
        Color stem=new Color(.21f,.25f,.09f,0).linear;
        for(int i=0;i<leaves;i++)
        {
            float t=(i+1f)/leaves;
            Vector3 p=root+direction*t+new Vector3(Mathf.Sin(t*9)*.07f,Mathf.Sin(t*5)*.06f,Mathf.Cos(t*12)*.08f)*scale;
            if(i%3==2||i==leaves-1){kit.Tube(previous,p,.012f*(1-t*.65f),0,stem,4);previous=p;}
            float angle=i*137.5f;Vector3 offset=Quaternion.Euler(Range(-30,30),angle,Range(-20,20))*Vector3.right*Range(.055f,.20f)*scale;
            Vector3 end=p+offset;
            Color leafColor=Color.Lerp(new Color(.88f,.94f,.78f),new Color(1,1,.96f),Range(0,1)).linear;
            kit.Leaf(end,Quaternion.Euler(Range(-50,35),angle,Range(-35,35)),Range(.115f,.16f)*scale,leafColor);
        }
    }
    static void Viaduct(Transform root)
    {
        var kit=new ProductionMeshKit();const float z=132,y=-7,r=7;
        for(int arch=0;arch<11;arch++)
        {
            float x=-88+arch*16;
            kit.Box(new Vector3(x-8,-21,z),new Vector3(2,37,5),0);
            for(int s=0;s<16;s++)
            {
                float a=s*Mathf.PI/16,b=(s+1)*Mathf.PI/16;
                foreach(float side in new[]{-1f,1f})
                {
                    float depth=z+side*2.5f;
                    kit.Quad(new Vector3(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,depth),new Vector3(x+Mathf.Cos(b)*r,y+Mathf.Sin(b)*r,depth),
                        new Vector3(x+Mathf.Cos(b)*(r+1.4f),y+Mathf.Sin(b)*(r+1.4f),depth),new Vector3(x+Mathf.Cos(a)*(r+1.4f),y+Mathf.Sin(a)*(r+1.4f),depth));
                }
            }
        }
        kit.Bevel(new Vector3(0,2.15f,z),new Vector3(180,1.4f,5.6f),.1f,0);
        kit.Box(new Vector3(0,3.1f,z-2.5f),new Vector3(180,.45f,.22f),1);kit.Box(new Vector3(0,3.1f,z+2.5f),new Vector3(180,.45f,.22f),1);
        Piece(root,"Limestone viaduct landmark",kit,new[]{limestone,metal,glass,roofDark});
    }
}
