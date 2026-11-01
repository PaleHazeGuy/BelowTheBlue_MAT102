using BelowTheBlue.Input;
using NUnit.Framework;
using UnityEngine;

namespace BelowTheBlue.Tests.Input
{
    public sealed class PlayerInputReaderTests
    {
        private GameObject gameObject;
        private StubInputSource source;
        private PlayerInputReader reader;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject("Player Input Reader Test");
            source = gameObject.AddComponent<StubInputSource>();
            reader = gameObject.AddComponent<PlayerInputReader>();
            reader.SetInputSource(source);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Sample_CopiesCurrentSourceState()
        {
            source.State = new PlayerInputState(Vector2.left, false);

            reader.Sample();

            Assert.That(reader.Current.Move, Is.EqualTo(Vector2.left));
        }

        [Test]
        public void Sample_ReportsInteractionOnlyOnPressEdge()
        {
            source.State = new PlayerInputState(Vector2.zero, true);

            reader.Sample();
            Assert.That(reader.InteractPressedThisFrame, Is.True);

            reader.Sample();
            Assert.That(reader.InteractPressedThisFrame, Is.False);

            source.State = PlayerInputState.Neutral;
            reader.Sample();
            source.State = new PlayerInputState(Vector2.zero, true);
            reader.Sample();

            Assert.That(reader.InteractPressedThisFrame, Is.True);
        }

        private sealed class StubInputSource : MonoBehaviour, IPlayerInputSource
        {
            public bool IsAvailable => true;
            public PlayerInputState State { get; set; }

            public PlayerInputState Read()
            {
                return State;
            }
        }
    }
}
