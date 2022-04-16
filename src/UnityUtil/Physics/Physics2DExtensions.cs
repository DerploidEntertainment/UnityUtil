namespace UnityEngine;

public static class Physics2DExtensions
{

    public static void AddForce(this Rigidbody2D rigidbody2D, Vector2 force, ForceMode mode = ForceMode.Force)
    {
        switch (mode) {
            case ForceMode.Force: rigidbody2D.AddForce(force); break;
            case ForceMode.Impulse: rigidbody2D.AddForce(force / Time.fixedDeltaTime); break;
            case ForceMode.Acceleration: rigidbody2D.AddForce(force * rigidbody2D.mass); break;
            case ForceMode.VelocityChange: rigidbody2D.AddForce(force * rigidbody2D.mass / Time.fixedDeltaTime); break;
        }
    }

}
