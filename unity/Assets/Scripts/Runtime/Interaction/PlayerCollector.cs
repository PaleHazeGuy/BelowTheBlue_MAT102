using BelowTheBlue.Input;
using UnityEngine;

namespace BelowTheBlue.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField, Min(0f)] private float range = 1.5f;
        [SerializeField] private LayerMask collectibleLayers = Physics2D.DefaultRaycastLayers;

        private readonly Collider2D[] nearby = new Collider2D[16];

        private void Awake()
        {
            input ??= GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            if (input == null || !input.InteractPressedThisFrame)
            {
                return;
            }

            int count = Physics2D.OverlapCircle(transform.position, range, new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = collectibleLayers,
                useTriggers = true
            }, nearby);

            Collectible closest = null;
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collectible candidate = nearby[i] != null
                    ? nearby[i].GetComponentInParent<Collectible>()
                    : null;
                if (candidate == null)
                {
                    continue;
                }

                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            closest?.Collect();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
