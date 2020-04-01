using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace UnityEngine.UI {
    [Serializable]
    public class UiBreakpoint
    {
        [Min(0f), TableColumnWidth(20, Resizable = true)]
        public float Value;
        [Tooltip("This event is raised when this breakpoint's value matches the corresponding property of the display (screen width/height, camera aspect ratio, etc.). Use this event to (de)activate GameObjects, adjust RectTransform properties, etc. This event should only be used to trigger UI changes; game logic should be triggered elsewhere. Not only is this good design, separating logic from presentation, but it also allows this event to be raised without issue in the Editor, so you can tweak UI at design time. Moreover, this event MAY BE RAISED MULTIPLE TIMES on game start, so any game logic would also be triggered multiple times. Remember that handlers for this event will only run in the Editor if they are set to run in 'Editor and Runtime'.")]
        public UnityEvent Matched = new UnityEvent();

        public UiBreakpoint(float value) {
            Assert.IsTrue(value >= 0f, "UI breakpoints can only be defined for non-negative values");
            Value = value;
        }
    }
}
