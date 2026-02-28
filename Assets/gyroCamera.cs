using UnityEngine;
using System.Collections;

public class GyroCameraFinal : MonoBehaviour
{
    Gyroscope gyro;
    bool gyroEnabled = false;
    Quaternion rotationFix;

    [Range(0.01f, 1f)]
    public float smooth = 0.15f;

    public float moveSpeed = 0f; 

    void Start()
    {
        StartCoroutine(InitGyro());
    }

    IEnumerator InitGyro()
    {
        yield return new WaitForSeconds(1f);

        if (!SystemInfo.supportsGyroscope)
        {
            Debug.Log("No Gyroscope Found");
            yield break;
        }

        gyro = Input.gyro;
        gyro.enabled = true;

        rotationFix = new Quaternion(0, 0, 1, 0);
        gyroEnabled = true;

        Debug.Log("Gyroscope Enabled");
    }

    void Update()
    {
        if (!gyroEnabled) return;

        // Get raw gyro rotation
        Quaternion rawRotation = gyro.attitude * rotationFix;

        // Convert quaternion → Euler
        Vector3 euler = rawRotation.eulerAngles;

        // Keep only left/right (yaw), remove up/down & tilt
        float yawOnly = euler.y;

        // Create filtered (locked) rotation
        Quaternion targetRotation = Quaternion.Euler(0f, yawOnly, 0f);

        // Smooth rotation
        // transform.localRotation = Quaternion.Slerp(
        //     transform.localRotation,
        //     targetRotation,
        //     smooth
        // );

        // Camera movement DISABLED for now (scene stays fixed)
        /*
        // Optional camera movement (forward/backward)
        if (moveSpeed > 0)
        {
            float forward = Input.acceleration.y; // tilt phone forward/back
            transform.position += transform.forward * forward * moveSpeed * Time.deltaTime;
        }
        */
    }
}
