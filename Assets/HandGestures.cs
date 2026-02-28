using UnityEngine;

public class HandGestures : MonoBehaviour
{
    public HandTracking tracker;
    public float pinchThreshold = 0.03f;
    public float openThreshold = 0.06f;
    public float grabRadius = 0.15f;
    public float throwForceMultiplier = 1.5f;

    // Hand rigidbodies for object interaction
    public Rigidbody leftHandRigidbody;
    public Rigidbody rightHandRigidbody;

    private Rigidbody leftHeld = null;
    private Rigidbody rightHeld = null;

    private ConfigurableJoint leftJoint = null;
    private ConfigurableJoint rightJoint = null;

    private Vector3 leftPrevPalm;
    private Vector3 rightPrevPalm;
    private Vector3 leftPalmVelocity;
    private Vector3 rightPalmVelocity;

    void Update()
    {
        HandleHand(tracker.LeftLandmarks, ref leftHeld, ref leftPrevPalm, ref leftPalmVelocity, "Left");
        HandleHand(tracker.RightLandmarks, ref rightHeld, ref rightPrevPalm, ref rightPalmVelocity, "Right");

        // Smoothly move held objects
        if (leftHeld != null) MoveHeldObject(leftHeld, tracker.LeftLandmarks[0]);
        if (rightHeld != null) MoveHeldObject(rightHeld, tracker.RightLandmarks[0]);
    }

    void HandleHand(Vector3[] lm, ref Rigidbody held, ref Vector3 prevPalm, ref Vector3 palmVelocity, string handName)
    {
        if (lm == null || lm.Length < 21 || lm[0] == Vector3.zero)
        {
            if (held != null)
            {
                TryRelease(ref held, palmVelocity, handName);
                Debug.Log($"{handName} tracking lost → force release");
            }
            return;
        }

        Vector3 thumb = lm[4];
        Vector3 index = lm[8];
        Vector3 palm = lm[0];

        palmVelocity = (palm - prevPalm) / Time.deltaTime;

        float pinchDist = Vector3.Distance(thumb, index);

        if (held == null && pinchDist < pinchThreshold)
            TryGrab(palm, ref held, handName);

        if (held != null && pinchDist > openThreshold)
            TryRelease(ref held, palmVelocity, handName);

        prevPalm = palm;
    }

    void TryGrab(Vector3 grabPos, ref Rigidbody held, string handName)
    {
        Collider[] colliders = Physics.OverlapSphere(grabPos, grabRadius);
        float minDist = float.MaxValue;
        Rigidbody closest = null;

        foreach (var col in colliders)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
            {
                float dist = Vector3.Distance(grabPos, rb.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = rb;
                }
            }
        }

        if (closest != null)
        {
            held = closest;
            held.isKinematic = false;

            ConfigurableJoint joint = held.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = (handName == "Left") ? leftHandRigidbody : rightHandRigidbody;

            // Lock rotation but allow slight movement
            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            JointDrive drive = new JointDrive
            {
                positionSpring = 500f,
                positionDamper = 50f,
                maximumForce = 1000f
            };
            joint.xDrive = drive;
            joint.yDrive = drive;
            joint.zDrive = drive;

            if (handName == "Left") leftJoint = joint;
            else rightJoint = joint;

            Debug.Log($"{handName} grabbed {held.name}");
        }
    }

    // Clamp throw velocity
    void TryRelease(ref Rigidbody held, Vector3 palmVelocity, string handName)
    {
        if (held == null) return;

        // Remove joint
        ConfigurableJoint joint = (handName == "Left") ? leftJoint : rightJoint;
        if (joint != null)
        {
            Destroy(joint);
            if (handName == "Left") leftJoint = null;
            else rightJoint = null;
        }

        // Apply clamped velocity
        Vector3 throwVel = Vector3.ClampMagnitude(palmVelocity * throwForceMultiplier, 2.5f);
        held.linearVelocity = throwVel;
        held.angularVelocity = Vector3.zero;

        held.Sleep(); // stop floating
        Debug.Log($"{handName} released {held.name}");
        held = null;
    }

    // Physics-safe movement using MovePosition
    void MoveHeldObject(Rigidbody held, Vector3 palm)
    {
        Vector3 targetPos = palm + Vector3.up * 0.05f;
        held.MovePosition(Vector3.Lerp(held.position, targetPos, 0.5f));
        held.angularVelocity = Vector3.zero;
    }
}