namespace UnityEngine {

    public class Configurable : MonoBehaviour {

        protected Configurator Configurator;

        [ConfigKey]
        [Tooltip("The key by which to look up configuration for this Component. A blank string (default) is equivalent to the fully qualified type name. Leading/trailing whitespace is ignored. For example, all components of type 'Derp' in the namespace 'MyGame', will use the default config key 'MyGame.Derp', and their field config keys will have the form 'MyGame.Derp.<fieldname>'. If you want a particular 'Derp' instance to be individually configurable, then you must give it a unique config key.")]
        public string ConfigKey;

        private void Awake() {
            DependencyInjector.ResolveDependenciesOf(this);

            Configurator.Configure(this);

            OnAwake();
        }
        public void Inject(Configurator configurator) {
            Configurator = configurator;
        }

        protected virtual void OnAwake() { }

    }

}
