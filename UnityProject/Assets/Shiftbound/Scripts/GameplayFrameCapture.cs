using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Shiftbound
{
    // Opt-in offscreen evidence using the actual shipped Canvas. A temporary
    // URP overlay camera preserves scene color; a second Base Camera.Render
    // clears the scene even with CameraClearFlags.Depth. No substitute HUD.
    // This readback path is never enabled during performance measurements.
    public static class GameplayFrameCapture
    {
        public static void Render(Camera camera,RenderTexture target)
        {
            var hud=Object.FindFirstObjectByType<ProductionHUDCanvas>();
            var canvas=hud!=null?hud.GetComponentInChildren<Canvas>():null;
            var oldTarget=camera.targetTexture;Camera ui=null;
            var cameraData=camera.GetUniversalAdditionalCameraData();
            try
            {
                if(canvas!=null)
                {
                    ui=new GameObject("Diagnostic shipped UI capture",typeof(Camera)).GetComponent<Camera>();
                    ui.enabled=false;ui.cullingMask=1<<5;ui.allowHDR=false;
                    ui.nearClipPlane=.01f;ui.farClipPlane=2;
                    var uiData=ui.GetUniversalAdditionalCameraData();
                    uiData.renderType=CameraRenderType.Overlay;
                    uiData.renderPostProcessing=false;uiData.renderShadows=false;
                    cameraData.cameraStack.Add(ui);
                    canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=ui;canvas.planeDistance=1;
                    Canvas.ForceUpdateCanvases();
                }
                // Camera.Render's URP single-camera path omits its overlay
                // stack. A StandardRequest renders the real scene and UI stack.
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                    new UnityEngine.Rendering.RenderPipeline.StandardRequest{destination=target});
            }
            finally
            {
                camera.targetTexture=oldTarget;
                if(ui!=null)cameraData.cameraStack.Remove(ui);
                if(canvas!=null){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;canvas.enabled=true;}
                if(ui!=null)Object.Destroy(ui.gameObject);
            }
        }
    }
}
