using UnityEngine;
using U = UnityEngine;

namespace UnityUtil.Physics2D;

/// <summary>
/// Adapted from this <a href="https://forum.unity.com/threads/need-rigidbody2d-addexplosionforce.212173/#post-1426983">Unity Forums post</a>
/// </summary>
public static class Rigidbody2DExtension
{
    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, ForceMode2D mode)
    {
        Vector3 dir = body.transform.position - explosionPosition;
        float dirMag = dir.magnitude;
        float forceMag = explosionForce * (1 - dirMag / explosionRadius);
        body.AddForce(dir / dirMag * forceMag, mode);
    }

    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier, ForceMode2D mode)
    {
        Vector3 dir = body.transform.position - explosionPosition;
        float dirMag = dir.magnitude;
        float forceMag = explosionForce * (1 - dirMag / explosionRadius);
        body.AddForce(dir / dirMag * forceMag, mode);

        float upliftForceMag = explosionForce * (1 - upliftModifier / explosionRadius);
        body.AddForce(-U.Physics2D.gravity.normalized * upliftForceMag, mode);
    }
}
