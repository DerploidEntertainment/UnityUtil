using Microsoft.Extensions.Logging;
using UnityEngine;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Physics;

/// <inheritdoc/>
internal static class PhysicsLoggerExtensions
{
    public static void DuplicateColliderFail(this MEL.ILogger logger, Collider collider) =>
        logger.LogWarning(new EventId(id: 0, nameof(DuplicateColliderFail)), $"{{{nameof(collider)}}} is not a BoxCollider, SphereCollider, or CapsuleCollider, so it will not be duplicated", collider.GetHierarchyName());
}
