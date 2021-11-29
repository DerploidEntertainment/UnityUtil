namespace UnityEngine.Logging {

    [CreateAssetMenu(menuName = nameof(UnityUtil) + "/" + nameof(UnityEngine.Logging) + "/" + nameof(TypeNameLogEnricher), fileName = "type-name-log-enricher")]
    public class TypeNameLogEnricher : LogEnricher {

        [Tooltip("'{0}' will be replaced by the name of the type. Read more about .NET composite formatting here: https://docs.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting")]
        public string FormatString = "Type {0}";

        public override string GetEnrichedLog(object source) =>
            source is null ? "" : string.Format(FormatString, source.GetType().Name);
    }

}
