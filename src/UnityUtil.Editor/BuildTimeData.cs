using System.Collections.Generic;

namespace UnityUtil.Editor {
    public static class BuildTimeData {

        private readonly static Dictionary<string, object> s_dict = new Dictionary<string, object>();

        public static bool IsAutoBuild { get; set; } = false;
        public static IReadOnlyDictionary<string, object> All => s_dict;
        public static bool TryGetValue(string key, out object value) => s_dict.TryGetValue(key, out value);
        public static void SetValue(string key, object value) => s_dict[key] = value;
    }
}
