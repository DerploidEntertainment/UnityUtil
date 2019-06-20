namespace UnityEngine {

    public class DontDestroyOnLoad : MonoBehaviour {

        private void Awake() => DontDestroyOnLoad(gameObject);

    }

}
