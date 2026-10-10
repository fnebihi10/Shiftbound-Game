using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static partial class ProductionUpgradeAuthoring
{
    public static void Motion()
    {
        Folders();EditorSceneManager.OpenScene(Scene);
        var courier=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var controller=(AnimatorController)courier.GetComponent<Animator>().runtimeAnimatorController;
        AnimationClip Source(string library,string suffix)=>AssetDatabase.LoadAllAssetsAtPath(library)
            .OfType<AnimationClip>().Single(c=>!c.name.StartsWith("__preview")&&c.name.EndsWith("|"+suffix,StringComparison.Ordinal));
        const string a="Assets/Shiftbound/ThirdParty/Quaternius/Animation/UAL1_Standard.fbx";
        const string b="Assets/Shiftbound/ThirdParty/Quaternius/Animation2/UAL2_Standard.fbx";
        var idle=(AnimationClip)controller.layers[0].stateMachine.states.Single(s=>s.state.name=="Idle").state.motion;
        var rootHeight=AnimationUtility.GetEditorCurve(idle,EditorCurveBinding.FloatCurve("",typeof(Animator),"RootT.y"));
        if(rootHeight==null)throw new Exception("Idle body-height reference is missing");
        float standingHeight=rootHeight.Evaluate(0);
        foreach(string name in new[]{"Takeoff","Jump","Fall","Land"})
        {
            var original=name=="Takeoff"?Source(b,"NinjaJump_Start"):name=="Jump"?Source(b,"NinjaJump_Idle_Loop"):
                name=="Land"?Source(b,"NinjaJump_Land"):Source(a,"Jump_Loop");
            var clip=UnityEngine.Object.Instantiate(original);clip.name="Courier "+name;
            void Muscle(string muscle,float value,float breath=0)
            {
                if(!HumanTrait.MuscleName.Contains(muscle))throw new Exception("Unknown Humanoid muscle "+muscle);
                clip.SetCurve("",typeof(Animator),muscle,new AnimationCurve(new Keyframe(0,value),new Keyframe(clip.length*.5f,value+breath),new Keyframe(clip.length,value)));
            }
            void Pose(string muscle,float start,float middle,float end)
            {
                // The licensed idle/fall clips are two seconds long, whereas
                // this motor reaches its apex in roughly .36 s. Place the pose
                // change inside the real arc rather than after the landing.
                float duration=name=="Takeoff"?.075f:name=="Jump"?.25f:name=="Fall"?.22f:.16f;
                clip.SetCurve("",typeof(Animator),muscle,new AnimationCurve(
                    new Keyframe(0,start),new Keyframe(duration*.45f,middle),new Keyframe(duration,end)));
            }
            // A consistent courier posture through takeoff, ascent and descent.
            // Retain the licensed lower-body recoil for takeoff/standing impact.
            foreach(string side in new[]{"Left","Right"})
            {
                Muscle(side+" Arm Down-Up",name=="Land"?-.80f:name=="Jump"?-.75f:-.65f,.025f);
                Muscle(side+" Arm Front-Back",name=="Takeoff"?.12f:name=="Fall"?-.15f:-.05f,.025f);
                Muscle(side+" Arm Twist In-Out",.02f);
                Muscle(side+" Forearm Stretch",name=="Takeoff"?-.35f:name=="Jump"?-.15f:name=="Fall"?.10f:.20f,.02f);
                Muscle(side+" Forearm Twist In-Out",.05f);
                Muscle(side+" Shoulder Down-Up",-.08f);
                Muscle(side+" Shoulder Front-Back",.05f);
            }
            if(name=="Jump"||name=="Fall")
            {
                bool rise=name=="Jump";
                Muscle("Left Upper Leg Front-Back",rise?.27f:.08f);
                Muscle("Right Upper Leg Front-Back",rise?.17f:.03f);
                Muscle("Left Upper Leg In-Out",.02f);Muscle("Right Upper Leg In-Out",.02f);
                Muscle("Left Upper Leg Twist In-Out",0);Muscle("Right Upper Leg Twist In-Out",0);
                Muscle("Left Lower Leg Stretch",rise?-.32f:.55f);
                Muscle("Right Lower Leg Stretch",rise?-.20f:.64f);
                Muscle("Left Foot Up-Down",.05f);Muscle("Right Foot Up-Down",.05f);
                Muscle("Spine Front-Back",.12f);Muscle("Chest Front-Back",.08f);
            }
            // Running leap: opposing arms and unequal knee lift retain momentum
            // instead of holding the courier in a symmetrical suspended pose.
            if(name=="Takeoff" || name=="Jump")
            {
                // A forward leap, not the source ninja's suspended tuck. The
                // ascent starts at the takeoff end pose to avoid a second recoil.
                bool launch=name=="Takeoff";
                Pose("Left Arm Front-Back",launch?.10f:.24f,.26f,.18f);
                Pose("Right Arm Front-Back",launch?-.10f:-.24f,-.26f,-.18f);
                Pose("Left Forearm Stretch",.12f,.02f,.14f);
                Pose("Right Forearm Stretch",.18f,.10f,.22f);
                Pose("Left Upper Leg Front-Back",launch?.08f:.22f,.26f,.14f);
                Pose("Right Upper Leg Front-Back",-.12f,-.08f,-.04f);
                Pose("Left Lower Leg Stretch",launch?.55f:.28f,.25f,.48f);
                Pose("Right Lower Leg Stretch",.62f,.48f,.65f);
                Muscle("Spine Front-Back",.08f);Muscle("Chest Front-Back",.04f);
            }
            if(name=="Fall")
            {
                Pose("Left Upper Leg Front-Back",.14f,.08f,.04f);
                Pose("Right Upper Leg Front-Back",-.04f,.02f,.04f);
                Pose("Left Lower Leg Stretch",.48f,.68f,.78f);
                Pose("Right Lower Leg Stretch",.65f,.72f,.78f);
                Pose("Left Arm Front-Back",.18f,.02f,-.08f);
                Pose("Right Arm Front-Back",-.18f,-.10f,-.08f);
                Muscle("Left Forearm Stretch",.18f);Muscle("Right Forearm Stretch",.18f);
            }
            if(name=="Land")
            {
                foreach(string side in new[]{"Left","Right"})
                {
                    Pose(side+" Upper Leg Front-Back",.04f,.15f,.02f);
                    Pose(side+" Lower Leg Stretch",.78f,.35f,.85f);
                    Pose(side+" Foot Up-Down",.02f,-.04f,.02f);
                    Pose(side+" Arm Front-Back",-.08f,.06f,0);
                    Pose(side+" Forearm Stretch",.18f,.08f,.22f);
                }
                Pose("Spine Front-Back",.05f,.12f,0);
                Pose("Chest Front-Back",.02f,.06f,0);
            }
            // The source ninja landing drops RootT.y from1.06 to.55 in.23s
            // and recovers over1.27s. That creates a deep visual collapse and
            // a snap when this game's .18s landing state ends. The motor owns
            // the jump arc; keep visual body translation in place, with only
            // a short3cm standing impact on top of the running IK response.
            foreach(string axis in new[]{"x","z"})
                clip.SetCurve("",typeof(Animator),"RootT."+axis,AnimationCurve.Constant(0,clip.length,0));
            clip.SetCurve("",typeof(Animator),"RootT.y",name=="Land"?
                new AnimationCurve(new Keyframe(0,standingHeight),new Keyframe(.055f,standingHeight-.03f),
                    new Keyframe(.16f,standingHeight),new Keyframe(clip.length,standingHeight)):
                AnimationCurve.Constant(0,clip.length,standingHeight));
            var settings=AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime=false;AnimationUtility.SetAnimationClipSettings(clip,settings);
            string path=Root+"/Motion/Courier-"+name+".anim";
            var asset=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(asset!=null){EditorUtility.CopySerialized(clip,asset);UnityEngine.Object.DestroyImmediate(clip);clip=asset;}
            else AssetDatabase.CreateAsset(clip,path);
            var state=controller.layers[0].stateMachine.states.Single(s=>s.state.name==name).state;state.motion=clip;EditorUtility.SetDirty(state);
            Debug.Log("UPGRADE AUTHORED MOTION: "+name+" Humanoid="+clip.isHumanMotion+" curves="+AnimationUtility.GetCurveBindings(clip).Length);
        }
        EditorUtility.SetDirty(controller);AssetDatabase.SaveAssets();
        Debug.Log("SHIFTBOUND COURIER MOTION PASSED");
    }
}
