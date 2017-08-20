using UnityEngine;

namespace Danware.Unity {

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PhysTarget : MonoBehaviour {
        public MonoBehaviour TargetComponent;
    }

}
