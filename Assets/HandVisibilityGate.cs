using UnityEngine;

public class HandVisibilityGate : MonoBehaviour
{
    public HandTracking tracker;
    public bool isLeftHand = true;

    Renderer[] renderers;
    LineRenderer[] lineRenderers;
    Collider[] colliders;
    Rigidbody[] rigidbodies;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        rigidbodies = GetComponentsInChildren<Rigidbody>(true);
    }

    void Update()
    {
        if (tracker == null) return;

        bool visible = isLeftHand
            ? tracker.isLeftHandTracked
            : tracker.isRightHandTracked;

        SetVisibility(visible);
    }

    void SetVisibility(bool visible)
    {

        foreach (var r in renderers)
            r.enabled = visible;

        foreach (var lr in lineRenderers)
            lr.enabled = visible;


        foreach (var c in colliders)
            c.enabled = visible;

        foreach (var rb in rigidbodies)
        {
            rb.detectCollisions = visible;

            if (!visible)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
} 