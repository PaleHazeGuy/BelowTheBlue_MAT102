using System;
using System.Collections;
using UnityEngine;

namespace BelowTheBlue.Interaction
{
    [DisallowMultipleComponent]
    public sealed class Collectible : MonoBehaviour
    {
        public static event Action<Collectible> Collected;

        [SerializeField] private string itemId = "sample";
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color highlightColor = new Color(0.35f, 0.8f, 1f, 1f);
        [SerializeField, Min(0.05f)] private float pickupDuration = 0.45f;
        [SerializeField, Range(0.5f, 2f)] private float spiralTurns = 1f;

        public string ItemId => itemId;
        public SpriteRenderer Renderer => targetRenderer;
        private Color normalColor = Color.white;
        private bool isCollecting;

        private void Awake()
        {
            targetRenderer ??= GetComponentInChildren<SpriteRenderer>();
            if (targetRenderer != null)
            {
                normalColor = targetRenderer.color;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (targetRenderer != null)
            {
                targetRenderer.color = highlighted ? highlightColor : normalColor;
            }
        }

        public bool BeginCapture()
        {
            if (isCollecting)
            {
                return false;
            }

            isCollecting = true;
            SetHighlighted(false);
            foreach (Collider2D itemCollider in GetComponentsInChildren<Collider2D>())
            {
                itemCollider.enabled = false;
            }

            return true;
        }

        public void CompleteCapture()
        {
            Collected?.Invoke(this);
            Destroy(gameObject);
        }

        public void Collect(Transform collector)
        {
            if (isCollecting || collector == null)
            {
                return;
            }

            isCollecting = true;
            foreach (Collider2D itemCollider in GetComponentsInChildren<Collider2D>())
            {
                itemCollider.enabled = false;
            }

            Collected?.Invoke(this);
            StartCoroutine(AnimateIntoSubmarine(collector));
        }

        private IEnumerator AnimateIntoSubmarine(Transform collector)
        {
            Vector3 startPosition = transform.position;
            Vector3 startScale = transform.localScale;
            Vector3 startOffset = startPosition - collector.position;
            float startAngle = Mathf.Atan2(startOffset.y, startOffset.x);
            float startRadius = startOffset.magnitude;
            float elapsed = 0f;

            while (elapsed < pickupDuration && collector != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / pickupDuration);
                float eased = t * t * (3f - 2f * t);
                float radius = startRadius * (1f - eased);
                float angle = startAngle + spiralTurns * Mathf.PI * 2f * eased;

                transform.position = collector.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    startPosition.z - collector.position.z);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, eased);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
