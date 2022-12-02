using UnityEngine;

namespace UnityUtil.Inventories;

[RequireComponent(typeof(Collectible))]
public class QuantityCollectible : MonoBehaviour
{

    public ManagedQuantity.ChangeMode ChangeMode = ManagedQuantity.ChangeMode.Absolute;

}
