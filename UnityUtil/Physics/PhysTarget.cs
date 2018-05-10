using UnityEngine;

namespace UnityEngine {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PhysTarget : MonoBehaviour {
        public MonoBehaviour TargetComponent;
    }

}
