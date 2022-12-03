using UnityEngine;
using UnityUtil.Logging;

namespace UnityUtil.Editor.Tests.Logging
{
    public class TestLoggerProvider : ILoggerProvider
    {
        public ILogger GetLogger(object source) => new TestLogger();
    }
}
