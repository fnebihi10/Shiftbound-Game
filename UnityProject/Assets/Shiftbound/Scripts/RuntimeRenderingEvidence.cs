using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shiftbound
{
    // One snapshot after rendering, outside sustained measurement. No readback,
    // device identifier or per-frame allocation. Describes the running player.
    public static class RuntimeRenderingEvidence
    {
        static bool pending;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install() => Request();
        public static void Request()
        {
            if(pending)return;
            pending=true;RenderPipelineManager.endCameraRendering+=Capture;
        }
        static void Capture(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=Camera.main)return;
            RenderPipelineManager.endCameraRendering-=Capture;pending=false;
            var rp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if(rp==null)return;
            var data=camera.GetUniversalAdditionalCameraData();
            var text=new StringBuilder();
            void Row(string key,object value)=>text.Append(key).Append('=').Append(value).Append('\n');
            Row("utc",DateTime.UtcNow.ToString("o"));Row("unity",Application.unityVersion);
            Row("model",SystemInfo.deviceModel);Row("os",SystemInfo.operatingSystem);
            Row("cpu",SystemInfo.processorType);Row("gpu",SystemInfo.graphicsDeviceName);
            Row("graphicsAPI",SystemInfo.graphicsDeviceType);Row("driver",SystemInfo.graphicsDeviceVersion);
            Row("ramMB",SystemInfo.systemMemorySize);Row("gpuMemoryMB",SystemInfo.graphicsMemorySize);
            Row("quality",QualitySettings.names[QualitySettings.GetQualityLevel()]);
            Row("pipeline",rp.name);Row("rendererType",data.scriptableRenderer.GetType().FullName);
            Row("screen",Screen.width+"x"+Screen.height);Row("cameraPixelRect",camera.pixelRect);
            Row("URP_ScaledScreenParams",Shader.GetGlobalVector("_ScaledScreenParams"));
            Row("renderScale",rp.renderScale);Row("upscaling",rp.upscalingFilter);
            Row("MSAA",rp.msaaSampleCount);Row("cameraAA",data.antialiasing);
            Row("dynamicResolution",camera.allowDynamicResolution);Row("HDR",rp.supportsHDR);
            Row("grading",rp.colorGradingMode);Row("postProcessing",data.renderPostProcessing);
            Row("shadowDistance",rp.shadowDistance);Row("shadowResolution",rp.mainLightShadowmapResolution);
            Row("shadowCascades",rp.shadowCascadeCount);Row("softShadows",rp.supportsSoftShadows);
            Row("skinWeights",QualitySettings.skinWeights);Row("mipLimit",QualitySettings.globalTextureMipmapLimit);
            Row("lodBias",QualitySettings.lodBias);Row("textureStreaming",QualitySettings.streamingMipmapsActive);
            Row("textureCurrentBytes",Texture.currentTextureMemory);Row("textureDesiredBytes",Texture.desiredTextureMemory);
            Row("frameCap",Application.targetFrameRate);Row("vSync",QualitySettings.vSyncCount);
            Row("refresh",Screen.currentResolution.refreshRateRatio);Row("safeArea",Screen.safeArea);
            Row("controlScale",PlayerPreferences.ControlScale);Row("controlInset",PlayerPreferences.ControlInset);
            Row("controlHeight",PlayerPreferences.ControlHeight);Row("touchSensitivity",PlayerPreferences.TouchSensitivity);
            Row("volume",PlayerPreferences.Volume);Row("cameraAssist",PlayerPreferences.CameraAssist);
            Row("lowPower",PlayerPreferences.LowPower);
            Row("fog",RenderSettings.fog+" "+RenderSettings.fogMode+" "+RenderSettings.fogStartDistance+".."+RenderSettings.fogEndDistance);
            foreach(var volume in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
                Row("volume",volume.name+" priority="+volume.priority+" weight="+volume.weight+" profile="+(volume.sharedProfile!=null?volume.sharedProfile.name:"none"));
            foreach(var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
                if(texture.name.Contains("Ivy")||texture.name.Contains("Limestone")||texture.name.Contains("DistantCity"))
                    Row("texture",texture.name+" "+texture.width+"x"+texture.height+" format="+texture.format+" mipCount="+texture.mipmapCount+" loadedMip="+texture.loadedMipmapLevel);
            foreach(var material in Resources.FindObjectsOfTypeAll<Material>())
                if(material.name.Contains("Rooted leaves")||material.name.Contains("panorama"))
                    Row("material",material.name+" shader="+(material.shader!=null?material.shader.name:"NULL")+" supported="+(material.shader!=null&&material.shader.isSupported));
            var identity=Resources.Load<TextAsset>("CandidateIdentity");
            if(identity!=null)File.WriteAllText(Path.Combine(Application.persistentDataPath,"render-build-identity.json"),identity.text);
            File.WriteAllText(Path.Combine(Application.persistentDataPath,"render-state.txt"),text.ToString());
            Debug.Log("SHIFTBOUND RUNTIME RENDER STATE:\n"+text);
        }
    }
}
