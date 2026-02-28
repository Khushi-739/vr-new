using UnityEngine;

public class FingerBoneFollow : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    void LateUpdate()
    {
        if (!startPoint || !endPoint) return;

        Vector3 mid = (startPoint.position + endPoint.position) * 0.5f;
        transform.position = mid;

        Vector3 dir = endPoint.position - startPoint.position;
        transform.up = dir.normalized;

        transform.localScale = new Vector3(
            transform.localScale.x,
            dir.magnitude * 0.5f,
            transform.localScale.z
        );
    }
} 