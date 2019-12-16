namespace UnityEngine.Logging {
    public interface ILogEnricher {
        public string GetEnrichedLog(object source);
    }
}
