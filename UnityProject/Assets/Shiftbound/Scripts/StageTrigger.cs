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
        private bool fired;

        private void OnTriggerEnter(Collider other)
        {
            if (fired || other.GetComponent<PlayerMotor>() == null) return;
            fired = true;
            if (kind == TriggerKind.Checkpoint)
                GameFlow.Instance?.SetCheckpoint(hasCheckpointPosition
                    ? checkpointPosition
                    : new Vector3(transform.position.x, 0.2f, transform.position.z));
            else GameFlow.Instance?.Complete();
        }
    }
}
