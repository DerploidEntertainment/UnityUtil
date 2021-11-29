﻿namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Collectible))]
    public class QuantityCollectible : MonoBehaviour {

        public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;

    }

}
