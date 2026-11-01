using BelowTheBlue.Input;
using UnityEngine;

namespace BelowTheBlue.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SubmarineController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [SerializeField, Range(0f, 90f)] private float maxTiltAngle = 30f;
        [SerializeField, Min(0f)] private float turnSpeed = 180f;
        [SerializeField, Min(0f)] private float knockbackDecay = 20f;

        private Rigidbody2D body;
        private SpriteRenderer targetRenderer;
        private int lastHorizontalDirection = -1;
        private Vector2 knockbackVelocity;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            targetRenderer = GetComponent<SpriteRenderer>();
            input ??= GetComponent<PlayerInputReader>();
            body.SetRotation(0f);
        }

        private void FixedUpdate()
        {
            Vector2 move = input == null ? Vector2.zero : input.Current.Move;
            body.linearVelocity = move * moveSpeed + knockbackVelocity;
            knockbackVelocity = Vector2.MoveTowards(
                knockbackVelocity,
                Vector2.zero,
                knockbackDecay * Time.fixedDeltaTime);

            if (move.sqrMagnitude > 0.0001f)
            {
                int previousHorizontalDirection = lastHorizontalDirection;

                if (move.x > 0f)
                {
                    lastHorizontalDirection = 1;
                }
                else if (move.x < 0f)
                {
                    lastHorizontalDirection = -1;
                }

                if (targetRenderer != null)
                {
                    targetRenderer.flipX = lastHorizontalDirection > 0;
                }

                float facingSign = lastHorizontalDirection > 0 ? 1f : -1f;
                float targetAngle = move.y * maxTiltAngle * facingSign;

                if (lastHorizontalDirection != previousHorizontalDirection)
                {
                    // A left/right change is a mirror operation, not a long turn.
                    body.SetRotation(targetAngle);
                }
                else
                {
                    body.SetRotation(Mathf.MoveTowardsAngle(
                        body.rotation,
                        targetAngle,
                        turnSpeed * Time.fixedDeltaTime));
                }
            }
        }

        public void ApplyKnockback(Vector2 velocity)
        {
            knockbackVelocity = velocity;
        }

        private void OnDisable()
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }
    }
}
