namespace Danware.Unity.Updating {

    public interface IUpdatable {

        /// <summary>
        /// This method is called every frame, if the object is enabled.
        /// When overridden, this method replaces the built-in MonoBehaviour Update() method so that
        /// updates on numerous objects can happen without relying on Unity's costly reflection-based callback mechanism.
        /// </summary>
        void UpdatableUpdate();

    }

}
