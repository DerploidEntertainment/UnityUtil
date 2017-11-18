namespace Danware.Unity.Updating {

    public interface IFixedUpdatable {

        /// <summary>
        /// This method is called every physics frame, if the object is enabled.
        /// When overridden, this method replaces the built-in MonoBehaviour FixedUpdate() method so that
        /// updates on numerous objects can happen without relying on Unity's costly reflection-based callback mechanism.
        /// </summary>
        void UpdatableFixedUpdate();

    }

}
