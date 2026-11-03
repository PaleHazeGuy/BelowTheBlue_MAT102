using BelowTheBlue.Core;
using UnityEngine;

namespace BelowTheBlue.World
{
    public enum ObstacleDamageMode
    {
        Damage,
        InstantKill
    }

    [DisallowMultipleComponent]
    public sealed class ContactDamage : MonoBehaviour
    {
        [SerializeField] private ObstacleDamageMode mode = ObstacleDamageMode.Damage;
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0f)] private float knockbackSpeed = 8f;

        public ObstacleDamageMode Mode => mode;

        private void Awake()
        {
            foreach (Collider2D obstacleCollider in GetComponents<Collider2D>())
            {
                obstacleCollider.isTrigger = true;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Apply(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Apply(other);
        }

        private void Apply(Collider2D other)
        {
            Health health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                if (mode == ObstacleDamageMode.InstantKill)
                {
                    SpriteRenderer vortexRenderer = GetComponent<SpriteRenderer>();
                    Vector3 sinkCenter = vortexRenderer != null
                        ? vortexRenderer.bounds.center
                        : transform.position;
                    Vector2 orbitRadii = vortexRenderer != null
                        ? new Vector2(vortexRenderer.bounds.extents.x, vortexRenderer.bounds.extents.y) * 0.8f
                        : Vector2.zero;
                    health.Kill(sinkCenter, orbitRadii);
                    return;
                }

                health.TakeDamage(damage);
                if (!health.IsAlive)
                {
                    return;
                }

                Vector2 away = (health.transform.position - transform.position).normalized;
                if (away.sqrMagnitude < 0.001f)
                {
                    away = Vector2.up;
                }

                var controller = health.GetComponent<BelowTheBlue.Player.SubmarineController>();
                if (controller != null)
                {
                    controller.ApplyKnockback(away * knockbackSpeed);
                }
            }
        }
    }
}
