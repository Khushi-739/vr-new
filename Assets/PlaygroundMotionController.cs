using UnityEngine;

public class PlaygroundMotionController : MonoBehaviour
{
    private Gyroscope gyro;
    private bool gyroEnabled;
    private Animator samAnimator;
    private Rigidbody headBallRb;

    [Header("Scene References")]
    public Transform headBall;      // SM_Ball_01 (rotation only)
    public Transform tiltSphere;    // Sphere (tilt movement)
    public Transform samCharacter;  // Sam (Animator)

    [Header("Smoothing")]
    public float rotationSmooth = 8f;

    [Header("Tilt Movement")]
    public float tiltMoveSpeed = 4f;

    [Header("Shake Detection")]
    public float shakeThreshold = 1.5f;
    public float shakeCooldown = 1f;

    private float lastShakeTime;

    void Start()
    {
        // -------------------------
        // Enable Gyroscope
        // -------------------------
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            gyroEnabled = true;
        }

        // -------------------------
        // Setup Animator
        // -------------------------
        if (samCharacter != null)
            samAnimator = samCharacter.GetComponent<Animator>();

        // -------------------------
        // Setup Rigidbody for headBall
        // -------------------------
        if (headBall != null)
        {
            headBallRb = headBall.GetComponent<Rigidbody>();

            if (headBallRb != null)
            {
                headBallRb.useGravity = false;
                headBallRb.isKinematic = true;   // IMPORTANT → stops physics movement
                headBallRb.constraints = RigidbodyConstraints.FreezePosition;
            }
        }

        Debug.Log("Gyro supported: " + SystemInfo.supportsGyroscope);
    }

    void Update()
    {
Debug.Log(gyro.rotationRateUnbiased);
        if (gyroEnabled)
            HandleHeadRotation();

        HandleTiltMovement();
        HandleShake();
    }

    // ------------------------------------------------
    // 1️⃣ HEAD ROTATION → ROTATE BALL IN PLACE
    // ------------------------------------------------
    void HandleHeadRotation()
{
    if (headBall == null) return;

    Vector3 rotationRate = gyro.rotationRateUnbiased;

    float sensitivity = 2.0f;

    Vector3 deltaRotation = new Vector3(
        -rotationRate.x,
        -rotationRate.y,
        rotationRate.z
    ) * sensitivity;

    headBall.Rotate(deltaRotation, Space.Self);
}

    // ------------------------------------------------
    // 2️⃣ TILT PHONE → MOVE SPHERE LEFT/RIGHT
    // ------------------------------------------------
    void HandleTiltMovement()
    {
        if (tiltSphere == null) return;

        Vector3 accel = Input.acceleration;

        float moveX = accel.x;

        Vector3 movement = new Vector3(moveX, 0f, 0f);

        tiltSphere.position += movement * tiltMoveSpeed * Time.deltaTime;
    }

    // ------------------------------------------------
    // 3️⃣ SHAKE → TRIGGER SAM JUMP
    // ------------------------------------------------
    void HandleShake()
    {
        if (samAnimator == null) return;

        Vector3 accel = Input.acceleration;

        if (accel.magnitude > shakeThreshold &&
            Time.time - lastShakeTime > shakeCooldown)
        {
            lastShakeTime = Time.time;

            samAnimator.SetTrigger("JumpTrigger");

            Debug.Log("Jump Triggered!");
        }
    }
}