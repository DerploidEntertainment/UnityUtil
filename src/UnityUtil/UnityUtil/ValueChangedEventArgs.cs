using System;

namespace UnityUtil;

public class ValueChangedEventArgs<T>(T oldValue, T newValue) : EventArgs
{
    public T OldValue { get; } = oldValue;
    public T NewValue { get; } = newValue;
}
