using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    public sealed class SceneFadeIn : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] private float fadeDuration = 0.8f;

        private IEnumerator Start()
        {
            var canvasObject = new GameObject("Scene Fade Overlay");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            RectTransform overlayTransform = canvasObject.GetComponent<RectTransform>();
            overlayTransform.anchorMin = Vector2.zero;
            overlayTransform.anchorMax = Vector2.one;
            overlayTransform.offsetMin = Vector2.zero;
            overlayTransform.offsetMax = Vector2.zero;

            var imageObject = new GameObject("Black Fade");
            imageObject.transform.SetParent(canvasObject.transform, false);
            RectTransform imageTransform = imageObject.AddComponent<RectTransform>();
            imageTransform.anchorMin = Vector2.zero;
            imageTransform.anchorMax = Vector2.one;
            imageTransform.offsetMin = Vector2.zero;
            imageTransform.offsetMax = Vector2.zero;

            Image image = imageObject.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            CanvasGroup group = imageObject.AddComponent<CanvasGroup>();
            group.alpha = 1f;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            Destroy(canvasObject);
        }
    }
}
