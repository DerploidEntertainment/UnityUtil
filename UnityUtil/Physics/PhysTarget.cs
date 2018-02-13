using UnityEngine;

namespace UnityUtil {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PhysTarget : MonoBehaviour {
        public MonoBehaviour TargetComponent;
    }

}
