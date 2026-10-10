using UnityEngine;

namespace Shiftbound
{
    // Retained serialized links and shared formatter; ProductionHUDCanvas renders
    // the shipped gameplay/pause/completion interface at native safe-area scale.
    public sealed class PremiumHUD : MonoBehaviour
    {
        public GameFlow flow;
        public WorldSwitcher worlds;
        public static string FormatTime(float seconds)
        {
            int tenths=Mathf.Max(0,Mathf.FloorToInt(seconds*10));
            return string.Format("{0:00}:{1:00}.{2}",tenths/600,(tenths/10)%60,tenths%10);
        }
    }
}