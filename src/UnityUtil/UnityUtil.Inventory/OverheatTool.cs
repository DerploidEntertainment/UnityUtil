﻿using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Updating;

namespace UnityUtil.Inventory;

/// <summary>
/// Type arguments are (bool isOverheated)
/// </summary>
[Serializable]
public class OverheatChangedEvent : UnityEvent<bool> { }

[RequireComponent(typeof(Tool))]
public class OverheatTool : Updatable
{
    private Tool? _tool;
    private Coroutine? _overheatRoutine;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public OverheatToolInfo? Info;

    public float CurrentHeat { get; private set; }
    public OverheatChangedEvent OverheatStateChanged = new();

    protected override void Awake()
    {
        base.Awake();

        if (Info!.StartingHeat > Info.MaxHeat)
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} was started with {nameof(Info.StartingHeat)} heat but it can only store a max of {Info.MaxHeat}");

        AddUpdate(doUpdate);

        // Initialize heat
        CurrentHeat = Info.StartingHeat;

        // Register Tool events
        _tool = GetComponent<Tool>();
        _tool.Using.AddListener(() =>
            _tool.Using.Cancel = CurrentHeat > Info.MaxHeat);
        _tool.Used.AddListener(() => {
            float heat = Info.HeatGeneratedPerUse * (Info.AbsoluteHeat ? 1f : Info.MaxHeat);
            CurrentHeat += heat;
            if (CurrentHeat > Info.MaxHeat) {
                OverheatStateChanged.Invoke(true);
                _overheatRoutine = StartCoroutine(doOverheatDuration());
            }
        });
    }
    private void doUpdate(float deltaTime)
    {
        // Cool this Tool, unless it is overheated
        if (CurrentHeat > 0 && _overheatRoutine is null) {
            float rate = Info!.AbsoluteHeat ? Info.CoolRate : Info.CoolRate * Info.MaxHeat;
            CurrentHeat = Mathf.Max(0, CurrentHeat - deltaTime * rate);
        }
    }

    private IEnumerator doOverheatDuration()
    {
        yield return new WaitForSeconds(Info!.OverheatDuration);
        OverheatStateChanged.Invoke(false);

        _overheatRoutine = null;
    }

}
