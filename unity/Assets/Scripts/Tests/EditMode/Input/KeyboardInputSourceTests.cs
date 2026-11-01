using BelowTheBlue.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BelowTheBlue.Tests.Input
{
    public sealed class KeyboardInputSourceTests : InputTestFixture
    {
        private GameObject gameObject;
        private KeyboardInputSource source;
        private Keyboard keyboard;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            gameObject = new GameObject("Keyboard Input Source Test");
            source = gameObject.AddComponent<KeyboardInputSource>();
            source.Read();
        }

        public override void TearDown()
        {
            Object.DestroyImmediate(gameObject);
            base.TearDown();
        }

        [Test]
        public void Read_MapsWasdToMovement()
        {
            Press(keyboard.wKey);
            Press(keyboard.dKey);

            PlayerInputState state = source.Read();

            Assert.That(state.Move.x, Is.GreaterThan(0f));
            Assert.That(state.Move.y, Is.GreaterThan(0f));
            Assert.That(state.Move.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Read_MapsSpaceToInteraction()
        {
            Press(keyboard.spaceKey);

            Assert.That(source.Read().InteractHeld, Is.True);

            Release(keyboard.spaceKey);

            Assert.That(source.Read().InteractHeld, Is.False);
        }

        [Test]
        public void Read_WhenDisabled_ReturnsNeutralState()
        {
            Press(keyboard.wKey);
            Press(keyboard.spaceKey);
            source.enabled = false;

            PlayerInputState state = source.Read();

            Assert.That(state.Move, Is.EqualTo(Vector2.zero));
            Assert.That(state.InteractHeld, Is.False);
        }
    }
}
