using UnityEngine;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Movement;

public class LookAt : Updatable
{

    [Tooltip(
        $"This Transform will be rotated to look at the {nameof(TransformToRotate)} or {nameof(TagToLookAt)} " +
        "depending on which is provided."
    )]
    public Transform? TransformToRotate;

    [Tooltip(
        $"The {nameof(TransformToRotate)} will be rotated to look at this Transform. " +
        $"This value overrides the {nameof(TagToLookAt)} field."
    )]
    public Transform? TransformToLookAt;

    [Tooltip(
        $"The {nameof(TransformToRotate)} will be rotated to look at the first GameObject with this Tag. " +
        "Useful for when the object/transform to be looked at will change at runtime."
    )]
    public string? TagToLookAt = null;

    public bool FlipOnLocalY;

    protected override void Awake()
    {
        base.Awake();

        AddUpdate(look);
    }
    private void look(float deltaTime)
    {
        if (TransformToRotate == null || (TransformToLookAt == null && TagToLookAt is null))
            return;

        Transform? target = TagToLookAt is null ? TransformToLookAt : GameObject.FindWithTag(TagToLookAt)?.transform;
        if (target != null) {
            TransformToRotate.LookAt(target, -U.Physics.gravity);
            if (FlipOnLocalY)
                TransformToRotate.localRotation *= Quaternion.Euler(180f * Vector3.up);
        }
    }

}
