using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.Logging;

namespace UnityUtil.Physics;

/// <inheritdoc/>
internal class PhysicsLogger<T>(ILoggerFactory loggerFactory, T context)
    : BaseUnityUtilLogger<T>(loggerFactory, context, eventIdOffset: 10_000)
{

    #region Warning

    public void DuplicateColliderFail(Collider collider) =>
        LogWarning(id: 0, nameof(DuplicateColliderFail), $"{{{nameof(collider)}}} is not a BoxCollider, SphereCollider, or CapsuleCollider, so it will not be duplicated", collider.GetHierarchyName());

    #endregion
}
