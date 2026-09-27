using System;
using System.Collections;
using UnityEngine;

namespace Shiftbound
{
    // Runs only when the built player is launched with -shiftboundSmoke.
    public sealed class SmokeRunner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void MaybeRun()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundSmoke") < 0) return;
            new GameObject("Shiftbound smoke check").AddComponent<SmokeRunner>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            var flow = GameFlow.Instance;
            var world = FindFirstObjectByType<WorldSwitcher>();
            var player = FindFirstObjectByType<PlayerMotor>();
            if (flow == null || world == null || player == null)
            {
                Fail("Missing scene systems");
                yield break;
            }

            player.Teleport(new Vector3(2.5f, 0.08f, 3f));
            Collider wall = world.alteredRoot.Find("Alternate wall test").GetComponent<Collider>();
            bool penetration = Physics.ComputePenetration(world.playerProbe, world.playerProbe.transform.position, world.playerProbe.transform.rotation, wall, wall.transform.position, wall.transform.rotation, out _, out float depth);
            Debug.Log("SHIFTBOUND DIAGNOSTIC: probe=" + world.playerProbe.transform.position + " wall=" + wall.transform.position + " enabled=" + wall.enabled + " penetration=" + penetration + " depth=" + depth);
            world.TrySwitch();
            if (world.IsAltered)
            {
                Fail("Shift into alternate wall was allowed");
                yield break;
            }

            player.Teleport(new Vector3(0f, 0.08f, 3f));
            world.TrySwitch();
            if (!world.IsAltered)
            {
                Fail("Safe ground shift was rejected");
                yield break;
            }

            player.Teleport(new Vector3(0f, 3f, 18f));
            world.TrySwitch();
            if (world.IsAltered)
            {
                Fail("Safe midair shift was rejected");
                yield break;
            }

            Vector3 checkpoint = new Vector3(0f, 0.2f, 30f);
            flow.SetCheckpoint(checkpoint);
            player.Teleport(new Vector3(0f, -20f, 30f));
            flow.Respawn();
            if (Vector3.Distance(player.transform.position, checkpoint) > 0.02f)
            {
                Fail("Checkpoint respawn returned the wrong position");
                yield break;
            }

            flow.Complete();
            if (flow.IsPlaying)
            {
                Fail("Goal did not end active play");
                yield break;
            }

            Debug.Log("SHIFTBOUND SMOKE PASSED: blocked shift, safe ground shift, midair shift, checkpoint respawn, goal.");
            Application.Quit(0);
        }

        private static void Fail(string message)
        {
            Debug.LogError("SHIFTBOUND SMOKE FAILED: " + message);
            Application.Quit(1);
        }
    }
}

