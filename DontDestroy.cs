using UnityEngine;

namespace Danware.Unity {

    public class DontDestroy : MonoBehaviour {

        private void Awake() => DontDestroyOnLoad(gameObject);

    }

}
