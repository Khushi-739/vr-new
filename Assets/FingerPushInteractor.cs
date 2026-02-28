using UnityEngine;

public class FingerPushInteractor : MonoBehaviour
{
    public float pushStrength = 0.015f;
    public float maxPushPerFrame = 0.01f;

    void OnTriggerStay(Collider other)
    {
        LogicInteractable obj = other.GetComponent<LogicInteractable>();
        if (obj == null || obj.IsGrabbed()) return;

        Vector3 fingerPos = transform.position;
        Vector3 objPos = other.transform.position;

        Vector3 dir = objPos - fingerPos;

        if (Vector3.Dot(dir.normalized, (objPos - fingerPos).normalized) <= 0f)
            return;

        float distance = dir.magnitude;

        if (distance > 0.03f) return;

        float force = Mathf.Lerp(pushStrength, 0f, distance / 0.03f);
        Vector3 move = dir.normalized * Mathf.Min(force * Time.deltaTime, maxPushPerFrame);

        other.transform.position += move;
    }
}