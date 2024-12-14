using UnityEngine;

public enum ShapeType
{
    SPHERE,
    PLANE
}

// *Unity Physics are not allowed. We must make EVERYTHING from scratch in this course!*
public class PhysicsBody : MonoBehaviour
{
    public Vector3 pos = Vector3.zero;
    public Vector3 vel = Vector3.zero;

    // Drag that's near-zero makes velocity smaller (ie 2 * 0.05 = 0.1
    // Drag that's near-one makes velocity larger (ie 2 * 0.95 = 1.9)
    // Drag must be between 0 and 1. Drag of 1 means no air resistance!
    public float drag = 1.0f;

    // 1.0 means mass of 1, 0 means mass of infinity
    public float invMass = 1.0f;

    // How collision detection is done (sphere or plane)
    public ShapeType shapeType = ShapeType.SPHERE;

    // If type is SPHERE, use radius
    public float radius = 1.0f;

    // If type is PLANE, use normal
    public Vector3 normal = Vector3.up;

    // Whether the body collided with another last physics update
    public bool collision = false;

    // Coefficient of friction (both static & kinetic for simplicity)
    public float frictionCoefficient = 1.0f;

    // Coefficient of restitution (1.0 = no energy lost, 0.0 = all energy lost)
    public float restitutionCoefficient = 1.0f;

    // Friction force as an impulse
    public Vector3 frictionImpulse = Vector3.zero;

    // Normal force as an impulse
    public Vector3 normalImpulse = Vector3.zero;

    public bool Dynamic()
    {
        return invMass == 0.0f ? false : true;
    }
}
