using UnityEngine;
using UnityUtil.Updating;

namespace UnityUtil.Movement;

/// <summary>
/// Tells all associated <see cref="LookAt"/> components when to look at a new <see cref="Transform"/>.
/// </summary>
public class LookAtAdjuster : Updatable
{
    [Tooltip($"These are the {nameof(LookAt)} components that will be told what new Transforms to look at.")]
    public LookAt[] AssociatedLookAts = [];

    /// <summary>
    /// You can actually set <see cref="LookAt.TransformToLookAt"/> directly.  This function was only created for use with UnityEvents.
    /// </summary>
    /// <param name="transform">The new <see cref="Transform"/> to make the <see cref="LookAt.TransformToRotate"/> look at.</param>
    public void SetTransformToLookAt(Transform transform)
    {
        for (int la = 0; la < AssociatedLookAts.Length; ++la)
            AssociatedLookAts[la].TransformToLookAt = transform;
    }
}
