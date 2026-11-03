using UnityEngine;

namespace BelowTheBlue.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PolygonCollider2D))]
    public sealed class SpriteColliderMirror : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private PolygonCollider2D polygon;
        private bool mirrored;

        private void Awake()
        {
            polygon = GetComponent<PolygonCollider2D>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            mirrored = false;
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            if (spriteRenderer == null || polygon == null || spriteRenderer.flipX == mirrored)
            {
                return;
            }

            mirrored = spriteRenderer.flipX;
            for (int i = 0; i < polygon.pathCount; i++)
            {
                Vector2[] points = polygon.GetPath(i);
                for (int k = 0; k < points.Length; k++)
                {
                    points[k] = new Vector2(-points[k].x, points[k].y);
                }

                System.Array.Reverse(points);
                polygon.SetPath(i, points);
            }
        }
    }
}
