using UnityEngine;
using TMPro;

public class SimpleGyroXR : MonoBehaviour
{
    private Gyroscope gyro;
    private bool gyroEnabled;

    public TextMeshProUGUI debugText;

    public float smoothSpeed = 15f;
    private Quaternion smoothedRotation;

    void Start()
    {
        EnableGyro();
        smoothedRotation = transform.localRotation;
    }

    void Update()
{
    if (!gyroEnabled) return;

    Quaternion q = gyro.attitude;

    // Convert right-handed (device) to Unity left-handed
    q = new Quaternion(q.x, q.y, -q.z, -q.w);

    // Apply correct landscape alignment
    Quaternion landscapeFix = Quaternion.Euler(90f, 0f, 0f);
    q = landscapeFix * q;

    // Remove roll safely
    Vector3 forward = q * Vector3.forward;
    forward.y = Mathf.Clamp(forward.y, -0.98f, 0.98f);

    Quaternion yawPitchOnly = Quaternion.LookRotation(forward, Vector3.up);

    smoothedRotation = Quaternion.Slerp(
        smoothedRotation,
        yawPitchOnly,
        1 - Mathf.Exp(-smoothSpeed * Time.deltaTime)
    );

    transform.localRotation = smoothedRotation;

    PrintSensorValues(yawPitchOnly);
}

    void EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            gyroEnabled = true;
        }
        else
        {
            Debug.Log("Gyroscope not supported");
        }
    }

    void PrintSensorValues(Quaternion rot)
    {
        if (debugText == null) return;

        Vector3 accel = Input.acceleration;

        debugText.text =
            "GYRO\n" +
            "Pitch: " + rot.eulerAngles.x.ToString("F2") + "\n" +
            "Yaw: " + rot.eulerAngles.y.ToString("F2") + "\n\n" +
            "ACCEL\n" +
            "X: " + accel.x.ToString("F2") + "\n" +
            "Y: " + accel.y.ToString("F2") + "\n" +
            "Z: " + accel.z.ToString("F2");
    }
}