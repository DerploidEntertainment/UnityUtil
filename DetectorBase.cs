using UnityEngine;

namespace Danware.Unity {

    [RequireComponent(typeof(Collider))]
    public abstract class DetectorBase : MonoBehaviour {

        public DetectorResponder Responder;

    }

}