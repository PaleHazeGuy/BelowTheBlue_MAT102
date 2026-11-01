using System;
using System.Collections;
using UnityEngine;

namespace BelowTheBlue.Core
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1)] private int maximum = 3;
        [SerializeField] private SpriteRenderer submarineRenderer;
        [SerializeField] private Sprite hitSprite;
        [SerializeField] private Sprite deadSprite;
        [SerializeField] private Color damageTint = new Color(1f, 0.25f, 0.25f, 1f);
        [SerializeField, Min(0f)] private float damageTintDuration = 0.18f;

        public int Maximum => maximum;
        public int Current { get; private set; }
        public bool IsAlive => Current > 0;

        public event Action<int, int> Changed;
        public event Action Died;
        private Color normalColor = Color.white;
        private Coroutine tintRoutine;
        private void Awake()
        {
            Current = maximum;
            ResolveSubmarineRenderer();
            if (submarineRenderer != null)
            {
                normalColor = submarineRenderer.color;
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || amount <= 0)
            {
                return;
            }

            Current = Mathf.Max(0, Current - amount);
            Changed?.Invoke(Current, maximum);
            ResolveSubmarineRenderer();
            if (submarineRenderer != null)
            {
                if (Current > 0)
                {
                    if (tintRoutine != null)
                    {
                        StopCoroutine(tintRoutine);
                    }

                    tintRoutine = StartCoroutine(ShowDamageTint());
                }

                if (Current == 0 && deadSprite != null)
                {
                    submarineRenderer.sprite = deadSprite;
                }
                else if (Current == 1 && hitSprite != null)
                {
                    submarineRenderer.sprite = hitSprite;
                }
            }

            if (Current == 0)
            {
                Died?.Invoke();
                var controller = GetComponent<BelowTheBlue.Player.SubmarineController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }
                var collector = GetComponent<BelowTheBlue.Interaction.PlayerCollector>();
                if (collector != null)
                {
                    collector.enabled = false;
                }

                var body = GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    body.linearVelocity = Vector2.zero;
                    body.simulated = false;
                }
            }
        }

        public void Kill(Vector3 sinkCenter, Vector2 orbitRadii)
        {
            if (IsAlive)
            {
                TakeDamage(Current);
                var ending = GetComponent<BelowTheBlue.World.SinkingEnding>();
                if (ending != null)
                {
                    ending.Begin(sinkCenter, orbitRadii);
                }
            }
        }

        private IEnumerator ShowDamageTint()
        {
            submarineRenderer.color = damageTint;
            yield return new WaitForSeconds(damageTintDuration);

            if (submarineRenderer != null && IsAlive)
            {
                submarineRenderer.color = normalColor;
            }

            tintRoutine = null;
        }

        private void ResolveSubmarineRenderer()
        {
            if (submarineRenderer == null)
            {
                submarineRenderer = GetComponent<SpriteRenderer>();
            }

            if (submarineRenderer == null)
            {
                submarineRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }

            if (submarineRenderer == null)
            {
                submarineRenderer = GetComponentInParent<SpriteRenderer>();
            }
        }
    }
}
