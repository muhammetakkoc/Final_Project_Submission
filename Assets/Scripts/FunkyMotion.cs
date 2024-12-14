using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunkyMotion : MonoBehaviour
{
    public float x;
    public float y;
    public float amplitude;
    public float frequency;
    public float time;

    void FixedUpdate()
    {
        // We don't want Time.deltaTime because it *varies*.
        // We want a fixed time-step to ensure our physics engine is stable!
        //Debug.Log(dt);
        
        float dt = Time.fixedDeltaTime;

        // Example of frequency (faster/slower time) & amplitude (min/max)
        y = y + (Mathf.Cos(time * frequency)) * frequency * amplitude * dt;
        x = x + (Mathf.Sin(time * frequency)) * frequency * amplitude * dt;
        float z = 0.0f;//Mathf.Sin(time * 10.0f) * 5.0f;

        // Homework 1: modify x and y to use sin & cos as indicated in the document!
        transform.position = new Vector3(x, y, z);

        time += dt;
    }

    // Variable timestamp, MUCH faster than the default fixed-time-stamp of 50hz
    //void Update()
    //{
    //    float dt = Time.deltaTime;
    //    Debug.Log(dt);
    //}
}
