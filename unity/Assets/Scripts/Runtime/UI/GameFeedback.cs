using BelowTheBlue.Core;
using BelowTheBlue.Interaction;
using BelowTheBlue.World;
using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    public sealed class GameFeedback : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private OxygenTimer oxygen;
        [SerializeField] private RunState runState;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private Image flash;
        [SerializeField] private ParticleSystem collectBurst;
        [SerializeField] private ParticleSystem explosion;
        [SerializeField] private ParticleSystem wreckFire;
        [SerializeField] private Color hitColor = new Color(1f, 0.15f, 0.1f, 0.5f);
        [SerializeField] private Color winColor = new Color(0.7f, 1f, 0.95f, 0.6f);
        [SerializeField] private Color lowOxygenColor = new Color(1f, 0.1f, 0.05f, 0.22f);
        [SerializeField, Min(1)] private int warningSeconds = 20;
        [SerializeField, Min(0.05f)] private float flashSeconds = 0.35f;

        private Color flashColor;
        private float flashTime;
        private float flashDuration;
        private bool lowOxygen;
        private bool ended;

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

            if (runState != null)
            {
                runState.Ended += OnEnded;
            }

            Collectible.Collected += OnCollected;
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

            if (runState != null)
            {
                runState.Ended -= OnEnded;
            }

            Collectible.Collected -= OnCollected;
        }

        private void Update()
        {
            if (flash == null)
            {
                return;
            }

            if (flashDuration > 0f)
            {
                flashTime += Time.unscaledDeltaTime;
                float k = 1f - Mathf.Clamp01(flashTime / flashDuration);
                Color c = flashColor;
                c.a *= k * k;
                flash.color = c;
                if (flashTime >= flashDuration)
                {
                    flashDuration = 0f;
                }
            }
            else if (lowOxygen && !ended)
            {
                Color c = lowOxygenColor;
                c.a *= 0.5f + 0.5f * Mathf.Sin(Time.time * 4f);
                flash.color = c;
            }
            else
            {
                Color c = flash.color;
                c.a = 0f;
                flash.color = c;
            }
        }

        private void Flash(Color color, float seconds)
        {
            flashColor = color;
            flashDuration = seconds;
            flashTime = 0f;
        }

        private void OnHealthChanged(int current, int maximum)
        {
            if (current > 0)
            {
                Flash(hitColor, flashSeconds);
                if (cameraShake != null)
                {
                    cameraShake.Shake(0.25f, 0.3f);
                }
            }
        }

        private void OnDied()
        {
            if (explosion != null && health != null && !health.WasSunk)
            {
                explosion.transform.position = health.transform.position;
                explosion.Play();

                if (wreckFire != null)
                {
                    wreckFire.transform.position = health.transform.position;
                    wreckFire.Play(true);
                }
            }

            Flash(hitColor, 0.8f);
            if (cameraShake != null)
            {
                cameraShake.Shake(0.5f, 0.7f);
            }
        }

        private void OnOxygenChanged(float remaining, float total)
        {
            lowOxygen = remaining > 0f && remaining <= warningSeconds;
        }

        private void OnEnded(RunOutcome outcome)
        {
            ended = true;
            if (outcome == RunOutcome.Won)
            {
                Flash(winColor, 1.2f);
            }
        }

        private void OnCollected(Collectible item)
        {
            if (collectBurst != null && item != null)
            {
                collectBurst.transform.position = item.transform.position;
                collectBurst.Play();
            }
        }
    }
}
