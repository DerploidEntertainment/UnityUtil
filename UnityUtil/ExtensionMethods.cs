using System.Linq;

namespace System.Collections.Generic {

    public static class ExtensionMethods {

        public static void DoWith<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (T item in source)
                action(item);
        }

    }

}
