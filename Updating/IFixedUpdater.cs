namespace Danware.Unity.Updating {

    public interface IFixedUpdater {

        /// <summary>
        /// Register an <see cref="IFixedUpdatable"/> to receive updates every physics frame.
        /// </summary>
        /// <param name="updatable">The <see cref="IFixedUpdatable"/> to receive updates every physics frame.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="updatable"/> is added to the set of <see cref="IFixedUpdatable"/>s; 
        /// <see langword="false"/> if it was already present.
        /// </returns>
        bool Register(IFixedUpdatable updatable);
        /// <summary>
        /// Unregister an <see cref="IFixedUpdatable"/> from receiving updates every physics frame.
        /// </summary>
        /// <param name="updatable">The <see cref="IFixedUpdatable"/> that no longer needs to receive updates every physics frame.</param>
        /// <returns><see langword="true"/> if <paramref name="updatable"/> is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        bool Unregister(IFixedUpdatable updatable);
        /// <summary>
        /// Calls the <see cref="IFixedUpdatable.UpdatableFixedUpdate"/> method of every registered <see cref="IFixedUpdatable"/>.
        /// </summary>
        void FixedUpdateAll();

    }

}
