using BelowTheBlue.Input;
using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    public sealed class IntroController : MonoBehaviour
    {
        private static bool seen;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSeen()
        {
            seen = false;
        }

        [SerializeField] private GameObject introRoot;
        [SerializeField] private GameObject[] pages;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private bool showEveryRun;

        private int pageIndex;
        private bool open;

        private void Start()
        {
            if (introRoot == null || pages == null || pages.Length == 0 || (seen && !showEveryRun))
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
            foreach (GameObject page in pages)
            {
                Button button = page.GetComponentInChildren<Button>(true);
                if (button != null)
                {
                    button.onClick.AddListener(Advance);
                }
            }

            ShowPage(0);
        }

        private void Update()
        {
            if (open && input != null && input.InteractPressedThisFrame)
            {
                Advance();
            }
        }

        private void OnDestroy()
        {
            if (pages == null)
            {
                return;
            }

            foreach (GameObject page in pages)
            {
                if (page == null)
                {
                    continue;
                }

                Button button = page.GetComponentInChildren<Button>(true);
                if (button != null)
                {
                    button.onClick.RemoveListener(Advance);
                }
            }
        }

        private void Advance()
        {
            if (!open)
            {
                return;
            }

            if (pageIndex + 1 < pages.Length)
            {
                ShowPage(pageIndex + 1);
                return;
            }

            open = false;
            seen = true;
            Time.timeScale = 1f;
            introRoot.SetActive(false);
        }

        private void ShowPage(int index)
        {
            pageIndex = index;
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].SetActive(i == index);
            }
        }
    }
}
