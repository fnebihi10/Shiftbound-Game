using UnityEngine;

namespace Shiftbound
{
    public sealed class CourierVisual : MonoBehaviour
    {
        public PlayerMotor motor;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform scarf;
        [Range(1f, 20f)] public float strideFrequency = 8f;
        [Range(0f, 60f)] public float strideAngle = 37f;
        private float phase;
        private Vector3 basePosition;

        private void Awake()
        {
            basePosition = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (motor == null) return;
            float speed = motor.HorizontalVelocity.magnitude;
            float weight = Mathf.Clamp01(speed / Mathf.Max(0.1f, motor.maxSpeed));
            phase += Time.deltaTime * strideFrequency * Mathf.Max(0.3f, weight);
            bool grounded = motor.Controller.isGrounded;
            float swing = Mathf.Sin(phase) * strideAngle * weight;
            if (!grounded) swing = 22f;
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-swing * 0.7f - 12f, 0f, -8f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(swing * 0.7f - 12f, 0f, 8f);
            if (scarf != null) scarf.localRotation = Quaternion.Euler(
                8f + Mathf.Sin(Time.time * 6f) * 7f, 0f, 0f);
            transform.localPosition = basePosition + Vector3.up *
                (grounded ? Mathf.Abs(Mathf.Sin(phase)) * 0.055f * weight : -0.06f);
        }
    }
}
