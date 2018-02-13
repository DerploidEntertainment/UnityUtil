using System;
using UnityEngine;

namespace UnityUtil {

    public class AbsoluteAnimation : AbsoluteUpdater {

        // INSPECTOR FIELDS
        public bool PlayOnStart;
        public string PlayOnStartStateName;

        //private fields
        private Animation _anim;
        private AnimationState _currState;
        private Action _completionHandler;
        private float _elapsedTime;
        private bool _playing;

        private void Awake() =>
            _anim = GetComponent<Animation>();
        void Start() {
            if (PlayOnStart)
                Play(PlayOnStartStateName);
        }

        protected override void doUpdates() {
            if (_playing) {
                _elapsedTime += _delta;
                _currState.normalizedTime = _elapsedTime / _currState.length;

                if (_elapsedTime >= _currState.length) {
                    _playing = false;

                    if (_currState.wrapMode == WrapMode.Loop)
                        Play(_currState.name);
                    else
                        _completionHandler?.Invoke();
                }
            }
        }

        public void Play(string stateName, Action completionHandler = null) {
            _elapsedTime = 0f;
            _currState = _anim[stateName];
            _currState.normalizedTime = 0;
            _currState.enabled = true;
            _currState.weight = 1;
            _completionHandler = completionHandler;
            _playing = true;
        }

    }

}