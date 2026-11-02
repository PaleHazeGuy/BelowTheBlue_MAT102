using BelowTheBlue.Input;
using UnityEngine;

namespace BelowTheBlue.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SubmarineController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField, Min(0f)] private float moveSpeed = 4f;
        [SerializeField, Range(0f, 90f)] private float maxTiltAngle = 90f;
        [SerializeField, Min(0f)] private float turnSpeed = 180f;
        [SerializeField, Min(0f)] private float knockbackDecay = 20f;
        [SerializeField, Range(0f, 1f)] private float slowedSpeedFactor = 0.25f;
        [SerializeField, Min(0.05f)] private float speedRecoverSeconds = 1.5f;

        private Rigidbody2D body;
        private SpriteRenderer targetRenderer;
        private int lastHorizontalDirection = -1;
        private Vector2 knockbackVelocity;
        private float recoverElapsed;
        private bool recovering;

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
            body.linearVelocity = move * moveSpeed * CurrentSpeedScale() + knockbackVelocity;
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
                float currentAngle = body.rotation;
                if (lastHorizontalDirection != previousHorizontalDirection)
                {
                    currentAngle = -currentAngle;
                }

                float pitch = Mathf.Abs(move.y) > 0.1f
                    ? Mathf.Atan2(move.y, Mathf.Abs(move.x)) * Mathf.Rad2Deg
                    : 0f;
                float targetAngle = Mathf.Clamp(pitch, -maxTiltAngle, maxTiltAngle) * facingSign;
                currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);

                body.SetRotation(currentAngle);
            }
        }

        public void ApplyKnockback(Vector2 velocity)
        {
            knockbackVelocity = velocity;
            recoverElapsed = 0f;
            recovering = true;
        }

        private float CurrentSpeedScale()
        {
            if (!recovering)
            {
                return 1f;
            }

            recoverElapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(recoverElapsed / speedRecoverSeconds);
            if (t >= 1f)
            {
                recovering = false;
                return 1f;
            }

            return Mathf.Lerp(slowedSpeedFactor, 1f, t * t * (3f - 2f * t));
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
