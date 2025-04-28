using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityUtil.Inputs;
using UnityUtil.Triggers;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Physics;

/// <summary>
/// Represents the means by which a <see cref="Liftable"/> came to be released from a <see cref="Lifter"/>.
/// </summary>
public enum LiftableReleaseType
{
    /// <summary>
    /// The <see cref="Liftable"/> was accidentally released, e.g. by a force breaking its connecting <see cref="Joint"/>.
    /// </summary>
    Accidental,
    /// <summary>
    /// The <see cref="Liftable"/> was purposefully released via player input.
    /// </summary>
    Purposeful,
    /// <summary>
    /// The <see cref="Liftable"/> was purposefully released by throwing.
    /// </summary>
    Thrown,
}

[Serializable]
public class LiftablePickupEvent : UnityEvent<Liftable, Lifter> { }
[Serializable]
public class LiftableReleaseEvent : UnityEvent<Liftable, Lifter, LiftableReleaseType> { }

[DisallowMultipleComponent]
public class Lifter : Updatable
{
    private Transform? _oldParent;
    private bool _oldKinematic;
    private bool _oldUseGravity;

    [Header("Inputs")]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? LiftInput;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? ThrowInput;

    [Header("Options")]
    [Tooltip(
        $"If true, {nameof(Liftable)}s will be attached to this {nameof(Lifter)} via the {nameof(LiftingJoint)}. " +
        $"Otherwise, the {nameof(Liftable)} will just be parented to the {nameof(LiftingObject)}."
    )]
    public bool LiftUsingPhysics = false;

    [Tooltip(
        $"If {nameof(LiftUsingPhysics)} is true, then {nameof(Liftable)}s will be attached to this {nameof(Lifter)} via the Joint on this GameObject. " +
        $"Ignored if {nameof(LiftUsingPhysics)} is false."
    )]
    public JointBreakTrigger? LiftingJoint;

    [Tooltip(
        $"If {nameof(LiftUsingPhysics)} is false, then {nameof(Liftable)}s will be attached to this Transform via parenting. " +
        $"Ignored if {nameof(LiftUsingPhysics)} is true."
    )]
    public Transform? LiftingObject;
    public LayerMask LiftableLayerMask;
    public float Reach = 4f;
    public float MaxMass = 10f;

    [Header("Throwing")]
    public bool CanThrow = true;
    public float ThrowForce = 10f;

    protected override void Awake()
    {
        base.Awake();

        if ((LiftUsingPhysics && LiftingJoint == null) || (!LiftUsingPhysics && LiftingObject == null)) {
            throw new InvalidOperationException(
                $"{this.GetHierarchyNameWithType()} must have a {nameof(LiftingJoint)} if {nameof(LiftUsingPhysics)} is set to true, " +
                $"or a {nameof(LiftingObject)} if {nameof(LiftUsingPhysics)} is set to false."
            );
        }

        AddUpdate(doUpdate);
    }

    private void doUpdate(float deltaTime)
    {
        // Get user input
        bool toggleLift = LiftInput!.Started();
        bool throwing = ThrowInput!.Started();
        if (toggleLift && throwing)
            return;

        // If the player pressed Use, then pick up or drop a load
        if (toggleLift) {
            if (CurrentLiftable == null)
                pickup();
            else {
                LiftingJoint!.Joint!.connectedBody = null;
                release(LiftableReleaseType.Purposeful);
            }
        }

        // If the player pressed Throw and throwing is currently applicable, then do throw actions
        if (throwing && CanThrow && CurrentLiftable != null)
            doThrow();
    }
    private void onJointBreak(Joint joint) => release(LiftableReleaseType.Accidental);

    public Liftable? CurrentLiftable { get; private set; }
    public LiftablePickupEvent LoadPickedUp = new();
    public LiftableReleaseEvent LoadReleased = new();

    private void pickup()
    {
        // Check if a physical object that's not too heavy is within range
        // If not, then just return
        Rigidbody? rb = null;
        bool loadAhead = U.Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Reach, LiftableLayerMask);
        if (loadAhead) {
            rb = hitInfo.collider.attachedRigidbody;
            if (rb != null && rb.mass <= MaxMass) {
                CurrentLiftable = rb.GetComponent<Liftable>();
                if (!CurrentLiftable?.CanLift ?? false)
                    CurrentLiftable = null;
            }
        }
        if (CurrentLiftable == null)
            return;

        // Connect that Liftable using Physics, if requested
        // Cant use Rigidbody.position/rotation b/c we're about to add it to a Joint
        Transform loadTrans = rb!.transform;
        _oldParent = loadTrans.parent;
        _oldKinematic = rb.isKinematic;
        _oldUseGravity = rb.useGravity;
        rb.useGravity = false;
        if (LiftUsingPhysics) {
            loadTrans.position = transform.TransformPoint(CurrentLiftable.LiftOffset);
            if (CurrentLiftable.UsePreferredRotation)
                loadTrans.rotation = transform.rotation * Quaternion.Euler(CurrentLiftable.PreferredLiftRotation);
            LiftingJoint!.Joint!.connectedBody = rb;
            rb.isKinematic = false;
            LiftingJoint.Broken.AddListener(onJointBreak);
        }

        // Otherwise, connect it via parenting
        else {
            loadTrans.parent = LiftingObject;
            rb.isKinematic = true;
            loadTrans.localPosition = CurrentLiftable.LiftOffset;
            if (CurrentLiftable.UsePreferredRotation)
                loadTrans.localRotation = Quaternion.Euler(CurrentLiftable.PreferredLiftRotation);
        }

        // Raise the PickUp event
        CurrentLiftable.Lifter = this;
        LoadPickedUp.Invoke(CurrentLiftable, this);
    }
    private void release(LiftableReleaseType releaseType)
    {
        // Disconnect the Liftable using Physics, if requested
        Rigidbody rb = CurrentLiftable!.GetComponent<Rigidbody>();
        if (LiftUsingPhysics)
            LiftingJoint!.Broken.RemoveListener(onJointBreak);

        // Otherwise, disconnect it by unparenting
        else
            CurrentLiftable.transform.parent = _oldParent;

        // Either way, adjust the Liftable's Rigidbody
        rb.isKinematic = _oldKinematic;
        rb.useGravity = _oldUseGravity;

        // Either way, raise the Released event
        CurrentLiftable.Lifter = null;
        Liftable liftable = CurrentLiftable;
        CurrentLiftable = null;
        LoadReleased.Invoke(liftable, this, releaseType);
    }
    private void doThrow()
    {
        // Disconnect the Liftable
        Liftable? liftable = CurrentLiftable;
        LiftingJoint!.Joint!.connectedBody = null;
        release(LiftableReleaseType.Thrown);

        // Apply the throw force
        Rigidbody rb = liftable!.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * ThrowForce, ForceMode.Impulse);
    }

}
