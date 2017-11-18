using UnityEngine;

namespace Danware.Unity.Updating {

    public class UpdaterSingleton : MonoBehaviour {

        private void Update() => Updater.Instance.UpdateAll();
        private void FixedUpdate() => FixedUpdater.Instance.FixedUpdateAll();

    }

}
