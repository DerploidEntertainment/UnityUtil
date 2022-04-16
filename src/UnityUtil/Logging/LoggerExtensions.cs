namespace UnityEngine.Logging;

public static class LoggerExtensions
{

    public static void LogWarning(this ILogger logger, object message) => logger.Log(LogType.Warning, message);
    public static void LogError(this ILogger logger, object message) => logger.Log(LogType.Error, message);

    public static void Log(this ILogger logger, object message, Object context) => logger.Log(LogType.Log, message, context);
    public static void LogWarning(this ILogger logger, object message, Object context) => logger.Log(LogType.Warning, message, context);
    public static void LogError(this ILogger logger, object message, Object context) => logger.Log(LogType.Error, message, context);

}
