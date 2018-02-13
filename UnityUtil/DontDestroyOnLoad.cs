using UnityEngine;

namespace UnityUtil {

    public class DontDestroyOnLoad : MonoBehaviour {

        private void Awake() => DontDestroyOnLoad(gameObject);

    }

}
