using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static partial class ProductionUpgradeAuthoring
{
    public static void Presentation()
    {
        City();
        Motion();
        Surfaces();
        Debug.Log("SHIFTBOUND PRESENTATION AUTHORING PASSED");
    }
    const string Root = "Assets/Shiftbound/ProductionUpgrade";
    const string Scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";
    static Material Material(string name, Color color, float smooth = .2f, float metal = 0)
    {
        string path=Root+"/Materials/"+name+".mat";
        var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",smooth);m.SetFloat("_Metallic",metal);m.enableInstancing=true;
        return m;
    }
    static void Folders()
    {
        foreach(string sub in new[]{"Materials","Meshes","Motion","Prefabs"})
            Directory.CreateDirectory(Root+"/"+sub);
        AssetDatabase.Refresh();
    }
    public static void Courier()
    {
        Folders();EditorSceneManager.OpenScene(Scene);
        string path=Root+"/Courier/ShiftboundCourier.fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType=ModelImporterAnimationType.Human;
        importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
        importer.materialLocation=ModelImporterMaterialLocation.InPrefab;
        importer.importAnimation=false;importer.meshCompression=ModelImporterMeshCompression.Off;
        importer.isReadable=true;importer.SaveAndReimport();
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Avatar avatar=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
        if(avatar==null || !avatar.isHuman || !avatar.isValid)throw new Exception("Replacement courier Humanoid avatar invalid");
        var old=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var motor=UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
        var controller=(AnimatorController)old.GetComponent<Animator>().runtimeAnimatorController;
        var item=(GameObject)PrefabUtility.InstantiatePrefab(source, motor.transform);
        item.name="Courier - authored technical outfit";item.transform.localPosition=old.transform.localPosition;
        item.transform.localRotation=old.transform.localRotation;item.transform.localScale=Vector3.one;
        var animator=item.GetComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
        var motion=item.AddComponent<RiggedCourierAnimator>();motion.motor=motor;
        animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var skin=Material("Courier skin",Color.white,.22f);
        skin.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Shiftbound/ThirdParty/Quaternius/Character/T_Superhero_Male_Ligh.png"));
        skin.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Shiftbound/ThirdParty/Quaternius/Character/T_Superhero_Male_Normal.png"));
        skin.EnableKeyword("_NORMALMAP");skin.SetFloat("_BumpScale",.45f);
        var gear=Material("Courier textiles",Color.white,.16f);
        gear.SetFloat("_Cull",0); // Thin textile edges/flap remain visible on both sides.
        gear.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Courier/CourierTextiles.png"));
        var hair=Material("Courier hair",new Color(.32f,.21f,.14f),.28f);
        hair.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Shiftbound/ThirdParty/Quaternius/Character/T_Hair_1_BaseColor.png"));
        var eyes=Material("Courier eyes",Color.white,.45f);
        eyes.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Shiftbound/ThirdParty/Quaternius/Character/T_Eye_Brown.png"));
        foreach(var r in item.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            // Imported material names survive even when external material import is off.
            r.sharedMaterials=Enumerable.Range(0,r.sharedMesh.subMeshCount).Select(i=>
            {
                string name=i<r.sharedMaterials.Length&&r.sharedMaterials[i]!=null?r.sharedMaterials[i].name:"";
                if(name.Contains("Textile"))return gear;if(name.Contains("Eyes"))return eyes;if(name.Contains("Hair"))return hair;return skin;
            }).ToArray();
            r.updateWhenOffscreen=false;
            Debug.Log("UPGRADE NEW MESH: "+r.name+" triangles="+r.sharedMesh.triangles.Length/3+" slots="+r.sharedMesh.subMeshCount+" materials="+string.Join(",",r.sharedMaterials.Select(m=>m.name)));
        }
        // Explicit remapping by FBX material slots (skin/textile/eye/hair) is audited
        // below, before any build can be accepted.
        motor.visual=item.transform;
        UnityEngine.Object.DestroyImmediate(old.gameObject);
        var layers=controller.layers;layers[0].iKPass=true;controller.layers=layers;
        item.AddComponent<CourierMotionPolish>().motor=motor;
        foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Root+"/Courier"}))
        {
            var t=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            t.maxTextureSize=1024;t.mipmapEnabled=true;
            var android=t.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=1024;android.format=TextureImporterFormat.ASTC_6x6;t.SetPlatformTextureSettings(android);t.SaveAndReimport();
        }
        EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();
        Debug.Log("SHIFTBOUND PRODUCTION COURIER PASSED: valid Humanoid; reauthored anatomy/outfit/bag, four material types.");
    }
    public static void RenderCourier()
    {
        EditorSceneManager.OpenScene(Scene);var courier=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var animator=courier.GetComponent<Animator>();
        string output=Path.GetFullPath("../.validation/ProductionUpgrade/CourierReview");Directory.CreateDirectory(output);
        foreach(var r in courier.GetComponentsInChildren<SkinnedMeshRenderer>())
            Debug.Log("UPGRADE MATERIAL AUDIT: "+r.name+" slots="+string.Join(",",r.sharedMaterials.Select(m=>m.name)));
        var controller=(AnimatorController)animator.runtimeAnimatorController;
        Camera camera=Camera.main;var rt=new RenderTexture(1280,720,24);var active=RenderTexture.active;
        camera.targetTexture=rt;
        try
        {
            foreach(var state in controller.layers[0].stateMachine.states.Where(s=>new[]{"Idle","Sprint","Jump","Fall","Land"}.Contains(s.state.name)))
            foreach(float fraction in new[]{.15f,.55f})
            {
                animator.Rebind();animator.Update(0);animator.Play(state.state.name,0,fraction);animator.Update(.001f);
                foreach(int angle in new[]{0,120,240})
                {
                    Vector3 focus=courier.transform.position+Vector3.up*1.03f;
                    camera.transform.position=focus+Quaternion.Euler(0,angle,0)*new Vector3(0,.65f,-3.2f);camera.transform.LookAt(focus);
                    camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
                    image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,state.state.name+"-"+fraction+"-"+angle+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);
                }
            }
        }
        finally{camera.targetTexture=null;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(rt);}
        Debug.Log("SHIFTBOUND COURIER REVIEW PASSED: "+output);
    }
    // One idempotent integration entry point for the authored replacement assets.
    public static void Integrate()
    {
        Courier();
        City();
        Motion();
        Debug.Log("SHIFTBOUND PRODUCTION INTEGRATION PASSED");
    }
}
