using UnityEngine;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    public sealed class FishSwim : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float swimDistance = 3f;
        [SerializeField, Min(0f)] private float swimSpeed = 1f;
        [SerializeField, Min(0f)] private float bobAmount = 0.15f;
        [SerializeField, Min(0f)] private float bobSpeed = 1.5f;
        [SerializeField] private bool artFacesLeft = true;

        private SpriteRenderer targetRenderer;
        private Vector3 origin;
        private float phase;
        private int direction = 1;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            origin = transform.position;
            phase = Random.value * Mathf.PI * 2f;
            direction = Random.value < 0.5f ? 1 : -1;
            ApplyFacing();
        }

        private void Update()
        {
            Vector3 position = transform.position;
            position.x += direction * swimSpeed * Time.deltaTime;
            float offset = position.x - origin.x;
            if (offset > swimDistance)
            {
                direction = -1;
                ApplyFacing();
            }
            else if (offset < -swimDistance)
            {
                direction = 1;
                ApplyFacing();
            }

            position.y = origin.y + Mathf.Sin(Time.time * bobSpeed + phase) * bobAmount;
            transform.position = position;
        }

        private void ApplyFacing()
        {
            if (targetRenderer != null)
            {
                targetRenderer.flipX = artFacesLeft ? direction > 0 : direction < 0;
            }
        }
    }
}
