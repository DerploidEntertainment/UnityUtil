using Sirenix.OdinInspector;
using System.Diagnostics.CodeAnalysis;

namespace UnityEngine.Movement {

    public class TargetOffsetMovement : MonoBehaviour
    {
        [Tooltip($"The Transform to keep at the given {nameof(Offset)} from the {nameof(Target)}")]
        [Required]
        public Transform? TransformToMove;

        [Tooltip($"The Transform being followed at the given {nameof(Offset)}")]
        [Required]
        public Transform? Target;

        [Tooltip($"The Offset at which to follow the {nameof(Target)} Transform")]
        public Vector3 Offset = new(0f, 0f, -10f);

        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Update() => TransformToMove.position = Target.position + Offset;

    }

}
