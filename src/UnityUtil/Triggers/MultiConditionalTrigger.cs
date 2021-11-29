namespace UnityEngine.Triggers
{

    public abstract class MultiConditionalTrigger : ConditionalTrigger {

        private const string MSG_ONLY_WHEN_CALLED = "Note that, if both " + nameof(TriggerWhenConditionsChanged) + " and " + nameof(TriggerWhenConditionsMaintained) + " are false, then this trigger will only raise events if its " + nameof(TriggerState) + "() method is called directly.";

        [Tooltip("If true, then this trigger will listen for " + nameof(BecameTrue) + " and " + nameof(BecameFalse) + " events raised by the " + nameof(Conditions) + ", and will raise either a 'became' or a 'still' event in response. " + MSG_ONLY_WHEN_CALLED)]
        public bool TriggerWhenConditionsChanged = true;
        [Tooltip("If true, then this trigger will listen for " + nameof(StillTrue) + " and " + nameof(StillFalse) + " events raised by the " + nameof(Conditions) + ", and will raise a 'still' event in response. " + MSG_ONLY_WHEN_CALLED)]
        public bool TriggerWhenConditionsMaintained = false;
        public ConditionalTrigger[] Conditions;

        protected virtual void Awake()
        {
            if (Conditions is not null) {
                foreach (ConditionalTrigger condition in Conditions) {
                    if (condition is not null)
                        addEventListeners(condition);
                }
            }
        }

        private void addEventListeners(ConditionalTrigger condition) {
            if (TriggerWhenConditionsChanged) {
                condition.BecameTrue.AddListener(() => ConditionBecameTrueListener(condition));
                condition.BecameFalse.AddListener(() => ConditionBecameFalseListener(condition));
            }

            if (TriggerWhenConditionsMaintained && RaiseStillEvents) {
                condition.StillTrue.AddListener(() => ConditionStillTrueListener(condition));
                condition.StillFalse.AddListener(() => ConditionStillFalseListener(condition));
            }
        }

        public void ResetEventListeners() {
            if (Conditions is null)
                return;

            foreach (ConditionalTrigger condition in Conditions) {
                if (condition is null)
                    continue;

                condition.BecameTrue.RemoveAllListeners();
                condition.BecameFalse.RemoveAllListeners();
                condition.StillTrue.RemoveAllListeners();
                condition.StillFalse.RemoveAllListeners();
                addEventListeners(condition);
            }
        }

        protected virtual void ConditionBecameTrueListener(ConditionalTrigger condition) { }
        protected virtual void ConditionBecameFalseListener(ConditionalTrigger condition) { }
        protected virtual void ConditionStillTrueListener(ConditionalTrigger condition) { }
        protected virtual void ConditionStillFalseListener(ConditionalTrigger condition) { }

    }

}
