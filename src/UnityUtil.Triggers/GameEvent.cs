// ----------------------------------------------------------------------------
// Inspired by Unite 2017 talk: "Game Architecture with Scriptable Objects"
// See: https://github.com/roboryantron/Unite2017/blob/master/Assets/Code/Events/GameEvent.cs
// ----------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

[CreateAssetMenu(menuName = $"{nameof(UnityUtil)}/{nameof(GameEvent)}", fileName = "event-something-happened")]
public class GameEvent : ScriptableObject
{
    public event EventHandler? Invoked;

    public UnityEvent Actions = new();

    [Button]
    public void Invoke()
    {
        Actions.Invoke();
        Invoked?.Invoke(this, EventArgs.Empty);
    }
}
