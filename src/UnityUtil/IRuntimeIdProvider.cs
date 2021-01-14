namespace UnityEngine
{
    /// <summary>
    /// Provides methods for getting a unique identifier at runtime
    /// </summary>
    public interface IRuntimeIdProvider
    {
        /// <summary>
        /// Get a new, unique identifier. Every call to this method at runtime is guaranteed to return a new integer,
        /// but different integers may be generated during each run.
        /// </summary>
        /// <returns>A new, unique identifier.</returns>
        int GetId();
    }
}
