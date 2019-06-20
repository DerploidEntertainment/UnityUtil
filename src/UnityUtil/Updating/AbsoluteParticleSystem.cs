using UnityEngine;

namespace UnityEngine {

    public class AbsoluteParticleSystem : AbsoluteUpdater {

        private ParticleSystem _particles;

        private void Awake() =>
            _particles = GetComponent<ParticleSystem>();

        protected override void doUpdates() =>
            _particles.Simulate(_delta, withChildren: true, restart: false, fixedTimeStep: false);

    }

}