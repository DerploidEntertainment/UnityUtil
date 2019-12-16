namespace UnityEngine.Logging {

    public static class LoggerExtensions {

        public static void LogWarning(this ILogger logger, object message) => logger.Log(LogType.Warning, message);
        public static void LogError(this ILogger logger, object message) => logger.Log(LogType.Error, message);

    }
}
