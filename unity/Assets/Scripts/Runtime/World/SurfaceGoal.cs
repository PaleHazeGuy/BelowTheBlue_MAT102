using BelowTheBlue.Core;
using UnityEngine;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    public sealed class SurfaceGoal : MonoBehaviour
    {
        [SerializeField] private RunState runState;

        private void Awake()
        {
            foreach (Collider2D goalCollider in GetComponents<Collider2D>())
            {
                goalCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (runState != null && other.GetComponentInParent<Health>() != null)
            {
                runState.ReachSurface();
            }
        }
    }
}
