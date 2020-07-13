using System.Collections.Generic;
using UnityEngine.Logging;
using UnityEngine.Triggers;

namespace UnityEngine.UI {

    public class UiStack : Configurable {

        private ILogger _logger;
        private readonly Stack<SimpleTrigger> _popTriggers = new Stack<SimpleTrigger>();

        public void Inject(ILoggerProvider loggerProvider) {
            _logger = loggerProvider.GetLogger(this);
        }

        public void PushUi(SimpleTrigger popTrigger) {
            if (popTrigger == null) {
                _logger.LogError($"A {nameof(popTrigger)} must be provided when pushing to the UI stack, so that the correct actions can be triggered when this UI is later popped.", context: this);
                return;
            }

            _popTriggers.Push(popTrigger);
        }
        public void PopUi() {
            if (_popTriggers.Count == 0) {
                _logger.LogWarning("No more UI to pop from stack", context: this);
                return;
            }

            SimpleTrigger popTrigger = _popTriggers.Pop();
            popTrigger.Trigger();
        }

    }

}
