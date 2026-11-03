using System.Diagnostics;
using BelowTheBlue.Infrastructure.Configuration;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BelowTheBlue.Input
{
    public sealed class ControllerInputSource : MonoBehaviour, IPlayerInputSource
    {
        [SerializeField] private KeyboardInputSource keyboardFallback;

        private readonly object gate = new object();
        private BluetoothSerialLink link;
        private PlayerInputState latest = PlayerInputState.Neutral;
        private long lastPacketTicks;
        private bool linkUp;
        private double staleSeconds = 0.5;
        private float deadZone;
        private bool invertX;
        private bool invertY;

        public bool IsAvailable => ControllerConnected || (keyboardFallback != null && keyboardFallback.IsAvailable);

        public bool LinkUp
        {
            get
            {
                lock (gate)
                {
                    return linkUp;
                }
            }
        }

        public bool ControllerConnected
        {
            get
            {
                lock (gate)
                {
                    return IsFresh();
                }
            }
        }

        private void OnEnable()
        {
            ConfigLoadResult result = ConfigLoader.LoadDefault();
            if (result.HasWarning)
            {
                Debug.LogWarning(result.Warning);
            }

            GameConfig config = result.Config;
            staleSeconds = config.serial.packetTimeoutMs / 1000.0;
            deadZone = config.input.deadZone;
            invertX = config.input.invertX;
            invertY = config.input.invertY;

            if (!config.serial.enabled)
            {
                return;
            }

            if (!BluetoothSerialLink.IsSupported)
            {
                Debug.LogWarning("Bluetooth controller is only supported on Linux. Using keyboard.");
                return;
            }

            link = new BluetoothSerialLink(
                config.serial.port,
                config.serial.preferredDeviceName,
                config.serial.reconnectIntervalMs,
                OnLine,
                OnConnection);
            link.Start();
        }

        private void OnDisable()
        {
            link?.Stop();
            link = null;
            lock (gate)
            {
                latest = PlayerInputState.Neutral;
                lastPacketTicks = 0;
                linkUp = false;
            }
        }

        public PlayerInputState Read()
        {
            PlayerInputState pad = PlayerInputState.Neutral;
            lock (gate)
            {
                if (IsFresh())
                {
                    pad = latest;
                }
            }

            PlayerInputState keys = keyboardFallback != null ? keyboardFallback.Read() : PlayerInputState.Neutral;
            Vector2 move = keys.Move.sqrMagnitude > 0.0001f ? keys.Move : pad.Move;
            return new PlayerInputState(move, keys.InteractHeld || pad.InteractHeld);
        }

        private bool IsFresh()
        {
            if (lastPacketTicks == 0)
            {
                return false;
            }

            double seconds = (Stopwatch.GetTimestamp() - lastPacketTicks) / (double)Stopwatch.Frequency;
            return seconds <= staleSeconds;
        }

        private void OnLine(string line)
        {
            if (!ControllerLineParser.TryParse(line, out PlayerInputState state))
            {
                return;
            }

            float x = Shape(state.Move.x, invertX);
            float y = Shape(state.Move.y, invertY);

            lock (gate)
            {
                latest = new PlayerInputState(new Vector2(x, y), state.InteractHeld);
                lastPacketTicks = Stopwatch.GetTimestamp();
            }
        }

        private float Shape(float value, bool invert)
        {
            float magnitude = Mathf.Abs(value);
            if (magnitude < deadZone)
            {
                return 0f;
            }

            float scaled = Mathf.Sign(value) * (magnitude - deadZone) / (1f - deadZone);
            return invert ? -scaled : scaled;
        }

        private void OnConnection(bool connected)
        {
            lock (gate)
            {
                linkUp = connected;
                if (!connected)
                {
                    lastPacketTicks = 0;
                    latest = PlayerInputState.Neutral;
                }
            }
        }
    }
}
