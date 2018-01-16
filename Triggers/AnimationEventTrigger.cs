using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil {

    [RequireComponent(typeof(Animator))]
    public class AnimationEventTrigger : MonoBehaviour {

        [Serializable]
        public class AnimationEventTriggerEvent : UnityEvent<Animator, string> { }

        // HIDDEN FIELDS
        public Animator Animator { get; private set; }

        // EVENT HANDLERS
        private void Awake() => Animator = GetComponent<Animator>();

        // API INTERFACE
        public AnimationEventTriggerEvent AnimationEventOccurred = new AnimationEventTriggerEvent();
        /// <summary>
        /// Warning! This method is not meant to be called programmatically.  Instead, create an <see cref="AnimationClip"/> with an <see cref="AnimationEvent"/> that calls this method.
        /// </summary>
        /// <param name="eventName">Name of the event that was raised by the <see cref="UnityEngine.Animator"/></param>
        public void RaiseEvent(string eventName) => AnimationEventOccurred.Invoke(Animator, eventName);

    }

}
