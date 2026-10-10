using UnityEngine;

namespace Shiftbound
{
    public sealed class AtmosphereController : MonoBehaviour
    {
        public WorldSwitcher worlds;
        public Color presentFog = new Color(0.59f, 0.72f, 0.80f);
        public Color alteredFog = new Color(0.67f, 0.68f, 0.52f);
        public Color presentAmbient = new Color(0.43f, 0.51f, 0.60f);
        public Color alteredAmbient = new Color(0.56f, 0.51f, 0.40f);
        public float transitionSpeed = 6f;

        private void Update()
        {
            if (worlds == null) return;
            Color wantedFog = worlds.IsAltered ? alteredFog : presentFog;
            Color wantedAmbient = worlds.IsAltered ? alteredAmbient : presentAmbient;
            float t = 1f - Mathf.Exp(-transitionSpeed * Time.unscaledDeltaTime);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, wantedFog, t);
            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, wantedAmbient, t);
            if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Trilight)
            {
                RenderSettings.ambientSkyColor = Color.Lerp(RenderSettings.ambientSkyColor, wantedAmbient, t);
                RenderSettings.ambientEquatorColor = Color.Lerp(RenderSettings.ambientEquatorColor, wantedAmbient * 0.82f, t);
                RenderSettings.ambientGroundColor = Color.Lerp(RenderSettings.ambientGroundColor, wantedAmbient * 0.48f, t);
            }
            if (worlds.sun != null)
                worlds.sun.color = Color.Lerp(worlds.sun.color,
                    worlds.IsAltered ? worlds.alteredLight : worlds.presentLight, t);
        }
    }
}
