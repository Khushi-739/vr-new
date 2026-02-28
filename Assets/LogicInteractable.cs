using UnityEngine;

public class LogicInteractable : MonoBehaviour
{
    [Header("Interaction")]
    public Transform leftGrabber;
    public Transform rightGrabber;
    public float followSpeed = 25f;

    Vector3 lastPos;
    Vector3 velocity;

    void Update()
    {
        bool left = leftGrabber != null;
        bool right = rightGrabber != null;

        if (!left && !right) return;

        Vector3 targetPos;


        if (left && right)
        {
            targetPos = (leftGrabber.position + rightGrabber.position) * 0.5f;
        }

        else
        {
            targetPos = left ? leftGrabber.position : rightGrabber.position;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );

        velocity = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Convenience helpers
    public bool IsGrabbed()
    {
        return leftGrabber != null || rightGrabber != null;
    }

    public void ReleaseAll()
    {
        leftGrabber = null;
        rightGrabber = null;
    }
} 