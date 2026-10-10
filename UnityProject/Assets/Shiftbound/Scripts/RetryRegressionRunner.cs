using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Shiftbound
{
    public sealed class RetryRegressionRunner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundRetryRegression")>=0)
                new GameObject("Instant retry regression").AddComponent<RetryRegressionRunner>();
        }
        IEnumerator Start()
        {
            yield return null;yield return null;
            var flow=GameFlow.Instance;var motor=flow.player;
            int scene=gameObject.scene.handle,playerId=motor.GetInstanceID();
            Vector3 start=flow.CheckpointPosition;
            float volume=PlayerPreferences.Volume,scale=PlayerPreferences.ControlScale;
            var checkpoint=Array.Find(FindObjectsByType<StageTrigger>(FindObjectsSortMode.None),t=>t.kind==StageTrigger.TriggerKind.Checkpoint&&t.hasCheckpointPosition);
            var keyboard=InputSystem.AddDevice<Keyboard>();
            for(int cycle=0;cycle<4;cycle++)
            {
                motor.Teleport(checkpoint.checkpointPosition);motor.Step(1f/60,Vector2.zero,false,false);
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return null;
                if(!checkpoint.HasFired||flow.CheckpointSequence==0){Fail("Actual checkpoint did not activate");yield break;}
                while(!flow.worlds.IsReady)yield return null;
                if(!flow.worlds.IsAltered&&flow.worlds.TrySwitch()!=WorldSwitcher.ShiftResult.Accepted){Fail("Setup Shift rejected");yield break;}
                if(cycle%2==0)flow.Complete();else flow.TogglePause();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space,Key.LeftShift));
                yield return null;
                flow.Restart();yield return null;yield return null;
                if(gameObject.scene.handle!=scene||flow.player.GetInstanceID()!=playerId||!flow.IsPlaying||flow.IsComplete||flow.IsPaused||
                    flow.CheckpointSequence!=0||checkpoint.HasFired||flow.worlds.IsAltered||
                    Vector3.Distance(motor.transform.position,start)>.15f||flow.Elapsed>.2f||
                    motor.HorizontalVelocity.sqrMagnitude>.01f||flow.input.JumpHeld||flow.input.ShiftPressed||
                    PlayerPreferences.Volume!=volume||PlayerPreferences.ControlScale!=scale||Time.timeScale!=1||AudioListener.pause)
                {Fail("Retry state, neutral input, scene retention or settings failed cycle="+cycle);yield break;}
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return null;yield return null;
                if(motor.IsGrounded||motor.VerticalVelocity<=0){Fail("Fresh Jump did not respond after retry");yield break;}
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                motor.Teleport(start);yield return null;
                Debug.Log("SHIFTBOUND RETRY CYCLE PASSED: "+cycle+" actual checkpoint; pause/completion; held-input suppression; fresh jump; retained scene/settings");
            }
            InputSystem.RemoveDevice(keyboard);
            Debug.Log("SHIFTBOUND RETRY REGRESSION PASSED");Application.Quit(0);
        }
        static void Fail(string message){Debug.LogError("SHIFTBOUND RETRY REGRESSION FAILED: "+message);Application.Quit(1);}
    }
}
