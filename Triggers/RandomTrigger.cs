using UnityEngine;

namespace Danware.Unity.Triggers {

    public class RandomTrigger : MonoBehaviour {

        public SimpleTrigger[] Triggers;

        public void Trigger() {
            int t = Random.Range(0, Triggers.Length);
            Triggers[t].Trigger();
        }

    }

}
