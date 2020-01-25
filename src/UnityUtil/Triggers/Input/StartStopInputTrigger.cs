using UnityEngine.Events;
using UnityEngine.Inputs;

namespace UnityEngine.Triggers.Input {

    public class StartStopInputTrigger : Updatable {

        public StartStopInput Input;
        public UnityEvent InputStarted = new UnityEvent();
        public UnityEvent InputStopped = new UnityEvent();

        protected override void BetterAwake() {
            RegisterUpdatesAutomatically = true;
            BetterUpdate = checkInputs;
        }

        private void checkInputs(float deltaTime) {
            if (Input.Started())
                InputStarted.Invoke();
            else if (Input.Stopped())
                InputStopped.Invoke();
        }

    }

}
