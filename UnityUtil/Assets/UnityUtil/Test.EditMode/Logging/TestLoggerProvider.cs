using UnityEngine;
using UnityEngine.Logging;

namespace UnityUtil.Test.EditMode.Logging {
    public class TestLoggerProvider : ILoggerProvider {
        public ILogger GetLogger(object source) => new TestLogger();
    }
}
