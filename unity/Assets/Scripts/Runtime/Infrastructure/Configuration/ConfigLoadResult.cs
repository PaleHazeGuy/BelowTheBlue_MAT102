namespace BelowTheBlue.Infrastructure.Configuration
{
    public sealed class ConfigLoadResult
    {
        public GameConfig Config { get; }
        public string FilePath { get; }
        public bool WasCreated { get; }
        public string Warning { get; }

        public bool HasWarning => !string.IsNullOrEmpty(Warning);

        internal ConfigLoadResult(
            GameConfig config,
            string filePath,
            bool wasCreated,
            string warning = null)
        {
            Config = config;
            FilePath = filePath;
            WasCreated = wasCreated;
            Warning = warning;
        }
    }
}
