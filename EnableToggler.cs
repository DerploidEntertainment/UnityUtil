using UnityEngine;

namespace Danware.Unity {

    public class EnableToggler : MonoBehaviour {

        // INSPECTOR FIELDS
        public MonoBehaviour ComponentToToggle;

        // API INTERFACE
        public void EnableComponent() {
            ComponentToToggle.enabled = true;
        }
        public void DisableComponent() {
            ComponentToToggle.enabled = false;
        }

    }

}
