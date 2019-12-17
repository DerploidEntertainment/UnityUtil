using UnityEngine;
using UnityEngine.Logging;

namespace UnityUtil.Test.EditMode {
    public class TestLoggerProvider : ILoggerProvider {
        public ILogger GetLogger(object source) => Debug.unityLogger;
    }
}
