using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityUtil.DependencyInjection;
using UnityUtil.Math;
using static Microsoft.Extensions.Logging.LogLevel;
using MEL = Microsoft.Extensions.Logging;
using U = UnityEngine;

namespace UnityUtil;

/// <summary>
/// Determines the direction in which <see cref="Spawner.Prefab"/> instances spawned by a <see cref="UnityUtil.Spawner"/> are launched.
/// </summary>
public enum SpawnDirection
{
    /// <summary>
    /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along the <see cref="UnityUtil.Spawner"/>'s forward vector.
    /// </summary>
    Straight,

    /// <summary>
    /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along a random vector within a cone centered around the <see cref="UnityUtil.Spawner"/>'s forward vector.
    /// </summary>
    ConeRandom,

    /// <summary>
    /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along a random vector on the boundary of a cone centered around the <see cref="UnityUtil.Spawner"/>'s forward vector.
    /// </summary>
    ConeBoundary,

    /// <summary>
    /// Spawned <see cref="Spawner.Prefab"/> instances will be launched along any random vector.
    /// </summary>
    AnyDirection,
}

public class Spawner : MonoBehaviour
{
    private ILogger<Spawner>? _logger;

    private GameObject? _previous;
    private long _count;

    [Tooltip(
        "The actual Unity prefab to spawn. We highly recommend using a PREFAB, as opposed to " +
        "an existing GameObject in the Scene, though either will technically work."
    )]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public GameObject? Prefab;

    [Tooltip($"All spawned instances of {nameof(Prefab)} will be parented to this Transform.")]
    public Transform? SpawnParent;

    [Tooltip(
        $"All spawned {nameof(Prefab)} instances will be given this name, along with a numeric suffix. " +
        $"If {nameof(DestroyPrevious)} is true, then the numeric suffix will not be added."
    )]
    public string BaseName = "Object";

    [Tooltip(
        $"If true, then previously spawned {nameof(Prefab)} instances will be destroyed before " +
        "the next instance is spawned, so there will only ever be one spawned instance in existence. " +
        "If false, then multiple instances may be spawned."
    )]
    public bool DestroyPrevious;

    private const string TOOLTIP_LAUNCH_SPEED =
        $"All spawned {nameof(Prefab)} instances will be launched in the {nameof(SpawnDirection)}, " +
        $"with at least this speed. Setting both {nameof(MinSpeed)} and {nameof(MaxSpeed)} to zero will " +
        $"spawn instances right at this {nameof(Spawner)}'s position, without any launching.";

    [Tooltip(TOOLTIP_LAUNCH_SPEED)]
    public float MinSpeed = 0f;

    [Tooltip(TOOLTIP_LAUNCH_SPEED)]
    public float MaxSpeed = 10f;

    [Tooltip($"Defines the direction in which spawned {nameof(Prefab)} instances will be launched.")]
    public SpawnDirection SpawnDirection = SpawnDirection.Straight;

    [Tooltip(
        $"If {nameof(SpawnDirection)} is set to {nameof(SpawnDirection.ConeRandom)} or {nameof(SpawnDirection.ConeBoundary)}, " +
        "then this value determines the half-angle of that cone. Otherwise, this value is ignored."
    )]
    [Range(0f, 90f)]
    public float ConeHalfAngle = 30f;

    public void Inject(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(this);

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    private void Awake() => DependencyInjector.Instance.ResolveDependenciesOf(this);

    public void Spawn()
    {
        // Destroy any previously spawned GameObjects, if requested
        if (_previous != null && DestroyPrevious)
            Destroy(_previous);

        string newName = $"{BaseName}{(DestroyPrevious ? "" : "_" + _count)}";
        log_Spawning(newName);

        // Instantiating a Prefab can sometimes give a GameObject or a Transform...we want the GameObject
        GameObject obj = (SpawnParent == null) ?
            Instantiate(Prefab!, transform.position, transform.rotation) :
            Instantiate(Prefab!, transform.position, transform.rotation, SpawnParent);
        obj.name = newName;
        if (!DestroyPrevious)
            ++_count;

        // If the Prefab has a Rigidbody, apply the requested velocity
#if DEBUG_2D
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
#else
        Rigidbody rb = obj.GetComponent<Rigidbody>();
#endif
        if (rb != null) {
#if DEBUG_2D
                Vector2 dir = getSpawnDirection();
#else
            Vector3 dir = getSpawnDirection();
#endif
            float speed = U.Random.Range(MinSpeed, MaxSpeed);
            if (rb.isKinematic)
                rb.linearVelocity = speed * dir;
            else
#if DEBUG_2D
                    rb.AddForce(speed * dir, ForceMode.VelocityChange);
#else
                rb.AddForce(speed * dir, ForceMode.VelocityChange);
#endif
        }

        _previous = obj;
    }

#if DEBUG_2D
        private Vector2 getSpawnDirection() =>
#else
    private Vector3 getSpawnDirection() =>
#endif
            SpawnDirection switch {
                SpawnDirection.Straight => transform.forward,
                SpawnDirection.ConeRandom => MoreMath.RandomConeVector(transform, ConeHalfAngle, onlyBoundary: false),
                SpawnDirection.ConeBoundary => MoreMath.RandomConeVector(transform, ConeHalfAngle, onlyBoundary: true),
#if DEBUG_2D
                SpawnDirection.AnyDirection => U.Random.insideUnitCircle.normalized,
#else
                SpawnDirection.AnyDirection => U.Random.onUnitSphere,
#endif
                _ => throw UnityObjectExtensions.SwitchDefaultException(SpawnDirection),
            };

    #region LoggerMessages

    private static readonly Action<MEL.ILogger, string, Exception?> LOG_SPAWNING_ACTION =
        LoggerMessage.Define<string>(Information,
            new EventId(id: 0, nameof(log_Spawning)),
            "Spawning object '{Object}'"
        );
    private void log_Spawning(string spawnedObjectName) => LOG_SPAWNING_ACTION(_logger!, spawnedObjectName, null);

    #endregion
}
