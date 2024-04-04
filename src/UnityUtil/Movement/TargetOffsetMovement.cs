using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Updating;

namespace UnityUtil.Movement;

public class TargetOffsetMovement : Updatable
{
    [Tooltip($"The Transform to keep at the given {nameof(Offset)} from the {nameof(Target)}")]
    [Required]
    public Transform? TransformToMove;

    [Tooltip($"The Transform being followed at the given {nameof(Offset)}")]
    [Required]
    public Transform? Target;

    [Tooltip($"The Offset at which to follow the {nameof(Target)} Transform")]
    public Vector3 Offset = new(0f, 0f, -10f);

    protected override void Awake()
    {
        base.Awake();

        RegisterUpdatesAutomatically = true;
        UpdateAction = move;
    }

    private void move(float deltaTime) => TransformToMove!.position = Target!.position + Offset;

}
