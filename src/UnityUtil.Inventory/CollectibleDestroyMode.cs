using UnityEngine;

namespace UnityUtil.Inventory;

/// <summary>
/// Determines under what circumstances, if any, a Collectible's <see cref="GameObject"/> will be destroyed.
/// </summary>
public enum CollectibleDestroyMode
{
    /// <summary>
    /// Never destroy the Collectible due to detection/collection.
    /// </summary>
    Never,

    /// <summary>
    /// Destroy the Collectible anytime it is detected, whehter the Detector uses it or not.
    /// </summary>
    WhenDetected,

    /// <summary>
    /// Destroy the Collectible only when it is used by the Detector.
    /// </summary>
    WhenUsed,

    /// <summary>
    /// Destroy the Collectible only when its contained value is depeleted, potentially allowing multiple collect events.
    /// </summary>
    WhenEmptied,
}
