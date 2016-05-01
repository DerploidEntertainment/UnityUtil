using UnityEngine;

using System.Collections;

namespace Danware.Unity3D.Inventory {

    public class Equippable : MonoBehaviour {
        // HIDDEN FIELDS
        private Transform _instance;

        // INSPECTOR FIELDS
        public Transform Model;
        public Vector3 ModelOffset = new Vector3(1f, -0.25f, 0.5f);
        public Vector3 ModelRotation = Vector3.zero;

        // API INTERFACE
        public void Equip(Transform inventory) {
            // If the Model has already been instantiated then just return
            if (_instance != null)
                return;

            // Otherwise, If a Model was provided, instantiate it and parent it to the Inventory
            _instance = Instantiate(Model);
            if (_instance != null) {
                _instance.parent = inventory;
                _instance.localPosition = ModelOffset;
                _instance.localRotation = Quaternion.Euler(ModelRotation);
            }
        }
    }

}
