using System;
using System.IO;
using UnityEngine;

namespace BelowTheBlue.Infrastructure.Configuration
{
    public static class ConfigLoader
    {
        public const string FileName = "config.json";

        public static string DefaultPath => Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", FileName));

        public static ConfigLoadResult LoadDefault()
        {
            return LoadOrCreate(DefaultPath);
        }

        public static ConfigLoadResult LoadOrCreate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A configuration file path is required.", nameof(filePath));
            }

            string fullPath = Path.GetFullPath(filePath);

            if (!File.Exists(fullPath))
            {
                GameConfig createdConfig = CreateDefault();

                try
                {
                    Save(fullPath, createdConfig);
                    return new ConfigLoadResult(createdConfig, fullPath, true);
                }
                catch (Exception exception) when (IsFileException(exception))
                {
                    return CreateFallbackResult(fullPath, exception);
                }
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                GameConfig config = CreateDefault();
                JsonUtility.FromJsonOverwrite(json, config);

                config.Validate();
                return new ConfigLoadResult(config, fullPath, false);
            }
            catch (Exception exception) when (IsFileException(exception) || exception is ArgumentException)
            {
                return CreateFallbackResult(fullPath, exception);
            }
        }

        public static void Save(string filePath, GameConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string fullPath = Path.GetFullPath(filePath);
            string directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            config.Validate();
            File.WriteAllText(fullPath, JsonUtility.ToJson(config, true));
        }

        private static GameConfig CreateDefault()
        {
            var config = new GameConfig();
            config.Validate();
            return config;
        }

        private static ConfigLoadResult CreateFallbackResult(string filePath, Exception exception)
        {
            return new ConfigLoadResult(
                CreateDefault(),
                filePath,
                false,
                $"Could not load configuration '{filePath}'. Defaults will be used. {exception.Message}");
        }

        private static bool IsFileException(Exception exception)
        {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is NotSupportedException;
        }
    }
}
