using System.Linq;

namespace UnityEngine.Triggers
{

    public class AndTrigger : MultiConditionalTrigger {

        private int _numTrue;

        protected override void Awake() {
            base.Awake();

            _numTrue = Conditions?.Count(c => c.IsConditionMet()) ?? 0;
        }

        public override bool IsConditionMet() => Conditions?.All(c => c.IsConditionMet()) ?? false;

        protected override void ConditionBecameTrueListener(ConditionalTrigger condition) =>
            (++_numTrue == Conditions.Length
                ? (RaiseBecameEvents ? BecameTrue : null)
                : (RaiseStillEvents ? StillFalse : null)
            )?.Invoke();
        protected override void ConditionBecameFalseListener(ConditionalTrigger condition) =>
            (--_numTrue == Conditions.Length - 1
                ? (RaiseBecameEvents ? BecameFalse : null)
                : (RaiseStillEvents ? StillFalse : null)
            )?.Invoke();
        protected override void ConditionStillTrueListener(ConditionalTrigger condition) {
            if (RaiseStillEvents)
                ((_numTrue == Conditions.Length) ? StillTrue : StillFalse).Invoke();
        }
        protected override void ConditionStillFalseListener(ConditionalTrigger condition) {
            if (RaiseStillEvents)
                StillFalse.Invoke();
        }

    }

}
