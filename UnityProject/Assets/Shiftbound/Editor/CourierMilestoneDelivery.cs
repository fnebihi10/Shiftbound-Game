using System;
using System.IO;
using System.Linq;
using System.Globalization;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CourierMilestoneDelivery
{
    const string Folder="Assets/Shiftbound/CourierMilestone/";
    static readonly System.Collections.Generic.Dictionary<string,float> Distances=new();
    public static void Audit()
    {
        string output=Environment.GetEnvironmentVariable("SHIFTBOUND_COURIER_AUDIT");
        Directory.CreateDirectory(output);
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        foreach(string file in new[]{"CourierV2.fbx","CourierMotion.fbx"})
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Folder+file);
            importer.animationType=ModelImporterAnimationType.Human;
            importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation=file=="CourierMotion.fbx";
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            if(file=="CourierMotion.fbx")
            {
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips)
                {
                    clip.loopTime=clip.name.Contains("Idle")||clip.name.Contains("Walk")||clip.name.Contains("Run")||clip.name.Contains("Sprint")||clip.name.Contains("Fall");
                    clip.lockRootRotation=true;clip.keepOriginalOrientation=true;
                    clip.lockRootHeightY=true;clip.keepOriginalPositionY=true;
                    clip.lockRootPositionXZ=true;clip.keepOriginalPositionXZ=true;
                }
                importer.clipAnimations=clips;importer.SaveAndReimport();
            }
        }
        var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"CourierV2.fbx"));
        var animator=go.GetComponent<Animator>();
        if(animator==null)animator=go.AddComponent<Animator>();
        if(!animator.avatar||!animator.avatar.isHuman||!animator.avatar.isValid)throw new Exception("Courier avatar invalid");
        string controllerPath=Folder+"MotionReview.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        var controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(Folder+"CourierMotion.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")))
        {
            var state=controller.layers[0].stateMachine.AddState(clip.name);state.motion=clip;
            Debug.Log("COURIER IMPORT: "+clip.name+" length="+clip.length+" human="+clip.isHumanMotion+" curves="+AnimationUtility.GetCurveBindings(clip).Length);
        }
        EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
        animator.enabled=true;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;animator.Rebind();animator.Update(.00001f);
        foreach(var renderer in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var materials=renderer.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                var source=materials[i];var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.name=source.name;mat.SetColor("_BaseColor",source.color);mat.SetFloat("_Smoothness",.18f);
                materials[i]=mat;
            }
            renderer.sharedMaterials=materials;renderer.updateWhenOffscreen=true;
            Debug.Log("COURIER IMPORT MESH: "+renderer.sharedMesh.vertexCount+" vertices; "+renderer.sharedMesh.triangles.Length/3+" triangles");
        }
        var csv=new System.Text.StringBuilder("state,phase,time,lx,ly,lz,rx,ry,rz,front\n");
        foreach(var entry in controller.layers[0].stateMachine.states)
        {
            var clip=(AnimationClip)entry.state.motion;
            Vector3 previousL=default,previousR=default;float travel=0;int travelSamples=0;
            float stance=clip.name.EndsWith("CourierWalk")?.55f:clip.name.EndsWith("CourierRun")?.32f:clip.name.EndsWith("CourierSprint")?.23f:0;
            for(int i=0;i<=120;i++)
            {
                float phase=i/120f;animator.Play("Base Layer."+entry.state.name,0,phase);animator.Update(.00001f);
                if(i==0)Debug.Log("COURIER STATE: "+entry.state.name+" initialized="+animator.isInitialized+" selected="+animator.GetCurrentAnimatorStateInfo(0).fullPathHash+" requested="+Animator.StringToHash("Base Layer."+entry.state.name));
                var l=go.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
                var r=go.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                if(i>0 && stance>0)
                {
                    float lp=phase,rp=(phase+.5f)%1;
                    if(lp>.05f && lp<stance-.03f){travel+=previousL.z-l.z;travelSamples++;}
                    if(rp>.05f && rp<stance-.03f){travel+=previousR.z-r.z;travelSamples++;}
                }
                previousL=l;previousR=r;
                var across=animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position-animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
                var up=animator.GetBoneTransform(HumanBodyBones.Head).position-animator.GetBoneTransform(HumanBodyBones.Hips).position;
                var front=Vector3.Cross(across,up).normalized;
                csv.AppendFormat(CultureInfo.InvariantCulture,"{0},{1:F6},{2:F6},{3:F6},{4:F6},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6}\n",entry.state.name,phase,phase*clip.length,l.x,l.y,l.z,r.x,r.y,r.z,Vector3.Dot(front,go.transform.forward));
            }
            if(travelSamples>0)
            {
                float distance=travel/travelSamples*120f;
                if(distance<.5f || distance>5f)throw new Exception("Invalid imported stance direction/distance: "+clip.name+" "+distance);
                Distances[clip.name.Substring(clip.name.LastIndexOf('|')+1)]=distance;
                Debug.Log("COURIER IMPORTED CYCLE DISTANCE: "+clip.name+" "+distance+" m");
            }
            foreach(float phase in new[]{0f,.20f,.45f,.70f})
            {
                animator.Play("Base Layer."+entry.state.name,0,phase);animator.Update(.00001f);
                foreach(var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    var baked=new Mesh();skin.BakeMesh(baked);Debug.Log("COURIER DEFORMATION: "+entry.state.name+" phase="+phase+" bounds="+baked.bounds+" first="+baked.vertices[0]+" bone="+skin.bones[0].name);UnityEngine.Object.DestroyImmediate(baked);
                }
                Capture(Camera.main,go,Path.Combine(output,entry.state.name.Replace('|','_')+"-"+phase.ToString("F2",CultureInfo.InvariantCulture)+".png"));
            }
        }
        File.WriteAllText(Path.Combine(output,"imported-gaits.csv"),csv.ToString());
        Debug.Log("COURIER IMPORT AUDIT PASSED: valid Humanoid, actual Animator samples and renders; visual acceptance pending.");
    }
    static void Capture(Camera camera,GameObject go,string file)
    {
        var originals=go.GetComponentsInChildren<SkinnedMeshRenderer>();
        var frozen=new System.Collections.Generic.List<GameObject>();
        foreach(var skin in originals)
        {
            var bake=new Mesh();skin.BakeMesh(bake);
            var proxy=new GameObject("Pose snapshot");proxy.transform.SetParent(skin.transform,false);
            proxy.AddComponent<MeshFilter>().sharedMesh=bake;proxy.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;
            skin.enabled=false;frozen.Add(proxy);
        }
        camera.transform.position=new Vector3(2.5f,1.6f,4.5f);camera.transform.LookAt(new Vector3(0,.9f,0));
        camera.fieldOfView=30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.16f,.20f);
        var target=new RenderTexture(768,768,24);camera.targetTexture=target;camera.Render();
        var previous=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(768,768,TextureFormat.RGB24,false);
        texture.ReadPixels(new Rect(0,0,768,768),0,0);texture.Apply();File.WriteAllBytes(file,texture.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
        foreach(var proxy in frozen){UnityEngine.Object.DestroyImmediate(proxy.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(proxy);}
        foreach(var skin in originals)skin.enabled=true;
    }
    public static void Integrate()
    {
        Audit();
        string skinNormal=Folder+"CourierSkinNormal.png";
        if(!AssetDatabase.LoadAssetAtPath<Texture2D>(skinNormal))AssetDatabase.CopyAsset("Assets/Shiftbound/ThirdParty/Quaternius/Character/T_Superhero_Male_Normal.png",skinNormal);
        var normalImporter=(TextureImporter)AssetImporter.GetAtPath(skinNormal);normalImporter.textureType=TextureImporterType.NormalMap;normalImporter.sRGBTexture=false;normalImporter.maxTextureSize=1024;normalImporter.SaveAndReimport();
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        var old=UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>();
        var motor=old.motor;var oldAnimator=old.GetComponent<Animator>();
        string controllerPath=Folder+"Courier.controller";AssetDatabase.DeleteAsset(controllerPath);
        if(!AssetDatabase.CopyAsset("Assets/Shiftbound/MaterialsV2/CourierMotion.controller",controllerPath))throw new Exception("Controller copy failed");
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        var states=controller.layers[0].stateMachine;
        var clips=AssetDatabase.LoadAllAssetsAtPath(Folder+"CourierMotion.fbx").OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
        AnimationClip Clip(string name)=>clips.Single(c=>c.name.EndsWith("|"+name,StringComparison.Ordinal));
        foreach(string parameter in new[]{"GaitPhase","JumpTime","JumpEnergy"})controller.AddParameter(parameter,AnimatorControllerParameterType.Float);
        var gait=new BlendTree{name="Authored travel matched gait",blendType=BlendTreeType.Simple1D,blendParameter="TravelSpeed",useAutomaticThresholds=false};
        AssetDatabase.AddObjectToAsset(gait,controller);
        gait.AddChild(Clip("CourierWalk"),1.5f);gait.AddChild(Clip("CourierRun"),3.6f);gait.AddChild(Clip("CourierSprint"),7.5f);
        var gaitState=states.AddState("AuthoredGait");gaitState.motion=gait;gaitState.timeParameter="GaitPhase";gaitState.timeParameterActive=true;
        var arc=new BlendTree{name="Animated stationary and moving jump",blendType=BlendTreeType.Simple1D,blendParameter="TakeoffSpeed",useAutomaticThresholds=false};
        AssetDatabase.AddObjectToAsset(arc,controller);arc.AddChild(Clip("CourierJumpStanding"),0);arc.AddChild(Clip("CourierJumpMoving"),5.2f);
        var light=new BlendTree{name="Released short jump",blendType=BlendTreeType.Simple1D,blendParameter="TakeoffSpeed",useAutomaticThresholds=false};
        AssetDatabase.AddObjectToAsset(light,controller);light.AddChild(Clip("CourierJumpLightStanding"),0);light.AddChild(Clip("CourierJumpLightMoving"),5.2f);
        var energy=new BlendTree{name="Jump effort from resolved arc",blendType=BlendTreeType.Simple1D,blendParameter="JumpEnergy",useAutomaticThresholds=false};
        AssetDatabase.AddObjectToAsset(energy,controller);energy.AddChild(light,.35f);energy.AddChild(arc,1f);
        var arcState=states.AddState("AnimatedArc");arcState.motion=energy;arcState.timeParameter="JumpTime";arcState.timeParameterActive=true;
        var rest=states.AddState("Rest");rest.motion=Clip("CourierIdle");states.defaultState=rest;states.AddState("AnimatedFall").motion=Clip("CourierFall");
        states.AddState("SoftRecovery").motion=Clip("CourierLandSoft");states.AddState("HardRecovery").motion=Clip("CourierLandHard");
        var layers=controller.layers;layers[0].iKPass=true;controller.layers=layers;
        var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"CourierV2.fbx"));
        go.name="Courier authoring candidate";go.transform.SetParent(old.transform.parent,false);
        go.transform.localPosition=old.transform.localPosition;go.transform.localRotation=Quaternion.identity;go.transform.localScale=old.transform.localScale;
        Debug.Log("COURIER INTEGRATION TRANSFORM: local="+go.transform.localPosition+" scale="+go.transform.localScale);
        var animator=go.GetComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
        var driver=go.AddComponent<Shiftbound.RiggedCourierAnimator>();driver.motor=motor;driver.authoredActions=true;
        driver.walkCycleDistance=Distances["CourierWalk"];driver.runCycleDistance=Distances["CourierRun"];driver.sprintCycleDistance=Distances["CourierSprint"];
        driver.AuthorSoleSupport();
        foreach(var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var materials=skin.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                string path=Folder+materials[i].name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetColor("_BaseColor",materials[i].color);mat.SetFloat("_Smoothness",.18f);AssetDatabase.CreateAsset(mat,path);}
                if(materials[i].name=="Courier eyes" || materials[i].name=="Courier skin")
                {
                    string texture="Assets/Shiftbound/ThirdParty/Quaternius/Character/"+(materials[i].name=="Courier eyes"?"T_Eye_Brown.png":"T_Superhero_Male_Dark.png");
                    mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texture));mat.SetColor("_BaseColor",Color.white);
                    if(materials[i].name=="Courier skin")
                    {
                        mat.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(skinNormal));mat.SetFloat("_BumpScale",.4f);mat.EnableKeyword("_NORMALMAP");
                    }
                    EditorUtility.SetDirty(mat);
                }
                materials[i]=mat;
            }
            skin.sharedMaterials=materials;
        }
        UnityEngine.Object.DestroyImmediate(old.gameObject);
        EditorUtility.SetDirty(controller);EditorUtility.SetDirty(motor);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("COURIER CANDIDATE INTEGRATED: isolated project only; original jog retained as default; authored gait opt-in.");
    }
    public static void PrepareBaseline()
    {
        EditorSceneManager.OpenScene(Folder+"BaselineReference.unity");
        var original=UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>();
        var root=original.gameObject;UnityEngine.Object.DestroyImmediate(original);
        root.AddComponent<Shiftbound.BaselineCourierMotion>();
        PrefabUtility.SaveAsPrefabAsset(root,"Assets/Shiftbound/Resources/CourierBaseline.prefab");
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        Debug.Log("COURIER BASELINE PREFAB SAVED: c69ca256 visual, controller and motion; evidence-only audio suppressed.");
    }
    public static void BuildQA(){Integrate();PrepareBaseline();SliceDelivery.BuildWindows();}
    public static void BuildContactRevision()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>().AuthorSoleSupport();
        EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();SliceDelivery.BuildWindows();
    }
}
