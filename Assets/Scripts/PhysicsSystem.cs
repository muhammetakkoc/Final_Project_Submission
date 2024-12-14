using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HitPair
{
    public int a = -1;
    public int b = -1;
    public Vector3 mtv = Vector3.zero;
}

public delegate void OnCollision(GameObject a, GameObject b/*, Vector3 mtv*/);

public class PhysicsSystem
{
    public OnCollision collisionCallback = null;
    public Vector3 gravity = Physics.gravity;
    private PhysicsBody[] bodies = null;

    public void Step(float dt)
    {
        // 1. Apply motion
        UpdateMotion(dt);

        // 2. Detect collisions
        List<HitPair> collisions = DetectCollisions();

        // 3. Resolve collisions
        ResolveCollisions(collisions);
    }

    // LE9 TODO -- Use impulse-momentum theorem to compute force from friction-force and normal-force value
    // I = F * t (impulse = net-force * time)
    private void UpdateMotion(float dt)
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            // Current physics object ("body")
            PhysicsBody body = bodies[i];

            // Only apply motion if the object is moveable
            if (body.Dynamic())
            {
                Vector3 acc = gravity;

                // Apply drag to velocity
                body.vel *= Mathf.Pow(body.drag, dt);

                // Apply motion
                Integrate(ref body.vel, acc, dt);
                Integrate(ref body.pos, body.vel, dt);
            }
        }
    }

    private List<HitPair> DetectCollisions()
    {
        // Reset collision flag before hit-testing
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].collision = false;
        }

        List<HitPair> collisions = new List<HitPair>();
        // Test all bodies for collision, store pairs of colliding objects
        for (int i = 0; i < bodies.Length; i++)
        {
            for (int j = i + 1; j < bodies.Length; j++)
            {
                PhysicsBody a = bodies[i];
                PhysicsBody b = bodies[j];

                Vector3 mtv = Vector3.zero;
                bool collision = false;
                if (a.shapeType == ShapeType.SPHERE && b.shapeType == ShapeType.SPHERE)
                {
                    collision = SphereSphere(a.pos, a.radius, b.pos, b.radius, out mtv);
                }
                else if (a.shapeType == ShapeType.SPHERE && b.shapeType == ShapeType.PLANE)
                {
                    collision = SpherePlane(a.pos, a.radius, b.pos, b.normal, out mtv);
                }
                else if (a.shapeType == ShapeType.PLANE && b.shapeType == ShapeType.SPHERE)
                {
                    collision = SpherePlane(b.pos, b.radius, a.pos, a.normal, out mtv);
                }
                else
                {
                    Debug.LogError("Invalid collision test");
                }
                a.collision |= collision;
                b.collision |= collision;

                if (collision)
                {
                    HitPair hitPair = new HitPair();
                    hitPair.a = i; hitPair.b = j;
                    hitPair.mtv = mtv;
                    collisions.Add(hitPair);
                }
            }
        }
        return collisions;
    }

    private void ResolveCollisions(List<HitPair> collisions)
    {
        // Pre-pass to ensure A is *always* dynamic and MTV points from B to A
        foreach (HitPair collision in collisions)
        {
            if (!bodies[collision.a].Dynamic())
            {
                int temp = collision.a;
                collision.a = collision.b;
                collision.b = temp;

                if (Vector3.Dot(Vector3.Normalize(collision.mtv), Vector3.Normalize(bodies[collision.a].pos - bodies[collision.b].pos)) < 0.0f)
                {
                    collision.mtv *= -1.0f;
                }
            }
        }

        ResolveVelocities(collisions);
        ResolvePositions(collisions);

        // Notify user a collision has occurred (might need to add more information or change where this is called)
        if (collisionCallback != null)
        {
            foreach (HitPair collision in collisions)
            {
                GameObject a = bodies[collision.a].gameObject;
                GameObject b = bodies[collision.b].gameObject;
                collisionCallback(a, b);
            }
        }
    }

    // Change velocity based on friction (& restutition in the future)
    void ResolveVelocities(List<HitPair> collisions)
    {
        foreach (HitPair collision in collisions)
        {
            PhysicsBody a = bodies[collision.a];
            PhysicsBody b = bodies[collision.b];

            // No motion to resolve if both bodies are static (should never happen, but still)
            float invMassSum = a.invMass + b.invMass;
            if (invMassSum <= Mathf.Epsilon)
                continue;
            
            // Velocity of A relative to B
            Vector3 velBA = a.vel - b.vel;
            Vector3 mtvDir = Vector3.Normalize(collision.mtv);
            float mtvMag = collision.mtv.magnitude;

            // How similar motion of A relative to B is to the direction we want to move A and/or B
            float t = Vector3.Dot(velBA, mtvDir);

            // Don't change velocities if object are already moving away from each other
            // (Only change if velocities within are less than 90 degrees of each other)
            if (t > 0.0f)
                continue;

            float restitution = Mathf.Min(a.restitutionCoefficient, b.restitutionCoefficient);
            float normalImpulseMagnitude = -(1.0f + restitution) * t / invMassSum;

            // p = mv --> v = p / m
            Vector3 normalImpulse = mtvDir * normalImpulseMagnitude;
            a.vel += normalImpulse * a.invMass;
            b.vel -= normalImpulse * b.invMass;

            Vector3 frictionImpulseDirection = Vector3.Normalize(velBA - (mtvDir * t));
            float frictionImpulseMagnitude = -Vector3.Dot(velBA, frictionImpulseDirection) / invMassSum;
            float mu = Mathf.Sqrt(a.frictionCoefficient * b.frictionCoefficient); // <-- Coulomb's Law (how to combine friction coefficients)
            frictionImpulseMagnitude = Mathf.Clamp(frictionImpulseMagnitude, -normalImpulseMagnitude * mu, normalImpulseMagnitude * mu);

            // p = mv --> v = p / m
            Vector3 frictionImpulse = frictionImpulseDirection * frictionImpulseMagnitude;
            a.vel += frictionImpulse * a.invMass;
            b.vel -= frictionImpulse * b.invMass;

            // Update impulses for visualization (impulses applied as velocities, these are just for rendering)
            a.normalImpulse = normalImpulse;
            b.normalImpulse = normalImpulse;
            a.frictionImpulse = frictionImpulse;
            b.frictionImpulse = -frictionImpulse;
        }
    }

    // Apply mtv to A and B
    void ResolvePositions(List<HitPair> collisions)
    {
        foreach (HitPair collision in collisions)
        {
            PhysicsBody a = bodies[collision.a];
            PhysicsBody b = bodies[collision.b];

            if (!b.Dynamic())
            {
                a.pos += collision.mtv;
            }
            else
            {
                a.pos += collision.mtv * 0.5f;
                b.pos -= collision.mtv * 0.5f;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < bodies.Length; i++)
            GameObject.Destroy(bodies[i].gameObject);
        bodies = new PhysicsBody[0];
    }

    // Apply changes from GameObject to PhysicsBody
    public void PreStep()
    {
        bodies = GameObject.FindObjectsByType<PhysicsBody>(FindObjectsSortMode.None);
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].shapeType == ShapeType.PLANE)
            {
                bodies[i].normal = bodies[i].transform.up;
            }

            bodies[i].pos = bodies[i].transform.position;
            bodies[i].gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    // Apply changes from PhysicsBody to GameObject
    public void PostStep()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            PhysicsBody body = bodies[i];
            Color color = body.collision ? Color.red : Color.green;
            body.GetComponent<Renderer>().material.color = color;
            body.transform.position = bodies[i].pos;

            // LE9 TODO -- Render friction & normal force instead of impulse
            if (body.shapeType == ShapeType.SPHERE)
            {
                Debug.DrawLine(body.pos, body.pos + body.normalImpulse * 5.0f, Color.cyan);
                Debug.DrawLine(body.pos, body.pos + body.frictionImpulse * 5.0f, Color.blue);
            }
        }
    }

    // "Integration" is updating a value based on the previous value + its change
    private void Integrate(ref Vector3 value, Vector3 change, float dt)
    {
        value = value + change * dt;
    }

    private bool SphereSphere(Vector3 position1, float radius1, Vector3 position2, float radius2)
    {
        // Collision if distance between centres is less than radii sum
        float distance = Vector3.Distance(position1, position2);
        float radiiSum = radius1 + radius2;
        return distance <= radiiSum;
    }

    private bool SpherePlane(Vector3 spherePosition, float radius, Vector3 planePosition, Vector3 normal)
    {
        // Collision if distance of circle projected onto plane normal is less than radius
        float distance = Vector3.Dot(spherePosition - planePosition, normal);
        return distance <= radius;
    }

    // Ensure MTV resolves A from B (to 1 from 2)
    private bool SphereSphere(Vector3 position1, float radius1, Vector3 position2, float radius2, out Vector3 mtv)
    {
        bool collision = SphereSphere(position1, radius1, position2, radius2);
        if (collision)
        {
            Vector3 direction = Vector3.Normalize(position1 - position2);
            float radiiSum = radius1 + radius2;
            float distance = Vector3.Distance(position1, position2);
            float depth = radiiSum - distance;
            mtv = direction * depth;
            // Expressed in one line (same math as above):
            //mtv = Vector3.Normalize(position1 - position2) * ((radius1 + radius2) - Vector3.Distance(position1, position2));
        }
        else
        {
            mtv = Vector3.zero;
        }
        return collision;
    }

    // Ensure MTV points FROM plane TO sphere
    private bool SpherePlane(Vector3 spherePosition, float radius, Vector3 planePosition, Vector3 normal, out Vector3 mtv)
    {
        bool collision = SpherePlane(spherePosition, radius, planePosition, normal);
        if (collision)
        {
            float distance = Vector3.Dot(spherePosition - planePosition, normal);
            float depth = radius - distance;
            mtv = normal * depth;
            // Expressed in one line (same math as above):
            //mtv = normal * (radius - Vector3.Dot(spherePosition - planePosition, normal));
        }
        else
        {
            mtv = Vector3.zero;
        }
        return collision;
    }
}
