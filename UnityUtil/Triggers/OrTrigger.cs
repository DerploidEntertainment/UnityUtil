using System.Linq;
using UnityEngine;

namespace UnityEngine.Triggers {

    public class OrTrigger : TriggerCondition {

        private int _numTrue = 0;

        [Tooltip("Should this trigger's BecameTrue event be raised every time one of the Conditions becomes true, even if another Condition was already true?")]
        public bool TriggerEveryTime = false;
        public TriggerCondition[] Conditions;

        private void Awake() {
            _numTrue = Conditions.Count(c => c.IsConditionMet());
            foreach (TriggerCondition c in Conditions) {
                c.BecameTrue.AddListener(() => {
                    if (++_numTrue == 1 || TriggerEveryTime)
                        BecameTrue.Invoke();
                });
                c.BecameFalse.AddListener(() => {
                    if (--_numTrue > 0)
                        BecameFalse.Invoke();
                });
            }
        }

        public override bool IsConditionMet() => Conditions.Any(c => c.IsConditionMet());

    }

}
