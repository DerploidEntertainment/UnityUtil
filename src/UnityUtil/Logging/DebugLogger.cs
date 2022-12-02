using System;
using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Logging;

public class DebugLogger : ILogger
{

    private readonly Func<string> _logEnricher;

    public DebugLogger(Func<string> logEnricher)
    {
        _logEnricher = logEnricher;
    }

    public ILogHandler logHandler { get => Debug.unityLogger.logHandler; set => Debug.unityLogger.logHandler = value; }
    public bool logEnabled { get => Debug.unityLogger.logEnabled; set => Debug.unityLogger.logEnabled = value; }
    public LogType filterLogType { get => Debug.unityLogger.filterLogType; set => Debug.unityLogger.filterLogType = value; }

    public bool IsLogTypeAllowed(LogType logType) => Debug.unityLogger.IsLogTypeAllowed(logType);
    public void Log(LogType logType, object message) => Debug.unityLogger.Log(logType, GetEnrichedLog(message));
    public void Log(LogType logType, object message, U.Object context) => Debug.unityLogger.Log(logType, message: GetEnrichedLog(message), context);
    public void Log(LogType logType, string tag, object message) => Debug.unityLogger.Log(logType, tag, GetEnrichedLog(message));
    public void Log(LogType logType, string tag, object message, U.Object context) => Debug.unityLogger.Log(logType, tag, GetEnrichedLog(message), context);
    public void Log(object message) => Debug.unityLogger.Log(GetEnrichedLog(message));
    public void Log(string tag, object message) => Debug.unityLogger.Log(tag, GetEnrichedLog(message));
    public void Log(string tag, object message, U.Object context) => Debug.unityLogger.Log(tag, GetEnrichedLog(message), context);
    public void LogError(string tag, object message) => Debug.unityLogger.LogError(tag, GetEnrichedLog(message));
    public void LogError(string tag, object message, U.Object context) => Debug.unityLogger.LogError(tag, GetEnrichedLog(message), context);
    public void LogException(Exception exception) => Debug.unityLogger.LogException(exception);
    public void LogException(Exception exception, U.Object context) => Debug.unityLogger.LogException(exception, context);
    public void LogFormat(LogType logType, string format, params object[] args) => Debug.unityLogger.LogFormat(logType, GetEnrichedLog(format), args);
    public void LogFormat(LogType logType, U.Object context, string format, params object[] args) => Debug.unityLogger.LogFormat(logType, context, GetEnrichedLog(format), args);
    public void LogWarning(string tag, object message) => Debug.unityLogger.LogWarning(tag, GetEnrichedLog(message));
    public void LogWarning(string tag, object message, U.Object context) => Debug.unityLogger.LogWarning(tag, GetEnrichedLog(message), context);

    internal string GetEnrichedLog(object message) => _logEnricher() + message.ToString();
}
