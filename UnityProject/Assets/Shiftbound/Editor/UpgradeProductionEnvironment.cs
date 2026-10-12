using System;
using System.Collections.Generic;
using System.Linq;
using Shiftbound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static partial class UpgradeProduction
{
    public static void RefineVegetation()
    {
        EditorSceneManager.OpenScene(Scene);
        int removed=0;
        foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(r.name=="Overgrowth silhouette") {r.enabled=false;EditorUtility.SetDirty(r);removed++;}
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("UPGRADE VEGETATION REFINED: removed "+removed+" unsupported spherical proxies; rooted shrubs retained.");
    }
    public static void ApplyEnvironment()
    {
        EditorSceneManager.OpenScene(Scene);EnsureFolders();
        var previous=GameObject.Find("Production roof and facade kit");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous);
        foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if((filter.name.StartsWith("Roof ",StringComparison.Ordinal) && (filter.name.EndsWith(" coping",StringComparison.Ordinal) || filter.name.EndsWith(" gutter",StringComparison.Ordinal))) || filter.name.StartsWith("Attached rooftop shrubs",StringComparison.Ordinal) || filter.name.StartsWith("Rooftop shrub branches",StringComparison.Ordinal))UnityEngine.Object.DestroyImmediate(filter.gameObject);
        var root=new GameObject("Production roof and facade kit").transform;
        var stone=Solid("Weathered limestone coping",new Color(.64f,.61f,.51f),.18f);
        var metal=Solid("Oxidised zinc drainage",new Color(.25f,.29f,.29f),.32f,.42f);
        var dark=Solid("Window recess and seals",new Color(.06f,.105f,.12f),.14f);
        var frame=Solid("Painted teal joinery",new Color(.11f,.20f,.21f),.3f,.15f);
        var glass=Solid("Muted sky glass",new Color(.145f,.24f,.26f),.62f,.15f);
        var leaf=Solid("Ivy mature leaf",new Color(.13f,.28f,.095f),.12f);
        var fresh=Solid("Ivy young leaf",new Color(.25f,.37f,.12f),.1f);
        var stem=Solid("Ivy woody stems",new Color(.20f,.17f,.09f),.1f);
        var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        var worlds=UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        var art=UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();
        var growth=new List<GameObject>();
        var preserved=art.overgrownOnly.Where(g=>g!=null).ToList();
        foreach(var r in all)
        {
            if(r.name=="Overgrowth silhouette" || r.name=="Recessed window" || r.name=="Window ledge" || r.name.Contains("ivy") || r.name.Contains("roof edge moss") || r.name.Contains("Stone slab"))r.enabled=false;
        }
        foreach(var parent in new[]{root,worlds.presentRoot,worlds.alteredRoot})
        {
            bool InGroup(Renderer r)=>parent==root?!r.transform.IsChildOf(worlds.presentRoot) && !r.transform.IsChildOf(worlds.alteredRoot):r.transform.IsChildOf(parent);
            var oldPlants=all.Where(r=>r.name.StartsWith("Plant stem",StringComparison.Ordinal) && InGroup(r)).ToArray();
            if(oldPlants.Length==0)continue;
            var stems=new Kit();var bush=new Kit();
            foreach(var plant in oldPlants)
            {
                Bounds b=plant.bounds;
                Vector3 basePoint=new Vector3(b.center.x,b.min.y,b.center.z);
                float height=Mathf.Clamp(b.size.y,.3f,.9f);
                for(int branch=0;branch<5;branch++)
                {
                    float angle=branch*137.5f*Mathf.Deg2Rad;
                    Vector3 side=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
                    Vector3 tip=basePoint+Vector3.up*(height*(.55f+branch*.1f))+side*.22f;
                    stems.Tube(basePoint,tip,.012f,5);
                    for(int node=1;node<=6;node++)
                    {
                        Vector3 p=Vector3.Lerp(basePoint,tip,node/6f);
                        float turn=angle*Mathf.Rad2Deg+node*97f;
                        bush.Leaf(p+side*.035f,.22f,.15f,Quaternion.Euler(15+node*5,turn,branch*11));
                        bush.Leaf(p-side*.035f,.19f,.13f,Quaternion.Euler(-25,turn+140,branch*8));
                    }
                }
            }
            foreach(var legacy in all.Where(r=>InGroup(r) && (r.name.StartsWith("Plant stem",StringComparison.Ordinal) || r.name.StartsWith("Pointed foliage leaf",StringComparison.Ordinal))))legacy.enabled=false;
            var a=PlaceKit(parent,"Attached rooftop shrubs "+parent.name,bush,leaf);
            var bgo=PlaceKit(parent,"Rooftop shrub branches "+parent.name,stems,stem);
            if(parent==root){growth.Add(a);growth.Add(bgo);}
        }
        var buildings=all.Where(r=>r.name=="Neighbouring city block" || r.name.EndsWith("building base",StringComparison.Ordinal)).OrderBy(r=>r.transform.position.z).ThenBy(r=>r.transform.position.x).ToArray();
        for(int i=0;i<buildings.Length;i++)
        {
            var body=buildings[i];
            MetreUv(body,.33f,"Facade body "+i);
            Bounds bounds=body.bounds;
            var reveals=new Kit();var joinery=new Kit();var panes=new Kit();var coping=new Kit();var pipes=new Kit();
            float floorTop=bounds.max.y-.55f;
            for(int side=0;side<4;side++)
            {
                bool alongX=side<2;
                Vector3 normal=side==0?Vector3.forward:side==1?Vector3.back:side==2?Vector3.right:Vector3.left;
                Vector3 tangent=Vector3.Cross(Vector3.up,normal);
                Quaternion rotation=Quaternion.LookRotation(normal);
                float span=alongX?bounds.size.x:bounds.size.z;
                int bays=Mathf.Max(2,Mathf.FloorToInt((span-.8f)/1.9f));
                float spacing=(span-.8f)/bays;
                Vector3 wall=new Vector3(bounds.center.x,0,bounds.center.z)+normal*(alongX?bounds.extents.z:bounds.extents.x);
                for(float y=floorTop-1.25f;y>Mathf.Max(bounds.min.y+1f,-17f);y-=2.85f)
                {
                    for(int bay=0;bay<bays;bay++)
                    {
                        Vector3 p=wall+tangent*((bay-(bays-1)*.5f)*spacing)+Vector3.up*y;
                        reveals.Box(p+normal*.017f,new Vector3(1.16f,1.65f,.035f),.018f,rotation);
                        panes.Box(p+normal*.038f,new Vector3(.95f,1.40f,.022f),.01f,rotation);
                        foreach(int sign in new[]{-1,1})
                        {
                            joinery.Box(p+tangent*(sign*.54f)+normal*.065f,new Vector3(.065f,1.60f,.12f),.009f,rotation);
                            joinery.Box(p+Vector3.up*(sign*.79f)+normal*.065f,new Vector3(1.14f,.07f,.12f),.009f,rotation);
                        }
                        joinery.Box(p+normal*.066f,new Vector3(.04f,1.53f,.10f),.006f,rotation);
                        coping.Box(p-Vector3.up*.86f+normal*.13f,new Vector3(1.32f,.11f,.31f),.025f,rotation);
                        coping.Box(p+Vector3.up*.87f+normal*.025f,new Vector3(1.28f,.10f,.08f),.016f,rotation);
                    }
                }
                // A continuous cornice and floor string course provide real
                // construction scale beneath the playable roof.
                coping.Box(wall+Vector3.up*(bounds.max.y-.17f)+normal*.07f,new Vector3(span+.12f,.22f,.24f),.035f,rotation);
                if(bounds.max.y>-2f)coping.Box(wall+Vector3.up*(bounds.max.y-3.4f)+normal*.045f,new Vector3(span,.10f,.14f),.015f,rotation);
            }
            Vector3 corner=new Vector3(bounds.min.x+.13f,bounds.max.y-.35f,bounds.min.z-.06f);
            Vector3 down=corner+Vector3.down*Mathf.Min(15f,bounds.size.y-.8f);
            pipes.Tube(corner,down,.055f);
            for(float y=corner.y-.4f;y>down.y;y-=2.6f)pipes.Box(new Vector3(corner.x,y,corner.z),new Vector3(.15f,.045f,.13f),.008f);
            PlaceKit(root,"Facade "+i+" recesses",reveals,dark).GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            PlaceKit(root,"Facade "+i+" joinery",joinery,frame).GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            PlaceKit(root,"Facade "+i+" glazing",panes,glass).GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            PlaceKit(root,"Facade "+i+" stonework",coping,stone);
            PlaceKit(root,"Facade "+i+" drainage",pipes,metal);
        }
        var roofs=all.Where(r=>r.GetComponent<BoxCollider>()!=null && !r.GetComponent<BoxCollider>().isTrigger && r.transform.lossyScale.y<=1.1f && r.transform.lossyScale.x>=1.5f && r.transform.lossyScale.z>=1.5f && (r.transform.parent==worlds.presentRoot || r.transform.parent==worlds.alteredRoot || r.transform.parent.name=="Shared rooftop structure")).ToArray();
        for(int i=0;i<roofs.Length;i++)
        {
            var roof=roofs[i];MetreUv(roof,.25f,"Roof surface "+i);
            var collider=roof.GetComponent<BoxCollider>();var bounds=GameplayCompareRunner.PlatformBounds(collider);
            // World-specific architecture belongs to that root so collision
            // surface previews and actual support retain their original rule.
            bool shared=roof.transform.parent.name=="Shared rooftop structure";
            Transform parent=shared?root:roof.transform.parent;
            var fascia=new Kit();var gutter=new Kit();
            foreach(int sign in new[]{-1,1})
            {
                float x=bounds.center.x+sign*(bounds.extents.x-.07f);
                fascia.Box(new Vector3(x,bounds.max.y-.10f,bounds.center.z),new Vector3(.16f,.18f,bounds.size.z),.026f);
                gutter.Box(new Vector3(x+sign*.065f,bounds.max.y-.24f,bounds.center.z),new Vector3(.07f,.06f,bounds.size.z),.014f);
            }
            fascia.Box(new Vector3(bounds.center.x,bounds.max.y-.16f,bounds.min.z+.02f),new Vector3(bounds.size.x,.22f,.13f),.02f);
            fascia.Box(new Vector3(bounds.center.x,bounds.max.y-.16f,bounds.max.z-.02f),new Vector3(bounds.size.x,.22f,.13f),.02f);
            PlaceKit(parent,"Roof "+i+" coping",fascia,stone);
            PlaceKit(parent,"Roof "+i+" gutter",gutter,metal);
            if(!shared)continue;
            // Growth roots in the roof perimeter and drains; none sits in the
            // jump runway. Folded closed leaves remain legible during orbit.
            var leaves=new Kit();var tips=new Kit();var vines=new Kit();
            var random=new System.Random(311+i);
            for(int side=0;side<2;side++)
            for(int strand=0;strand<7;strand++)
            {
                float x=side==0?bounds.min.x+.06f:bounds.max.x-.06f;
                float z=Mathf.Lerp(bounds.min.z+.25f,bounds.max.z-.25f,(strand+.5f)/7f);
                Vector3 anchor=new Vector3(x,bounds.max.y+.018f,z);
                float length=.45f+(float)random.NextDouble()*1.7f;
                Vector3 last=anchor;
                for(int node=0;node<12;node++)
                {
                    float t=node/11f;
                    Vector3 p=anchor+new Vector3((side==0?-1:1)*(.04f+Mathf.Sin(t*4f+strand)*.06f),-t*length,Mathf.Sin(t*5f+strand)*.06f);
                    if(node>0)vines.Tube(last,p,.008f,5);last=p;
                    for(int pair=0;pair<2;pair++)
                    {
                        float spread=(pair==0?-1:1)*(.06f+(float)random.NextDouble()*.04f);
                        Vector3 leafPos=p+new Vector3((side==0?-1:1)*.035f,0,spread);
                        Quaternion rot=Quaternion.Euler(75f+(float)random.NextDouble()*40f,side==0?90:-90,spread*400f);
                        (node>8?tips:leaves).Leaf(leafPos,.13f+(float)random.NextDouble()*.13f,.10f+(float)random.NextDouble()*.10f,rot);
                    }
                }
                for(int leafIndex=0;leafIndex<8;leafIndex++)
                    leaves.Leaf(anchor+new Vector3((side==0?1:-1)*leafIndex*.04f,.018f+leafIndex*.006f,Mathf.Sin(leafIndex*2f)*.1f),.2f,.15f,Quaternion.Euler(leafIndex*3,leafIndex*71,0));
            }
            growth.Add(PlaceKit(root,"Roof "+i+" rooted ivy",leaves,leaf));
            growth.Add(PlaceKit(root,"Roof "+i+" ivy tips",tips,fresh));
            growth.Add(PlaceKit(root,"Roof "+i+" woody vines",vines,stem));
        }
        art.overgrownOnly=preserved.Concat(growth).ToArray();
        art.presentDeck=PhysicalSurface(art.presentDeck);
        art.overgrownDeck=PhysicalSurface(art.overgrownDeck);
        // Inactive detail must not become opaque decoration over a ghost roof.
        worlds.previewCollisionSurfacesOnly=true;
        EditorUtility.SetDirty(worlds);EditorUtility.SetDirty(art);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        SliceDelivery.ValidateScene();
        Debug.Log("UPGRADE ENVIRONMENT APPLIED: buildings="+buildings.Length+" roofs="+roofs.Length+" closed-leaf groups="+growth.Count+"; baseline skyline/light/camera/colliders retained.");
    }

    private static void MetreUv(Renderer renderer,float density,string name)
    {
        var filter=renderer.GetComponent<MeshFilter>();if(filter==null || filter.sharedMesh==null)return;
        string path=Folder+"/Meshes/"+name+".asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh==null){mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);AssetDatabase.CreateAsset(mesh,path);}
        var vertices=mesh.vertices;var normals=mesh.normals;var uv=new Vector2[vertices.Length];
        for(int i=0;i<vertices.Length;i++)
        {
            Vector3 p=Vector3.Scale(vertices[i],renderer.transform.lossyScale);Vector3 n=normals[i];
            uv[i]=(Mathf.Abs(n.y)>.5f?new Vector2(p.x,p.z):Mathf.Abs(n.x)>.5f?new Vector2(p.z,p.y):new Vector2(p.x,p.y))*density;
        }
        mesh.uv=uv;mesh.RecalculateTangents();filter.sharedMesh=mesh;EditorUtility.SetDirty(mesh);
        foreach(var material in renderer.sharedMaterials)
        {
            // Clone the approved surface instead of mutating its historic asset.
            renderer.sharedMaterial=PhysicalSurface(material);
        }
    }

    private static Material PhysicalSurface(Material material)
    {
        if(material==null)return null;
        if(AssetDatabase.GetAssetPath(material).StartsWith(Folder,StringComparison.Ordinal))return material;
        string path=Folder+"/Materials/"+material.name+" physical scale.mat";
        var result=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(result==null){result=new Material(material);AssetDatabase.CreateAsset(result,path);}
        result.SetTextureScale("_BaseMap",Vector2.one);result.SetTextureScale("_BumpMap",Vector2.one);
        result.SetFloat("_Smoothness",.10f);EditorUtility.SetDirty(result);return result;
    }
}
