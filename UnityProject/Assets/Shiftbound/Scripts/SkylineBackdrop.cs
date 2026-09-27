using UnityEngine;

namespace Shiftbound
{
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SkylineBackdrop : MonoBehaviour
    {
        public Camera viewer;
        public WorldSwitcher worlds;
        public Material presentMaterial;
        public Material overgrownMaterial;
        [Min(20f)] public float distance = 180f;
        [Range(1f, 1.3f)] public float overscan = 1.08f;

        private MeshRenderer mesh;

        private void Awake()
        {
            mesh = GetComponent<MeshRenderer>();
            if (viewer == null) viewer = Camera.main;
        }

        private void LateUpdate() { Refresh(); }

        public void Refresh()
        {
            if (viewer == null) return;
            if (mesh == null) mesh = GetComponent<MeshRenderer>();
            Material wanted = worlds != null && worlds.IsAltered ? overgrownMaterial : presentMaterial;
            if (wanted != null && mesh.sharedMaterial != wanted) mesh.sharedMaterial = wanted;

            // A distant matte layer follows the view. Nearby roofs and buildings remain real 3D
            // objects in front of it, with working perspective and collision.
            transform.position = viewer.transform.position + viewer.transform.forward * distance;
            transform.rotation = viewer.transform.rotation;
            float halfHeight = Mathf.Tan(viewer.fieldOfView * Mathf.Deg2Rad * 0.5f) * distance;
            transform.localScale = new Vector3(halfHeight * 2f * viewer.aspect * overscan,
                halfHeight * 2f * overscan, 1f);
        }
    }
}


