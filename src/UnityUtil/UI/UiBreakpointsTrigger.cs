using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public class UiBreakpointsTrigger : MonoBehaviour
    {
        public UiBreakpoints UiBreakpoints;

        [ValidateInput(nameof(isNumBreakpointsValid), ContinuousValidationCheck = true)]
        [Tooltip(
            $"Define one event for each breakpoint in the associated {nameof(UiBreakpoints)}. " +
            $"Every time {nameof(Trigger)} is invoked, the events corresponding to the currently matching breakpoints will be raised. " +
            $"For example, if {nameof(UiBreakpoints)} has 3 breakpoints, and only the 2nd one is currently matching, " +
            $"then the 2nd event from this array will be raised when {nameof(Trigger)} is called."
        )]
        public UnityEvent[] BreakpointTriggers;

        public void Awake() => this.AssertAssociation(UiBreakpoints, nameof(UiBreakpoints));

        public void Trigger()
        {
            for (int x = 0; x < UiBreakpoints.Breakpoints.Length; ++x) {
                if (UiBreakpoints.Breakpoints[x].IsMatched)
                    BreakpointTriggers[x].Invoke();
            }
        }

        private bool isNumBreakpointsValid(UnityEvent[] triggers, ref string message)
        {
            bool valid = UiBreakpoints is null || triggers.Length == UiBreakpoints.Breakpoints.Length;
            if (!valid) {
                message = $"The associated {nameof(UI.UiBreakpoints)} object has {UiBreakpoints.Breakpoints.Length} {nameof(UiBreakpoints.Breakpoints)}, " +
                    $"but you have defined matching {nameof(BreakpointTriggers)} for {(BreakpointTriggers.Length < UiBreakpoints.Breakpoints.Length ? "only" : "")} {BreakpointTriggers.Length}.";
            }

            return valid;
        }
    }
}
