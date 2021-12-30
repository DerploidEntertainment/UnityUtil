namespace UnityEngine
{
    /// <inheritdoc/>
    public class RuntimeIdProvider : IRuntimeIdProvider
    {
        private static int s_nextId;

        /// <inheritdoc/>
        public int GetId() => s_nextId++;
    }
}
