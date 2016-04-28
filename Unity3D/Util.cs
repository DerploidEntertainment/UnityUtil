using UnityEngine;

namespace Danware.Unity3D {
    public static class Util {
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
