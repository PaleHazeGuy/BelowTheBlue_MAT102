using System;
using UnityEngine;

namespace BelowTheBlue.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour inputSourceBehaviour;

        private IPlayerInputSource inputSource;

        public PlayerInputState Current { get; private set; } = PlayerInputState.Neutral;
        public bool InteractPressedThisFrame { get; private set; }

        private void Awake()
        {
            ResolveInputSource();
        }

        private void Update()
        {
            Sample();
        }

        private void OnDisable()
        {
            Current = PlayerInputState.Neutral;
            InteractPressedThisFrame = false;
        }

        public void SetInputSource(MonoBehaviour source)
        {
            if (source != null && source is not IPlayerInputSource)
            {
                throw new ArgumentException(
                    $"{source.GetType().Name} must implement {nameof(IPlayerInputSource)}.",
                    nameof(source));
            }

            inputSourceBehaviour = source;
            inputSource = source as IPlayerInputSource;
            Current = PlayerInputState.Neutral;
            InteractPressedThisFrame = false;
        }

        public void Sample()
        {
            if (inputSource == null)
            {
                ResolveInputSource();
            }

            PlayerInputState next = inputSource?.Read() ?? PlayerInputState.Neutral;
            InteractPressedThisFrame = next.InteractHeld && !Current.InteractHeld;
            Current = next;
        }

        private void ResolveInputSource()
        {
            if (inputSourceBehaviour != null)
            {
                inputSource = inputSourceBehaviour as IPlayerInputSource;
                return;
            }

            foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            {
                if (behaviour is IPlayerInputSource source)
                {
                    inputSourceBehaviour = behaviour;
                    inputSource = source;
                    return;
                }
            }
        }
    }
}
