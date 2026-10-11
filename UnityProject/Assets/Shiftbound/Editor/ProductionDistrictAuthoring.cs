using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class ProductionUpgradeAuthoring
{
    public static void RepresentativePass()
    {
        Courier();City();District();Surfaces();RenderCourier();
        Debug.Log("SHIFTBOUND REPRESENTATIVE AUTHORING PASSED");
    }
    // Composed ground-level context for the existing route, not a new level.
    // Streets meet the -29m foundations. All scenery is static and collider-free.
    public static void District()
    {
        Folders();EditorSceneManager.OpenScene(Scene);
        string collision=CollisionSignature();
        var old=GameObject.Find("Production district - street and courtyard");
        if(old!=null)UnityEngine.Object.DestroyImmediate(old);
        var root=new GameObject("Production district - street and courtyard").transform;
        var asphalt=Material("District asphalt",new Color(.18f,.21f,.23f),.08f);
        var kerb=Material("District pale pavement",new Color(.59f,.57f,.49f),.12f);
        var brick=Material("District weathered brick",new Color(.49f,.28f,.20f),.10f);
        var trim=Material("District stone trim",new Color(.75f,.71f,.60f),.12f);
        var dark=Material("District recessed shopfront",new Color(.10f,.19f,.22f),.36f);
        var moss=Material("District courtyard planting",new Color(.16f,.28f,.19f),.05f);
        var paint=Material("District street paint",new Color(.79f,.73f,.51f),.08f);
        var street=new ProductionMeshKit(4);
        // A connected network: central delivery lane, cross street, pedestrian
        // forecourt. No floating slab decorations or random scatter.
        street.Box(new Vector3(0,-29.16f,30),new Vector3(320,.28f,340),0);
        street.Box(new Vector3(7,-28.98f,24),new Vector3(4,.10f,120),0);
        street.Box(new Vector3(0,-28.96f,-12),new Vector3(104,.12f,7),0);
        foreach(float side in new[]{-1f,1f})
        {
            street.Bevel(new Vector3(7+side*3,-28.85f,24),new Vector3(2,.32f,120),.09f,1);
            street.Bevel(new Vector3(0,-28.84f,-12+side*5.2f),new Vector3(104,.34f,3.4f),.09f,1);
            // Drainage strip explains the street edge and scale from above.
            street.Box(new Vector3(7+side*2.05f,-28.90f,24),new Vector3(.12f,.025f,116),2);
        }
        for(int line=0;line<24;line++)
            street.Box(new Vector3(7,-28.913f,-30+line*5),new Vector3(.12f,.014f,2),3);
        foreach(float z in new[]{-17f,-7f})for(int n=0;n<7;n++)
            street.Box(new Vector3(-2.7f+n*.9f,-28.89f,z),new Vector3(.5f,.018f,2.7f),3);
        DistrictPiece(root,"Connected streets and pavements",street,new[]{asphalt,kerb,dark,paint});

        // A low mixed-use perimeter block behind the opening rooftop. Two
        // wings frame an occupied courtyard, with a visible shop arcade below.
        var block=new ProductionMeshKit(4);
        DistrictWing(block,new Vector3(-27,-20,-31),new Vector3(28,18,9),false);
        DistrictWing(block,new Vector3(-39,-21,-18),new Vector3(8,16,18),true);
        DistrictWing(block,new Vector3(26,-22,-30),new Vector3(24,14,10),true);
        DistrictPiece(root,"Courtyard perimeter and corner shops",block,new[]{brick,trim,dark,asphalt});
        var court=new ProductionMeshKit(4);
        court.Bevel(new Vector3(-26,-28.82f,-19),new Vector3(18,.3f,13),.06f,0);
        // Ordered circulation around a planted sunken court; enclosing walls,
        // seating and a drainage channel are one authored ground composition.
        court.Box(new Vector3(-26,-28.64f,-19),new Vector3(9,.13f,6),2);
        foreach(float side in new[]{-1f,1f})
        {
            court.Bevel(new Vector3(-26+side*5,-28.42f,-19),new Vector3(.65f,.56f,7.6f),.08f,1);
            court.Bevel(new Vector3(-26,-28.43f,-19+side*3.9f),new Vector3(10.65f,.55f,.65f),.08f,1);
            court.Box(new Vector3(-26+side*7,-28.49f,-19),new Vector3(.8f,.48f,4),3);
        }
        DistrictPiece(root,"Enclosed courtyard and seating",court,new[]{kerb,trim,moss,brick});
        var growth=new GameObject("Courtyard - attached Overgrown canopy").transform;
        growth.SetParent(root,false);random=new System.Random(7814);
        var plants=new ProductionMeshKit(1);
        // Roots attach to courtyard planting and the two actual roof edges.
        IvyMass(plants,new Vector3(-27,-10.9f,-26.6f),1,3.5f,1.35f);
        IvyMass(plants,new Vector3(-38,-12.9f,-10),-1,3.2f,1.25f);
        IvyMass(plants,new Vector3(-30,-28.35f,-19),1,.8f,1.35f);
        var ivy=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Rooted leaves.mat");
        if(ivy==null)throw new Exception("Missing existing foliage material");
        DistrictPiece(growth,"Courtyard rooted canopy",plants,new[]{ivy});
        var art=UnityEngine.Object.FindFirstObjectByType<Shiftbound.RooftopWorldArt>();
        art.overgrownOnly=art.overgrownOnly.Where(o=>o!=null).Concat(new[]{growth.gameObject}).ToArray();
        growth.gameObject.SetActive(false);
        // Shared palette: saturated facade fields previously made every mass
        // look like the same toy block. Retain colour as a district accent.
        Material("Ivory mineral render",new Color(.70f,.66f,.55f),.12f);
        Material("Warm terracotta render",new Color(.58f,.32f,.23f),.10f);
        Material("Cool limestone render",new Color(.42f,.52f,.51f),.13f);
        Material("Blue recessed glazing",new Color(.14f,.23f,.27f),.52f,.12f);
        if(collision!=CollisionSignature())throw new Exception("District changed route collision");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        Debug.Log("SHIFTBOUND DISTRICT AUTHORING PASSED: connected streets, mixed-use perimeter, planted courtyard; unchanged collision");
    }
    static void DistrictPiece(Transform parent,string name,ProductionMeshKit kit,Material[] materials)
    {
        string path=Root+"/Meshes/"+name+".asset";
        Mesh mesh=kit.Create(name);var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing!=null){EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}
        else AssetDatabase.CreateAsset(mesh,path);
        var o=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(parent,false);
        o.GetComponent<MeshFilter>().sharedMesh=mesh;
        var r=o.GetComponent<MeshRenderer>();r.sharedMaterials=materials;
        // Below the playable roofs: avoid filling the mobile shadow atlas.
        r.shadowCastingMode=ShadowCastingMode.Off;
        GameObjectUtility.SetStaticEditorFlags(o,StaticEditorFlags.BatchingStatic);
    }
    static void DistrictWing(ProductionMeshKit kit,Vector3 c,Vector3 size,bool gable)
    {
        Bounds b=new Bounds(c,size);kit.Box(c,size,0);
        kit.Bevel(new Vector3(c.x,b.max.y+.1f,c.z),new Vector3(size.x+.3f,.24f,size.z+.3f),.04f,1);
        // A sloping tiled roof differentiates residential courtyard wings from
        // the flat service roofs on the playable route.
        if(gable)
        {
            float top=b.max.y+1.8f;
            kit.Quad(new Vector3(b.min.x,top,c.z),new Vector3(b.max.x,top,c.z),new Vector3(b.max.x,b.max.y,b.min.z),new Vector3(b.min.x,b.max.y,b.min.z),3);
            kit.Quad(new Vector3(b.min.x,b.max.y,b.max.z),new Vector3(b.max.x,b.max.y,b.max.z),new Vector3(b.max.x,top,c.z),new Vector3(b.min.x,top,c.z),3);
            kit.Triangle(new Vector3(b.min.x,b.max.y,b.max.z),new Vector3(b.min.x,top,c.z),new Vector3(b.min.x,b.max.y,b.min.z),0);
            kit.Triangle(new Vector3(b.max.x,b.max.y,b.min.z),new Vector3(b.max.x,top,c.z),new Vector3(b.max.x,b.max.y,b.max.z),0);
        }
        int bays=Mathf.Max(2,Mathf.FloorToInt(size.x/3));
        for(int side=0;side<2;side++)for(int bay=0;bay<bays;bay++)
        {
            float x=b.min.x+(bay+.5f)*size.x/bays,z=side==0?b.min.z-.04f:b.max.z+.04f;
            for(float y=b.min.y+2;y<b.max.y-1;y+=3.1f)
            {
                float width=y<b.min.y+3?2.3f:1.35f,height=y<b.min.y+3?2.7f:1.75f;
                kit.Box(new Vector3(x,y,z),new Vector3(width+.22f,height+.22f,.07f),1);
                kit.Box(new Vector3(x,y,z+(side==0?-.045f:.045f)),new Vector3(width,height,.03f),2);
                kit.Box(new Vector3(x,y-height*.5f-.12f,z),new Vector3(width+.35f,.16f,.35f),1);
            }
        }
    }
}
