using UnityEngine;

using System.Collections;

namespace Danware.Unity.Inventory {
    
    [RequireComponent(typeof(Collider))]
    public abstract class Collectible : MonoBehaviour {
        // HIDDEN FIELDS
        private int _collectLayer;
        // API INTERFACE
        public void Collect(Transform targetRoot) {
            doCollect(targetRoot);
        }
        public void Drop(Transform target) {
            doDrop(target);
        }
        
        // INSPECTOR FIELDS
        [Tooltip("If there is a GameObject that gives this Collectible a physical representation, then reference it here.")]
        public GameObject PhysicalObject;
        [Tooltip("If dropped, the item will take this many seconds to become collectible again")]
        public float DropRefactoryPeriod = 1.5f;

        // HELPER FUNCTIONS
        protected virtual void doCollect(Transform targetRoot) { }
        protected virtual void doDrop(Transform target) {
            // Prevent the Collectible from being collected again until the refactory period has passed
            StartCoroutine(pauseCollectibility());
        }
        private IEnumerator pauseCollectibility() {
            _collectLayer = gameObject.layer;
            gameObject.layer = LayerMask.NameToLayer("Default");
            yield return new WaitForSeconds(DropRefactoryPeriod);
            gameObject.layer = _collectLayer;
        }
    }

}
