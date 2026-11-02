using BelowTheBlue.Input;
using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    public sealed class IntroController : MonoBehaviour
    {
        private static bool seen;

        [SerializeField] private GameObject introRoot;
        [SerializeField] private Button startButton;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private bool showEveryRun;

        private bool open;

        private void Start()
        {
            if (introRoot == null || (seen && !showEveryRun))
            {
                if (introRoot != null)
                {
                    introRoot.SetActive(false);
                }

                return;
            }

            open = true;
            Time.timeScale = 0f;
            introRoot.SetActive(true);
            if (startButton != null)
            {
                startButton.onClick.AddListener(Begin);
            }
        }

        private void Update()
        {
            if (open && input != null && input.InteractPressedThisFrame)
            {
                Begin();
            }
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(Begin);
            }
        }

        private void Begin()
        {
            if (!open)
            {
                return;
            }

            open = false;
            seen = true;
            Time.timeScale = 1f;
            introRoot.SetActive(false);
        }
    }
}
