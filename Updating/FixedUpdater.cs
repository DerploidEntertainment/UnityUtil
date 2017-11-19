using System.Collections.Generic;
using System.Linq;

namespace Danware.Unity.Updating {

    public sealed class FixedUpdater : IFixedUpdater {

        private HashSet<IFixedUpdatable> _updatables = new HashSet<IFixedUpdatable>();

        // ENFORCE SINGLETON PATTERN
        private static FixedUpdater s_instance = new FixedUpdater();
        private FixedUpdater() { }
        /// <summary>
        /// The singleton <see cref="FixedUpdater"/> instance.
        /// </summary>
        public static FixedUpdater Instance => s_instance;

        // INTERFACE
        /// <inheritdoc/>
        public bool Register(IFixedUpdatable updatable) => _updatables.Add(updatable);
        /// <inheritdoc/>
        public bool Unregister(IFixedUpdatable updatable) => _updatables.Remove(updatable);
        /// <inheritdoc/>
        public void FixedUpdateAll() {
            IFixedUpdatable[] updatables = _updatables.ToArray();
            foreach (IUpdatable fu in updatables)
                fu.UpdatableUpdate();
        }

    }

}
