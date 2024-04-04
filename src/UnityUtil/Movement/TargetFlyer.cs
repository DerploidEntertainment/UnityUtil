using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.Updating;

namespace UnityUtil.Movement;

public class TargetFlyer : Updatable
{
    private float _tSinceIdleMove;
    private float _tSinceIdleTorque;
    private float _idleMovePeriod;
    private float _idleTorquePeriod;

    public bool Idle;

    [Required]
    public Rigidbody? FlyingRigidbody;

    [Header("Flying Settings")]
    public Vector3 Target;
    public float MoveSpeed = 20f;
    public float MoveAccel = 35f;
    public bool RotateClockWise = true;
    public float RotateSpeed = 5f;
    public float RotateAccel = 5f;

    [Header("Idle Settings")]
    public float MinMovePeriod = 0.5f;
    public float MaxMovePeriod = 2f;
    public float MinMoveSpeed = 0.5f;
    public float MaxMoveSpeed = 2f;
    public float MinTorquePeriod = 0.5f;
    public float MaxTorquePeriod = 2f;
    public float MinTorqueSpeed = 0.5f;
    public float MaxTorqueSpeed = 2f;

    protected override void Awake()
    {
        base.Awake();

        // Set initial idle move/torque periods for if/when we idle
        _idleMovePeriod = Random.Range(MinMovePeriod, MaxMovePeriod);
        _idleTorquePeriod = Random.Range(MinTorquePeriod, MaxTorquePeriod);

        RegisterUpdatesAutomatically = true;
        FixedUpdateAction = flyOrIdle;
    }
    private void flyOrIdle(float deltaTime)
    {
        if (FlyingRigidbody == null)
            return;

        // If idling, then just apply an occasional, random force
        if (Idle) {
            FlyingRigidbody.AddForce(getIdleForce(deltaTime), ForceMode.VelocityChange);
            FlyingRigidbody.AddTorque(getIdleTorque(deltaTime), ForceMode.VelocityChange);
        }

        // Move/rotate towards the target vector
        else {
            FlyingRigidbody.AddForce(getFlyingForce(Target), ForceMode.Acceleration);
            FlyingRigidbody.AddTorque(getFlyingTorque(Target), ForceMode.Acceleration);
        }
    }

    private Vector3 getFlyingForce(Vector3 targetPosition)
    {
        Vector3 netForce = Vector3.zero;

        // Add a Force to move towards the target position at constant velocity
        Vector3 toward = (targetPosition - transform.position).normalized;
        var vToward = Vector3.Project(FlyingRigidbody!.velocity, toward);
        float factor = vToward.normalized == toward ? Mathf.Sign(MoveSpeed * MoveSpeed - vToward.sqrMagnitude) : 1;
        netForce += factor * MoveAccel * toward;

        // Add a Force to reduce any velocity in the normal direction
        var vNorm = Vector3.ProjectOnPlane(FlyingRigidbody.velocity, toward);
        Vector3 norm = vNorm.normalized;
        if (vNorm.sqrMagnitude > 0f)
            netForce -= MoveAccel * norm;

        return netForce;
    }
    private Vector3 getFlyingTorque(Vector3 targetPosition)
    {
        Vector3 netTorque = Vector3.zero;

        // Add a Force to rotate around the target direction at constant angular velocity
        Vector3 toward = (targetPosition - transform.position).normalized;
        var wToward = Vector3.Project(FlyingRigidbody!.angularVelocity, toward);
        float factor = wToward.normalized == toward ? Mathf.Sign(RotateSpeed * RotateSpeed - wToward.sqrMagnitude) : 1;
        factor *= RotateClockWise ? 1 : -1;
        netTorque += factor * RotateAccel * toward;

        // Add a Force to reduce any angular velocity around the normal direction
        var wNorm = Vector3.ProjectOnPlane(FlyingRigidbody.angularVelocity, toward);
        Vector3 norm = wNorm.normalized;
        if (wNorm.sqrMagnitude > 0f)
            netTorque -= RotateAccel * norm;

        return netTorque;
    }
    private Vector3 getIdleForce(float deltaTime)
    {
        _tSinceIdleMove += deltaTime;
        if (_tSinceIdleMove >= _idleMovePeriod) {
            _tSinceIdleMove -= _idleMovePeriod;
            _idleMovePeriod = Random.Range(MinMovePeriod, MaxMovePeriod);
            float mag = Random.Range(MinMoveSpeed, MaxMoveSpeed);
            return mag * Random.onUnitSphere;
        }

        return Vector3.zero;
    }
    private Vector3 getIdleTorque(float deltaTime)
    {
        _tSinceIdleTorque += deltaTime;
        if (_tSinceIdleTorque >= _idleTorquePeriod) {
            _tSinceIdleTorque -= _idleTorquePeriod;
            _idleTorquePeriod = Random.Range(MinTorquePeriod, MaxTorquePeriod);
            float mag = Random.Range(MinTorqueSpeed, MaxTorqueSpeed);
            return mag * Random.onUnitSphere;
        }

        return Vector3.zero;
    }

}
