using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.World
{
    [DisallowMultipleComponent]
    public sealed class RadialVignette : MonoBehaviour
    {
        [SerializeField] private Material vignetteMaterial;
        [SerializeField] private Sprite portholeFrame;
        [SerializeField, Range(0.8f, 1.5f)] private float frameScreenScale = 1.25f;

        private void Awake()
        {
            if (vignetteMaterial == null)
            {
                Debug.LogError("RadialVignette needs its RadialVignette material assigned.", this);
                return;
            }

            var canvasObject = new GameObject("Radial Vignette Overlay");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateFullScreenImage(canvas.transform, "Radial Vignette", vignetteMaterial, null);

            if (portholeFrame != null)
            {
                Image frame = CreateFullScreenImage(canvas.transform, "Porthole Frame", null, portholeFrame);
                frame.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                frame.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                float frameSize = Mathf.Min(Screen.width, Screen.height) * frameScreenScale;
                frame.rectTransform.sizeDelta = new Vector2(frameSize, frameSize);
                frame.preserveAspect = true;
            }
        }

        private static Image CreateFullScreenImage(Transform parent, string objectName, Material material, Sprite sprite)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            var rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image image = imageObject.GetComponent<Image>();
            image.material = material;
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }
    }
}
