using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static partial class ProductionUpgradeAuthoring
{
    // Offline pose authoring on the delivered skeleton. No per-frame muscle
    // arrays, animation sampling, allocation or skeleton solver on the phone.
    public static void Motion()
    {
        Folders(); EditorSceneManager.OpenScene(Scene);
        var courier=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var controller=(AnimatorController)courier.GetComponent<Animator>().runtimeAnimatorController;
        var idle=(AnimationClip)controller.layers[0].stateMachine.states.Single(s=>s.state.name=="Idle").state.motion;
        float RootValue(string property)=>AnimationUtility.GetEditorCurve(idle,
            EditorCurveBinding.FloatCurve("",typeof(Animator),property))?.Evaluate(0)
            ?? throw new Exception("Missing idle reference: "+property);
        float standingHeight=RootValue("RootT.y");
        var copy=UnityEngine.Object.Instantiate(courier.gameObject);
        copy.hideFlags=HideFlags.HideAndDontSave;
        copy.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        copy.transform.localScale=Vector3.one;
        var rig=copy.GetComponent<Animator>();
        using(var handler=new HumanPoseHandler(rig.avatar,copy.transform))
        try
        {
            idle.SampleAnimation(copy,0);
            var referencePose=new HumanPose();handler.GetHumanPose(ref referencePose);
            Quaternion poseBasis=referencePose.bodyRotation;
            Debug.Log("COURIER POSE BASIS: "+poseBasis.eulerAngles);
            float[] Pose(float lead,float trail,float knee,float arm,float spread,float lean,bool neutral=false)
            {
                idle.SampleAnimation(copy,0);
                Transform Bone(HumanBodyBones bone)=>rig.GetBoneTransform(bone)
                    ?? throw new Exception("Courier bone missing: "+bone);
                Quaternion leftFoot=Bone(HumanBodyBones.LeftFoot).rotation;
                Quaternion rightFoot=Bone(HumanBodyBones.RightFoot).rotation;
                Quaternion head=Bone(HumanBodyBones.Head).rotation;
                if(!neutral)
                {
                    var chest=Bone(HumanBodyBones.Chest);
                    chest.rotation=Quaternion.AngleAxis(lean,poseBasis*Vector3.right)*chest.rotation;
                    void Aim(HumanBodyBones from,HumanBodyBones to,Vector3 direction)
                    {
                        var a=Bone(from);var b=Bone(to);
                        a.rotation=Quaternion.FromToRotation(b.position-a.position,poseBasis*direction.normalized)*a.rotation;
                    }
                    Aim(HumanBodyBones.LeftUpperArm,HumanBodyBones.LeftLowerArm,new Vector3(-spread,-.90f,-arm));
                    Aim(HumanBodyBones.RightUpperArm,HumanBodyBones.RightLowerArm,new Vector3(spread,-.90f,arm));
                    Aim(HumanBodyBones.LeftLowerArm,HumanBodyBones.LeftHand,new Vector3(-.08f,-.35f,.70f));
                    Aim(HumanBodyBones.RightLowerArm,HumanBodyBones.RightHand,new Vector3(.08f,-.35f,.70f));
                    Aim(HumanBodyBones.LeftUpperLeg,HumanBodyBones.LeftLowerLeg,new Vector3(-.035f,-1f,lead));
                    Aim(HumanBodyBones.RightUpperLeg,HumanBodyBones.RightLowerLeg,new Vector3(.035f,-1f,trail));
                    Aim(HumanBodyBones.LeftLowerLeg,HumanBodyBones.LeftFoot,new Vector3(0,-1f,-knee));
                    Aim(HumanBodyBones.RightLowerLeg,HumanBodyBones.RightFoot,new Vector3(0,-1f,-knee*.7f));
                    Bone(HumanBodyBones.LeftFoot).rotation=leftFoot;
                    Bone(HumanBodyBones.RightFoot).rotation=rightFoot;
                    Bone(HumanBodyBones.Head).rotation=head;
                }
                var pose=new HumanPose();handler.GetHumanPose(ref pose);
                if(pose.muscles==null || pose.muscles.Any(v=>float.IsNaN(v)||float.IsInfinity(v)))
                    throw new Exception("Invalid authored humanoid pose");
                return pose.muscles;
            }
            float[] rest=Pose(0,0,0,0,0,0,true);
            float[] launch=Pose(.40f,-.20f,.55f,.46f,.09f,8);
            float[] apex=Pose(.23f,-.10f,.28f,.28f,.12f,4);
            float[] reach=Pose(.13f,.07f,.10f,.10f,.25f,2);
            float[] impact=Pose(.34f,.28f,.50f,.10f,.15f,10);
            foreach(string name in new[]{"Takeoff","Jump","Fall","Land"})
            {
                string path=Root+"/Motion/Courier-"+name+".anim";
                var asset=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if(asset==null)throw new Exception("Existing humanoid clip missing: "+path);
                var clip=UnityEngine.Object.Instantiate(asset);clip.name="Courier "+name;
                foreach(var binding in AnimationUtility.GetCurveBindings(clip))
                    AnimationUtility.SetEditorCurve(clip,binding,null);
                float duration=name=="Takeoff"?.075f:name=="Jump"?.25f:name=="Fall"?.24f:.16f;
                float[] first=name=="Takeoff"?rest:name=="Jump"?launch:name=="Fall"?apex:reach;
                float[] middle=name=="Takeoff"?launch:name=="Jump"?apex:name=="Fall"?reach:impact;
                float[] last=name=="Takeoff"?launch:name=="Jump"?apex:name=="Fall"?reach:rest;
                for(int muscle=0;muscle<HumanTrait.MuscleCount;muscle++)
                    clip.SetCurve("",typeof(Animator),HumanTrait.MuscleName[muscle],new AnimationCurve(
                        new Keyframe(0,first[muscle]),new Keyframe(duration*.5f,middle[muscle]),
                        new Keyframe(duration,last[muscle]),new Keyframe(1,last[muscle])));
                foreach(string axis in new[]{"x","z"})
                    clip.SetCurve("",typeof(Animator),"RootT."+axis,AnimationCurve.Constant(0,1,0));
                clip.SetCurve("",typeof(Animator),"RootT.y",name=="Land"?
                    new AnimationCurve(new Keyframe(0,standingHeight),new Keyframe(.055f,standingHeight-.03f),
                        new Keyframe(.16f,standingHeight),new Keyframe(1,standingHeight)):
                    AnimationCurve.Constant(0,1,standingHeight));
                foreach(string axis in new[]{"x","y","z","w"})
                    clip.SetCurve("",typeof(Animator),"RootQ."+axis,AnimationCurve.Constant(0,1,RootValue("RootQ."+axis)));
                var settings=AnimationUtility.GetAnimationClipSettings(clip);
                settings.startTime=0;settings.stopTime=1;settings.loopTime=false;
                AnimationUtility.SetAnimationClipSettings(clip,settings);
                EditorUtility.CopySerialized(clip,asset);UnityEngine.Object.DestroyImmediate(clip);
                EditorUtility.SetDirty(asset);
                Debug.Log("UPGRADE SKELETON AUTHORED MOTION: "+name+" Humanoid="+asset.isHumanMotion+
                    " curves="+AnimationUtility.GetCurveBindings(asset).Length);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("SHIFTBOUND COURIER MOTION PASSED: coherent skeleton-derived poses; neutral body rotation; preserved motor arc");
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }
}
