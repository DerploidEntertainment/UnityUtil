using UnityEngine;

using Danware.Unity.Input;

namespace Danware.Unity {

    public class Use : MonoBehaviour {
        // INSPECTOR FIELDS
        public float Reach = 5f;
        public LayerMask UseLayer;

        // API INTERFACE
        public static StartStopInput UseInput { get; set; }

        // EVENT HANDLERS
        private void Update() {
            // Get user input
            bool use = UseInput.Started;

            // Use the Useable currently being looked at
            if (use) {
                Useable u = objAhead();
                if (u != null)
                    u.Use();
            }
        }
        private Useable objAhead() {
            Useable uAhead = null;

            // Locate any object on the Use layer that is within reach
            RaycastHit hitInfo;
            bool useableAhead = Physics.Raycast(transform.position, transform.forward, out hitInfo, Reach, UseLayer);
            if (useableAhead)
                uAhead = hitInfo.transform.GetComponent<Useable>();

            return uAhead;
        }
    }

}
