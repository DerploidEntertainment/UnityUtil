using System.Linq;
using System.Collections.Generic;

namespace Danware.Unity.Updating {

    public sealed class Updater : IUpdater {

        private HashSet<IUpdatable> _updatables = new HashSet<IUpdatable>();

        // ENFORCE SINGLETON PATTERN
        private static Updater s_instance = new Updater();
        private Updater() { }
        /// <summary>
        /// The singleton <see cref="Updater"/> instance.
        /// </summary>
        public static Updater Instance => s_instance;

        // INTERFACE
        /// <inheritdoc/>
        public bool Register(IUpdatable updatable) => _updatables.Add(updatable);
        /// <inheritdoc/>
        public bool Unregister(IUpdatable updatable) => _updatables.Remove(updatable);
        /// <inheritdoc/>
        public void UpdateAll() {
            IUpdatable[] updatables = _updatables.ToArray();
            foreach (IUpdatable u in updatables)
                u.UpdatableUpdate();
        }

    }

}
