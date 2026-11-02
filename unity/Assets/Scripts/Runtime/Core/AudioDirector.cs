using BelowTheBlue.Interaction;
using UnityEngine;

namespace BelowTheBlue.Core
{
    [DisallowMultipleComponent]
    public sealed class AudioDirector : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private OxygenTimer oxygen;
        [SerializeField] private RunState runState;
        [SerializeField] private CollectorArm arm;

        [SerializeField] private AudioClip ambient;
        [SerializeField] private AudioClip hit;
        [SerializeField] private AudioClip grab;
        [SerializeField] private AudioClip collect;
        [SerializeField] private AudioClip warning;
        [SerializeField] private AudioClip win;
        [SerializeField] private AudioClip lose;

        [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.35f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.8f;
        [SerializeField, Min(1)] private int warningSeconds = 20;

        private AudioSource loopSource;
        private AudioSource effectsSource;
        private int lastWarningSecond = -1;
        private int lastHealth = int.MaxValue;

        private void Awake()
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.clip = ambient;
            loopSource.loop = true;
            loopSource.volume = ambientVolume;
            loopSource.playOnAwake = false;
            effectsSource = gameObject.AddComponent<AudioSource>();
            effectsSource.playOnAwake = false;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Changed += OnHealthChanged;
            }

            if (oxygen != null)
            {
                oxygen.Changed += OnOxygenChanged;
            }

            if (runState != null)
            {
                runState.Ended += OnEnded;
            }

            if (arm != null)
            {
                arm.Grabbed += OnGrabbed;
            }

            Collectible.Collected += OnCollected;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Changed -= OnHealthChanged;
            }

            if (oxygen != null)
            {
                oxygen.Changed -= OnOxygenChanged;
            }

            if (runState != null)
            {
                runState.Ended -= OnEnded;
            }

            if (arm != null)
            {
                arm.Grabbed -= OnGrabbed;
            }

            Collectible.Collected -= OnCollected;
        }

        private void Start()
        {
            if (ambient != null)
            {
                loopSource.Play();
            }
        }

        private void OnHealthChanged(int current, int maximum)
        {
            if (current < lastHealth && current > 0)
            {
                Play(hit);
            }

            lastHealth = current;
        }

        private void OnOxygenChanged(float remaining, float total)
        {
            int second = Mathf.CeilToInt(remaining);
            if (second <= warningSeconds && second > 0 && second != lastWarningSecond)
            {
                lastWarningSecond = second;
                Play(warning);
            }
        }

        private void OnGrabbed()
        {
            Play(grab);
        }

        private void OnCollected(Collectible item)
        {
            Play(collect);
        }

        private void OnEnded(RunOutcome outcome)
        {
            loopSource.volume = ambientVolume * 0.4f;
            Play(outcome == RunOutcome.Won ? win : lose);
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                effectsSource.PlayOneShot(clip, effectsVolume);
            }
        }
    }
}
