using Microsoft.Extensions.Logging;
using UnityUtil.Logging;

namespace UnityUtil.Inventory;

/// <inheritdoc/>
internal class InventoriesLogger<T>(ILoggerFactory loggerFactory, T context)
    : BaseUnityUtilLogger<T>(loggerFactory, context, eventIdOffset: 10_000)
{

    #region Information

    public void Collected(InventoryCollectible collectible) =>
        LogInformation(id: 0, nameof(Collected), $"Collected {{{nameof(collectible)}}}", collectible.ItemRoot!.GetHierarchyName());

    public void Dropped(InventoryCollectible collectible) =>
        LogInformation(id: 1, nameof(Dropped), $"Dropped {{{nameof(collectible)}}}", collectible.ItemRoot!.GetHierarchyName());

    #endregion

    #region Warning

    public void WeaponGizmosUnknownPhysicsCastShape(PhysicsCastShape shape) =>
        LogWarning(id: 0, nameof(WeaponGizmosUnknownPhysicsCastShape), $"Could not draw {nameof(Weapon)} Gizmos for {nameof(PhysicsCastShape)} {{{nameof(shape)}}}", shape);

    #endregion

}
