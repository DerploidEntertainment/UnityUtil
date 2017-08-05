using System;

using UnityEngine;

namespace Danware.Unity {

    [Serializable]
    public class UnityLayer {

        [SerializeField]
        private int _layerIndex = 0;

        public int Mask => 1 << _layerIndex;
        public int LayerIndex {
            get => _layerIndex;
            set {
                if (0 < value && value < 32)
                    _layerIndex = value;
            }
        }
    }

}
