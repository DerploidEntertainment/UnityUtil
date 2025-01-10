namespace UnityUtil;

/// <inheritdoc/>
public class RuntimeIdProvider : IRuntimeIdProvider
{
    private int _nextId;

    /// <inheritdoc/>
    public int GetNewId() => _nextId++;
}
