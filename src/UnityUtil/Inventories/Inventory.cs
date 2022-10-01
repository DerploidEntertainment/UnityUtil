using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityUtil.DependencyInjection;
using UnityUtil.Logging;

namespace UnityEngine.Inventories;

[Serializable]
public class InventoryItemEvent : UnityEvent<InventoryCollectible> { }

public class Inventory : MonoBehaviour
{
    private ILogger? _logger;
    private readonly HashSet<InventoryCollectible> _collectibles = new();

    public int MaxItems = 10;
    [Tooltip("If true, then this Inventory can collect multiple items with the same name.")]
    public bool AllowMultiple = false;
    [Tooltip("If dropped, items will take this many seconds to become collectible again.")]
    public float DropRefactoryPeriod = 1.5f;
    public Vector3 LocalDropOffset = Vector3.one;
    public InventoryItemEvent ItemCollected = new();
    public InventoryItemEvent ItemDropped = new();

    public void Inject(ILoggerProvider loggerProvider) => _logger = loggerProvider.GetLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public InventoryCollectible[] GetCollectibles() => _collectibles.ToArray();
    public GameObject[] GetItems() => _collectibles.Select(c => c.ItemRoot!).ToArray();
    public bool Collect(InventoryCollectible collectible)
    {
        // Make sure an actual Collectible was provided, and that there is room for it
        Assert.IsNotNull(collectible, $"{this.GetHierarchyNameWithType()} cannot collect null!");

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
        _collectibles.Add(collectible);

        // Raise the item collected event
        _logger!.Log($"Collected {collectible.ItemRoot?.GetHierarchyName()}", context: this);
        ItemCollected.Invoke(collectible);

        return true;
    }
    public void Drop(InventoryCollectible collectible)
    {
        // Make sure a valid collectible was provided
        Assert.IsNotNull(collectible, $"{this.GetHierarchyNameWithType()} cannot drop null!");
        Assert.IsTrue(_collectibles.Contains(collectible), $"{this.GetHierarchyNameWithType()} was told to drop an {typeof(InventoryCollectible).Name} that it never collected!");

        StartCoroutine(doDrop(collectible));
    }
    public void DropAll()
    {
        InventoryCollectible[] collectibles = _collectibles.ToArray();
        for (int c = 0; c < collectibles.Length; ++c)
            StartCoroutine(doDrop(collectibles[c]));
    }

    private IEnumerator doDrop(InventoryCollectible collectible)
    {
        // Drop it as a new Collectible
        collectible.Root!.SetActive(true);
        collectible.transform.position = transform.TransformPoint(LocalDropOffset);
        Transform itemTrans = collectible.ItemRoot!.transform;
        itemTrans.parent = transform;

        // Remove the provided collectible from the Inventory
        _collectibles.Remove(collectible);

        // Raise the item dropped event
        _logger!.Log($"Dropped {collectible.ItemRoot.GetHierarchyName()}", context: this);
        ItemDropped.Invoke(collectible);

        // Prevent its re-collection for the requested duration
        for (int c = 0; c < collectible.CollidersToToggle.Length; ++c)
            collectible.CollidersToToggle[c].enabled = false;
        yield return new WaitForSeconds(DropRefactoryPeriod);
        for (int c = 0; c < collectible.CollidersToToggle.Length; ++c)
            collectible.CollidersToToggle[c].enabled = true;
    }

}
