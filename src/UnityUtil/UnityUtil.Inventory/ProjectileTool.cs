using System.Diagnostics.CodeAnalysis;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityUtil.Inventory;

[RequireComponent(typeof(Tool))]
public class ProjectileTool : MonoBehaviour
{
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public ProjectileToolInfo? Info;

    [Tooltip("The ProjectilePrefab will be parented to this Transform after it is instantiated.")]
    [RequiredIn(PrefabKind.NonPrefabInstance)]
    public Transform? ProjectileParent;

    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity message")]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Unity message")]
    private void Awake()
    {
        Tool? tool = GetComponent<Tool>();
        tool.Used.AddListener(spawnProjectile);
    }

    private void spawnProjectile()
    {
        // Instantiate the Projectile
        Vector3 pos = transform.TransformPoint(Info!.SpawnPosition);
        Quaternion rot = transform.rotation * Quaternion.Euler(Info.SpawnRotation);
        GameObject projectile = Instantiate(Info.ProjectilePrefab, pos, rot, ProjectileParent)!;

        // Propel the Projectile forward, if requested/possible
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb?.AddForce(transform.TransformDirection(Info.InitialVelocity), ForceMode.VelocityChange);
    }

}
