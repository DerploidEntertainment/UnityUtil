using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Updating {

    [DisallowMultipleComponent]
    public class UpdaterSingleton : MonoBehaviour {

        // HIDDEN FIELDS
        private static int s_refCount = 0;

        // EVENT HANDLERS
        private void Awake() {
            // Make sure this component is a singleton
            ++s_refCount;
            Assert.IsTrue(s_refCount == 1, $"There can be only one instance of {typeof(UpdaterSingleton)} in a scene!  You have {s_refCount}!");
        }

        private void Update()      => Updater.Instance.UpdateAll();
        private void FixedUpdate() => Updater.Instance.FixedUpdateAll();
        private void LateUpdate()  => Updater.Instance.LateUpdateAll();

    }

}
