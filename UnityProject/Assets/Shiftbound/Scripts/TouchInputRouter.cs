using System.Collections.Generic;
using UnityEngine;

namespace Shiftbound
{
    // Own contacts from begin to end; UI never becomes camera input. The explicit
    // Jump-to-Shift slide chord retains held Jump and fires once in Shift's core.
    public sealed class TouchInputRouter
    {
        private enum Role { Ignored, Move, Jump, Shift, Camera }
        private readonly Dictionary<int, Role> owners = new Dictionary<int, Role>();
        private int moveId = -1, jumpId = -1, shiftId = -1, cameraId = -1;
        private bool jumpSlideUsed;
        private Vector2 cameraPrevious;
        public Rect Stick, Jump, Shift, Camera;
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool ShiftPressed { get; private set; }
        public int ContactCount => owners.Count;
        public void BeginFrame() { Look = Vector2.zero; JumpPressed = JumpReleased = ShiftPressed = false; }
        public void Begin(int id, Vector2 p)
        {
            if (owners.ContainsKey(id)) return;
            Role role = Role.Ignored;
            if (Jump.Contains(p))
            {
                if (jumpId < 0) { role = Role.Jump; jumpId = id; JumpHeld = JumpPressed = true; jumpSlideUsed = false; }
            }
            else if (Shift.Contains(p)) { if (shiftId < 0) { role = Role.Shift; shiftId = id; ShiftPressed = true; } }
            else if (Stick.Contains(p)) { if (moveId < 0) { role = Role.Move; moveId = id; } }
            else if (Camera.Contains(p) && cameraId < 0) { role = Role.Camera; cameraId = id; cameraPrevious = p; }
            owners.Add(id, role);
            Drag(id, p);
        }
        public void Drag(int id, Vector2 p)
        {
            if (!owners.TryGetValue(id, out Role role)) return;
            if (role == Role.Move)
            {
                Vector2 axes = (p - Stick.center) / (Stick.width * .32f);
                axes.y = -axes.y;
                float length = axes.magnitude;
                Move = length <= .12f ? Vector2.zero : axes.normalized * Mathf.Clamp01((length - .12f) / .88f);
            }
            if (role == Role.Camera) { Look += p - cameraPrevious; cameraPrevious = p; }
            Rect core = new Rect(Shift.x + Shift.width*.18f, Shift.y + Shift.height*.18f, Shift.width*.64f, Shift.height*.64f);
            if (role == Role.Jump && !jumpSlideUsed && shiftId < 0 && core.Contains(p))
            { ShiftPressed = true; jumpSlideUsed = true; }
        }
        public void End(int id)
        {
            if (!owners.TryGetValue(id, out Role role)) return;
            if (role == Role.Move) { moveId = -1; Move = Vector2.zero; }
            if (role == Role.Jump) { jumpId = -1; JumpHeld = false; JumpReleased = true; }
            if (role == Role.Camera) cameraId = -1;
            if (role == Role.Shift) shiftId = -1;
            owners.Remove(id);
        }
        public void Reset()
        {
            owners.Clear(); moveId = jumpId = shiftId = cameraId = -1; jumpSlideUsed = false;
            Move = Look = Vector2.zero;
            JumpHeld = JumpPressed = JumpReleased = ShiftPressed = false;
        }
    }
}
