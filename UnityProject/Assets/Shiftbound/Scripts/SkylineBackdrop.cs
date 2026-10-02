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

        private Vector3 anchor;
        private Quaternion orientation;

        private void Start()
        {
            // Fix the skyline in world space. Camera orbit now reveals perspective
            // against the playable rooftops instead of dragging the city along.
            anchor = viewer != null ? viewer.transform.position + Vector3.forward * distance :
                new Vector3(0f, 0f, distance);
            anchor.x = 0f;
            orientation = Quaternion.identity;
            Refresh();
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
            if (anchor == Vector3.zero) Start();
            transform.position = anchor;
            transform.rotation = orientation;
            float halfHeight = Mathf.Tan(viewer.fieldOfView * Mathf.Deg2Rad * 0.5f) * distance;
            transform.localScale = new Vector3(halfHeight * 2f * viewer.aspect * overscan * 3f,
                halfHeight * 2f * overscan * 2f, 1f);
        }
    }
}


