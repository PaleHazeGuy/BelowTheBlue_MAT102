using System.Collections;
using BelowTheBlue.Core;
using BelowTheBlue.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    public sealed class GameHudController : MonoBehaviour
    {
        [Header("Sources")]
        [SerializeField] private Health health;
        [SerializeField] private OxygenTimer oxygen;
        [SerializeField] private PlayerCollector collector;
        [SerializeField] private RunState runState;

        [Header("Oxygen")]
        [SerializeField] private Text oxygenValue;
        [Tooltip("Parent whose child Images are the oxygen lamps, in order.")]
        [SerializeField] private Transform oxygenSegments;
        [SerializeField, Min(1)] private int warningSeconds = 20;
        [SerializeField] private Color oxygenLitColor = new Color(0.36f, 0.95f, 0.75f, 1f);
        [SerializeField] private Color oxygenWarningColor = new Color(1f, 0.35f, 0.25f, 1f);
        [SerializeField] private Color unlitColor = new Color(0.04f, 0.16f, 0.12f, 1f);

        [Header("Health")]
        [SerializeField] private Text healthValue;
        [Tooltip("Parent whose child Images are the health lamps, in order.")]
        [SerializeField] private Transform healthSegments;
        [SerializeField] private Color healthLitColor = new Color(1f, 0.48f, 0.27f, 1f);
        [SerializeField] private Color healthUnlitColor = new Color(0.2f, 0.08f, 0.05f, 1f);

        [Header("Samples")]
        [SerializeField] private Text samplesValue;

        [Header("Prompt")]
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptLabel;
        [SerializeField] private string collectPrompt = "Press the red button to collect";

        [Header("Death")]
        [SerializeField] private Image glassCrack;
        [SerializeField, Min(0f)] private float crackFadeSeconds = 0.2f;

        private Image[] oxygenLamps;
        private Image[] healthLamps;
        private int lastOxygenSeconds = -1;

        private void Awake()
        {
            oxygenLamps = CollectLamps(oxygenSegments);
            healthLamps = CollectLamps(healthSegments);
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Changed += OnHealthChanged;
                health.Died += OnDied;
            }

            if (oxygen != null)
            {
                oxygen.Changed += OnOxygenChanged;
            }

            if (collector != null)
            {
                collector.TargetChanged += OnTargetChanged;
            }

            if (runState != null)
            {
                runState.SamplesChanged += OnSamplesChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Changed -= OnHealthChanged;
                health.Died -= OnDied;
            }

            if (oxygen != null)
            {
                oxygen.Changed -= OnOxygenChanged;
            }

            if (collector != null)
            {
                collector.TargetChanged -= OnTargetChanged;
            }

            if (runState != null)
            {
                runState.SamplesChanged -= OnSamplesChanged;
            }
        }

        private void Start()
        {
            if (health != null)
            {
                OnHealthChanged(health.Current, health.Maximum);
            }

            if (oxygen != null)
            {
                OnOxygenChanged(oxygen.Remaining, oxygen.Duration);
            }

            RefreshSamples();
            SetPrompt(null);
            if (glassCrack != null)
            {
                glassCrack.gameObject.SetActive(false);
            }
        }

        private void OnOxygenChanged(float remaining, float total)
        {
            int seconds = Mathf.CeilToInt(remaining);
            if (seconds == lastOxygenSeconds)
            {
                return;
            }

            lastOxygenSeconds = seconds;
            bool warning = seconds <= warningSeconds;

            if (oxygenValue != null)
            {
                oxygenValue.text = $"{seconds / 60:00}:{seconds % 60:00}";
            }

            int lit = total > 0f
                ? Mathf.CeilToInt(Mathf.Clamp01(remaining / total) * oxygenLamps.Length)
                : 0;
            SetLamps(oxygenLamps, lit, warning ? oxygenWarningColor : oxygenLitColor, unlitColor);
        }

        private void OnHealthChanged(int current, int maximum)
        {
            if (healthValue != null)
            {
                healthValue.text = $"{current} / {maximum}";
            }

            SetLamps(healthLamps, current, healthLitColor, healthUnlitColor);
        }

        private void OnSamplesChanged(int collected, int required)
        {
            RefreshSamples();
        }

        private void OnTargetChanged(Collectible target)
        {
            SetPrompt(target);
        }

        private void OnDied()
        {
            SetPrompt(null);
            if (glassCrack != null)
            {
                StartCoroutine(FadeInCrack());
            }
        }

        private void RefreshSamples()
        {
            if (samplesValue != null && runState != null)
            {
                samplesValue.text = $"{runState.SamplesCollected} / {runState.SamplesRequired}";
            }
        }

        private void SetPrompt(Collectible target)
        {
            bool visible = target != null && (health == null || health.IsAlive);
            if (promptRoot != null)
            {
                promptRoot.SetActive(visible);
            }

            if (visible && promptLabel != null)
            {
                promptLabel.text = collectPrompt;
            }
        }

        private IEnumerator FadeInCrack()
        {
            glassCrack.gameObject.SetActive(true);
            Color color = glassCrack.color;
            float elapsed = 0f;
            while (elapsed < crackFadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = Mathf.Clamp01(elapsed / crackFadeSeconds);
                glassCrack.color = color;
                yield return null;
            }

            color.a = 1f;
            glassCrack.color = color;
        }

        private static void SetLamps(Image[] lamps, int litCount, Color litColor, Color offColor)
        {
            for (int i = 0; i < lamps.Length; i++)
            {
                if (lamps[i] != null)
                {
                    lamps[i].color = i < litCount ? litColor : offColor;
                }
            }
        }

        private static Image[] CollectLamps(Transform root)
        {
            if (root == null)
            {
                return new Image[0];
            }

            var lamps = new Image[root.childCount];
            for (int i = 0; i < lamps.Length; i++)
            {
                lamps[i] = root.GetChild(i).GetComponent<Image>();
            }

            return lamps;
        }
    }
}
