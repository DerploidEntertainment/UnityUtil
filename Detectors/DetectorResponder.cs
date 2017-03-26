using UnityEngine;

namespace Danware.Unity {

    public abstract class DetectorResponder : MonoBehaviour {

        protected abstract void Detector_Detected(object sender, ColliderDetectedEventArgs e);

    }

}
