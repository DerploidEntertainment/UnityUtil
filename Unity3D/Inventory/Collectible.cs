using UnityEngine;

namespace Danware.Unity3D.Inventory {

    [RequireComponent(typeof(Rigidbody))]    // So it can be detected
    public class Collectible : MonoBehaviour {
        // INSPECTOR FIELDS
        public GameObject Item;

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(Item != null, "Collectible {0} was not given an object!", this.name);
        }
    }

}
