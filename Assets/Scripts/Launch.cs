using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launch : MonoBehaviour
{
    Vector3 acc = Vector3.zero;
    Vector3 vel = Vector3.zero;
    Vector3 pos = Vector3.zero;
    bool launched = false;

    // Poll input in update to prevent missed inputs, apply single-use changes on-input.
    void Update()
    {
        // Reset
        if (Input.GetKeyDown(KeyCode.R))
            ResetProjectile();

        // Launch
        if (Input.GetKeyDown(KeyCode.L))
            LaunchProjectile();
    }

    // ***DO NOT PUT INPUT P0LLING IN HERE AS IT ONLY RUNS AT 50HZ BY DEFAULT***
    void FixedUpdate()
    {
        // Update motion
        float dt = Time.fixedDeltaTime;
        vel = vel + acc * dt;
        pos = pos + vel * dt;

        // Render motion
        transform.position = pos;

        // Disregarding this part of the lab
        //Debug.DrawLine(transform.position, transform.position + launchDirection * launchMagnitude, Color.magenta);
    }

    void ResetProjectile()
    {
        Debug.Log("Reseting Projectile...");
        acc = Vector3.zero;
        vel = Vector3.zero;
        pos = Vector3.zero;
        launched = false;
    }

    void LaunchProjectile()
    {
        // Extra practice: change the velocity to move the projectile
        // *forward* instead of *right* (yz instead of xy)
        // **(Note to self -- show curve rendering and motion solving in lecture)**
        float launchSpeed = 10.0f;
        float launchAngle = 60.0f;
        Vector3 launchDirection = new Vector2(
            Mathf.Cos(launchAngle * Mathf.Deg2Rad),
            Mathf.Sin(launchAngle * Mathf.Deg2Rad));

        Debug.Log("Launching Projectile!!!");
        Vector3 launchVelocity = launchDirection * launchSpeed;
        acc = Physics.gravity;
        vel = launchVelocity;
        launched = true;
    }
}
