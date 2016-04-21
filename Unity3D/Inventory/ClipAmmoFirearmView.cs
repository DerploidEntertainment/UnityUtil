using UnityEngine;

namespace Danware.Unity3D.Inventory {
    
    public class ClipAmmoFirearmView : MonoBehaviour {
        // HIDDEN FIELDS
        private AudioSource _misfireAudio;
        private AudioSource _reloadAudio;

        // INSPECTOR FIELDS
        public ClipAmmoFirearm ClipAmmoFirearm;
        public AudioClip MisfireAudioClip;
        public AudioClip ReloadAudioClip;

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(ClipAmmoFirearm != null, "ClipAmmoFirearmView {0} was not associated with any ClipAmmoFirearm!", this.name);
            init();
        }
        private void handleMisfired(object sender, Firearm.FirearmEventArgs e) {
            _misfireAudio.Play();
        }
        private void handleAmmoChanged(object sender, ClipAmmoFirearm.AmmoChangedEventArgs e) {
            if (e.NewAmmo > e.OldAmmo)
                _reloadAudio.Play();
        }

        // HELPER FUNCTIONS
        private void init() {
            _misfireAudio = gameObject.AddComponent<AudioSource>();
            _misfireAudio.clip = MisfireAudioClip;
            _misfireAudio.loop = false;
            _misfireAudio.playOnAwake = false;

            _reloadAudio = gameObject.AddComponent<AudioSource>();
            _reloadAudio.clip = ReloadAudioClip;
            _reloadAudio.loop = false;
            _reloadAudio.playOnAwake = false;

            ClipAmmoFirearm.Misfired += handleMisfired;
            ClipAmmoFirearm.ClipAmmoChanged += handleAmmoChanged;
        }

    }

}
