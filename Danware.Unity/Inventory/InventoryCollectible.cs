using System.Collections;
using UnityEngine;

namespace Danware.Unity.Inventory {

    [RequireComponent(typeof(Collectible))]
    public class InventoryCollectible : MonoBehaviour {

        // HIDDEN FIELDS
        private Collectible _collectible;
        private Transform _dropTrans;

        // INSPECTOR FIELDS
        [Tooltip("If there is a GameObject that gives this Collectible a physical representation, then reference it here.")]
        public GameObject PhysicalObject;
        public GameObject Item;
        [Tooltip("If dropped, the item will take this many seconds to become collectible again")]
        public float DropRefactoryPeriod = 1.5f;

        // EVENT HANDLERS
        private void Awake() {
            // Register Collectible events
            _collectible = GetComponent<Collectible>();
            _collectible.Collected += (sender, e) => doCollect(e.TargetRoot);
        }
        private void OnDisable() {
            doDrop();
        }

        // API INTERFACE
        public IEnumerator Drop() {
            if (!isActiveAndEnabled)
                yield return null;

            enabled = false;
            yield return new WaitForSeconds(DropRefactoryPeriod);
            enabled = true;
        }

        // HELPERS
        private void doCollect(Transform targetRoot) {
            if (!isActiveAndEnabled)
                return;

            // Try to give this Item to the target Inventory
            Inventory inv = targetRoot.GetComponentInChildren<Inventory>();
            bool success = (inv == null) ? false : inv.Give(this);
            if (!success)
                return;

            // If it was successfully given, then do collect actions
            PhysicalObject.SetActive(false);
            Item.transform.parent = inv.transform;
            Item.transform.localPosition = new Vector3(0f, 0f, 0f);
            Item.transform.localRotation = Quaternion.identity;
            _dropTrans = targetRoot;
        }
        private void doDrop() {
            // Drop it as a new Collectible
            PhysicalObject.SetActive(true);
            PhysicalObject.transform.position = _dropTrans.position;
            Item.transform.parent = transform;
        }
    }

}
