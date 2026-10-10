using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shiftbound
{
    // Pixel/shadow limits are candidates for device validation, not FPS claims.
    // Canvas stays at native safe-area resolution; only the scene is scaled.
    public sealed class MobileRenderingBudget : MonoBehaviour
    {
        static MobileRenderingBudget instance;
        UniversalRenderPipelineAsset budget, original;
        int width, height; bool low;
        Volume phoneGrade;
        public static bool Requested => Application.isMobilePlatform || Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundMobileRendering")>=0;
        public static void Apply()
        {
            if(!Requested)return;
            if(instance==null){instance=new GameObject("Phone rendering budget").AddComponent<MobileRenderingBudget>();DontDestroyOnLoad(instance.gameObject);}
            instance.Configure();
        }
        void Configure()
        {
            bool changed=budget==null||width!=Screen.width||height!=Screen.height||low!=PlayerPreferences.LowPower;
            if(budget==null)
            {
                // A desktop diagnostic must start from the actual phone asset
                // and renderer, including its lack of SSAO. Cloning PC here
                // retained desktop features and invalidated the comparison.
                if(!Application.isMobilePlatform)
                {
                    int mobile=Array.IndexOf(QualitySettings.names,"Mobile");
                    if(mobile>=0)QualitySettings.SetQualityLevel(mobile,false);
                }
                original=QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
                var source=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
                if(!Application.isMobilePlatform)
                {
                    var follow=Camera.main!=null?Camera.main.GetComponent<FollowCamera>():null;
                    if(follow==null||follow.mobileDiagnosticPipeline==null)
                        throw new InvalidOperationException("Mobile comparison requires the serialized Android pipeline");
                    source=follow.mobileDiagnosticPipeline;
                    QualitySettings.skinWeights=SkinWeights.FourBones;QualitySettings.lodBias=1;
                }
                if(source==null)return;
                budget=Instantiate(source);budget.name="Shiftbound phone runtime budget";
                QualitySettings.renderPipeline=budget;
            }
            width=Screen.width;height=Screen.height;low=PlayerPreferences.LowPower;
            budget.renderScale=Mathf.Min(1f,(low?960f:1536f)/Mathf.Max(width,height),(low?540f:864f)/Mathf.Min(width,height));
            budget.upscalingFilter=UpscalingFilterSelection.FSR;
            budget.fsrOverrideSharpness=true;budget.fsrSharpness=.35f;
            // Keep the same restrained ACES/contrast grade as Windows. Dropping
            // the entire grade flattened limestone/cloth and clipped the yellow
            // jacket on the actual S10+. No bloom or extra effect chain.
            budget.supportsHDR=true;budget.hdrColorBufferPrecision=HDRColorBufferPrecision._32Bits;
            budget.colorGradingMode=ColorGradingMode.HighDynamicRange;
            budget.msaaSampleCount=low?1:2;
            budget.shadowCascadeCount=1;budget.shadowDistance=low?22:35;
            budget.mainLightShadowmapResolution=low?512:1024;
            Camera camera=Camera.main;
            if(camera!=null){camera.allowHDR=true;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;}
            if(phoneGrade==null)
            {
                phoneGrade=gameObject.AddComponent<Volume>();phoneGrade.isGlobal=true;phoneGrade.priority=100;
                var profile=ScriptableObject.CreateInstance<VolumeProfile>();phoneGrade.sharedProfile=profile;
                profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);
                var grade=profile.Add<ColorAdjustments>();grade.contrast.Override(3);grade.saturation.Override(-4);
                profile.Add<Bloom>().intensity.Override(0);
            }
            if(Application.isMobilePlatform){QualitySettings.vSyncCount=0;Application.targetFrameRate=low?30:60;}
            Debug.Log("SHIFTBOUND PHONE BUDGET: tier="+(low?30:60)+" native="+width+"x"+height+" scale="+budget.renderScale+" HDR32/ACES, bloom=0 shadow="+budget.shadowDistance+"/"+budget.mainLightShadowmapResolution+"; device pacing requires measurement");
            if(changed)RuntimeRenderingEvidence.Request();
        }
        void Update(){if(width!=Screen.width||height!=Screen.height||low!=PlayerPreferences.LowPower)Configure();}
        void OnDestroy(){if(phoneGrade!=null)Destroy(phoneGrade.sharedProfile);if(budget!=null){QualitySettings.renderPipeline=original;Destroy(budget);}if(instance==this)instance=null;}
    }
}
