using BelowTheBlue.Core;
using UnityEngine;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    public sealed class PropellerBubbles : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer facingRenderer;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem bubbles;
        [SerializeField] private Vector2 tailOffset = new Vector2(8.2f, -3.4f);
        [SerializeField, Min(0f)] private float movingThreshold = 0.15f;
        [SerializeField, Min(0f)] private float movingRate = 22f;
        [SerializeField, Min(0f)] private float idleRate = 3f;

        private void Awake()
        {
            facingRenderer ??= GetComponent<SpriteRenderer>();
            body ??= GetComponent<Rigidbody2D>();
            health ??= GetComponent<Health>();
        }

        private void LateUpdate()
        {
            if (bubbles == null)
            {
                return;
            }

            bool flipped = facingRenderer != null && facingRenderer.flipX;
            float x = flipped ? -tailOffset.x : tailOffset.x;
            bubbles.transform.position = transform.TransformPoint(new Vector3(x, tailOffset.y, 0f));

            bool alive = health == null || health.IsAlive;
            bool moving = body != null && body.linearVelocity.sqrMagnitude > movingThreshold * movingThreshold;
            var emission = bubbles.emission;
            emission.rateOverTime = !alive ? 0f : (moving ? movingRate : idleRate);
        }
    }
}
