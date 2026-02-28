 using UnityEngine;

public class HandLogicInteractor : MonoBehaviour
{
    public HandTracking tracker;
    public bool isLeftHand;

    LogicInteractable current;

    void OnTriggerEnter(Collider other)
    {
        LogicInteractable obj = other.GetComponent<LogicInteractable>();
        if (obj == null) return;

        current = obj;
    }

    void OnTriggerExit(Collider other)
    {
        if (current != null && other.gameObject == current.gameObject)
            current = null;
    }

    void Update()
    {
        if (current == null) return;

        Vector3[] lm = isLeftHand ? tracker.LeftLandmarks : tracker.RightLandmarks;
        if (lm[0] == Vector3.zero) return;

        float pinch = Vector3.Distance(lm[4], lm[8]);

        // Grab
        if (current != null && pinch < 0.03f)
{
    if (isLeftHand && current.leftGrabber == null)
        current.leftGrabber = transform;

    if (!isLeftHand && current.rightGrabber == null)
        current.rightGrabber = transform;
}


        // Release
       if (current != null && pinch > 0.06f)
{
    if (isLeftHand)
        current.leftGrabber = null;
    else
        current.rightGrabber = null;
}

    }
} 