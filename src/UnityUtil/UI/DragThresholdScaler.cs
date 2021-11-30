using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// This class is used to automatically scale an <see cref="EventSystem"/>'s <see cref="EventSystem.pixelDragThreshold"/> with respect to a <see cref="Canvas"/>.
    /// </summary>
    /// <remarks>
    /// See the comments by user @runevision on this Unity forum post for more info: https://forum.unity.com/threads/buttons-within-scroll-rect-are-difficult-to-press-on-mobile.265682/
    /// </remarks>
    public class DragThresholdScaler : Configurable
    {
        private const string TOOLTIP =
            $"{nameof(EventSystem)}'s {nameof(EventSystems.EventSystem.pixelDragThreshold)} will be scaled by the product of " +
            $"{nameof(DragThresholdFactor)} and {nameof(CanvasScaler)}'s {nameof(UI.CanvasScaler.scaleFactor)}. " +
            $"This means that the {nameof(EventSystems.EventSystem.pixelDragThreshold)} will scale as needed on screens with different pixel densities.";

        [Tooltip(TOOLTIP), Required]
        public EventSystem? EventSystem;

        [Tooltip(TOOLTIP), Required]
        public CanvasScaler? CanvasScaler;

        [Min(0f), Tooltip(TOOLTIP)]
        public int DragThresholdFactor = 5;

        protected override void Awake() {
            base.Awake();

            EventSystem.pixelDragThreshold = DragThresholdFactor * (int)CanvasScaler.scaleFactor;
        }

    }
}
