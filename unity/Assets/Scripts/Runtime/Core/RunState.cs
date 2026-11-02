using System;
using BelowTheBlue.Interaction;
using BelowTheBlue.Player;
using UnityEngine;

namespace BelowTheBlue.Core
{
    public enum RunOutcome
    {
        Playing,
        Won,
        Lost
    }

    public enum LoseReason
    {
        None,
        Died,
        OutOfOxygen,
        NotEnoughSamples
    }

    [DisallowMultipleComponent]
    public sealed class RunState : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private OxygenTimer oxygen;
        [SerializeField, Min(0)] private int samplesRequired = 3;

        public RunOutcome Outcome { get; private set; }
        public LoseReason Reason { get; private set; }
        public int SamplesCollected { get; private set; }
        public int SamplesRequired => samplesRequired;
        public bool IsPlaying => Outcome == RunOutcome.Playing;

        public event Action<int, int> SamplesChanged;
        public event Action<RunOutcome> Ended;

        private void OnEnable()
        {
            Collectible.Collected += OnCollected;
            if (health != null)
            {
                health.Died += OnDied;
            }

            if (oxygen != null)
            {
                oxygen.Depleted += OnDepleted;
            }
        }

        private void OnDisable()
        {
            Collectible.Collected -= OnCollected;
            if (health != null)
            {
                health.Died -= OnDied;
            }

            if (oxygen != null)
            {
                oxygen.Depleted -= OnDepleted;
            }
        }

        public void ReachSurface()
        {
            if (!IsPlaying)
            {
                return;
            }

            if (SamplesCollected >= samplesRequired)
            {
                End(RunOutcome.Won, LoseReason.None);
            }
            else
            {
                End(RunOutcome.Lost, LoseReason.NotEnoughSamples);
            }
        }

        private void OnCollected(Collectible collectible)
        {
            if (!IsPlaying)
            {
                return;
            }

            SamplesCollected++;
            SamplesChanged?.Invoke(SamplesCollected, samplesRequired);
        }

        private void OnDied()
        {
            End(RunOutcome.Lost, LoseReason.Died);
        }

        private void OnDepleted()
        {
            End(RunOutcome.Lost, LoseReason.OutOfOxygen);
        }

        private void End(RunOutcome outcome, LoseReason reason)
        {
            if (!IsPlaying)
            {
                return;
            }

            Outcome = outcome;
            Reason = reason;

            if (oxygen != null)
            {
                oxygen.Stop();
            }

            if (outcome == RunOutcome.Won || reason == LoseReason.NotEnoughSamples)
            {
                FreezeSubmarine();
            }

            Ended?.Invoke(outcome);
        }

        private void FreezeSubmarine()
        {
            if (health == null)
            {
                return;
            }

            var controller = health.GetComponent<SubmarineController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            var collector = health.GetComponent<PlayerCollector>();
            if (collector != null)
            {
                collector.enabled = false;
            }

            var body = health.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }
    }
}
