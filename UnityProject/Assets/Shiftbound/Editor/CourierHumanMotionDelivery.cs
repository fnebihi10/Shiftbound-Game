using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static class CourierHumanMotionDelivery
{
    public static void IntegrateAndBuildWindows()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        var courier=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var animator=courier.GetComponent<Animator>();
        if(!animator.isHuman)throw new Exception("Expected integrated anatomical courier.");
        var actions=AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>("Assets/Shiftbound/CourierHuman/HumanMotion.overrideController");
        if(actions==null)throw new Exception("Missing inspected anatomical action set.");
        int replaced=0;
        var pairs=new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip,AnimationClip>>();
        actions.GetOverrides(pairs);
        foreach(var pair in pairs)
        {
            if(pair.Key.name.Contains("Courier"))
            {
                if(pair.Value==null || !pair.Value.isHumanMotion)throw new Exception("Missing anatomical action: "+pair.Key.name);
                replaced++;
            }
            else if(pair.Value!=null)throw new Exception("Preferred original movement must remain unchanged: "+pair.Key.name);
        }
        if(replaced!=11)throw new Exception("Expected eleven inspected anatomical actions.");
        animator.runtimeAnimatorController=actions;
        courier.authoredActions=true;courier.authoredLocomotion=false;
        // Measured after import on this same anatomical avatar. Original Jog
        // remains default; these distances serve only the opt-in gait review.
        courier.walkCycleDistance=1.387906f;
        courier.runCycleDistance=2.402378f;
        courier.sprintCycleDistance=3.585596f;
        EditorUtility.SetDirty(animator);EditorUtility.SetDirty(courier);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        Debug.Log("HUMAN OWN-RIG MOTION INTEGRATED: eleven actions; original Jog, motor, camera and world preserved. Production acceptance remains pending.");
        SliceDelivery.BuildWindows();
    }
}
