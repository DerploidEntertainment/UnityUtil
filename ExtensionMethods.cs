using System.Linq;

namespace System.Collections.Generic {

    public static class ExtensionMethods {

        public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T> source) where T : class =>
            source.Where(s => s != null);
        public static IEnumerable<T> WhereNonDefault<T>(this IEnumerable<T> source) where T : struct =>
            source.Where(s => !s.Equals(default(T)));
        public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class =>
            source.Where(s => s != null && predicate(s));
        public static IEnumerable<T> WhereNonDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct =>
            source.Where(s => !s.Equals(default(T)) && predicate(s));

        public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : class =>
            source.Select(s => selector(s)).Where(s => s != null);
        public static IEnumerable<TResult> SelectNonDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : struct =>
            source.Select(s => selector(s)).Where(s => !s.Equals(default(TResult)));

        public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult> selector) where TResult : class =>
            source.Select((s, index) => selector(s, index)).Where(s => s != null);
        public static IEnumerable<TResult> SelectNonDefault<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult> selector) where TResult : struct =>
            source.Select((s, index) => selector(s, index)).Where(s => !s.Equals(default(TResult)));

        public static void DoWith<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (T item in source)
                action(item);
        }

    }

}
