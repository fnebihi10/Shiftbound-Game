using UnityEngine;
namespace Shiftbound
{
    // A constructed visual skin for an existing collision surface. It receives
    // the same inactive-world preview treatment without duplicating collision.
    public sealed class WorldSurfaceSkin : MonoBehaviour
    {
        public Collider collisionSurface;
    }
}
