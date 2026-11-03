using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BelowTheBlue.Input
{
    public sealed class BluetoothSerialLink
    {
        private const int AfBluetooth = 31;
        private const int SockStream = 1;
        private const int BtProtoRfcomm = 3;
        private const short PollIn = 1;
        private const int RfcommChannel = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct PollFd
        {
            public int fd;
            public short events;
            public short revents;
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int socket(int domain, int type, int protocol);

        [DllImport("libc", SetLastError = true)]
        private static extern int connect(int fd, byte[] address, int length);

        [DllImport("libc", SetLastError = true)]
        private static extern int read(int fd, byte[] buffer, int count);

        [DllImport("libc", SetLastError = true)]
        private static extern int close(int fd);

        [DllImport("libc", SetLastError = true)]
        private static extern int poll(PollFd[] fds, UIntPtr count, int timeoutMilliseconds);

        private readonly string port;
        private readonly string deviceName;
        private readonly int reconnectMilliseconds;
        private readonly Action<string> onLine;
        private readonly Action<bool> onConnection;
        private volatile bool stopRequested;
        private Thread thread;

        public BluetoothSerialLink(string portSetting, string preferredDeviceName, int reconnectIntervalMs, Action<string> lineReceived, Action<bool> connectionChanged)
        {
            port = portSetting;
            deviceName = preferredDeviceName;
            reconnectMilliseconds = reconnectIntervalMs;
            onLine = lineReceived;
            onConnection = connectionChanged;
        }

        public static bool IsSupported
        {
            get
            {
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
                return true;
#else
                return false;
#endif
            }
        }

        public void Start()
        {
            if (!IsSupported || thread != null)
            {
                return;
            }

            stopRequested = false;
            thread = new Thread(Run) { IsBackground = true };
            thread.Start();
        }

        public void Stop()
        {
            stopRequested = true;
            thread = null;
        }

        private void Run()
        {
            while (!stopRequested)
            {
                byte[] address = ResolveAddress();
                if (address != null)
                {
                    int fd = Open(address);
                    if (fd >= 0)
                    {
                        onConnection?.Invoke(true);
                        Pump(fd);
                        close(fd);
                        onConnection?.Invoke(false);
                    }
                }

                for (int waited = 0; waited < reconnectMilliseconds && !stopRequested; waited += 50)
                {
                    Thread.Sleep(50);
                }
            }
        }

        private byte[] ResolveAddress()
        {
            byte[] direct = ParseAddress(port);
            if (direct != null)
            {
                return direct;
            }

            if (!string.Equals(port, "auto", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return FindPairedDevice(deviceName);
        }

        private static byte[] FindPairedDevice(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            try
            {
                ProcessStartInfo info = new ProcessStartInfo("bluetoothctl", "devices")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(info))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    if (!process.WaitForExit(3000))
                    {
                        process.Kill();
                        return null;
                    }

                    foreach (string line in output.Split('\n'))
                    {
                        string[] parts = line.Trim().Split(new[] { ' ' }, 3);
                        if (parts.Length == 3
                            && parts[0] == "Device"
                            && string.Equals(parts[2].Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            return ParseAddress(parts[1]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        private static int Open(byte[] address)
        {
            int fd = socket(AfBluetooth, SockStream, BtProtoRfcomm);
            if (fd < 0)
            {
                return -1;
            }

            byte[] sockaddr = new byte[10];
            sockaddr[0] = AfBluetooth;
            Array.Copy(address, 0, sockaddr, 2, 6);
            sockaddr[8] = RfcommChannel;

            if (connect(fd, sockaddr, sockaddr.Length) != 0)
            {
                close(fd);
                return -1;
            }

            return fd;
        }

        private void Pump(int fd)
        {
            byte[] chunk = new byte[256];
            StringBuilder pending = new StringBuilder();
            PollFd[] fds = { new PollFd { fd = fd, events = PollIn } };

            while (!stopRequested)
            {
                int ready = poll(fds, new UIntPtr(1), 250);
                if (ready < 0)
                {
                    return;
                }

                if (ready == 0)
                {
                    continue;
                }

                int count = read(fd, chunk, chunk.Length);
                if (count <= 0)
                {
                    return;
                }

                pending.Append(Encoding.ASCII.GetString(chunk, 0, count));
                string text = pending.ToString();
                int newline = text.LastIndexOf('\n');
                if (newline < 0)
                {
                    if (pending.Length > 1024)
                    {
                        pending.Clear();
                    }

                    continue;
                }

                string[] lines = text.Substring(0, newline).Split('\n');
                pending.Clear();
                pending.Append(text.Substring(newline + 1));
                for (int i = 0; i < lines.Length; i++)
                {
                    onLine?.Invoke(lines[i]);
                }
            }
        }

        private static byte[] ParseAddress(string mac)
        {
            if (string.IsNullOrWhiteSpace(mac))
            {
                return null;
            }

            string[] parts = mac.Trim().Split(':');
            if (parts.Length != 6)
            {
                return null;
            }

            byte[] bytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                if (!byte.TryParse(parts[5 - i], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                {
                    return null;
                }
            }

            return bytes;
        }
    }
}
