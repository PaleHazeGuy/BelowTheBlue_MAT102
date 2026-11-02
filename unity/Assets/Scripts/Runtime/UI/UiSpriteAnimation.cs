using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class UiSpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 8f;
        [SerializeField] private bool loop = true;

        private Image target;
        private float timer;
        private int index;

        private void Awake()
        {
            target = GetComponent<Image>();
        }

        private void OnEnable()
        {
            timer = 0f;
            index = 0;
            Apply();
        }

        private void Update()
        {
            if (frames == null || frames.Length < 2)
            {
                return;
            }

            timer += Time.unscaledDeltaTime;
            float duration = 1f / framesPerSecond;
            while (timer >= duration)
            {
                timer -= duration;
                if (index < frames.Length - 1)
                {
                    index++;
                }
                else if (loop)
                {
                    index = 0;
                }

                Apply();
            }
        }

        private void Apply()
        {
            if (target != null && frames != null && frames.Length > 0)
            {
                target.sprite = frames[index];
            }
        }
    }
}
