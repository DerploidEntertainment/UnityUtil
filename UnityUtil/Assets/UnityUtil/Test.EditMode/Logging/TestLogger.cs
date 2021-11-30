using System;
using UnityEngine;

namespace UnityUtil.Test.EditMode.Logging
{
    internal class TestLogger : ILogger
    {

        public ILogHandler logHandler { get; set; }
        public bool logEnabled { get; set; }
        public LogType filterLogType { get; set; }

        public int NumLogs { get; private set; } = 0;
        public int NumAsserts { get; private set; } = 0;
        public int NumWarnings { get; private set; } = 0;
        public int NumErrors { get; private set; } = 0;
        public int NumExceptions { get; private set; } = 0;

        public bool IsLogTypeAllowed(LogType logType) => true;

        public void Log(LogType logType, object message) => incrementLogCounts(logType);
        public void Log(LogType logType, object message, UnityEngine.Object context) => incrementLogCounts(logType);
        public void Log(LogType logType, string tag, object message) => incrementLogCounts(logType);
        public void Log(LogType logType, string tag, object message, UnityEngine.Object context) => incrementLogCounts(logType);
        public void Log(object message) => incrementLogCounts(LogType.Log);
        public void Log(string tag, object message) => incrementLogCounts(LogType.Log);
        public void Log(string tag, object message, UnityEngine.Object context) => incrementLogCounts(LogType.Log);

        public void LogError(string tag, object message) => incrementLogCounts(LogType.Error);
        public void LogError(string tag, object message, UnityEngine.Object context) => incrementLogCounts(LogType.Error);

        public void LogException(Exception exception) => incrementLogCounts(LogType.Exception);
        public void LogException(Exception exception, UnityEngine.Object context) => incrementLogCounts(LogType.Exception);

        public void LogFormat(LogType logType, string format, params object[] args) => incrementLogCounts(logType);
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) => incrementLogCounts(logType);

        public void LogWarning(string tag, object message) => incrementLogCounts(LogType.Warning);
        public void LogWarning(string tag, object message, UnityEngine.Object context) => incrementLogCounts(LogType.Warning);

        private void incrementLogCounts(LogType logType)
        {
            switch (logType) {
                case LogType.Error: ++NumErrors; break;
                case LogType.Assert: ++NumAsserts; break;
                case LogType.Warning: ++NumWarnings; break;
                case LogType.Log: ++NumLogs; break;
                case LogType.Exception: ++NumExceptions; break;
                default: throw UnityObjectExtensions.SwitchDefaultException(logType);
            }
        }
    }

}
