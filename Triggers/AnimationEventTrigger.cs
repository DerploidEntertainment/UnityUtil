using UnityEngine;
using UnityEngine.Events;

namespace Danware.Unity {

    [RequireComponent(typeof(Animator))]
    public class AnimationEventTrigger : MonoBehaviour {

        public class AnimationEventTriggerEvent : UnityEvent<Animator, string> { }

        // HIDDEN FIELDS
        private Animator _animator;

        // EVENT HANDLERS
        private void Awake() => _animator = GetComponent<Animator>();

        // API INTERFACE
        public AnimationEventTriggerEvent AnimationEventOccurred = new AnimationEventTriggerEvent();
        /// <summary>
        /// Warning! This method must not be called programmatically.  Instead, create an <see cref="AnimationClip"/> with an <see cref="AnimationEvent"/> that calls this method.
        /// </summary>
        /// <param name="eventName">Name of the event that was raised by the <see cref="Animator"/></param>
        public void RaiseEvent(string eventName) => AnimationEventOccurred.Invoke(_animator, eventName);

    }

}
