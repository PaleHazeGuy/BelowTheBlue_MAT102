using UnityEngine;

namespace BelowTheBlue.Input
{
    public readonly struct PlayerInputState
    {
        public static PlayerInputState Neutral => new PlayerInputState(Vector2.zero, false);

        public Vector2 Move { get; }
        public bool InteractHeld { get; }

        public PlayerInputState(Vector2 move, bool interactHeld)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            InteractHeld = interactHeld;
        }
    }
}
