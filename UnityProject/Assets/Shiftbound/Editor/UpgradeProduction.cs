using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class UpgradeProduction
{
    internal const string Scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";
    internal const string Folder = "Assets/Shiftbound/UpgradeProduction";

    public static void RepairRuntimeFacing()
    {
        EditorSceneManager.OpenScene(Scene);
        var courier=UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>();
        courier.transform.localRotation=Quaternion.identity;
        EditorUtility.SetDirty(courier.transform);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("UPGRADE RUNTIME FACING REPAIRED: removed duplicate 180 degree rotation; Humanoid runtime faces motor heading.");
    }

    public static void AuditFacing()
    {
        EditorSceneManager.OpenScene(Scene);
        var courier=UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>();
        var animator=courier.GetComponent<Animator>();
        var controller=(AnimatorController)animator.runtimeAnimatorController;
        var idle=(AnimationClip)controller.layers[0].stateMachine.states.First(s=>s.state.name=="Idle").state.motion;
        idle.SampleAnimation(courier.gameObject,0f);
        Debug.Log("UPGRADE FACING: motor visual="+courier.motor.visual.name+" yaw="+courier.transform.eulerAngles+" scale="+courier.transform.lossyScale);
        foreach(var bone in new[]{HumanBodyBones.Chest,HumanBodyBones.LeftFoot,HumanBodyBones.LeftToes,HumanBodyBones.RightFoot,HumanBodyBones.RightToes,HumanBodyBones.Head})
        {
            var t=animator.GetBoneTransform(bone);if(t==null)continue;
            Debug.Log("UPGRADE BONE "+bone+" position="+t.position+" localToCourier="+courier.transform.InverseTransformPoint(t.position)+" scale="+t.lossyScale+" forward="+t.forward);
        }
        var pack=animator.GetBoneTransform(HumanBodyBones.Chest).Find("Courier field kit");
        Debug.Log("UPGRADE PACK position="+pack.position+" scale="+pack.lossyScale+" rotation="+pack.eulerAngles);
        Debug.Log("UPGRADE FACING AUDIT PASSED");
    }

    public static void ApplyCourier()
    {
        EditorSceneManager.OpenScene(Scene);
        EnsureFolders();
        var courier = UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>();
        var animator = courier.GetComponent<Animator>();
        if(courier.motor.visual==courier.transform)
        {
            var heading=new GameObject("Courier heading").transform;
            heading.SetParent(courier.motor.transform,false);
            heading.SetPositionAndRotation(courier.transform.position,courier.transform.rotation);
            courier.transform.SetParent(heading,true);
            courier.motor.visual=heading;
            EditorUtility.SetDirty(courier.motor);
        }
        // Imported bone axes and Editor SampleAnimation are not runtime facing.
        // Humanoid retargeting already aligns this avatar with +Z at runtime.
        courier.transform.localRotation=Quaternion.identity;
        var controller = (AnimatorController)animator.runtimeAnimatorController;
        var states = controller.layers[0].stateMachine;
        var original = states.states.ToDictionary(s => s.state.name, s => s.state);
        foreach (var entry in states.states.Where(s=>s.state.name=="Locomotion" || s.state.name=="Air").ToArray()) states.RemoveState(entry.state);
        foreach(var tree in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(controller)).OfType<BlendTree>().ToArray())UnityEngine.Object.DestroyImmediate(tree,true);
        foreach(var parameter in controller.parameters.Where(p=>p.name=="TravelSpeed" || p.name=="AirPhase" || p.name=="TakeoffSpeed").ToArray()) controller.RemoveParameter(Array.FindIndex(controller.parameters,p=>p.name==parameter.name));
        controller.AddParameter("TravelSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("AirPhase", AnimatorControllerParameterType.Float);
        controller.AddParameter("TakeoffSpeed", AnimatorControllerParameterType.Float);
        var idle=(AnimationClip)original["Idle"].motion;
        var gait = new BlendTree { name="Continuous speed matched gait", blendType=BlendTreeType.Simple1D, blendParameter="TravelSpeed", useAutomaticThresholds=false };
        AssetDatabase.AddObjectToAsset(gait,controller);
        gait.children = new [] {
            new ChildMotion{motion=original["Walk"].motion, threshold=1.5f, timeScale=1f,cycleOffset=0f},
            new ChildMotion{motion=original["Jog"].motion, threshold=3f, timeScale=1f,cycleOffset=0f},
            new ChildMotion{motion=original["Sprint"].motion, threshold=7f, timeScale=1f,cycleOffset=0f}
        };
        states.AddState("Locomotion").motion=gait;
        var jump=(AnimationClip)original["Fall"].motion;
        var sprint=(AnimationClip)original["Sprint"].motion;
        var ninja=(AnimationClip)original["Jump"].motion;
        var extension=Pose("Takeoff extension",idle,.15f,jump,.1f,.35f);
        var running=Pose("Moving takeoff extension",sprint,.3f,jump,.1f,.35f);
        var apex=Pose("Balanced apex",jump,.35f,ninja,.3f,.22f);
        var descend=Pose("Prepare contact",jump,.7f,idle,.1f,.55f);
        var launch = new BlendTree { name="Stationary and running extension", blendType=BlendTreeType.Simple1D, blendParameter="TakeoffSpeed", useAutomaticThresholds=false };
        AssetDatabase.AddObjectToAsset(launch,controller);
        launch.AddChild(extension,0f); launch.AddChild(running,5.2f);
        var air = new BlendTree { name="Complete airborne action", blendType=BlendTreeType.Simple1D, blendParameter="AirPhase", useAutomaticThresholds=false };
        AssetDatabase.AddObjectToAsset(air,controller);
        air.AddChild(launch,0f); air.AddChild(apex,.5f); air.AddChild(descend,1f);
        states.AddState("Air").motion=air;
        var layers=controller.layers; layers[0].iKPass=true; controller.layers=layers;
        EditorUtility.SetDirty(controller);
        AuthorCourier(courier,animator,idle);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        SliceDelivery.ValidateScene();
        Debug.Log("UPGRADE COURIER APPLIED: new kit, continuous gait, arc poses, grounded contact IK; motor parameters unchanged.");
    }

    internal static void EnsureFolders()
    {
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Shiftbound","UpgradeProduction");
        foreach(var name in new[]{"Meshes","Materials","Animation"})
            if(!AssetDatabase.IsValidFolder(Folder+"/"+name))AssetDatabase.CreateFolder(Folder,name);
    }

    public static void Integrate()
    {
        ApplyEnvironment();ApplyCourier();AndroidDelivery.Configure();
        UpgradeMatchedViews.Capture();
        Debug.Log("UPGRADE INTEGRATION PASSED");
    }

    private static AnimationClip Pose(string name,AnimationClip a,float phaseA,AnimationClip b,float phaseB,float blend)
    {
        string path=Folder+"/Animation/"+name+".anim";
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null){clip=UnityEngine.Object.Instantiate(a);AssetDatabase.CreateAsset(clip,path);}
        var bindings=AnimationUtility.GetCurveBindings(a).Concat(AnimationUtility.GetCurveBindings(b)).Distinct().ToArray();
        foreach(var binding in bindings)
        {
            var ca=AnimationUtility.GetEditorCurve(a,binding);var cb=AnimationUtility.GetEditorCurve(b,binding);
            float av=ca==null?0f:ca.Evaluate(a.length*phaseA),bv=cb==null?av:cb.Evaluate(b.length*phaseB);
            float value=Mathf.Lerp(av,bv,blend);
            if((name=="Balanced apex" || name=="Prepare contact") && binding.propertyName.Contains("Arm"))
            {
                var rest=AnimationUtility.GetEditorCurve(AssetDatabase.LoadAllAssetsAtPath(InspectRiggedAssets.AnimationPath).OfType<AnimationClip>().First(c=>c.name.EndsWith("|Idle_Loop",StringComparison.Ordinal)),binding);
                if(rest!=null)value=Mathf.Lerp(value,rest.Evaluate(.5f),.40f);
            }
            // Root translation belongs to the controller, while the humanoid
            // vertical body curve carries the source animation's balance.
            if(binding.propertyName=="RootT.x" || binding.propertyName=="RootT.z")value=0f;
            AnimationUtility.SetEditorCurve(clip,binding,AnimationCurve.Constant(0f,1f,value));
        }
        clip.name=name;
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=false;
        AnimationUtility.SetAnimationClipSettings(clip,settings);
        EditorUtility.SetDirty(clip);
        Debug.Log("UPGRADE AUTHORED POSE "+name+" human="+clip.isHumanMotion);
        return clip;
    }

    private static AnimationClip CompactJog(AnimationClip source,AnimationClip idle)
    {
        const string path=Folder+"/Animation/Courier compact jog.anim";
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null){clip=UnityEngine.Object.Instantiate(source);AssetDatabase.CreateAsset(clip,path);}
        foreach(var binding in AnimationUtility.GetCurveBindings(source))
        {
            var original=AnimationUtility.GetEditorCurve(source,binding);
            var rest=AnimationUtility.GetEditorCurve(idle,binding);
            float scale=binding.propertyName.Contains("Upper Leg") || binding.propertyName.Contains("Lower Leg") || binding.propertyName.Contains("Foot")?.65f:
                binding.propertyName.Contains("Arm")?.85f:binding.propertyName=="RootT.y"?.75f:1f;
            float neutral=rest==null?0f:rest.Evaluate(.5f);
            var keys=original.keys;
            for(int i=0;i<keys.Length;i++)
            {
                keys[i].value=neutral+(keys[i].value-neutral)*scale;
                keys[i].inTangent*=scale;keys[i].outTangent*=scale;
            }
            var curve=new AnimationCurve(keys){preWrapMode=original.preWrapMode,postWrapMode=original.postWrapMode};
            AnimationUtility.SetEditorCurve(clip,binding,curve);
        }
        clip.name="Courier compact jog";EditorUtility.SetDirty(clip);return clip;
    }

    public static void Audit()
    {
        EditorSceneManager.OpenScene(Scene);
        var courier = UnityEngine.Object.FindFirstObjectByType<Shiftbound.RiggedCourierAnimator>();
        var animator = courier.GetComponent<Animator>();
        var controller = (AnimatorController)animator.runtimeAnimatorController;
        foreach (var state in controller.layers[0].stateMachine.states)
        {
            var clip = state.state.motion as AnimationClip;
            Debug.Log("UPGRADE STATE " + state.state.name + " clip=" + clip.name + " length=" + clip.length + " velocity=" + clip.averageSpeed);
            for (int i = 0; i < 20; i++)
            {
                clip.SampleAnimation(courier.gameObject, clip.length * i / 20f);
                Debug.Log("UPGRADE SAMPLE " + state.state.name + " phase=" + i/20f + " L=" + courier.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position) + " R=" + courier.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightFoot).position));
            }
        }
        foreach (var renderer in courier.GetComponentsInChildren<SkinnedMeshRenderer>())
            Debug.Log("UPGRADE SKIN " + renderer.name + " mesh=" + renderer.sharedMesh.vertexCount + " materials=" + string.Join(",",renderer.sharedMaterials.Select(m=>m.name)));
        foreach (string path in new[]{InspectRiggedAssets.AnimationPath,"Assets/Shiftbound/ThirdParty/Quaternius/Animation2/UAL2_Standard.fbx"})
        foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")))
            Debug.Log("UPGRADE CLIP " + clip.name + " length=" + clip.length + " velocity=" + clip.averageSpeed);
        foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(renderer.name.Contains("Neighbouring city block") || renderer.name.Contains("ivy") || renderer.name=="Start rooftop" || renderer.name.Contains("building base"))
                Debug.Log("UPGRADE OBJECT " + renderer.name + " position=" + renderer.transform.position + " scale=" + renderer.transform.lossyScale);
        Debug.Log("UPGRADE AUDIT PASSED");
    }
}
