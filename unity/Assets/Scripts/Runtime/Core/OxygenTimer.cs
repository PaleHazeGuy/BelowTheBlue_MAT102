using System;
using UnityEngine;

namespace BelowTheBlue.Core
{
    [DisallowMultipleComponent]
    public sealed class OxygenTimer : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float durationSeconds = 105f;
        [SerializeField] private Health health;

        public float Duration => durationSeconds;
        public float Remaining { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action<float, float> Changed;
        public event Action Depleted;

        private void Awake()
        {
            Remaining = durationSeconds;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += Stop;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= Stop;
            }
        }

        private void Start()
        {
            IsRunning = true;
            Changed?.Invoke(Remaining, durationSeconds);
        }

        private void Update()
        {
            if (!IsRunning)
            {
                return;
            }

            Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);
            Changed?.Invoke(Remaining, durationSeconds);

            if (Remaining <= 0f)
            {
                IsRunning = false;
                Depleted?.Invoke();

                if (health != null)
                {
                    health.TakeDamage(health.Current);
                }
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }
    }
}
