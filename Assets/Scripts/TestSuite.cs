using UnityEngine;

public class TestSuite : MonoBehaviour
{
    float Time(float vi, float theta, float g = 1.6f)
    {
        theta = Mathf.Deg2Rad * theta;
        return 2.0f * vi * Mathf.Sin(theta) / g;
    }

    float Range(float vi, float theta, float g = 1.6f)
    {
        theta = Mathf.Deg2Rad * theta;
        return ((vi * vi) * Mathf.Sin(theta * 2.0f)) / g;
    }

    void Start()
    {
        const int count = 19;
        float vi = 120.0f;

        // Solve for time & range, given angle
        float[] times = new float[count];
        float[] ranges = new float[count];
        float[] angles = new float[count];

        angles[0] = 0.0f;
        angles[1] = 3.19f;
        angles[2] = 6.42f;
        angles[3] = 9.736f;
        angles[4] = 13.194f;
        angles[5] = 16.874f;
        angles[6] = 20.905f;
        angles[7] = 25.529f;
        angles[8] = 31.367f;
        angles[9] = 45.0f;
        angles[10] = 58.633f;
        angles[11] = 64.471f;
        angles[12] = 69.095f;
        angles[13] = 73.126f;
        angles[14] = 76.806f;
        angles[15] = 80.264f;
        angles[16] = 83.58f;
        angles[17] = 86.81f;
        angles[18] = 90.0f;

        for (int i = 0; i < count; i++)
        {
            float angle = angles[i];
            float time = Time(vi, angle);
            float range = Range(vi, angle);
            Debug.Log("Angle: " + angle + ", Time: " + time + ", Range: " + range);
        }
    }
}
