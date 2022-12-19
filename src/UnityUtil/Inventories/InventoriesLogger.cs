using Microsoft.Extensions.Logging;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Inventories;

/// <inheritdoc/>
internal class InventoriesLogger<T> : BaseUnityUtilLogger<T>
{
    public InventoriesLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 10_000) { }

    #region Information

    public void Collected(InventoryCollectible collectible) =>
        Log(id: 0, nameof(Collected), Information, $"Collected {{{nameof(collectible)}}}", collectible.ItemRoot!.GetHierarchyName());

    public void Dropped(InventoryCollectible collectible) =>
        Log(id: 1, nameof(Dropped), Information, $"Dropped {{{nameof(collectible)}}}", collectible.ItemRoot!.GetHierarchyName());

    #endregion

    #region Warning

    public void WeaponGizmosUnknownPhysicsCastShape(PhysicsCastShape shape) =>
        Log(id: 0, nameof(WeaponGizmosUnknownPhysicsCastShape), Warning, $"Could not draw {nameof(Weapon)} Gizmos for {nameof(PhysicsCastShape)} {{{nameof(shape)}}}", shape);

    #endregion

    #region Error

    #endregion

}
