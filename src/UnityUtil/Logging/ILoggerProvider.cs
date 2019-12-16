namespace UnityEngine.Logging {
    public interface ILoggerProvider {
        public ILogger GetLogger(object source);
    }
}
