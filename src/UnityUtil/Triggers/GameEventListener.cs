// ----------------------------------------------------------------------------
// Inspired by Unite 2017 talk: "Game Architecture with Scriptable Objects"
// ----------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUtil.Triggers;

public class GameEventListener : MonoBehaviour
{
    [Required, Tooltip("Event to listen to")]
    public GameEvent? Event;

    [Button, ShowInInspector]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Invoked by Odin Inspector button in Unity Editor")]
    private void invokeActions() => Actions.Invoke();

    [Tooltip($"Actions to invoke when {nameof(Event)} is raised")]
    public UnityEvent Actions = new();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnEnable() => Register();

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void OnDisable() => Unregister();

    public void Register() => Event!.Invoked += doInvoke;
    public void Unregister() => Event!.Invoked -= doInvoke;
    private void doInvoke(object sender, EventArgs e) => Actions.Invoke();
}
