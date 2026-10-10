using System;
using System.Collections;
using UnityEngine;

namespace Shiftbound
{
    public sealed class OrbitExperienceScenario : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundOrbitScenario")>=0)
                new GameObject("Orbit experience scenario").AddComponent<OrbitExperienceScenario>();
        }
        IEnumerator Start()
        {
            bool repeat=Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundRepeatScenario")>=0;
            Application.targetFrameRate=60; QualitySettings.vSyncCount=0;
            yield return new WaitForSecondsRealtime(1f);
            var driver=gameObject.AddComponent<ProductionInputDriver>();
            do {
            GameFlow.Instance.TogglePause();
            yield return new WaitForSecondsRealtime(2f);
            GameFlow.Instance.TogglePause();
            // Input System gamepad orbit through the normal free camera, no pose snaps.
            driver.SetLook(new Vector2(.6f,0)); yield return new WaitForSecondsRealtime(5.3f);
            driver.SetLook(Vector2.zero); driver.Set(Vector2.zero,false,true);
            yield return new WaitForSecondsRealtime(.8f);
            driver.Set(Vector2.zero,false); driver.SetLook(new Vector2(.6f,0));
            yield return new WaitForSecondsRealtime(5.3f); driver.SetLook(Vector2.zero);
            // Ordinary responsive inputs: run, release/brake, reverse, jump and
            // immediate movement, then approach equipment for camera collision.
            driver.Set(Vector2.up,false); yield return new WaitForSecondsRealtime(.35f);
            driver.Set(Vector2.zero,false); yield return new WaitForSecondsRealtime(.3f);
            driver.Set(Vector2.down,false); yield return new WaitForSecondsRealtime(.35f);
            driver.Set(Vector2.up,true); yield return new WaitForSecondsRealtime(.65f);
            driver.Set(Vector2.down,false); yield return new WaitForSecondsRealtime(.6f);
            driver.Set(Vector2.left,false); yield return new WaitForSecondsRealtime(.5f);
            driver.Set(Vector2.zero,false); driver.SetLook(new Vector2(.4f,.15f));
            yield return new WaitForSecondsRealtime(2f); driver.SetLook(Vector2.zero);
            GameFlow.Instance.Respawn();
            yield return new WaitForSecondsRealtime(1f);
            Debug.Log("SHIFTBOUND ORBIT SCENARIO COMPLETE: synthetic gamepad through production camera/motor; no human/controller comfort claim.");
            } while(repeat);
        }
    }
}
