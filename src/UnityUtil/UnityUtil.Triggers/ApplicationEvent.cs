// ----------------------------------------------------------------------------
// Inspired by Unite 2017 talk: "Game Architecture with Scriptable Objects"
// See: https://github.com/roboryantron/Unite2017/blob/master/Assets/Code/Events/GameEvent.cs
// ----------------------------------------------------------------------------

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Triggers;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(ApplicationEvent)}", fileName = "event")]
public class ApplicationEvent : ScriptableObject
{
    public event EventHandler? Invoked;

    [Button]
    public void Invoke() => Invoked?.Invoke(sender: this, EventArgs.Empty);
}
