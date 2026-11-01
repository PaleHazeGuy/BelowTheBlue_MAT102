using UnityEngine;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;

        private SpriteRenderer targetRenderer;
        private float frameTimer;
        private int frameIndex;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            if (frames != null && frames.Length > 0)
            {
                targetRenderer.sprite = frames[0];
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length < 2)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / framesPerSecond;
            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % frames.Length;
                targetRenderer.sprite = frames[frameIndex];
            }
        }
    }
}
