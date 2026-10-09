using System;
using System.Collections;
using UnityEngine;

namespace Shiftbound
{
    public sealed class ReachAuditRunner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundReachAudit") >= 0)
                new GameObject("Reach audit").AddComponent<ReachAuditRunner>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            var motor = FindFirstObjectByType<PlayerMotor>();
            motor.enabled = false;
            motor.view = null;
            foreach (var trigger in FindObjectsByType<StageTrigger>(FindObjectsSortMode.None)) trigger.enabled = false;
            string[] sources = { "First landing", "Midair takeoff", "Left lookout" };
            string[] destinations = { "Shift landing", "Midair landing", "Return route" };
            bool requireLessons = Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundRequireShiftLessons") >= 0;
            foreach (int rate in new[] { 30, 60, 120, 144, 0 })
            {
            MeasureGap(motor, rate);
            for (int obstacle = 0; obstacle < sources.Length; obstacle++)
            {
                Bounds source = GameObject.Find(sources[obstacle]).GetComponent<BoxCollider>().bounds;
                Bounds destination = GameObject.Find(destinations[obstacle]).GetComponent<BoxCollider>().bounds;
                int successes = 0;
                float farthest = source.max.z;
                foreach (float delay in new[] { -0.08f, 0f, 0.04f, 0.08f, 0.12f })
                foreach (bool tap in new[] { false, true })
                {
                    motor.Teleport(new Vector3(source.center.x, source.max.y + 0.08f, source.max.z - 1.8f));
                    Physics.SyncTransforms();
                    float dt;
                    bool launched = false, wasSupported = false;
                    float unsupported = 0f;
                    bool landed = false;
                    for (int frame = 0; frame < Mathf.Max(rate, 60) * 3; frame++)
                    {
                        dt = Interval(rate, frame);
                        Vector3 p = motor.transform.position;
                        Vector2 direction = new Vector2(destination.center.x - p.x, destination.center.z - p.z);
                        Vector2 axes = Vector2.ClampMagnitude(direction / 1.2f, 1f);
                        bool press = false;
                        if (!launched)
                        {
                            if (motor.IsGrounded) wasSupported = true;
                            if (wasSupported && !motor.IsGrounded) unsupported += dt;
                            if ((delay < 0f && p.z >= source.max.z - motor.maxSpeed * -delay) ||
                                (delay >= 0f && wasSupported && !motor.IsGrounded && unsupported >= delay))
                            { press = true; launched = true; }
                        }
                        motor.Step(dt, axes, press, press && tap);
                        farthest = Mathf.Max(farthest, motor.transform.position.z);
                        p = motor.transform.position;
                        if (launched && motor.IsGrounded && p.z > source.max.z + 0.5f &&
                            p.x >= destination.min.x && p.x <= destination.max.x &&
                            p.z >= destination.min.z && p.z <= destination.max.z)
                        { landed = true; break; }
                        if (p.y < -2f) break;
                    }
                    if (landed) successes++;
                    Debug.Log("SHIFTBOUND BYPASS SAMPLE: " + sources[obstacle] + " -> " + destinations[obstacle] +
                        " rate=" + rate + " delay=" + delay + " tap=" + tap + " landed=" + landed);
                }
                Debug.Log("SHIFTBOUND REACH AUDIT: " + sources[obstacle] + " -> " + destinations[obstacle] +
                    " rate=" + rate + " bypasses=" + successes + "/10 farthestZ=" + farthest);
                if (requireLessons && obstacle < 2 && successes > 0)
                {
                    Debug.LogError("SHIFTBOUND REACH AUDIT FAILED: mandatory lesson bypassed: " + sources[obstacle]);
                    Application.Quit(1); yield break;
                }
            }
            }
            Debug.Log("SHIFTBOUND REACH AUDIT COMPLETE: samples are mechanical evidence, not exhaustive proof or human testing.");
            Application.Quit(0);
        }

        private static void MeasureGap(PlayerMotor motor, int rate)
        {
            var root = new GameObject("Reach measurement platforms");
            var launch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            launch.transform.SetParent(root.transform);
            launch.transform.position = new Vector3(1000f, -0.5f, -2f);
            launch.transform.localScale = new Vector3(10f, 1f, 4f);
            var landing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            landing.transform.SetParent(root.transform);
            landing.transform.localScale = new Vector3(10f, 1f, 4f);
            float largest = 0f;
            float largestTap = 0f;
            float dt;
            for (float gap = 0.5f; gap <= 9f; gap += 0.25f)
            foreach (float delay in new[] { -0.08f, 0f, 0.04f, 0.08f, 0.12f })
            foreach (bool tap in new[] { false, true })
            {
                landing.transform.position = new Vector3(1000f, -0.5f, gap + 2f);
                motor.Teleport(new Vector3(1000f, 0.08f, -2f));
                Physics.SyncTransforms();
                bool supported = false, jumped = false;
                float offTime = 0f;
                for (int i = 0; i < Mathf.Max(rate, 60) * 3; i++)
                {
                    dt = Interval(rate, i);
                    if (motor.IsGrounded) supported = true;
                    if (supported && !motor.IsGrounded) offTime += dt;
                    bool press = !jumped && ((delay < 0f && motor.transform.position.z >= motor.maxSpeed * delay) ||
                        (delay >= 0f && supported && !motor.IsGrounded && offTime >= delay));
                    if (press) jumped = true;
                    motor.Step(dt, Vector2.up, press, press && tap);
                    if (jumped && motor.IsGrounded && motor.transform.position.z >= gap)
                    {
                        if (tap) largestTap = Mathf.Max(largestTap, gap); else largest = Mathf.Max(largest, gap);
                        break;
                    }
                    if (motor.transform.position.y < -2f) break;
                }
            }
            root.SetActive(false); Destroy(root);
            Debug.Log("SHIFTBOUND PRACTICAL REACH: rate=" + rate + " heldMaxGap=" + largest +
                " tappedMaxGap=" + largestTap + " sampling=0.25m; delay sweep includes coyote and real capsule support.");
        }

        private static float Interval(int rate, int frame)
        {
            if (rate > 0) return 1f / rate;
            switch (frame % 4) { case 0: return 1f / 30f; case 1: return 1f / 120f;
                case 2: return 1f / 60f; default: return 1f / 144f; }
        }
    }
}
