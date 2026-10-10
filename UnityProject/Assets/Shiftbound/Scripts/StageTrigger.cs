using UnityEngine;

namespace Shiftbound
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class StageTrigger : MonoBehaviour
    {
        public enum TriggerKind { Checkpoint, Goal }
        public TriggerKind kind;
        public bool hasCheckpointPosition;
        public Vector3 checkpointPosition;
        public float checkpointFacingYaw;
        [TextArea] public string checkpointHint = "";
        [Tooltip("Optional shared landing ending the first Overgrown bridge lesson.")]
        public Transform shiftBridgeLessonExit;
        public bool HasFired => fired;
        private bool fired;
        public void ResetForRun() => fired=false;

        private void OnTriggerEnter(Collider other)
        {
            TryActivate(other);
        }

        private void OnTriggerStay(Collider other)
        {
            // Arrival may enter the volume in the air. Commit only once supported.
            TryActivate(other);
        }

        private void TryActivate(Collider other)
        {
            PlayerMotor motor = other.GetComponent<PlayerMotor>();
            if (fired || motor == null || !motor.IsGrounded ||
                GameFlow.Instance == null || !GameFlow.Instance.IsPlaying) return;
            fired = true;
            if (kind == TriggerKind.Checkpoint)
                GameFlow.Instance?.SetCheckpoint(hasCheckpointPosition
                    ? checkpointPosition
                    : new Vector3(transform.position.x, 0.2f, transform.position.z), checkpointFacingYaw, checkpointHint,
                    shiftBridgeLessonExit);
            else GameFlow.Instance?.Complete();
        }
    }
}
