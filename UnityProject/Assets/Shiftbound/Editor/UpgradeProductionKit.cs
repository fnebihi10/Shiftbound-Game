using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class UpgradeProduction
{
    // Metre-authored kit: closed profiles, separate material groups, and UVs
    // derived from dimensions. Generated once in the Editor; no runtime builders.
    internal sealed class Kit
    {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>();
        readonly List<int> triangles=new List<int>();

        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float density=.5f)
        {
            int start=vertices.Count;
            vertices.AddRange(new[]{a,b,c,d});
            uv.AddRange(new[]{Vector2.zero,new Vector2(Vector3.Distance(a,b)*density,0),new Vector2(Vector3.Distance(a,b)*density,Vector3.Distance(b,c)*density),new Vector2(0,Vector3.Distance(b,c)*density)});
            triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
        }
        public void Triangle(Vector3 a,Vector3 b,Vector3 c)
        {
            int i=vertices.Count;vertices.AddRange(new[]{a,b,c});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.up});triangles.AddRange(new[]{i,i+1,i+2});
        }
        public void Box(Vector3 p,Vector3 size,float bevel=.015f,Quaternion? rotation=null)
        {
            Quaternion r=rotation??Quaternion.identity;
            Vector3 Point(float x,float y,float z)=>p+r*new Vector3(x,y,z);
            float x=size.x*.5f,z=size.z*.5f,h=size.y*.5f;
            // Sub-pixel bevels on windows do not justify eight-sided profiles
            // on a phone. Keep the detailed profile for coping and the satchel.
            if(bevel<.02f)
            {
                Quad(Point(-x,-h,-z),Point(-x,h,-z),Point(x,h,-z),Point(x,-h,-z));
                Quad(Point(x,-h,z),Point(x,h,z),Point(-x,h,z),Point(-x,-h,z));
                Quad(Point(-x,-h,z),Point(-x,h,z),Point(-x,h,-z),Point(-x,-h,-z));
                Quad(Point(x,-h,-z),Point(x,h,-z),Point(x,h,z),Point(x,-h,z));
                Quad(Point(-x,h,-z),Point(-x,h,z),Point(x,h,z),Point(x,h,-z));
                Quad(Point(-x,-h,z),Point(-x,-h,-z),Point(x,-h,-z),Point(x,-h,z));
                return;
            }
            float b=Mathf.Min(bevel,Mathf.Min(x,z)*.4f);
            Vector2[] shape={new Vector2(-x+b,-z),new Vector2(x-b,-z),new Vector2(x,-z+b),new Vector2(x,z-b),new Vector2(x-b,z),new Vector2(-x+b,z),new Vector2(-x,z-b),new Vector2(-x,-z+b)};
            float lip=Mathf.Min(bevel,h*.35f);
            for(int i=0;i<8;i++)
            {
                var a=shape[i];var next=shape[(i+1)%8];
                Quad(Point(a.x,-h,a.y),Point(a.x,h-lip,a.y),Point(next.x,h-lip,next.y),Point(next.x,-h,next.y));
                Vector2 at=a*(1f-b/Mathf.Max(.01f,Mathf.Min(x,z))*.4f),bt=next*(1f-b/Mathf.Max(.01f,Mathf.Min(x,z))*.4f);
                Quad(Point(a.x,h-lip,a.y),Point(at.x,h,at.y),Point(bt.x,h,bt.y),Point(next.x,h-lip,next.y));
                Triangle(Point(0,h,0),Point(bt.x,h,bt.y),Point(at.x,h,at.y));
                Triangle(Point(0,-h,0),Point(a.x,-h,a.y),Point(next.x,-h,next.y));
            }
        }
        public void Tube(Vector3 a,Vector3 b,float radius,int sides=6)
        {
            Vector3 axis=(b-a).normalized;
            Vector3 u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.right:Vector3.up).normalized;
            Vector3 v=Vector3.Cross(axis,u);
            for(int i=0;i<sides;i++)
            {
                float t=i*Mathf.PI*2/sides,n=(i+1)*Mathf.PI*2/sides;
                Vector3 p=(u*Mathf.Cos(t)+v*Mathf.Sin(t))*radius,q=(u*Mathf.Cos(n)+v*Mathf.Sin(n))*radius;
                Quad(a+p,a+q,b+q,b+p);Triangle(a,a+q,a+p);Triangle(b,b+p,b+q);
            }
        }
        public void Leaf(Vector3 p,float length,float width,Quaternion rotation)
        {
            Vector3[] shape={new Vector3(0,0,-length*.5f),new Vector3(-width*.4f,0,-length*.22f),new Vector3(-width*.5f,0,length*.12f),new Vector3(0,0,length*.5f),new Vector3(width*.5f,0,length*.12f),new Vector3(width*.4f,0,-length*.22f)};
            Vector3 top=p+rotation*new Vector3(0,width*.15f,0),bottom=p+rotation*new Vector3(0,-.004f,0);
            for(int i=0;i<shape.Length;i++)
            {
                Vector3 a=p+rotation*shape[i],b=p+rotation*shape[(i+1)%shape.Length];
                Triangle(top,a,b);Triangle(bottom,b,a);
            }
        }
        public Mesh Save(string name)
        {
            string path=Folder+"/Meshes/"+name+".asset";
            Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,path);}else mesh.Clear();
            mesh.name=name;mesh.indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16;
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);return mesh;
        }
    }

    internal static Material Solid(string name,Color color,float roughness=.2f,float metal=0f)
    {
        string path=Folder+"/Materials/"+name+".mat";
        Material m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",roughness);m.SetFloat("_Metallic",metal);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
    }
    internal static GameObject PlaceKit(Transform parent,string name,Kit kit,Material material,bool isStatic=true)
    {
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
        go.GetComponent<MeshFilter>().sharedMesh=kit.Save(name);go.GetComponent<MeshRenderer>().sharedMaterial=material;
        if(isStatic)GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic);
        return go;
    }

    private static void AuthorCourier(Shiftbound.RiggedCourierAnimator courier,Animator animator,AnimationClip idle)
    {
        idle.SampleAnimation(courier.gameObject,0f);
        var chest=animator.GetBoneTransform(HumanBodyBones.Chest);
        if(chest==null)throw new Exception("Courier chest bone is required.");
        var old=animator.GetBoneTransform(HumanBodyBones.Spine).Find("Courier travel backpack");
        if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var existing=chest.Find("Courier field kit");if(existing!=null)UnityEngine.Object.DestroyImmediate(existing.gameObject);
        Transform root=new GameObject("Courier field kit").transform;root.SetParent(chest,false);
        // Fit in the courier frame, then retain the chest bone's authored bind.
        Vector3 facing=-courier.transform.forward;
        root.SetPositionAndRotation(chest.position-facing*.17f-Vector3.up*.055f,Quaternion.LookRotation(facing,Vector3.up));
        Vector3 inheritedScale=root.lossyScale;
        root.localScale=new Vector3(1f/inheritedScale.x,1f/inheritedScale.y,1f/inheritedScale.z);
        var fabric=Solid("Courier petrol canvas",new Color(.055f,.22f,.25f),.12f);
        var leather=Solid("Courier dark webbing",new Color(.075f,.09f,.105f),.13f);
        var buckle=Solid("Courier brushed brass",new Color(.64f,.51f,.30f),.45f,.65f);
        var ochre=Solid("Courier saffron shell",new Color(.80f,.40f,.105f),.14f);
        var trouser=Solid("Courier indigo work trousers",new Color(.07f,.115f,.17f),.12f);
        var shoe=Solid("Courier graphite boots",new Color(.055f,.065f,.075f),.15f);
        foreach(var skin in courier.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var materials=skin.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                if(materials[i].name=="Courierjacket")materials[i]=ochre;
                if(materials[i].name=="Couriertrousers" || skin.name=="Casual_Legs")materials[i]=trouser;
                if(materials[i].name=="Couriershoes")materials[i]=shoe;
            }
            skin.sharedMaterials=materials;
        }
        var pack=new Kit();
        pack.Box(new Vector3(0,0,-.025f),new Vector3(.34f,.43f,.20f),.045f);
        pack.Box(new Vector3(0,.225f,-.035f),new Vector3(.32f,.08f,.15f),.028f);
        pack.Box(new Vector3(0,-.08f,-.135f),new Vector3(.25f,.18f,.04f),.02f);
        PlaceKit(root,"Courier rolltop satchel",pack,fabric,false);
        var trim=new Kit();var straps=new Kit();
        foreach(int sign in new[]{-1,1})
        {
            // Webbing contours around the shoulder rather than floating in a
            // rectangular block above the back. Chest ownership preserves fit.
            Vector3[] p={new Vector3(sign*.12f,-.17f,.09f),new Vector3(sign*.15f,.08f,.08f),new Vector3(sign*.19f,.30f,.10f),new Vector3(sign*.18f,.33f,.24f),new Vector3(sign*.16f,.12f,.35f),new Vector3(sign*.13f,-.20f,.28f)};
            for(int i=0;i<p.Length-1;i++)
            {
                Vector3 d=p[i+1]-p[i];straps.Box((p[i]+p[i+1])*.5f,new Vector3(.04f,.013f,d.magnitude+.012f),.003f,Quaternion.LookRotation(d));
            }
            straps.Box(new Vector3(sign*.115f,-.035f,-.153f),new Vector3(.024f,.32f,.013f),.003f);
            trim.Box(new Vector3(sign*.115f,.07f,-.165f),new Vector3(.044f,.052f,.018f),.007f);
        }
        trim.Box(new Vector3(0,-.045f,-.162f),new Vector3(.12f,.033f,.012f),.004f);
        trim.Box(new Vector3(0,-.086f,-.162f),new Vector3(.058f,.012f,.012f),.003f);
        PlaceKit(root,"Courier contoured webbing",straps,leather,false);
        PlaceKit(root,"Courier buckles and route badge",trim,buckle,false);
        Debug.Log("UPGRADE CHARACTER: saffron workwear, indigo legs, petrol rolltop satchel; existing licensed humanoid skin and rig retained.");
    }
}
