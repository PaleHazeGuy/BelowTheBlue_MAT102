using UnityEngine;
using UnityEngine.InputSystem;

namespace BelowTheBlue.Input
{
    public sealed class KeyboardInputSource : MonoBehaviour, IPlayerInputSource
    {
        private InputAction moveAction;
        private InputAction interactAction;

        public bool IsAvailable => Keyboard.current != null;

        private void Awake()
        {
            EnsureActions();
        }

        private void OnEnable()
        {
            EnsureActions();
            moveAction.Enable();
            interactAction.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            interactAction?.Disable();
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            interactAction?.Dispose();
        }

        public PlayerInputState Read()
        {
            if (!isActiveAndEnabled || !IsAvailable)
            {
                return PlayerInputState.Neutral;
            }

            EnsureActions();

            if (!moveAction.enabled)
            {
                moveAction.Enable();
                interactAction.Enable();
            }

            return new PlayerInputState(
                moveAction.ReadValue<Vector2>(),
                interactAction.IsPressed());
        }

        private void EnsureActions()
        {
            if (moveAction != null)
            {
                return;
            }

            moveAction = new InputAction("Keyboard Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            interactAction = new InputAction(
                "Keyboard Interact",
                InputActionType.Button,
                "<Keyboard>/space");
        }
    }
}
