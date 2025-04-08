using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Inputs;
using UnityUtil.Math;
using UnityUtil.Updating;
using U = UnityEngine;

namespace UnityUtil.Movement;

public class CharacterFPSWalker : Updatable
{
    private float _oldHeight;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public CharacterController? ControllerToMove;

    [Header("Inputs")]

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? SprintInput;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? CrouchInput;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public StartStopInput? JumpInput;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public ValueInput? HorizontalInput;

    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public ValueInput? VerticalInput;


    [Header("Speed")]
    public float WalkSpeed = 15f;
    public float SprintSpeed = 30f;
    public bool CanSprint = true;
    public float CrouchSpeed = 5f;
    public bool CanCrouch = true;
    public float CrouchHeight = 1f;
    public bool LimitDiagonalSpeed = true;
    public bool ReduceSpeedOnSlopes = true;

    [Header("Jumping")]
    public float Mass = 70f;
    public bool CanJump = true;
    public float JumpHeight = 3f;

    protected override void Awake()
    {
        base.Awake();

        _oldHeight = ControllerToMove!.height;
        CrouchHeight = Mathf.Min(CrouchHeight, _oldHeight);

        AddUpdate(doUpdate);
    }
    private void doUpdate(float deltaTime)
    {
        Vector3 targetV = Vector3.zero;

        // Adjust target velocity for jumping, if its allowed
        if (CanJump) {
            bool jumped = JumpInput!.Started();
            targetV += jumpComponent(deltaTime, jumped);
        }

        // Adjust target velocity for movement, if its allowed
        bool sprinting = SprintInput!.Happening();
        bool crouching = CrouchInput!.Happening();
        float inputHorz = HorizontalInput!.DiscreteValue();   // raw means only returns one of: { -1, 0, 1 }
        float inputVert = VerticalInput!.DiscreteValue();     // raw means only returns one of: { -1, 0, 1 }
        targetV += moveComponent(inputHorz, inputVert, CanSprint && sprinting, CanCrouch && crouching);

        // Do crouching if its allowed
        crouch(CanCrouch && crouching);

        // Move the rigidbody to the target velocity
        ControllerToMove!.Move(targetV * deltaTime);
    }

    private Vector3 jumpComponent(float deltaTime, bool jumping)
    {
        // Account for gravity
        Vector3 jumpV = Vector3.zero;
        float g = U.Physics.gravity.magnitude * Mass;

        // Account for jumping (if it is allowed and the button was pressed)
        if (jumping && ControllerToMove!.isGrounded)
            jumpV.y += Mathf.Sqrt(2f * g * JumpHeight);
        else
            jumpV.y -= g * deltaTime;

        return jumpV;
    }
    private void crouch(bool crouching) => ControllerToMove!.height = crouching ? CrouchHeight : _oldHeight;
    private Vector3 moveComponent(float horz, float vert, bool sprinting, bool crouching)
    {
        // Determine the slope of the ground
        bool hitGround = U.Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.PositiveInfinity);
        float slopeAngle = hitGround ? Vector3.Angle(Vector3.up, hitInfo.normal) : 0f;

        // Get the target movement vector (speed + direction)
        float targetSpeed = getTargetSpeed(horz, vert, sprinting, crouching, slopeAngle);
        Vector3 unitDir = (transform.forward * vert + transform.right * horz).normalized;
        Vector3 targetV = targetSpeed * unitDir;

        return targetV;
    }
    private float getTargetSpeed(float horz, float vert, bool sprinting, bool crouching, float slopeAngle)
    {
        // If we're not moving then just return zero
        if (horz == 0f && vert == 0f)
            return 0f;

        // Adjust for running/crouching (pressing both cancels each other out)
        float speed = WalkSpeed;
        if (sprinting && !crouching && vert != 0f)
            speed = SprintSpeed;
        else if (crouching && !sprinting)
            speed = CrouchSpeed;

        // Account for diagonal motion
        bool isDiagonal = horz != 0f && vert != 0f;
        if (isDiagonal && LimitDiagonalSpeed)
            speed /= MoreMath.Sqrt2;

        // Account for slopes
        // If speed decreases with slope, then speed = 0 when slope = slopeLimit
        float slopeRatio = 1f - slopeAngle / ControllerToMove!.slopeLimit;
        speed *= slopeRatio;

        return speed;
    }

}
