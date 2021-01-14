namespace UnityEngine
{
    /// <inheritdoc/>
    public class RuntimeIdProvider : IRuntimeIdProvider
    {
        private static int s_nextId = 0;

        /// <inheritdoc/>
        public int GetId() => s_nextId++;
    }
}
