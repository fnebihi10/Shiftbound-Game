using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static class CourierAssetReview
{
    public static void Run()
    {
        string output=Environment.GetEnvironmentVariable("SHIFTBOUND_COURIER_AUDIT");
        Directory.CreateDirectory(output);
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        var courier=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var animator=courier.GetComponent<Animator>();
        var controller=(AnimatorController)animator.runtimeAnimatorController;
        var csv=new System.Text.StringBuilder("state,phase,time,lx,ly,lz,rx,ry,rz,forward_dot\n");
        animator.applyRootMotion=false;animator.Rebind();animator.Update(0f);
        foreach(var entry in controller.layers[0].stateMachine.states)
        {
            if(!(entry.state.motion is AnimationClip clip))continue;
            Debug.Log("COURIER REVIEW CLIP: "+entry.state.name+" asset="+AssetDatabase.GetAssetPath(clip)+" name="+clip.name+" length="+clip.length+" averageSpeed="+clip.averageSpeed+" human="+clip.isHumanMotion);
            for(int i=0;i<=120;i++)
            {
                float phase=i/120f;
                animator.Play("Base Layer."+entry.state.name,0,phase);animator.Update(0f);
                Vector3 l=courier.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
                Vector3 r=courier.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                Vector3 across=animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position-animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
                Vector3 up=animator.GetBoneTransform(HumanBodyBones.Head).position-animator.GetBoneTransform(HumanBodyBones.Hips).position;
                Vector3 forward=Vector3.Cross(across,up);forward.y=0;
                float dot=Vector3.Dot(forward.normalized,courier.transform.forward);
                csv.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"{0},{1:F6},{2:F6},{3:F6},{4:F6},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6}\n",entry.state.name,phase,phase*clip.length,l.x,l.y,l.z,r.x,r.y,r.z,dot);
            }
        }
        File.WriteAllText(Path.Combine(output,"source-gaits.csv"),csv.ToString());
        foreach(var renderer in courier.GetComponentsInChildren<SkinnedMeshRenderer>())
            Debug.Log("COURIER REVIEW MESH: "+renderer.name+" vertices="+renderer.sharedMesh.vertexCount+" triangles="+renderer.sharedMesh.triangles.Length/3+" bones="+renderer.bones.Length+" bounds="+renderer.localBounds+" materials="+string.Join(",",renderer.sharedMaterials.Select(m=>m.name)));
        foreach(string path in new[]{InspectRiggedAssets.AnimationPath,"Assets/Shiftbound/ThirdParty/Quaternius/Animation2/UAL2_Standard.fbx"})
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            if(importer.animationType!=ModelImporterAnimationType.Human)
            {importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.SaveAndReimport();}
            foreach(var clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")))
                Debug.Log("COURIER AVAILABLE CLIP: "+Path.GetFileName(path)+" | "+clip.name+" | seconds="+clip.length+" | human="+clip.isHumanMotion);
        }
        Debug.Log("COURIER ASSET REVIEW PASSED: actual Animator sampling, source clips retained; not artistic acceptance.");
    }
}
