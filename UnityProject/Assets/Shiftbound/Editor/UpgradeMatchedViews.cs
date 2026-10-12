using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Shiftbound;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class UpgradeMatchedViews
{
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static void Capture()
    {
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
        string baseline=Path.Combine(root,".validation/ProductionUpgrade/Upgrade20261011/Reference/Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        const string temporary="Assets/Shiftbound/Scenes/UpgradeReferenceInspection.unity";
        if(File.Exists(temporary))throw new Exception("Temporary inspection scene already exists; refusing to overwrite it.");
        File.Copy(baseline,temporary);AssetDatabase.ImportAsset(temporary,ImportAssetOptions.ForceSynchronousImport);
        try
        {
            foreach(bool candidate in new[]{false,true})
            {
                EditorSceneManager.OpenScene(candidate?UpgradeProduction.Scene:temporary);
                var worlds=UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
                var player=UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
                var follow=UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
                var camera=Camera.main;camera.aspect=1280f/720;
                Invoke(worlds,"Awake");Invoke(follow,"Awake");follow.Snap();
                foreach(var skyline in UnityEngine.Object.FindObjectsByType<SkylineBackdrop>(FindObjectsSortMode.None))skyline.Refresh();
                var courier=UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
                var motion=(AnimatorController)courier.GetComponent<Animator>().runtimeAnimatorController;
                var idle=(AnimationClip)motion.layers[0].stateMachine.states.First(s=>s.state.name=="Idle").state.motion;
                idle.SampleAnimation(courier.gameObject,.6f);
                Vector3[] positions={new Vector3(0,.08f,1),new Vector3(0,.08f,21),new Vector3(0,.08f,1),new Vector3(0,.08f,1),new Vector3(0,.08f,35.5f)};
                float[] yaw={0,35,125,180,-75};float[] pitch={18,25,12,18,42};
                string[] names={"opening","exposed","close","rear","below"};
                for(int view=0;view<names.Length;view++)
                foreach(bool altered in new[]{false,true})
                {
                    typeof(WorldSwitcher).GetField("activeWorld",Private).SetValue(worlds,altered?1:0);Invoke(worlds,"Apply");
                    var art=UnityEngine.Object.FindFirstObjectByType<RooftopWorldArt>();Invoke(art,"Apply");
                    foreach(var atmosphere in UnityEngine.Object.FindObjectsByType<AtmosphereController>(FindObjectsSortMode.None))
                    {
                        Color ambient=altered?atmosphere.alteredAmbient:atmosphere.presentAmbient;
                        RenderSettings.fogColor=altered?atmosphere.alteredFog:atmosphere.presentFog;
                        RenderSettings.ambientLight=ambient;
                        if(RenderSettings.ambientMode==UnityEngine.Rendering.AmbientMode.Trilight)
                        {RenderSettings.ambientSkyColor=ambient;RenderSettings.ambientEquatorColor=ambient*.6f;RenderSettings.ambientGroundColor=ambient*.28f;}
                        worlds.sun.color=altered?worlds.alteredLight:worlds.presentLight;
                    }
                    player.transform.position=positions[view];
                    follow.distance=view==2?4f:8f;follow.SetInspectionOrbit(yaw[view],pitch[view]);
                    foreach(var skyline in UnityEngine.Object.FindObjectsByType<SkylineBackdrop>(FindObjectsSortMode.None))skyline.Refresh();
                    Physics.SyncTransforms();
                    var target=new RenderTexture(1280,720,24);var old=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                    var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                    string directory=Environment.GetEnvironmentVariable("SHIFTBOUND_MATCHED_DIR") ?? Path.Combine(root,"ArtDirection/Upgrade20261011/Matched");Directory.CreateDirectory(directory);
                    File.WriteAllBytes(Path.Combine(directory,(candidate?"after-":"before-")+names[view]+(altered?"-overgrown":"-present")+".png"),image.EncodeToPNG());
                    camera.targetTexture=null;RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);
                }
            }
        }
        finally {EditorSceneManager.OpenScene(UpgradeProduction.Scene);AssetDatabase.DeleteAsset(temporary);}
        Debug.Log("UPGRADE MATCHED VIEWS PASSED: original reference scene and candidate, same camera/pose/world procedure; Editor renders, no HUD or motion acceptance.");
    }
    static void Invoke(object target,string method)=>target.GetType().GetMethod(method,Private).Invoke(target,null);
}
