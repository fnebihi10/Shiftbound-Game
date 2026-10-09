using System;

namespace Shiftbound
{
    // Input memory only. No physics or Unity clock: the motor owns support and takeoff.
    public sealed class JumpIntent
    {
        private float coyoteLeft;
        private float bufferLeft;
        private bool supported;
        private bool pressedThisStep;
        public bool Held { get; private set; }

        public void Advance(float dt, bool supported, bool pressed, bool released,
            float coyoteTime, float bufferTime)
        {
            this.supported = supported;
            pressedThisStep = pressed;
            coyoteLeft = supported ? coyoteTime : Math.Max(0f, coyoteLeft - dt);
            bufferLeft = Math.Max(0f, bufferLeft - dt);
            if (pressed) { bufferLeft = bufferTime; Held = true; }
            if (released) Held = false;
        }

        public void SynchronizeHeld(bool held) { Held = held; }

        public bool TryConsume(out bool held)
        {
            held = Held;
            if ((!pressedThisStep && bufferLeft <= 0f) || (!supported && coyoteLeft <= 0f)) return false;
            bufferLeft = 0f;
            coyoteLeft = 0f;
            pressedThisStep = false;
            supported = false;
            return true;
        }

        public void Reset() { coyoteLeft = 0f; bufferLeft = 0f; Held = false; supported = false; pressedThisStep = false; }
    }
}
