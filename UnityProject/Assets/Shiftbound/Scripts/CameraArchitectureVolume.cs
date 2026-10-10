using System.Collections.Generic;
using UnityEngine;

namespace Shiftbound
{
    // Visual architecture may obstruct orbit without becoming traversal/Shift
    // collision. World-space bounds are baked by the environment authoring kit.
    public sealed class CameraArchitectureVolume : MonoBehaviour
    {
        public Bounds bounds;
        static readonly HashSet<CameraArchitectureVolume> active=new HashSet<CameraArchitectureVolume>();
        void OnEnable()=>active.Add(this);
        void OnDisable()=>active.Remove(this);
        public static float ClipDistance(Vector3 focus,Vector3 direction,float distance,float radius)
        {
            var ray=new Ray(focus,direction);
            foreach(var volume in active)
            {
                Bounds expanded=volume.bounds;expanded.Expand(radius*2);
                if(!expanded.Contains(focus)&&expanded.IntersectRay(ray,out float hit)&&hit<distance)
                    distance=Mathf.Max(.55f,hit-.035f);
            }
            return distance;
        }
    }
}
