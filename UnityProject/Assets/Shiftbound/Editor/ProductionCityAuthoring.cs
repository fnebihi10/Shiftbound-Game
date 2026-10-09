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
        var licensed=AssetDatabase.LoadAssetAtPath<Material>("Assets/Shiftbound/ProductionBenchmark/Materials/Roof concrete.mat");
        if(licensed!=null){paving.SetTexture("_BaseMap",licensed.GetTexture("_BaseMap"));paving.SetTexture("_BumpMap",licensed.GetTexture("_BumpMap"));paving.SetFloat("_BumpScale",.18f);paving.EnableKeyword("_NORMALMAP");}
        limestone=Material("Limestone coping",new Color(.78f,.77f,.67f),.16f);
        var facade=AssetDatabase.LoadAssetAtPath<Material>("Assets/Shiftbound/ProductionBenchmark/Materials/Facade limestone.mat");
        if(facade!=null){limestone.SetTexture("_BaseMap",facade.GetTexture("_BaseMap"));limestone.SetTexture("_BumpMap",facade.GetTexture("_BumpMap"));limestone.EnableKeyword("_NORMALMAP");limestone.SetFloat("_BumpScale",.25f);}
        metal=Material("Weathered zinc",new Color(.29f,.34f,.34f),.42f,.65f);
        glass=Material("Blue recessed glazing",new Color(.11f,.21f,.25f),.7f,.1f);
        roofDark=Material("Roof membrane",new Color(.21f,.26f,.27f),.16f);
        leaf=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Rooted leaves.mat");
        if(leaf==null){leaf=new Material(Shader.Find("Shiftbound/Rooted foliage"));AssetDatabase.CreateAsset(leaf,Root+"/Materials/Rooted leaves.mat");}
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
                Bounds structure=new Bounds();structure.SetMinMax(new Vector3(b.min.x-.125f,-29,b.min.z-.125f),new Vector3(b.max.x+.125f,b.max.y-.75f,b.max.z+.125f));
                var shell=new ProductionMeshKit();Building(shell,structure,false);Piece(root,"Route building "+r.name,shell,new[]{limestone,metal,glass,roofDark});
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
                    Vine(vines,new Vector3(x,b.max.y+.03f,z),new Vector3(side*.2f,-Range(1.0f,2.2f),Range(-.3f,.3f)),Range(.9f,1.15f),36);
                    Vine(vines,new Vector3(x,b.max.y+.03f,z),new Vector3(-side*.45f,.15f,Range(-.7f,.8f)),.65f,22);
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
            Piece(root,"Coping construction "+r.name,kit,palette);r.enabled=false;
        }
        // Re-author the three existing equipment/collision objects as service
        // cabinets with an actual raised cap, hinges, louver and panel recesses.
        foreach(var r in originals.Where(r=>r.GetComponent<BoxCollider>()!=null&&!r.GetComponent<BoxCollider>().isTrigger&&(r.name=="Rooftop utility"||r.name=="Alternate wall test"||r.name=="Rooftop ventilation unit")))
        {
            Bounds b=r.bounds;var kit=new ProductionMeshKit();
            kit.Bevel(b.center,b.size,.035f,2);kit.Bevel(b.center+Vector3.up*b.extents.y,new Vector3(b.size.x+.09f,.075f,b.size.z+.12f),.018f,2);
            float z=b.min.z-.008f;kit.Box(new Vector3(b.center.x,b.center.y,z),new Vector3(b.size.x*.78f,b.size.y*.83f,.014f),3);
            for(int n=0;n<9;n++)kit.Box(new Vector3(b.center.x,b.min.y+.20f+n*.066f,z-.015f),new Vector3(b.size.x*.7f,.026f,.04f),2);
            foreach(float side in new[]{-1f,1f})
            {kit.Box(new Vector3(b.center.x+side*b.size.x*.40f,b.center.y,z-.03f),new Vector3(.025f,b.size.y*.86f,.026f),2);kit.Box(new Vector3(b.center.x+side*b.size.x*.28f,b.center.y+.12f,z-.035f),new Vector3(.025f,.14f,.055f),2);}
            Transform owner=r.transform.IsChildOf(worlds.alteredRoot)?alteredDecor:root;
            var service=Piece(owner,"Constructed service "+r.name,kit,palette);service.AddComponent<WorldSurfaceSkin>().collisionSurface=r.GetComponent<Collider>();r.enabled=false;
            var vines=new ProductionMeshKit(1);Vine(vines,new Vector3(b.max.x-.05f,b.max.y+.08f,b.min.z+.06f),new Vector3(-.22f,-b.size.y*.75f,-.08f),.9f,40);Piece(growth,"Attached equipment ivy "+r.name,vines,new[]{leaf});
        }
        int index=0;
        foreach(Bounds b in buildings.Where(b=>Mathf.Abs(b.center.x)>8))
        {
            Bounds grounded=b;grounded.SetMinMax(new Vector3(b.min.x,-29,b.min.z),b.max);
            var kit=new ProductionMeshKit();Building(kit,grounded,false);Piece(root,"Limestone city block "+index,kit,new[]{limestone,metal,glass,roofDark});
            var plants=new ProductionMeshKit(1);
            if(index%2==0)for(int n=0;n<4;n++)Vine(plants,new Vector3(b.min.x+.3f,b.max.y+.05f,b.min.z+1+n*1.2f),new Vector3(-.12f,-Range(2,4),.4f),1.2f,35);
            if(index%2==0)Piece(growth,"Rooted city facade "+index,plants,new[]{leaf});index++;
        }
        // Composed middle/far ring: stable geometry in both worlds. The viaduct
        // remains the skyline landmark. This is scenery, no added playable area.
        for(int block=0;block<24;block++)
        {
            float a=block*Mathf.PI*2/24;float radius=block%3==0?82:Range(105,145);
            Vector3 center=new Vector3(Mathf.Sin(a)*radius,-12,30+Mathf.Cos(a)*radius);
            float height=block%6==0?48:Range(25,37);center.y=-28+height*.5f;
            var kit=new ProductionMeshKit();Building(kit,new Bounds(center,new Vector3(Range(10,17),height,Range(9,15))),true);
            Piece(root,"Middle distance block "+block,kit,new[]{limestone,metal,glass,roofDark});
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
            foreach(var m in new[]{pano.present,pano.overgrown}){Texture texture=m.GetTexture("_MainTex");m.shader=Shader.Find("Shiftbound/Registered far city");m.SetTexture("_MainTex",texture);EditorUtility.SetDirty(m);}
            RenderSettings.skybox=pano.present;
        }
        art.sharedDecks=Array.Empty<Renderer>();art.overgrownOnly=new[]{growth.gameObject};growth.gameObject.SetActive(false);
        Camera.main.farClipPlane=240;
        var follow=UnityEngine.Object.FindFirstObjectByType<FollowCamera>();follow.distance=6.7f;follow.lookAhead=1.5f;follow.lookHeight=1.35f;
        worlds.sun.intensity=1.65f;worlds.sun.shadowStrength=.85f;
        foreach(var atmosphere in UnityEngine.Object.FindObjectsByType<AtmosphereController>(FindObjectsSortMode.None)){atmosphere.presentFog=new Color(.68f,.77f,.81f);atmosphere.alteredFog=new Color(.73f,.77f,.68f);}
        RenderSettings.fogStartDistance=72;RenderSettings.fogEndDistance=250;
        if(before!=CollisionSignature())throw new Exception("Production city altered existing collision contract");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        Debug.Log("SHIFTBOUND PRODUCTION CITY PASSED: roofs="+roofCount+" retired renderers="+retired+" authored meshes="+meshIndex+" collision unchanged");
    }
    static void Building(ProductionMeshKit kit,Bounds b,bool distant)
    {
        kit.Box(b.center,b.size,0);
        // Setback crown and physical coping establish a different roof silhouette.
        kit.Bevel(new Vector3(b.center.x,b.max.y+.13f,b.center.z),new Vector3(b.size.x+.24f,.25f,b.size.z+.24f),.055f,0);
        kit.Bevel(new Vector3(b.center.x,b.max.y+.22f,b.center.z),new Vector3(b.size.x-.4f,.12f,b.size.z-.4f),.025f,3);
        int floors=Mathf.Max(1,Mathf.FloorToInt(b.size.y/3.05f));
        for(int side=0;side<4;side++)
        {
            float width=side%2==0?b.size.x:b.size.z,depth=side%2==0?b.size.z:b.size.x;
            int cols=Mathf.Max(1,Mathf.FloorToInt(width/2.65f));Quaternion rot=Quaternion.Euler(0,side*90,0);
            Vector3 P(float x,float y,float z)=>b.center+rot*new Vector3(x,y,z);
            for(int floor=0;floor<floors;floor++)for(int col=0;col<cols;col++)
            {
                float x=-width*.5f+(col+.5f)*width/cols,y=-b.extents.y+(floor+.53f)*b.size.y/floors;
                // An opaque recessed reveal, small sill, thin frame and split pane.
                float z=depth*.5f+.012f;float w=col%3==0?1.05f:.85f,h=1.55f;
                kit.Quad(P(x-w*.5f-.075f,y-h*.5f-.075f,z),P(x+w*.5f+.075f,y-h*.5f-.075f,z),P(x+w*.5f+.075f,y+h*.5f+.075f,z),P(x-w*.5f-.075f,y+h*.5f+.075f,z),1);
                kit.Quad(P(x-w*.5f,y-h*.5f,z+.016f),P(x+w*.5f,y-h*.5f,z+.016f),P(x+w*.5f,y+h*.5f,z+.016f),P(x-w*.5f,y+h*.5f,z+.016f),2);
                if(!distant)
                {
                    Vector3 sill=P(x,y-h*.5f-.09f,z+.10f);Vector3 sillSize=side%2==0?new Vector3(w+.21f,.10f,.24f):new Vector3(.24f,.10f,w+.21f);kit.Bevel(sill,sillSize,.014f,0);
                    kit.Tube(P(x,y-h*.5f,z+.025f),P(x,y+h*.5f,z+.025f),.018f,1,sides:4);
                }
            }
            // Real corner drainage/service pipes and a mid-height belt course.
            if(!distant)kit.Tube(P(-width*.5f+.12f,-b.extents.y,depth*.5f+.09f),P(-width*.5f+.12f,b.extents.y,depth*.5f+.09f),.06f,1,sides:8);
        }
        if(!distant)
        {
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
    static void Vine(ProductionMeshKit kit,Vector3 root,Vector3 direction,float scale,int leaves)
    {
        Vector3 previous=root;
        Color stem=new Color(.21f,.25f,.09f,0);
        for(int i=0;i<leaves;i++)
        {
            float t=(i+1f)/leaves;
            Vector3 p=root+direction*t+new Vector3(Mathf.Sin(t*9)*.07f,Mathf.Sin(t*5)*.06f,Mathf.Cos(t*12)*.08f)*scale;
            kit.Tube(previous,p,.012f*(1-t*.65f),0,stem,5);previous=p;
            float angle=i*137.5f;Vector3 offset=Quaternion.Euler(Range(-30,30),angle,Range(-20,20))*Vector3.right*Range(.055f,.20f)*scale;
            Vector3 end=p+offset;kit.Tube(p,end,.005f,0,stem,3);
            Color leafColor=Color.Lerp(new Color(.13f,.28f,.055f),new Color(.38f,.52f,.12f),Range(0,1));
            kit.Leaf(end,Quaternion.Euler(Range(-65,55),angle,Range(-40,40)),Range(.13f,.22f)*scale,leafColor);
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
