using UnityEngine;

namespace Danware.Unity {

    public class Destroyer : MonoBehaviour {

        // INSPECTOR FIELDS
        public GameObject ObjectToDestroy;

        // API INTERFACE
        public void DestroyGameObject() {
            Destroy(ObjectToDestroy);
        }
        public void DestroyGameObjectImmediate() {
            DestroyImmediate(ObjectToDestroy);
        }

    }

}
