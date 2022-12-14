using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

namespace UnityUtil.Physics;

/// <inheritdoc/>
internal class PhysicsLogger<T> : BaseUnityUtilLogger<T>
{
    public PhysicsLogger(ILoggerFactory loggerFactory, T context)
        : base(loggerFactory, context, eventIdOffset: 10_000) { }

    #region Information

    #endregion

    #region Warning

    public void DuplicateColliderFail(Collider collider) =>
        Log(id: 0, nameof(DuplicateColliderFail), Warning, $"{{{nameof(collider)}}} is not a BoxCollider, SphereCollider, or CapsuleCollider, so it will not be duplicated", collider.GetHierarchyName());

    #endregion

    #region Error

    #endregion

}
