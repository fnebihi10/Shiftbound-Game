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
            Renderer presentRenderer = world.presentRoot.GetComponentInChildren<Renderer>();
            Material[] originalSlots = presentRenderer.sharedMaterials;

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
            if (presentRenderer.sharedMaterials.Length != originalSlots.Length)
            {
                Fail("Inactive world lost a material slot");
                yield break;
            }

            player.Teleport(new Vector3(0f, 3f, 18f));
            world.TrySwitch();
            if (world.IsAltered)
            {
                Fail("Safe midair shift was rejected");
                yield break;
            }
            Material[] restoredSlots = presentRenderer.sharedMaterials;
            for (int slot = 0; slot < originalSlots.Length; slot++)
                if (restoredSlots[slot] != originalSlots[slot])
                {
                    Fail("World shift did not restore material slot " + slot);
                    yield break;
                }

            player.enabled = false;
            foreach (int fps in new[] { 30, 60, 120, 144 })
            {
                float dt = 1f / fps;
                player.Teleport(new Vector3(0f, 0.08f, 3f));
                player.Step(dt, Vector2.zero, false, false);
                if (!player.IsGrounded)
                {
                    Fail("Ground probe failed at " + fps + " FPS");
                    yield break;
                }
                player.Step(dt, Vector2.zero, true, false);
                if (player.VerticalVelocity <= 0f)
                {
                    Fail("Jump failed at " + fps + " FPS");
                    yield break;
                }
                for (int frame = 0; frame < 3; frame++)
                    player.Step(dt, Vector2.zero, false, false);
                float beforeRepeat = player.VerticalVelocity;
                player.Step(dt, Vector2.zero, true, false);
                if (player.VerticalVelocity >= beforeRepeat)
                {
                    Fail("Ascent refreshed jump at " + fps + " FPS");
                    yield break;
                }
                for (int frame = 0; frame < fps * 2 && !player.IsGrounded; frame++)
                    player.Step(dt, Vector2.zero, false, false);
                if (!player.IsGrounded)
                {
                    Fail("Jump did not land at " + fps + " FPS");
                    yield break;
                }
            }
            player.enabled = true;

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

            Debug.Log("SHIFTBOUND SMOKE PASSED: blocked shift, safe ground shift, midair shift, material slots, jumps at 30/60/120/144 steps, checkpoint respawn, goal.");
            Application.Quit(0);
        }

        private static void Fail(string message)
        {
            Debug.LogError("SHIFTBOUND SMOKE FAILED: " + message);
            Application.Quit(1);
        }
    }
}

