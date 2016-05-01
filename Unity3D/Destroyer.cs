using UnityEngine;

namespace Danware.Unity3D {

    public class Destroyer : MonoBehaviour {
        // API INTERFACE
        public void DestroyGameObject() {
            Destroy(this.gameObject);
        }
        public void DestroyGameObjectImmediate() {
            DestroyImmediate(this.gameObject);
        }
    }

}
