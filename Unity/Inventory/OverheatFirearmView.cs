using UnityEngine;

namespace Danware.Unity.Inventory {
    
    public class OverheatFirearmView : MonoBehaviour {
        // HIDDEN FIELDS
        private AudioSource _onOverheatAudio;
        private AudioSource _whileOverheatedAudio;

        // INSPECTOR FIELDS
        public OverheatFirearm OverheatFirearm;
        public AudioClip OnOverheatAudioClip;
        public AudioClip WhileOverheatedAudioClip;

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(OverheatFirearm != null, "OverheatFirearmView {0} was not associated with any OverheatFirearm!", this.name);
            init();
        }
        private void handleOverheated(object sender, OverheatFirearm.OverheatChangedEventArgs e) {
            // Play/stop sounds according to whether the Firearm just started/stopped being overheated
            if (e.Overheated) {
                _onOverheatAudio.Play();
                _whileOverheatedAudio.Play();
            }
            else {
                _whileOverheatedAudio.Stop();
            }
        }

        // HELPER FUNCTIONS
        protected void init() {
            _onOverheatAudio = gameObject.AddComponent<AudioSource>();
            _onOverheatAudio.clip = OnOverheatAudioClip;
            _onOverheatAudio.loop = false;
            _onOverheatAudio.playOnAwake = false;

            _whileOverheatedAudio = gameObject.AddComponent<AudioSource>();
            _whileOverheatedAudio.clip = WhileOverheatedAudioClip;
            _whileOverheatedAudio.loop = true;
            _whileOverheatedAudio.playOnAwake = false;

            OverheatFirearm.OverheatStateChanged += handleOverheated;
        }

    }

}
