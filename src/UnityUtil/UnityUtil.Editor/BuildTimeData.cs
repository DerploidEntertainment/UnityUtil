using System.Collections.Generic;

namespace UnityUtil.Editor;

public static class BuildTimeData
{
    private static readonly Dictionary<string, object> DATA = [];

    public static bool IsAutoBuild { get; set; }
    public static IReadOnlyDictionary<string, object> All => DATA;
    public static bool TryGetValue(string key, out object value) => DATA.TryGetValue(key, out value);
    public static void SetValue(string key, object value) => DATA[key] = value;
}
