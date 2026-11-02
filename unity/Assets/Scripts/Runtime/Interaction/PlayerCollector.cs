using System;
using BelowTheBlue.Input;
using UnityEngine;

namespace BelowTheBlue.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private LayerMask collectibleLayers = Physics2D.DefaultRaycastLayers;
        [Tooltip("Optional. When set, the arm reaches out and pulls the item in; otherwise it is collected instantly.")]
        [SerializeField] private CollectorArm arm;

        [Header("Front collect hitbox (world units)")]
        [Tooltip("Sprite whose flipX tells which way the sub faces. The art faces left when flipX is off.")]
        [SerializeField] private SpriteRenderer facingRenderer;
        [Tooltip("Distance from the sub's centre to the hitbox centre, toward the nose.")]
        [SerializeField] private float forwardOffset = 2.1f;
        [Tooltip("Shift of the hitbox along the sub's up axis (the hull sits below the sprite centre).")]
        [SerializeField] private float verticalOffset = -0.7f;
        [SerializeField] private Vector2 size = new Vector2(2.4f, 2.4f);
        [SerializeField] private bool showHitboxGizmo = true;

        private readonly Collider2D[] nearby = new Collider2D[16];
        private Collectible highlightedCollectible;

        public Collectible Target => highlightedCollectible;

        public event Action<Collectible> TargetChanged;

        private void Awake()
        {
            input ??= GetComponent<PlayerInputReader>();
            facingRenderer ??= GetComponent<SpriteRenderer>();
            arm ??= GetComponent<CollectorArm>();
        }

        private void Update()
        {
            Collectible closest = FindClosestCollectible();
            if (closest != highlightedCollectible)
            {
                highlightedCollectible?.SetHighlighted(false);
                highlightedCollectible = closest;
                highlightedCollectible?.SetHighlighted(true);
                TargetChanged?.Invoke(highlightedCollectible);
            }

            if (input != null && input.InteractPressedThisFrame && (arm == null || !arm.IsBusy))
            {
                if (closest != null)
                {
                    if (arm != null)
                    {
                        arm.TryGrab(closest);
                    }
                    else
                    {
                        closest.Collect(transform);
                    }
                }

                if (highlightedCollectible != null)
                {
                    highlightedCollectible = null;
                    TargetChanged?.Invoke(null);
                }
            }
        }

        private Collectible FindClosestCollectible()
        {
            GetHitbox(out Vector2 center, out float angle);
            int count = Physics2D.OverlapBox(center, size, angle, new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = collectibleLayers,
                useTriggers = true
            }, nearby);

            Vector2 right = transform.right;
            Vector2 up = transform.up;
            Collectible closest = null;
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = nearby[i];
                Collectible candidate = hit != null ? hit.GetComponentInParent<Collectible>() : null;
                if (candidate == null)
                {
                    continue;
                }

                Vector2 offset = (Vector2)hit.bounds.center - center;
                if (Mathf.Abs(Vector2.Dot(offset, right)) > size.x * 0.5f
                    || Mathf.Abs(Vector2.Dot(offset, up)) > size.y * 0.5f)
                {
                    continue;
                }

                float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        private void GetHitbox(out Vector2 center, out float angleDegrees)
        {
            float side = facingRenderer != null && facingRenderer.flipX ? 1f : -1f;
            center = (Vector2)transform.position
                + (Vector2)transform.right * (side * forwardOffset)
                + (Vector2)transform.up * verticalOffset;
            angleDegrees = transform.eulerAngles.z;
        }

        private void OnDisable()
        {
            highlightedCollectible?.SetHighlighted(false);
            if (highlightedCollectible != null)
            {
                highlightedCollectible = null;
                TargetChanged?.Invoke(null);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showHitboxGizmo)
            {
                return;
            }

            facingRenderer ??= GetComponent<SpriteRenderer>();
            GetHitbox(out Vector2 center, out float angle);
            Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
            Gizmos.color = new Color(0.3f, 1f, 1f, 0.15f);
            Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, size.y, 0.05f));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0.05f));
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
