namespace Danware.Unity.Updating {

    public interface IUpdater {

        /// <summary>
        /// Register an <see cref="IUpdatable"/> to receive updates every frame.
        /// </summary>
        /// <param name="updatable">The <see cref="IUpdatable"/> to receive updates every frame.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="updatable"/> is added to the set of <see cref="IUpdatable"/>s; 
        /// <see langword="false"/> if it was already present.
        /// </returns>
        bool Register(IUpdatable updatable);
        /// <summary>
        /// Unregister an <see cref="IUpdatable"/> from receiving updates every frame.
        /// </summary>
        /// <param name="updatable">The <see cref="IUpdatable"/> that no longer needs to receive updates every frame.</param>
        /// <returns><see langword="true"/> if <paramref name="updatable"/> is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        bool Unregister(IUpdatable updatable);
        /// <summary>
        /// Calls the <see cref="IUpdatable.UpdatableUpdate"/> method of every registered <see cref="IUpdatable"/>.
        /// </summary>
        void UpdateAll();

    }

}
