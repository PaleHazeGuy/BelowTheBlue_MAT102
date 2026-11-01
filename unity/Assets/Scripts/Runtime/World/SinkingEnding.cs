using UnityEngine;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    public sealed class SinkingEnding : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float duration = 2.4f;
        [SerializeField, Range(1, 5)] private int minimumSpiralTurns = 2;
        [SerializeField, Range(1, 6)] private int maximumSpiralTurns = 3;
        [SerializeField, Min(0f)] private float finalScale = 0f;
        [SerializeField, Min(0.1f)] private float cameraZoomSize = 1.8f;
        [SerializeField, Min(0f)] private float cameraFollowSpeed = 6f;

        private Vector3 startPosition;
        private Vector3 startScale;
        private Quaternion startRotation;
        private Vector3 sinkCenter;
        private float startAngle;
        private float spiralTurns;
        private float elapsed;
        private bool isSinking;
        private Camera endingCamera;
        private Vector3 cameraOffset;
        private float startingCameraSize;
        private Vector2 orbitRadii;
        private float startingOrbitScale;

        public void Begin(Vector3 center, Vector2 targetOrbitRadii)
        {
            if (isSinking)
            {
                return;
            }

            isSinking = true;
            sinkCenter = center;
            startPosition = transform.position;
            startScale = transform.localScale;
            startRotation = transform.rotation;
            Vector2 offset = startPosition - sinkCenter;
            orbitRadii = new Vector2(
                Mathf.Max(0.01f, targetOrbitRadii.x),
                Mathf.Max(0.01f, targetOrbitRadii.y));
            startAngle = Mathf.Atan2(offset.y / orbitRadii.y, offset.x / orbitRadii.x);
            startingOrbitScale = Mathf.Max(1f, Mathf.Sqrt(
                Mathf.Pow(offset.x / orbitRadii.x, 2f)
                + Mathf.Pow(offset.y / orbitRadii.y, 2f)));
            spiralTurns = Random.Range(minimumSpiralTurns, maximumSpiralTurns + 1);
            endingCamera = Camera.main;
            if (endingCamera != null)
            {
                var cameraFollow = endingCamera.GetComponent<CameraFollow>();
                if (cameraFollow != null)
                {
                    cameraFollow.enabled = false;
                }

                cameraOffset = endingCamera.transform.position - startPosition;
                if (endingCamera.orthographic)
                {
                    startingCameraSize = endingCamera.orthographicSize;
                }
            }

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

            foreach (Collider2D playerCollider in GetComponents<Collider2D>())
            {
                playerCollider.enabled = false;
            }
        }

        private void Update()
        {
            if (!isSinking)
            {
                return;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);
            float orbitScale = startingOrbitScale * (1f - eased);
            float angle = startAngle + spiralTurns * Mathf.PI * 2f * eased;

            transform.position = sinkCenter + new Vector3(
                Mathf.Cos(angle) * orbitRadii.x * orbitScale,
                Mathf.Sin(angle) * orbitRadii.y * orbitScale,
                0f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.one * finalScale, eased);
            transform.rotation = startRotation;

            if (endingCamera != null)
            {
                Vector3 cameraTarget = transform.position + cameraOffset;
                float follow = 1f - Mathf.Exp(-cameraFollowSpeed * Time.deltaTime);
                endingCamera.transform.position = Vector3.Lerp(
                    endingCamera.transform.position,
                    cameraTarget,
                    follow);

                if (endingCamera.orthographic)
                {
                    endingCamera.orthographicSize = Mathf.Lerp(startingCameraSize, cameraZoomSize, eased);
                }
            }

            if (t >= 1f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
