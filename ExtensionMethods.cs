using UnityEngine;

using System;
using System.Linq;

namespace Danware.Unity {

    public static class ExtensionMethods {
        
        public static T Raycast<T>(this MonoBehaviour obj, Vector3 origin, Vector3 direction) where T : MonoBehaviour {
            RaycastHit hitInfo;
            return doRaycast<T>(origin, direction, out hitInfo);
        }
        public static T Raycast<T>(this MonoBehaviour obj, Vector3 origin, Vector3 direction, float maxDistance) where T : MonoBehaviour {
            RaycastHit hitInfo;
            return doRaycast<T>(origin, direction, out hitInfo, maxDistance);
        }
        public static T Raycast<T>(this MonoBehaviour obj, Vector3 origin, Vector3 direction, out RaycastHit hitInfo) where T : MonoBehaviour {
            return doRaycast<T>(origin, direction, out hitInfo);
        }
        public static T Raycast<T>(this MonoBehaviour obj, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance) where T : MonoBehaviour {
            return doRaycast<T>(origin, direction, out hitInfo, maxDistance);
        }
        public static T Raycast<T>(this MonoBehaviour obj, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, QueryTriggerInteraction queryTriggerInteraction) where T : MonoBehaviour {
            return doRaycast<T>(origin, direction, out hitInfo, maxDistance, queryTriggerInteraction);
        }
        private static T doRaycast<T>(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) where T : MonoBehaviour {
            T result = null;

            bool hit = Physics.Raycast(origin, direction, out hitInfo, maxDistance, Physics.DefaultRaycastLayers, queryTriggerInteraction);
            if (hit)
                result = hitInfo.collider.GetComponent<T>();

            return result;
        }      

    }

}

namespace System.Collections.Generic {

    public static class ExtensionMethods {

        public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T> source) where T : class {
            return source.Where(s => s != null);
        }
        public static IEnumerable<T> WhereNonDefault<T>(this IEnumerable<T> source) where T : struct {
            return source.Where(s => !s.Equals(default(T)));
        }
        public static IEnumerable<T> WhereNonNull<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class {
            return source.Where(s => s != null && predicate(s));
        }
        public static IEnumerable<T> WhereNonDefault<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : struct {
            return source.Where(s => !s.Equals(default(T)) && predicate(s));
        }

        public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : class {
            return source.Select(s => selector(s)).Where(s => s != null);
        }
        public static IEnumerable<TResult> SelectNonDefault<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector) where TResult : struct {
            return source.Select(s => selector(s)).Where(s => !s.Equals(default(TResult)));
        }

        public static IEnumerable<TResult> SelectNonNull<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult> selector) where TResult : class {
            return source.Select((s, index) => selector(s, index)).Where(s => s != null);
        }
        public static IEnumerable<TResult> SelectNonDefault<T, TResult>(this IEnumerable<T> source, Func<T, int, TResult> selector) where TResult : struct {
            return source.Select((s, index) => selector(s, index)).Where(s => !s.Equals(default(TResult)));
        }

        public static void DoWith<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (T item in source)
                action(item);
        }

    }

}
