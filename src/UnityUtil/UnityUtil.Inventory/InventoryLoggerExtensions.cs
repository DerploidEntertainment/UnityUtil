using Microsoft.Extensions.Logging;

namespace UnityUtil.Inventory;

/// <inheritdoc/>
internal static class InventoryLoggerExtensions
{
    #region Information

    public static void Collected(this ILogger logger, InventoryCollectible collectible) =>
        logger.LogInformation(new EventId(id: 0, nameof(Collected)), $"Collected {{{nameof(collectible)}}}", collectible.ItemRoot!.GetHierarchyName());

    public static void Dropped(this ILogger logger, InventoryCollectible collectible) =>
        logger.LogInformation(new EventId(id: 0, nameof(Dropped)), $"Dropped {{{nameof(collectible)}}}", collectible.ItemRoot!.GetHierarchyName());

    #endregion

    #region Warning

    public static void WeaponGizmosUnknownPhysicsCastShape(this ILogger logger, PhysicsCastShape shape) =>
        logger.LogWarning(new EventId(id: 0, nameof(WeaponGizmosUnknownPhysicsCastShape)), $"Could not draw {nameof(Weapon)} Gizmos for {nameof(PhysicsCastShape)} {{{nameof(shape)}}}", shape);

    #endregion

}
