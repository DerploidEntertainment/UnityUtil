using UnityEngine.Events;

namespace Danware.Unity.Triggers {
    
    public class DetectorTrigger : DetectorResponder {

        // INSPECTOR FIELDS
        public UnityEvent Triggered = new UnityEvent();
        public ColliderDetectorBase[] Detectors;

        // EVENT HANDLERS
        public void Awake() {
            foreach (ColliderDetectorBase detector in Detectors)
                detector.Detected += Detector_Detected;
        }
        protected override void Detector_Detected(object sender, ColliderDetectedEventArgs e) => Triggered.Invoke();
    }

}

