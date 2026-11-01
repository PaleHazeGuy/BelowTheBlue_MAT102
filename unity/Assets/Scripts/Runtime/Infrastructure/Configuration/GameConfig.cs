using System;
using UnityEngine;

namespace BelowTheBlue.Infrastructure.Configuration
{
    [Serializable]
    public sealed class GameConfig
    {
        public SerialSettings serial = new SerialSettings();
        public InputSettings input = new InputSettings();

        public void Validate()
        {
            serial ??= new SerialSettings();
            input ??= new InputSettings();

            serial.Validate();
            input.Validate();
        }
    }

    [Serializable]
    public sealed class SerialSettings
    {
        public bool enabled = true;
        public string port = "auto";
        public string preferredDeviceName = "Sigma Controller";
        public int baudRate = 115200;
        public int readTimeoutMs = 100;
        public int packetTimeoutMs = 500;
        public int reconnectIntervalMs = 2000;

        internal void Validate()
        {
            port = string.IsNullOrWhiteSpace(port) ? "auto" : port.Trim();
            preferredDeviceName = string.IsNullOrWhiteSpace(preferredDeviceName)
                ? "Sigma Controller"
                : preferredDeviceName.Trim();

            baudRate = baudRate > 0 ? baudRate : 115200;
            readTimeoutMs = Mathf.Clamp(readTimeoutMs, 1, 5000);
            packetTimeoutMs = Mathf.Clamp(packetTimeoutMs, readTimeoutMs, 10000);
            reconnectIntervalMs = Mathf.Clamp(reconnectIntervalMs, 100, 60000);
        }
    }

    [Serializable]
    public sealed class InputSettings
    {
        public float deadZone = 0.15f;
        public bool invertX;
        public bool invertY;

        internal void Validate()
        {
            deadZone = Mathf.Clamp(deadZone, 0f, 0.95f);
        }
    }
}
