using BelowTheBlue.Input;
using NUnit.Framework;
using UnityEngine;

namespace BelowTheBlue.Tests.Input
{
    public sealed class PlayerInputStateTests
    {
        [Test]
        public void Constructor_ClampsMovementToUnitLength()
        {
            var state = new PlayerInputState(new Vector2(1f, 1f), false);

            Assert.That(state.Move.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Neutral_HasNoMovementOrInteraction()
        {
            Assert.That(PlayerInputState.Neutral.Move, Is.EqualTo(Vector2.zero));
            Assert.That(PlayerInputState.Neutral.InteractHeld, Is.False);
        }
    }
}
