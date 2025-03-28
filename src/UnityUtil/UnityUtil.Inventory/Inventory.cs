using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Unity.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;

namespace UnityUtil.Inventory;

[Serializable]
public class InventoryItemEvent : UnityEvent<InventoryCollectible> { }

public class Inventory : MonoBehaviour
{
    private ILogger<Inventory>? _logger;
    private readonly HashSet<InventoryCollectible> _collectibles = [];

    public int MaxItems = 10;
    [Tooltip("If true, then this Inventory can collect multiple items with the same name.")]
    public bool AllowMultiple = false;
    [Tooltip("If dropped, items will take this many seconds to become collectible again.")]
    public float DropRefactoryPeriod = 1.5f;
    public Vector3 LocalDropOffset = Vector3.one;
    public InventoryItemEvent ItemCollected = new();
    public InventoryItemEvent ItemDropped = new();

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public InventoryCollectible[] GetCollectibles() => [.. _collectibles];
    public GameObject[] GetItems() => _collectibles.Select(c => c.ItemRoot!).ToArray();
    public bool Collect(InventoryCollectible collectible)
    {
        if (collectible == null)
            throw new ArgumentNullException(nameof(collectible), $"{this.GetHierarchyNameWithType()} cannot collect null");

        // If there is no room for the item, then just return that it wasn't collected
        if (_collectibles.Count == MaxItems)
            return false;
        if (!AllowMultiple && _collectibles.Select(c => c.ItemRoot!.name).Contains(collectible.ItemRoot!.name))
            return false;

        // Otherwise, do collect actions
        Transform itemTrans = collectible.ItemRoot!.transform;
        itemTrans.parent = transform;
        itemTrans.localPosition = new Vector3(0f, 0f, 0f);
        itemTrans.localRotation = Quaternion.identity;
        collectible.Root!.SetActive(false);

        // Place the collectible in the Inventory
        _ = _collectibles.Add(collectible);

        // Raise the item collected event
        _logger!.Collected(collectible);
        ItemCollected.Invoke(collectible);

        return true;
    }
    public void Drop(InventoryCollectible collectible)
    {
        if (collectible == null)
            throw new ArgumentNullException(nameof(collectible), $"{this.GetHierarchyNameWithType()} cannot drop null");

        // Make sure a valid collectible was provided
        if (!_collectibles.Contains(collectible))
            throw new InvalidOperationException($"{this.GetHierarchyNameWithType()} was told to drop an {typeof(InventoryCollectible).Name} that it never collected!");

        _ = StartCoroutine(doDrop(collectible));
    }
    public void DropAll()
    {
        foreach (InventoryCollectible inventoryCollectible in _collectibles)
            _ = StartCoroutine(doDrop(inventoryCollectible));
    }

    private IEnumerator doDrop(InventoryCollectible collectible)
    {
        // Drop it as a new Collectible
        collectible.Root!.SetActive(true);
        collectible.transform.position = transform.TransformPoint(LocalDropOffset);
        Transform itemTrans = collectible.ItemRoot!.transform;
        itemTrans.parent = transform;

        // Remove the provided collectible from the Inventory
        _ = _collectibles.Remove(collectible);

        // Raise the item dropped event
        _logger!.Dropped(collectible);
        ItemDropped.Invoke(collectible);

        // Prevent its re-collection for the requested duration
        for (int c = 0; c < collectible.CollidersToToggle.Length; ++c)
            collectible.CollidersToToggle[c].enabled = false;
        yield return new WaitForSeconds(DropRefactoryPeriod);
        for (int c = 0; c < collectible.CollidersToToggle.Length; ++c)
            collectible.CollidersToToggle[c].enabled = true;
    }

}
