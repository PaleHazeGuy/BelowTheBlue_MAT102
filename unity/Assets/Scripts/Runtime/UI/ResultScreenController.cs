using System.Collections;
using BelowTheBlue.Core;
using BelowTheBlue.Input;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    public sealed class ResultScreenController : MonoBehaviour
    {
        [SerializeField] private RunState runState;
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private CanvasGroup winGroup;
        [SerializeField] private CanvasGroup loseGroup;
        [SerializeField] private Text winDetail;
        [SerializeField] private Text loseDetail;
        [SerializeField] private Button winRetry;
        [SerializeField] private Button loseRetry;
        [SerializeField, Min(0f)] private float winDelay = 0.8f;
        [SerializeField, Min(0f)] private float loseDelay = 2.2f;
        [SerializeField, Min(0.05f)] private float fadeSeconds = 0.4f;
        [SerializeField] private string diedText = "The hull gave way.";
        [SerializeField] private string outOfOxygenText = "The oxygen ran out before the surface.";
        [SerializeField] private string notEnoughSamplesText = "You surfaced without enough samples.";

        [SerializeField, Min(0f)] private float retryAfterSeconds = 0.6f;

        private bool shown;
        private bool leaving;
        private bool ended;
        private float endedAt;

        private void OnEnable()
        {
            if (runState != null)
            {
                runState.Ended += OnEnded;
            }

            if (winRetry != null)
            {
                winRetry.onClick.AddListener(Retry);
            }

            if (loseRetry != null)
            {
                loseRetry.onClick.AddListener(Retry);
            }
        }

        private void OnDisable()
        {
            if (runState != null)
            {
                runState.Ended -= OnEnded;
            }

            if (winRetry != null)
            {
                winRetry.onClick.RemoveListener(Retry);
            }

            if (loseRetry != null)
            {
                loseRetry.onClick.RemoveListener(Retry);
            }
        }

        private void Start()
        {
            Hide(winGroup);
            Hide(loseGroup);
        }

        private void Update()
        {
            if (ended && !leaving && Time.unscaledTime - endedAt >= retryAfterSeconds && input != null && input.InteractPressedThisFrame)
            {
                Retry();
            }
        }

        private void OnEnded(RunOutcome outcome)
        {
            ended = true;
            endedAt = Time.unscaledTime;
            if (outcome == RunOutcome.Won)
            {
                StartCoroutine(Show(winGroup, winDelay, WinDetailText(), winDetail));
                return;
            }

            float delay = runState.Reason == LoseReason.NotEnoughSamples ? winDelay : loseDelay;
            StartCoroutine(Show(loseGroup, delay, LoseDetailText(), loseDetail));
        }

        private string WinDetailText()
        {
            int shownCount = Mathf.Min(runState.SamplesCollected, runState.SamplesRequired);
            return $"Samples collected: {shownCount} / {runState.SamplesRequired}";
        }

        private string LoseDetailText()
        {
            switch (runState.Reason)
            {
                case LoseReason.OutOfOxygen:
                    return outOfOxygenText;
                case LoseReason.NotEnoughSamples:
                    return notEnoughSamplesText;
                default:
                    return diedText;
            }
        }

        private IEnumerator Show(CanvasGroup group, float delay, string detail, Text detailLabel)
        {
            yield return new WaitForSecondsRealtime(delay);

            if (detailLabel != null)
            {
                detailLabel.text = detail;
            }

            group.alpha = 0f;
            group.gameObject.SetActive(true);
            shown = true;

            for (float elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Clamp01(elapsed / fadeSeconds);
                yield return null;
            }

            group.alpha = 1f;
        }

        private static void Hide(CanvasGroup group)
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.gameObject.SetActive(false);
            }
        }

        private void Retry()
        {
            if (leaving)
            {
                return;
            }

            leaving = true;
            IntroController.RequestIntro();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
