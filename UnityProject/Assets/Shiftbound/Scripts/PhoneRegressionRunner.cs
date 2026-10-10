using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Shiftbound
{
    // Real Input System touch records -> production polling/input/motor/camera.
    // Synthetic contacts cannot establish physical comfort or Android behavior.
    public sealed class PhoneRegressionRunner : MonoBehaviour
    {
        private Touchscreen screen;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundPhoneRegression") >= 0)
                new GameObject("Phone input regression").AddComponent<PhoneRegressionRunner>();
        }
        private void Send(int id, TouchPhase phase, Vector2 ui)
        {
            var phone = GameFlow.Instance.input.Phone;
            Vector2 point = new Vector2(Screen.safeArea.x + ui.x * phone.Scale, Screen.safeArea.yMax - ui.y * phone.Scale);
            InputSystem.QueueStateEvent(screen, new TouchState { touchId = id, phase = phase, position = point, pressure = 1f });
        }
        private IEnumerator Start()
        {
            Application.targetFrameRate = 60;
            yield return null;
            var flow = GameFlow.Instance;
            var motor = flow.player;
            var input = flow.input;
            var phone = input.Phone;
            var camera = flow.cameraRig;
            screen = InputSystem.AddDevice<Touchscreen>();
            phone.Layout();
            motor.Teleport(new Vector3(0f, .08f, 1f));
            yield return null; yield return null;
            Vector3 before = motor.transform.position;
            Send(1, TouchPhase.Began, phone.Router.Stick.center + Vector2.down * 65f);
            Send(2, TouchPhase.Began, phone.Router.Jump.center);
            yield return null; yield return null;
            if (!Check(input.Move.y > .9f && input.JumpHeld && !motor.IsGrounded && motor.VerticalVelocity > 0f, "real simultaneous move/held Jump reached motor")) yield break;
            bool oldWorld = flow.worlds.IsAltered;
            Send(3, TouchPhase.Began, phone.Router.Shift.center);
            yield return null; yield return null;
            if (!Check(flow.worlds.IsAltered != oldWorld && input.JumpHeld && motor.HorizontalVelocity.sqrMagnitude > .1f,
                "touch Shift during held movement/jump preserved motion")) yield break;
            float orbit = camera.OrbitYaw;
            Send(2, TouchPhase.Moved, new Vector2(phone.Width * .65f, 320f));
            yield return null; yield return null;
            if (!Check(Mathf.Abs(Mathf.DeltaAngle(orbit, camera.OrbitYaw)) < .1f, "button drag cannot rotate camera")) yield break;
            Send(2, TouchPhase.Canceled, phone.Router.Jump.center);
            Send(3, TouchPhase.Ended, phone.Router.Shift.center);
            yield return null; yield return null;
            if (!Check(!input.JumpHeld && input.Move.y > .9f, "cancelled Jump releases independently")) yield break;
            Send(4, TouchPhase.Began, new Vector2(phone.Width * .7f, 300f));
            yield return null;
            Send(4, TouchPhase.Moved, new Vector2(phone.Width * .7f + 90f, 315f));
            yield return null; yield return null;
            if (!Check(Mathf.Abs(Mathf.DeltaAngle(orbit, camera.OrbitYaw)) > 5f, "manual touch orbit while moving")) yield break;
            Vector3 suspended = motor.transform.position;
            flow.Suspend();
            yield return null; yield return null;
            if (!Check(flow.IsPaused && input.Move == Vector2.zero && !input.JumpHeld && motor.transform.position == suspended,
                "interruption pauses and clears contacts")) yield break;
            flow.TogglePause();
            yield return null; yield return null;
            if (!Check(input.Move == Vector2.zero && !input.JumpHeld && !input.ShiftPressed, "old held contacts cannot act after resume")) yield break;
            Send(1, TouchPhase.Ended, phone.Router.Stick.center);
            Send(4, TouchPhase.Ended, new Vector2(phone.Width * .7f, 300f));
            yield return null; yield return null; yield return null;
            flow.Respawn();
            yield return null; yield return null;
            if (!Check(motor.IsGrounded && motor.HorizontalVelocity.sqrMagnitude < .001f && !input.JumpHeld,
                "retry resets motor/touch without buffered jump")) yield break;
            Send(8, TouchPhase.Began, phone.Router.Jump.center);
            yield return null; yield return null;
            if (!Check(motor.VerticalVelocity > 0f, "new contact works after neutral resume/retry")) yield break;
            oldWorld = flow.worlds.IsAltered;
            Send(9, TouchPhase.Began, phone.Router.Stick.center + Vector2.down * 65f);
            Send(8, TouchPhase.Moved, phone.Router.Shift.center);
            yield return null; yield return null;
            if (!Check(flow.worlds.IsAltered != oldWorld && input.JumpHeld && input.Move.y>.9f,
                "two-thumb slide Shift preserves held Jump and movement")) yield break;
            Send(9, TouchPhase.Ended, phone.Router.Stick.center);
            Send(8, TouchPhase.Ended, phone.Router.Jump.center);
            yield return null;yield return null;yield return null;
            flow.TogglePause();yield return null;yield return null;
            int saves=PlayerPreferences.SaveCount;float oldSize=PlayerPreferences.ControlScale,oldInset=PlayerPreferences.ControlInset;
            // A stationary text contact owns the menu. A second contact must
            // neither acquire a slider nor trigger repeated preference writes.
            Vector2 textPoint=new Vector2(phone.MenuRect(2).x+25,phone.MenuRect(2).center.y);
            Send(20,TouchPhase.Began,textPoint);yield return null;
            Send(21,TouchPhase.Began,new Vector2(phone.MenuRect(3).xMax-10,phone.MenuRect(3).center.y));
            for(int i=0;i<75;i++)yield return null;
            if(!Check(PlayerPreferences.ControlScale==oldSize&&PlayerPreferences.ControlInset==oldInset&&PlayerPreferences.SaveCount==saves,
                "stationary menu and secondary contact cause zero preference writes"))yield break;
            Send(20,TouchPhase.Ended,textPoint);Send(21,TouchPhase.Ended,phone.MenuRect(3).center);
            yield return null;yield return null;
            Vector2 track=new Vector2(phone.MenuRect(2).x+375,phone.MenuRect(2).center.y);
            Send(22,TouchPhase.Began,track);yield return null;yield return null;
            for(int i=0;i<60;i++)yield return null;
            if(!Check(PlayerPreferences.ControlScale!=oldSize&&PlayerPreferences.SaveCount==saves,"slider adjusts without per-frame disk writes"))yield break;
            Send(22,TouchPhase.Canceled,track);yield return null;yield return null;
            if(!Check(PlayerPreferences.SaveCount==saves+1,"cancelled owned slider commits once"))yield break;
            phone.Cancel();PlayerPreferences.Save();
            if(!Check(PlayerPreferences.SaveCount==saves+1,"unchanged interruption persistence is idempotent"))yield break;
            PlayerPreferences.ControlScale=oldSize;PlayerPreferences.ControlInset=oldInset;PlayerPreferences.Save();
            flow.Complete();yield return null;yield return null;yield return null;
            saves=PlayerPreferences.SaveCount;
            Send(23,TouchPhase.Began,track);for(int i=0;i<20;i++)yield return null;
            Send(23,TouchPhase.Ended,track);yield return null;yield return null;
            if(!Check(PlayerPreferences.ControlScale==oldSize&&PlayerPreferences.SaveCount==saves,"completion message rows cannot change settings"))yield break;
            Debug.Log("SHIFTBOUND PHONE REGRESSION PASSED: synthetic Touchscreen events through production touch/input/motor/camera; simultaneous actions, UI ownership, cancellation, suspend/resume/retry. Physical Android gate remains open.");
            InputSystem.RemoveDevice(screen); screen = null;
            Application.Quit(0);
        }
        bool Check(bool pass, string label)
        {
            if (pass) { Debug.Log("SHIFTBOUND PHONE CASE PASS: " + label); return true; }
            Debug.LogError("SHIFTBOUND PHONE REGRESSION FAILED: " + label); Application.Quit(1); return false;
        }
        private void OnDestroy() { if (screen != null) InputSystem.RemoveDevice(screen); }
    }
}
