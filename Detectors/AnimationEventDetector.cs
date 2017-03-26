using System;

using UnityEngine;

namespace Danware.Unity {

    [RequireComponent(typeof(Animator))]
    public class AnimationEventDetector : MonoBehaviour {

        // HIDDEN FIELDS
        private Animator _animator;
        private EventHandler<AnimationEventOccurredEventArgs> _detectedInvoker;

        private void Awake() {
            _animator = GetComponent<Animator>();
        }

        // API INTERFACE
        public event EventHandler<AnimationEventOccurredEventArgs> AnimationEventOccurred {
            add { _detectedInvoker += value; }
            remove { _detectedInvoker -= value; }
        }
        public void RaiseEvent(string eventName) {
            AnimationEventOccurredEventArgs args = new AnimationEventOccurredEventArgs(this, _animator, eventName);
            _detectedInvoker?.Invoke(this, args);
        }

    }

    public class AnimationEventOccurredEventArgs : EventArgs {
        public AnimationEventOccurredEventArgs(AnimationEventDetector detector, Animator animator, string eventName) {
            Detector = detector;
            Animator = animator;
            EventName = eventName;
        }
        public AnimationEventDetector Detector { get; }
        public Animator Animator { get; }
        public string EventName { get; }
    }

}
