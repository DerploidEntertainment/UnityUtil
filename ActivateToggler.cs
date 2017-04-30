using UnityEngine;

namespace Danware.Unity {

    public class ActivateToggler : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject ObjectToToggle;

        // API INTERFACE
        public void Activate() {
            ObjectToToggle.SetActive(true);
        }
        public void Deactivate() {
            ObjectToToggle.SetActive(false);
        }

    }

}
