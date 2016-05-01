using UnityEngine;

namespace Danware.Unity3D.Inventory {
    
    [RequireComponent(typeof(LineRenderer))]
    public class FirearmView : MonoBehaviour {
        // HIDDEN FIELDS
        private LineRenderer _line;
        private AudioSource _audio;

        // INSPECTOR FIELDS
        public Firearm Firearm;
        public AudioClip FireAudioClip;
        public float FlashDuration = 0.25f;
        public float FlashWidth = 0.1f;
        public Vector3 FlashOffset = new Vector3(0.2f, 0f, 0.1f);

        // EVENT HANDLERS
        private void Awake() {
            Debug.AssertFormat(Firearm != null, "FirearmView {0} was not associated with any Firearm!", this.name);
            init();
        }
        private void handleFired(object sender, Firearm.FireEventArgs e) {
            // Play sound
            _audio.Play();

            // Render a "flash" for the given duration
            _line.enabled = true;
            Transform trans = e.Firearm.transform;
            Vector3 pos0 = trans.TransformPoint(trans.localPosition + FlashOffset);
            Vector3 pos1 = trans.position + Firearm.Range * e.Direction;
            _line.SetPosition(0, pos0);
            _line.SetPosition(1, pos1);
            Invoke("clearFlash", FlashDuration);

            // Listen for whichever target ends up being affected
            Firearm.TargetData td = new Firearm.TargetData();
            td.Callback += handleTarget;
            RaycastHit[] keys = new RaycastHit[e.TargetPriorities.Count];
            e.TargetPriorities.Keys.CopyTo(keys, 0);
            foreach (RaycastHit k in keys)
                e.Add(k, td);
        }
        private void handleTarget(RaycastHit hit) {
            // Cut the "flash" back to the impact point
            Vector3 pos1 = hit.point;
            _line.SetPosition(1, pos1);
        }

        // HELPER FUNCTIONS
        private void init() {
            _line = GetComponent<LineRenderer>();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.clip = FireAudioClip;
            _audio.loop = false;
            _audio.playOnAwake = false;
            
            _line.SetWidth(FlashWidth, FlashWidth);
            
            Firearm.Fired += handleFired;
        }
        private void clearFlash() {
            _line.enabled = false;
        }

    }

}
