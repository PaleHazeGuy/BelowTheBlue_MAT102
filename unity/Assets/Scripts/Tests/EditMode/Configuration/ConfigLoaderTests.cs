using System.IO;
using BelowTheBlue.Infrastructure.Configuration;
using NUnit.Framework;

namespace BelowTheBlue.Tests.Configuration
{
    public sealed class ConfigLoaderTests
    {
        private string directory;
        private string configPath;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), $"BelowTheBlue-{System.Guid.NewGuid():N}");
            configPath = Path.Combine(directory, ConfigLoader.FileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void LoadOrCreate_WhenMissing_CreatesDefaultConfig()
        {
            ConfigLoadResult result = ConfigLoader.LoadOrCreate(configPath);

            Assert.That(result.WasCreated, Is.True);
            Assert.That(result.HasWarning, Is.False);
            Assert.That(File.Exists(configPath), Is.True);
            Assert.That(result.Config.serial.port, Is.EqualTo("auto"));
            Assert.That(result.Config.serial.preferredDeviceName, Is.EqualTo("Sigma Controller"));
        }

        [Test]
        public void LoadOrCreate_ReadsConfiguredValues()
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(configPath,
                "{\"serial\":{\"port\":\"COM7\"},\"input\":{\"deadZone\":0.25}}");

            ConfigLoadResult result = ConfigLoader.LoadOrCreate(configPath);

            Assert.That(result.WasCreated, Is.False);
            Assert.That(result.HasWarning, Is.False);
            Assert.That(result.Config.serial.port, Is.EqualTo("COM7"));
            Assert.That(result.Config.serial.baudRate, Is.EqualTo(115200));
            Assert.That(result.Config.input.deadZone, Is.EqualTo(0.25f));
        }

        [Test]
        public void LoadOrCreate_ClampsInvalidRanges()
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(configPath,
                "{\"serial\":{\"readTimeoutMs\":200,\"packetTimeoutMs\":10},"
                + "\"input\":{\"deadZone\":4.0}}");

            GameConfig config = ConfigLoader.LoadOrCreate(configPath).Config;

            Assert.That(config.serial.packetTimeoutMs, Is.EqualTo(200));
            Assert.That(config.input.deadZone, Is.EqualTo(0.95f));
        }

        [Test]
        public void LoadOrCreate_WhenMalformed_UsesDefaultsWithoutReplacingFile()
        {
            const string malformedJson = "{not-json";
            Directory.CreateDirectory(directory);
            File.WriteAllText(configPath, malformedJson);

            ConfigLoadResult result = ConfigLoader.LoadOrCreate(configPath);

            Assert.That(result.HasWarning, Is.True);
            Assert.That(result.Config.serial.port, Is.EqualTo("auto"));
            Assert.That(File.ReadAllText(configPath), Is.EqualTo(malformedJson));
        }
    }
}
