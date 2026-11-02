using UnityEngine;
using UnityEngine.Rendering;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraShake : MonoBehaviour
    {
        private Camera targetCamera;
        private float amplitude;
        private float duration;
        private float elapsed;
        private Vector3 appliedOffset;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBegin;
            RenderPipelineManager.endCameraRendering += OnEnd;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBegin;
            RenderPipelineManager.endCameraRendering -= OnEnd;
            Restore();
        }

        public void Shake(float strength, float seconds)
        {
            if (strength >= amplitude * (1f - Progress()))
            {
                amplitude = strength;
                duration = Mathf.Max(0.01f, seconds);
                elapsed = 0f;
            }
        }

        private float Progress()
        {
            return duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
        }

        private void Update()
        {
            if (duration > 0f)
            {
                elapsed += Time.unscaledDeltaTime;
                if (elapsed >= duration)
                {
                    amplitude = 0f;
                    duration = 0f;
                }
            }
        }

        private void OnBegin(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering != targetCamera || amplitude <= 0f)
            {
                return;
            }

            float falloff = 1f - Progress();
            Vector2 random = Random.insideUnitCircle * amplitude * falloff;
            appliedOffset = new Vector3(random.x, random.y, 0f);
            transform.position += appliedOffset;
        }

        private void OnEnd(ScriptableRenderContext context, Camera rendering)
        {
            if (rendering == targetCamera)
            {
                Restore();
            }
        }

        private void Restore()
        {
            if (appliedOffset != Vector3.zero)
            {
                transform.position -= appliedOffset;
                appliedOffset = Vector3.zero;
            }
        }
    }
}
