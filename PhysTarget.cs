using UnityEngine;

namespace Danware.Unity {

    [RequireComponent(typeof(Collider))]
    public class PhysTarget : MonoBehaviour {
        public MonoBehaviour TargetComponent;
    }

}
