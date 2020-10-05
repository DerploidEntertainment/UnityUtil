using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.EventSystems;
using UnityEngine.Logging;

namespace UnityEngine.UI {
    /// <summary>
    /// This class is used to automatically scale an <see cref="EventSystem"/>'s <see cref="EventSystem.pixelDragThreshold"/> with respect to a <see cref="Canvas"/>.
    /// </summary>
    /// <remarks>
    /// See the comments by user @runevision on this Unity forum post for more info: https://forum.unity.com/threads/buttons-within-scroll-rect-are-difficult-to-press-on-mobile.265682/
    /// </remarks>
    public class DragThresholdScaler : Configurable {

        private const string TOOLTIP = nameof(EventSystem) + "'s " + nameof(EventSystems.EventSystem.pixelDragThreshold) + " will be scaled by the product of " + nameof(DragThresholdFactor) + " and " + nameof(CanvasScaler) + "'s " + nameof(UI.CanvasScaler.scaleFactor) + ". This means that the " + nameof(EventSystems.EventSystem.pixelDragThreshold) + " will scale as needed on screens with different pixel densities.";

        [Tooltip(TOOLTIP)]
        public EventSystem EventSystem;

        [Tooltip(TOOLTIP)]
        public CanvasScaler CanvasScaler;

        [Min(0f), Tooltip(TOOLTIP)]
        public int DragThresholdFactor;

        [Conditional("UNITY_EDITOR")]
        [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
        [SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Unity message")]
        private void Reset() => DragThresholdFactor = 5;

        protected override void Awake() {
            base.Awake();

            this.AssertAssociation(EventSystem, nameof(EventSystem));
            this.AssertAssociation(CanvasScaler, nameof(CanvasScaler));

            EventSystem.pixelDragThreshold = DragThresholdFactor * (int)CanvasScaler.scaleFactor;
        }

    }
}
