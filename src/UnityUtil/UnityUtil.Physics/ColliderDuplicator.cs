﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityUtil.DependencyInjection;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;

namespace UnityUtil.Physics;

public enum ChildColliderDuplicateMode
{
    None,
    ImmediateChildCollidersOnly,
    AllChildCollidersFlattened,
    AllChildCollidersHierarchy,
}
public enum ChangeTriggerMode
{
    KeepOriginal,
    MakeAllTriggers,
    MakeAllColliders,
}

public class ColliderDuplicator : MonoBehaviour
{
    private ILogger<ColliderDuplicator>? _logger;

    [Tooltip("Each Collider selected for duplication will be duplicated under each of these GameObjects.")]
    public Transform? NewParentOfDuplicates;

    [Tooltip("Select the behavior for automatically duplicating child Colliders.")]
    public ChildColliderDuplicateMode ChildColliderDuplication = ChildColliderDuplicateMode.None;

    [Tooltip(
        "Add additional Colliders to duplicate here. If these are child Colliders, " +
        $"we recommend that you set {nameof(ChildColliderDuplication)} to '{nameof(ChildColliderDuplicateMode.None)}'."
    )]
    public Collider[] CollidersToDuplicate = [];

    [Tooltip("Select the behavior for changing the 'isTrigger' field of all duplicate Colliders")]
    public ChangeTriggerMode ChangeTriggerMode = ChangeTriggerMode.KeepOriginal;

    [Tooltip("All duplicate Colliders will be placed in the Layer with this name.")]
    public string DuplicateLayerName = "";

    [Tooltip("If set, all duplicate Colliders will have a PhysTarget component attached that targets this value.")]
    public MonoBehaviour? PhysicsTarget;


    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Start()
    {
        // Create duplicate Colliders

        List<Transform> duplicates = createDuplicates(NewParentOfDuplicates);

        // Parent these Colliders to the requested object
        // This must happen in a separate loop, or else we duplicate the duplicates also!
        foreach (Transform child in duplicates) {
            child.parent = NewParentOfDuplicates;
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }
    }

    private List<Transform> createDuplicates(Transform? newParent)
    {
        List<Transform> dupls = ChildColliderDuplication switch {
            ChildColliderDuplicateMode.ImmediateChildCollidersOnly => duplicateImmediateChildren(),
            ChildColliderDuplicateMode.AllChildCollidersFlattened => duplicateAllChildrenFlat(),
            ChildColliderDuplicateMode.AllChildCollidersHierarchy => newParent == null ? [] : duplicateAllChildrenHierarchy(newParent),
            _ => [],
        };

        // Duplicate other child Colliders
        foreach (Collider c in CollidersToDuplicate) {
            var newChild = new GameObject(c.name);
            duplicateCollider(c, newChild);
            dupls.Add(newChild.transform);
        }

        return dupls;
    }

    private List<Transform> duplicateImmediateChildren()
    {
        // Get Colliders on immediate children
        IEnumerable<Collider> childColls = gameObject
            .GetComponentsInChildren<Collider>()
            .Where(c => c.transform.parent == transform);

        // Duplicate each Collider on its own new GameObject
        List<Transform> dupls = [];
        foreach (Collider c in childColls) {
            var newChild = new GameObject(c.name);
            duplicateCollider(c, newChild);
            dupls.Add(newChild.transform);
        }

        return dupls;
    }

    private List<Transform> duplicateAllChildrenFlat()
    {
        // Get Colliders on the root object and all children
        IEnumerable<Collider> childColls = gameObject.GetComponentsInChildren<Collider>();

        // Duplicate each Collider on its own new GameObject
        List<Transform> dupls = [];
        foreach (Collider c in childColls) {
            var newChild = new GameObject(c.name);
            duplicateCollider(c, newChild);
            dupls.Add(newChild.transform);
        }

        return dupls;
    }

    private List<Transform> duplicateAllChildrenHierarchy(Transform newParent)
    {
        _ = duplicateHierarchy(transform, newParent);
        return [];
    }

    private List<Transform> duplicateHierarchy(Transform origParent, Transform duplParent)
    {
        // Duplicate each Collider of the original GameObject to the new GameObject
        foreach (Collider c in origParent.GetComponents<Collider>())
            duplicateCollider(c, duplParent.gameObject);

        // Get Colliders on immediate children
        IEnumerable<Transform> origChildren = origParent.GetComponentsInChildren<Transform>().Where(t => t.parent == origParent);

        // Duplicate each of these Colliders on a new child of the new GameObject
        // Then recursively duplicate grandchildren
        foreach (Transform origChild in origChildren) {
            Transform duplChild = new GameObject(origChild.name).transform;
            duplChild.parent = duplParent;
            duplChild.localPosition = Vector3.zero;
            duplChild.localRotation = Quaternion.identity;
            duplChild.localScale = Vector3.one;

            _ = duplicateHierarchy(origChild, duplChild);
        }

        return [];
    }

    private void duplicateCollider(Collider collider, GameObject newParent)
    {
        Collider? newColl;

        // Copy BoxCollider properties
        if (collider is BoxCollider origBox) {
            BoxCollider newBox = newParent.AddComponent<BoxCollider>();
            newBox.center = origBox.center;
            newBox.size = origBox.size;
            newColl = newBox;
        }

        // Copy SphereCollider properties
        else if (collider is SphereCollider origSphere) {
            SphereCollider newSphere = newParent.AddComponent<SphereCollider>();
            newSphere.center = origSphere.center;
            newSphere.radius = origSphere.radius;
            newColl = newSphere;
        }

        // Copy CapsuleCollider properties
        else if (collider is CapsuleCollider origCapsule) {
            CapsuleCollider newCapsule = newParent.AddComponent<CapsuleCollider>();
            newCapsule.center = origCapsule.center;
            newCapsule.radius = origCapsule.radius;
            newCapsule.height = origCapsule.height;
            newCapsule.direction = origCapsule.direction;
            newColl = newCapsule;
        }

        else {
            log_Failed(collider);
            return;
        }

        // Copy general Collider properties
        newColl.material = collider.material;
        newColl.gameObject.layer = LayerMask.NameToLayer(DuplicateLayerName);
        switch (ChangeTriggerMode) {
            case ChangeTriggerMode.KeepOriginal:
                newColl.isTrigger = collider.isTrigger;
                break;

            case ChangeTriggerMode.MakeAllTriggers:
                newColl.isTrigger = true;
                break;

            case ChangeTriggerMode.MakeAllColliders:
                newColl.isTrigger = false;
                break;
        }

        // Attach a physics target component, if requested
        if (PhysicsTarget != null) {
            PhysTarget target = newParent.AddComponent<PhysTarget>();
            if (target != null)
                target.TargetComponent = PhysicsTarget;
        }
    }

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, Exception?> LOG_FAILED_ACTION =
        LoggerMessage.Define<string>(Warning,
            new EventId(id: 0, nameof(log_Failed)),
            "Collider '{Collider}' is not a BoxCollider, SphereCollider, or CapsuleCollider, so it will not be duplicated"
        );
    private void log_Failed(Collider collider) => LOG_FAILED_ACTION(_logger!, collider.GetHierarchyName(), null);

    #endregion
}
