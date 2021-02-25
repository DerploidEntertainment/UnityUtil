using Sirenix.OdinInspector;

namespace UnityEngine.Triggers
{
    public class RandomTrigger : MonoBehaviour
    {
        public SimpleTrigger[] Triggers;

        [Button]
        public void Trigger() {
            int t = Random.Range(0, Triggers.Length);
            Triggers[t].Trigger();
        }

    }

}
