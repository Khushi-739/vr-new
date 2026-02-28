using UnityEngine;

public class LandmarkVisibilityGate : MonoBehaviour
{
    public HandTracking tracker;
    public bool isLeftHand = true;

    GameObject[] points;

    void Start()
    {
        points = isLeftHand
            ? tracker.LeftHandPoints
            : tracker.RightHandPoints;
    }

    void Update()
    {
        bool visible = isLeftHand
            ? tracker.isLeftHandTracked
            : tracker.isRightHandTracked;

        if (points == null) return;

        foreach (var p in points)
        {
            if (p != null && p.activeSelf != visible)
                p.SetActive(visible);
        }
    }
} 