using UnityEngine;
using System.Linq;

namespace Danware.Unity.Triggers {

    public class AndTrigger : TriggerCondition {

        private int _numTrue = 0;

        [Tooltip("Should this trigger's BecameFalse event be raised every time one of the Conditions becomes false, even if another Condition was already false?")]
        public bool TriggerEveryTime = false;
        public TriggerCondition[] Conditions;

        private void Awake() {
            _numTrue = Conditions.Count(c => c.IsConditionMet());
            foreach (TriggerCondition c in Conditions) {
                c.BecameTrue.AddListener(() => {
                    if (++_numTrue == Conditions.Length)
                        BecameTrue.Invoke();
                });
                c.BecameFalse.AddListener(() => {
                    if (--_numTrue == Conditions.Length - 1 || TriggerEveryTime)
                        BecameFalse.Invoke();
                });
            }
        }

        public override bool IsConditionMet() => Conditions.All(c => c.IsConditionMet());

    }

}
