namespace UnityEngine.Logging {
    public abstract class LogEnricher : ScriptableObject, ILogEnricher {
        public abstract string GetEnrichedLog(object source);
    }
}
