using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static class CourierHumanDelivery
{
    const string Folder="Assets/Shiftbound/CourierHuman/";
    static GameObject reviewModel;
    static RiggedCourierAnimator reviewOld;
    public static void Run()
    {
        string output=Environment.GetEnvironmentVariable("SHIFTBOUND_COURIER_AUDIT");Directory.CreateDirectory(output);
        var importer=(ModelImporter)AssetImporter.GetAtPath(Folder+"CourierHuman.fbx");
        importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation=false;importer.SaveAndReimport();
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        var old=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"CourierHuman.fbx"));
        model.name="Courier human candidate";
        reviewModel=model;reviewOld=old;
        var animator=model.GetComponent<Animator>();
        if(!animator.avatar || !animator.avatar.isValid || !animator.avatar.isHuman)throw new Exception("Alternative courier needs a valid Humanoid avatar.");
        model.transform.SetParent(old.transform.parent,false);model.transform.localRotation=Quaternion.identity;
        animator.runtimeAnimatorController=old.GetComponent<Animator>().runtimeAnimatorController;
        animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var materials=skin.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                string name=materials[i].name.ToLowerInvariant();var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name="Inspection "+name;
                string texture=name.Contains("courier pack")?null:name.Contains("short02")?"short02_diffuse.png":name.Contains("eyebrow")?"eyebrow002.png":name.Contains("male_casual")?"male_casualsuit05_diffuse.png":name.Contains("shoes")?"shoes05_diffuse.png":name.Contains("eye")||name.Contains("low-poly")?"brown_eye.png":"young_darkskinned_male_diffuse.png";
                if(texture!=null)material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"CourierHuman.fbm/"+texture));
                material.SetColor("_BaseColor",texture==null?materials[i].color:Color.white);material.SetFloat("_Smoothness",.15f);
                if(name.Contains("golden canvas"))material.SetColor("_BaseColor",new Color(.75f,.34f,.055f));
                if(name.Contains("golden canvas"))
                {
                    material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"CourierHuman.fbm/CourierCanvas.png"));
                    string path=Folder+"CourierHuman.fbm/CourierCanvasNormal.png";
                    var normal=(TextureImporter)AssetImporter.GetAtPath(path);normal.textureType=TextureImporterType.NormalMap;normal.sRGBTexture=false;normal.SaveAndReimport();
                    material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));material.SetFloat("_BumpScale",.45f);material.EnableKeyword("_NORMALMAP");
                }
                if(name.Contains("navy webbing"))material.SetColor("_BaseColor",new Color(.025f,.055f,.075f));
                if(name.Contains("reflective marker")){material.SetColor("_BaseColor",new Color(.7f,.76f,.65f));material.SetFloat("_Smoothness",.4f);}
                if(name.Contains("short02") || name.Contains("eyebrow"))
                {material.SetFloat("_AlphaClip",1);material.EnableKeyword("_ALPHATEST_ON");material.SetFloat("_Cutoff",.45f);material.SetFloat("_Cull",0);material.SetOverrideTag("RenderType","TransparentCutout");material.renderQueue=2450;}
                if(name.Contains("male_casual"))
                {
                    string path=Folder+"CourierHuman.fbm/male_casualsuit05_normal.png";
                    var normal=(TextureImporter)AssetImporter.GetAtPath(path);normal.textureType=TextureImporterType.NormalMap;normal.sRGBTexture=false;normal.maxTextureSize=1024;normal.SaveAndReimport();
                    material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));material.SetFloat("_BumpScale",.55f);material.EnableKeyword("_NORMALMAP");
                }
                string materialPath=Folder+name+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(existing){EditorUtility.CopySerialized(material,existing);UnityEngine.Object.DestroyImmediate(material);material=existing;EditorUtility.SetDirty(material);}else AssetDatabase.CreateAsset(material,materialPath);
                materials[i]=material;
            }
            skin.sharedMaterials=materials;skin.updateWhenOffscreen=true;
            Debug.Log("HUMAN COURIER MESH: "+skin.name+" vertices="+skin.sharedMesh.vertexCount+" triangles="+skin.sharedMesh.triangles.Length/3);
        }
        animator.Rebind();animator.Play("Base Layer.Rest",0,0);animator.Update(.00001f);animator.Update(.00001f);
        Bounds bounds=WorldBounds(model);model.transform.localScale=Vector3.one*(1.77f/bounds.size.y);
        animator.Update(.00001f);bounds=WorldBounds(model);
        var controller=old.motor.GetComponent<CharacterController>();float floor=old.motor.transform.position.y+controller.center.y-controller.height*.5f-controller.skinWidth;
        model.transform.position+=Vector3.up*(floor-bounds.min.y);
        foreach(var renderer in old.GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var across=animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position-animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
        var up=animator.GetBoneTransform(HumanBodyBones.Head).position-animator.GetBoneTransform(HumanBodyBones.Hips).position;
        float front=Vector3.Dot(Vector3.Cross(across,up).normalized,model.transform.forward);
        if(front<.7f)throw new Exception("Alternative avatar anatomical facing invalid: "+front);
        var inspection=UnityEngine.Object.Instantiate(Camera.main.gameObject);inspection.name="Temporary courier inspection";
        var camera=inspection.GetComponent<Camera>();camera.enabled=false;
        foreach(string state in new[]{"Rest","Locomotion","AuthoredGait","AnimatedArc","AnimatedFall","SoftRecovery","HardRecovery"})
        foreach(float phase in new[]{0f,.25f,.5f,.75f})
        {
            animator.SetFloat("TravelSpeed",3.6f);animator.SetFloat("GaitPhase",phase);animator.SetFloat("JumpTime",phase);animator.SetFloat("JumpEnergy",1);animator.SetFloat("TakeoffSpeed",5.2f);
            animator.Play("Base Layer."+state,0,phase);animator.Update(.00001f);animator.Update(.00001f);
            foreach(bool rear in new[]{false,true})
            {
                var focus=model.transform.position+Vector3.up*.9f;camera.transform.position=focus+new Vector3(rear?-2:2,.5f,rear?-4:4);camera.transform.LookAt(focus);camera.fieldOfView=32;
                Capture(camera,model,Path.Combine(output,state+"-"+phase.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+(rear?"-rear.png":"-front.png")));
            }
        }
        UnityEngine.Object.DestroyImmediate(inspection);AssetDatabase.SaveAssets();
        // Do not save this inspection into GoldenRooftops.
        Debug.Log("HUMAN COURIER REVIEW PASSED: Humanoid, forward anatomy, existing Jog and authored action retargeting captured. Artistic acceptance pending.");
    }
    public static void IntegrateAndBuildWindows()
    {
        Run();
        var old=reviewOld;
        var model=reviewModel;var animator=model.GetComponent<Animator>();
        var driver=model.AddComponent<RiggedCourierAnimator>();driver.motor=old.motor;driver.authoredActions=true;
        float Distance(float speed,float stance)
        {
            Vector3 previousL=default,previousR=default;float travel=0;int samples=0;
            for(int i=0;i<=120;i++)
            {
                float phase=i/120f;animator.SetFloat("TravelSpeed",speed);animator.SetFloat("GaitPhase",phase);
                animator.Play("Base Layer.AuthoredGait",0,phase);animator.Update(.00001f);animator.Update(.00001f);
                var l=model.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
                var r=model.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                float lp=phase,rp=(phase+.5f)%1;
                if(i>0 && lp>.05f && lp<stance-.03f){travel+=previousL.z-l.z;samples++;}
                if(i>0 && rp>.05f && rp<stance-.03f){travel+=previousR.z-r.z;samples++;}
                previousL=l;previousR=r;
            }
            float distance=travel/samples*120;
            if(distance<.5f || distance>5)throw new Exception("Alternative avatar stride invalid: "+distance);
            Debug.Log("HUMAN IMPORTED CYCLE DISTANCE: speed="+speed+" distance="+distance);return distance;
        }
        driver.walkCycleDistance=Distance(1.5f,.55f);driver.runCycleDistance=Distance(3.6f,.32f);driver.sprintCycleDistance=Distance(7.5f,.23f);
        driver.AuthorSoleSupport();
        if(old.motor.visual==old.transform)old.motor.visual=model.transform;
        UnityEngine.Object.DestroyImmediate(old.gameObject);
        EditorUtility.SetDirty(driver);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("HUMAN COURIER QA INTEGRATED: default original Jog, new actions and foot/toe support; gameplay acceptance pending.");
        SliceDelivery.BuildWindows();
    }
    static Bounds WorldBounds(GameObject model)
    {
        Bounds bounds=default;bool initialized=false;
        foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {var mesh=new Mesh();skin.BakeMesh(mesh);foreach(var vertex in mesh.vertices){var point=skin.transform.TransformPoint(vertex);if(!initialized){bounds=new Bounds(point,Vector3.zero);initialized=true;}else bounds.Encapsulate(point);}UnityEngine.Object.DestroyImmediate(mesh);}
        return bounds;
    }
    static void Capture(Camera camera,GameObject model,string file)
    {
        var skins=model.GetComponentsInChildren<SkinnedMeshRenderer>();var frozen=new System.Collections.Generic.List<GameObject>();
        foreach(var skin in skins){var bake=new Mesh();skin.BakeMesh(bake);var proxy=new GameObject("Inspection pose");proxy.transform.SetParent(skin.transform,false);proxy.AddComponent<MeshFilter>().sharedMesh=bake;proxy.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;frozen.Add(proxy);}
        var target=new RenderTexture(768,768,24);camera.targetTexture=target;camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
        var texture=new Texture2D(768,768,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,768,768),0,0);texture.Apply();File.WriteAllBytes(file,texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
        foreach(var proxy in frozen){UnityEngine.Object.DestroyImmediate(proxy.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(proxy);}foreach(var skin in skins)skin.enabled=true;
    }
}
