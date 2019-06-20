namespace UnityEngine.Inventory {

    [RequireComponent(typeof(Collectible))]
    public class QuantityCollectible : MonoBehaviour {

        // INSPECTOR FIELDS
        public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;

    }

}
