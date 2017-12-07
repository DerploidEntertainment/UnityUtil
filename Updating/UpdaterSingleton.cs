using UnityEngine;
using UnityEngine.Assertions;

namespace Danware.Unity.Updating {

    [DisallowMultipleComponent]
    public class UpdaterSingleton : MonoBehaviour {

        // HIDDEN FIELDS
        private static int s_refs = 0;

        // EVENT HANDLERS
        private void Awake() {
            // Make sure this component is a singleton
            ++s_refs;
            Assert.IsTrue(s_refs == 1, $"There can be only one instance of {typeof(UpdaterSingleton)} in a scene!  You have {s_refs}!");
        }

        private void Update()      => Updater.Instance.UpdateAll();
        private void FixedUpdate() => Updater.Instance.FixedUpdateAll();
        private void LateUpdate()  => Updater.Instance.LateUpdateAll();

    }

}
