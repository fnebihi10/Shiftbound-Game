using UnityEngine;

namespace Shiftbound
{
    // Far-field art only; nearby buildings remain meshes with perspective.
    public sealed class WorldPanorama : MonoBehaviour
    {
        public WorldSwitcher worlds;
        public Material present;
        public Material overgrown;
        private bool? applied;

        private void Start() { Refresh(); }
        private void LateUpdate() { Refresh(); }
        private void Refresh()
        {
            if (worlds == null || applied == worlds.IsAltered) return;
            applied = worlds.IsAltered;
            RenderSettings.skybox = worlds.IsAltered ? overgrown : present;
            // Static ambient/reflection deliberately avoid a GI rebuild on Shift.
        }
    }
}
