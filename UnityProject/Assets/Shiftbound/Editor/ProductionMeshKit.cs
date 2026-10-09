using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Offline mesh authoring. Shared materials and per-roof meshes bound draw cost;
// decorative meshes contain no gameplay colliders or runtime generators.
public sealed class ProductionMeshKit
{
    readonly List<Vector3> vertices=new List<Vector3>();
    readonly List<Vector2> uv=new List<Vector2>();
    readonly List<Color> colors=new List<Color>();
    readonly List<int>[] slots;
    public ProductionMeshKit(int count=4){slots=new List<int>[count];for(int i=0;i<count;i++)slots[i]=new List<int>();}
    public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,int slot=0,Color color=default)
    {
        if(color==default)color=Color.white;int k=vertices.Count;vertices.AddRange(new[]{a,b,c,d});
        float w=Vector3.Distance(a,b)*.5f,h=Vector3.Distance(b,c)*.5f;uv.AddRange(new[]{Vector2.zero,new Vector2(w,0),new Vector2(w,h),new Vector2(0,h)});
        colors.AddRange(new[]{color,color,color,color});slots[slot].AddRange(new[]{k,k+1,k+2,k,k+2,k+3});
    }
    public void Box(Vector3 c,Vector3 s,int slot=0)
    {
        Vector3 P(float x,float y,float z)=>c+Vector3.Scale(new Vector3(x,y,z)*.5f,s);
        Quad(P(-1,-1,1),P(1,-1,1),P(1,1,1),P(-1,1,1),slot);Quad(P(1,-1,-1),P(-1,-1,-1),P(-1,1,-1),P(1,1,-1),slot);
        Quad(P(-1,-1,-1),P(-1,-1,1),P(-1,1,1),P(-1,1,-1),slot);Quad(P(1,-1,1),P(1,-1,-1),P(1,1,-1),P(1,1,1),slot);
        Quad(P(-1,1,1),P(1,1,1),P(1,1,-1),P(-1,1,-1),slot);Quad(P(-1,-1,-1),P(1,-1,-1),P(1,-1,1),P(-1,-1,1),slot);
    }
    public void Bevel(Vector3 c,Vector3 s,float r,int slot=0)
    {
        float x=s.x*.5f,z=s.z*.5f,y=s.y*.5f;r=Mathf.Min(r,Mathf.Min(x,z)*.25f);
        Vector3[] a={new Vector3(-x+r,0,-z),new Vector3(x-r,0,-z),new Vector3(x,0,-z+r),new Vector3(x,0,z-r),new Vector3(x-r,0,z),new Vector3(-x+r,0,z),new Vector3(-x,0,z-r),new Vector3(-x,0,-z+r)};
        for(int i=0;i<8;i++)
        {
            int j=(i+1)%8;Vector3 topA=a[i]*.99f,topB=a[j]*.99f;
            Quad(c+a[j]+Vector3.down*y,c+a[i]+Vector3.down*y,c+a[i]+Vector3.up*(y-r),c+a[j]+Vector3.up*(y-r),slot);
            Quad(c+a[j]+Vector3.up*(y-r),c+a[i]+Vector3.up*(y-r),c+topA+Vector3.up*y,c+topB+Vector3.up*y,slot);
            Quad(c+Vector3.up*y,c+topB+Vector3.up*y,c+topA+Vector3.up*y,c+Vector3.up*y,slot);
            Quad(c+Vector3.down*y,c+a[i]+Vector3.down*y,c+a[j]+Vector3.down*y,c+Vector3.down*y,slot);
        }
    }
    public void Tube(Vector3 a,Vector3 b,float radius,int slot=0,Color color=default,int sides=6)
    {
        Vector3 axis=(b-a).normalized;Vector3 right=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f?Vector3.forward:Vector3.up).normalized;
        Vector3 up=Vector3.Cross(axis,right);
        for(int i=0;i<sides;i++){float t=i*Mathf.PI*2/sides,n=(i+1)*Mathf.PI*2/sides;Vector3 u=(right*Mathf.Cos(t)+up*Mathf.Sin(t))*radius,v=(right*Mathf.Cos(n)+up*Mathf.Sin(n))*radius;Quad(a+u,b+u,b+v,a+v,slot,color);}
    }
    public void Leaf(Vector3 center,Quaternion rotation,float size,Color color)
    {
        // Folded heart-shaped leaf with a real central ridge and irregular lobes.
        Vector3[] edge={new Vector3(0,0,-.34f),new Vector3(-.42f,.02f,-.52f),new Vector3(-.64f,-.025f,-.22f),new Vector3(-.48f,-.02f,.26f),new Vector3(0,0,.82f),new Vector3(.48f,-.02f,.26f),new Vector3(.64f,-.025f,-.22f),new Vector3(.42f,.02f,-.52f)};
        Vector3 P(Vector3 p)=>center+rotation*(p*size);
        for(int i=0;i<8;i++){int j=(i+1)%8;Color c=color*(i<4?.94f:1.04f);c.a=1;Quad(P(new Vector3(0,.085f,0)),P(edge[i]),P(edge[j]),P(new Vector3(0,.085f,0)),0,c);}
        Color vein=color*1.35f;vein.a=.8f;Tube(P(new Vector3(0,.088f,-.32f)),P(new Vector3(0,.023f,.65f)),size*.009f,0,vein,3);
    }
    public Mesh Create(string name)
    {
        var m=new Mesh{name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
        m.SetVertices(vertices);m.SetUVs(0,uv);m.SetColors(colors);m.subMeshCount=slots.Length;
        for(int i=0;i<slots.Length;i++)m.SetTriangles(slots[i],i);
        m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;
    }
}
