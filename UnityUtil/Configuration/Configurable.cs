namespace UnityEngine {

    public class Configurable : MonoBehaviour {

        #pragma warning disable CS0649      // Field '' is never assigned to, and will always have its default value null
        [Inject]
        protected readonly Configurator Configurator;
        #pragma warning restore CS0649

        [ConfigKey]
        [Tooltip("The key by which to look up configuration for this Component. A blank string (default) is equivalent to the fully qualified type name. Leading/trailing whitespace is ignored. For example, all components of type 'Derp' in the namespace 'MyGame', will use the default config key 'MyGame.Derp', and their field config keys will have the form 'MyGame.Derp.<fieldname>'. If you want a particular 'Derp' instance to be individually configurable, then you must give it a unique config key.")]
        public string ConfigKey;

        private void Awake() {
            DependencyInjector.Inject(this);

            Configurator.Configure(this);

            OnAwake();
        }

        protected virtual void OnAwake() { }

    }

}
