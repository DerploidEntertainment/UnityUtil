using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityUtil.Inputs;
using UnityUtil.Updating;

namespace UnityUtil.Inventories;

/// <summary>
/// Type arguments are (int oldClipAmmo, int oldBackupAmmo, int newClipAmmo, int newBackupAmmo)
/// </summary>
[Serializable]
public class AmmoEvent : UnityEvent<int, int, int, int> { }

[RequireComponent(typeof(Tool))]
public class AmmoTool : Updatable
{
    private Tool? _tool;

    [Required] public AmmoToolInfo? Info;
    [Required] public StartStopInput? ReloadInput;

    /// <summary>
    /// The amount of ammo currently in the main clip.
    /// </summary>
    public int CurrentClipAmmo { get; private set; }

    /// <summary>
    /// The total amount of ammo in all backup clips.
    /// </summary>
    public int CurrentBackupAmmo { get; private set; }

    /// <summary>
    /// Load this <see cref="AmmoTool"/> with a specified amount of ammo.
    /// </summary>
    /// <param name="ammo">The amount of ammo with which to load the <see cref="AmmoTool"/>.</param>
    /// <returns>The amount of left-over ammo that could not be stored in the <see cref="AmmoTool"/>'s clips.  Will always be >= 0.</returns>
    public int Load(int ammo) =>
        ammo < 0
            ? throw new ArgumentOutOfRangeException(nameof(ammo), $"Cannot load {this.GetHierarchyNameWithType()} with a negative amount of ammo!")
            : doLoad(ammo);

    /// <summary>
    /// Reload this <see cref="AmmoTool"/>'s current clip from its backup ammo
    /// </summary>
    public void ReloadClip() => doReloadClip();

    public AmmoEvent Loaded = new();
    public AmmoEvent AmmoReduced = new();

    protected override void Awake()
    {
        base.Awake();

        if (Info!.StartingAmmo > Info.MaxClipAmmo * (Info.MaxBackupClips + 1))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} was started with {nameof(Info.StartingAmmo)} ammo but it can only store a max of {Info.MaxClipAmmo} * ({Info.MaxClipAmmo * (Info.MaxBackupClips + 1)}!");

        // Initialize ammo
        doLoad(Info.StartingAmmo);

        // Register Tool events
        _tool = GetComponent<Tool>();
        _tool.Using.AddListener(() =>
            _tool.Using.Cancel = CurrentClipAmmo == 0);
        _tool.Used.AddListener(() => {
            int oldClip = CurrentClipAmmo;
            CurrentClipAmmo -= 1;
            AmmoReduced.Invoke(oldClip, CurrentBackupAmmo, CurrentClipAmmo, CurrentBackupAmmo);
        });

        RegisterUpdatesAutomatically = true;
        BetterUpdate = checkForReload;
    }

    private void checkForReload(float deltaTime)
    {
        if (ReloadInput?.Started() ?? false)
            doReloadClip();
    }

    private void doReloadClip()
    {
        // Fill the current clip as much as possible from backup ammo
        int oldClip = CurrentClipAmmo;
        int neededAmmo = Mathf.Clamp(Info!.MaxClipAmmo - CurrentClipAmmo, 0, CurrentBackupAmmo);
        CurrentClipAmmo += neededAmmo;
        CurrentBackupAmmo -= neededAmmo;

        // Raise the Reloaded event
        if (CurrentClipAmmo != oldClip)
            Loaded.Invoke(oldClip, CurrentBackupAmmo, CurrentClipAmmo, CurrentBackupAmmo);
    }

    private int doLoad(int ammo)
    {
        int oldClip = CurrentClipAmmo;
        int oldBackup = CurrentBackupAmmo;

        // Fill the current clip as much as possible
        if (CurrentClipAmmo < Info!.MaxClipAmmo) {
            int usableAmmo = Mathf.Min(Info.MaxClipAmmo - CurrentClipAmmo, ammo);
            CurrentClipAmmo += usableAmmo;
            ammo -= usableAmmo;
        }

        // Fill the backup ammo as much as possible
        if (CurrentBackupAmmo < Info.MaxClipAmmo * Info.MaxBackupClips) {
            int usableAmmo = Mathf.Min(Info.MaxBackupClips * Info.MaxClipAmmo - CurrentBackupAmmo, ammo);
            CurrentBackupAmmo += usableAmmo;
            ammo -= usableAmmo;
        }

        // Raise the Reloaded event, if ammo was actually used
        if (CurrentClipAmmo != oldClip || CurrentBackupAmmo != oldBackup)
            Loaded.Invoke(oldClip, oldBackup, CurrentClipAmmo, CurrentBackupAmmo);

        // Return how much ammo was left over
        return ammo;
    }

}
