using System;
using UnityEngine;
namespace Shiftbound
{
    // Opt-in evidence framing only. Shipping camera settings are untouched.
    public sealed class CourierCloseReview:MonoBehaviour
    {
        public static bool Requested=>Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundCourierCloseReview")>=0;
    }
}
