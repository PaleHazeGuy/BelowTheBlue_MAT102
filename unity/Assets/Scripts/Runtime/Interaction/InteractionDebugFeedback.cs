using BelowTheBlue.Input;
using UnityEngine;

namespace BelowTheBlue.Interaction
{
    public sealed class InteractionDebugFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color activeColor = new Color(0.3f, 1f, 1f, 1f);

        private Color normalColor = Color.white;

        private void Awake()
        {
            input ??= GetComponent<PlayerInputReader>();
            targetRenderer ??= GetComponent<SpriteRenderer>();

            if (targetRenderer != null)
            {
                normalColor = targetRenderer.color;
            }
        }

        private void Update()
        {
            if (targetRenderer != null && input != null)
            {
                targetRenderer.color = input.Current.InteractHeld
                    ? activeColor
                    : normalColor;
            }
        }

        private void OnDisable()
        {
            if (targetRenderer != null)
            {
                targetRenderer.color = normalColor;
            }
        }
    }
}
