using UnityEngine;

namespace UnityUtil {

    public class DontDestroy : MonoBehaviour {

        private void Awake() => DontDestroyOnLoad(gameObject);

    }

}
