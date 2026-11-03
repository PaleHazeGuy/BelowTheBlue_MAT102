using UnityEngine;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FadeOverPlayer : MonoBehaviour
    {
        [SerializeField] private Collider2D player;
        [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.4f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 6f;
        [SerializeField, Range(0.1f, 1f)] private float overlapWidthScale = 0.7f;

        private SpriteRenderer spriteRenderer;
        private float baseAlpha;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseAlpha = spriteRenderer.color.a;
            if (player == null)
            {
                GameObject found = GameObject.FindGameObjectWithTag("Player");
                player = found != null ? found.GetComponent<Collider2D>() : null;
            }
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            Bounds area = spriteRenderer.bounds;
            area.Expand(new Vector3(-area.size.x * (1f - overlapWidthScale), 0f, 0f));
            bool covering = area.Intersects(player.bounds);

            Color color = spriteRenderer.color;
            float target = covering ? fadedAlpha * baseAlpha : baseAlpha;
            color.a = Mathf.MoveTowards(color.a, target, fadeSpeed * Time.unscaledDeltaTime);
            spriteRenderer.color = color;
        }
    }
}
