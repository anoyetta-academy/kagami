using RainbowMage.OverlayPlugin;

namespace kagami
{
    public static class Logger
    {
        public delegate void LogDelegate(LogLevel level, string message, params object[] args);

        public static LogDelegate LogCallback { get; set; }

        public static void Error(string mesasge, params object[] args) => LogCallback?.Invoke(LogLevel.Error, mesasge);

        public static void Warn(string mesasge, params object[] args) => LogCallback?.Invoke(LogLevel.Warning, mesasge);

        public static void Info(string mesasge, params object[] args) => LogCallback?.Invoke(LogLevel.Info, mesasge);
    }
}
