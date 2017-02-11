using UnityEngine;

namespace Danware.Unity {

    [RequireComponent(typeof(Collider))]
    public abstract class DetectorBase : MonoBehaviour {

        // HIDDEN FIELDS
        protected Collider _collider;

        // INSPECTOR FIELDS
        public DetectorResponder Responder;

        // EVENT HANDLERS
        private void Awake() {
            _collider = GetComponent<Collider>();
        }

    }

}