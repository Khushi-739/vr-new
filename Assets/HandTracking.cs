using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandTracking : MonoBehaviour
{

    public bool isLeftHandTracked;
    public bool isRightHandTracked;

    [Header("Detection Timeout")]
    public float handLostTimeout = 0.15f;

    private float leftHandLastSeenTime = -1f;
    private float rightHandLastSeenTime = -1f;

    [Header("Hand Separation Bias")]
    public float handCenterBias = 0.005f;

    [Header("Tracking State")]
    public float trackingTimeout = 0.2f;
    private float lastPacketTime;

    // Internal tracking state
    public bool isLeftTracked;
    public bool isRightTracked;

    [Header("UDP Manager")]
    public Manager udpReceive;

    [Header("Left Hand")]
    public GameObject[] LeftHandPoints = new GameObject[21];
    public Transform LeftHandAnchor;

    [Header("Right Hand")]
    public GameObject[] RightHandPoints = new GameObject[21];
    public Transform RightHandAnchor;

    [HideInInspector] public Vector3[] LeftLandmarks = new Vector3[21];
    [HideInInspector] public Vector3[] RightLandmarks = new Vector3[21];

    private float camWidth = 1920f;
    private float camHeight = 1080f;

    public float groundY = 0.05f;

    [Header("Hand Interactors (Physics Reference)")]
    public Transform leftInteractor;
    public Transform rightInteractor;

    void Start()
    {
        foreach (var p in LeftHandPoints)
            p.transform.SetParent(LeftHandAnchor, false);

        foreach (var p in RightHandPoints)
            p.transform.SetParent(RightHandAnchor, false);
    }

    void Update()
    {
        string raw = udpReceive.data;

        if (string.IsNullOrEmpty(raw))
        {
            CheckTimeout();
            ApplyHandLostTimeouts();
            SyncPublicState();
            return;
        }

        isLeftTracked = false;
        isRightTracked = false;

        raw = raw.Trim('[', ']');
        string[] both = raw.Split('|');

        if (both.Length != 2)
        {
            CheckTimeout();
            ApplyHandLostTimeouts();
            SyncPublicState();
            return;
        }

        string leftData = both[0].Replace("L:", "");
        string rightData = both[1].Replace("R:", "");

        // LEFT hand
        if (!string.IsNullOrEmpty(leftData) && leftData.Split(',').Length == 63)
        {
            isLeftTracked = UpdateHand(leftData, LeftHandPoints, LeftHandAnchor, LeftLandmarks);
            if (isLeftTracked)
                leftHandLastSeenTime = Time.time;
        }
        else
        {
            ClearLandmarks(LeftLandmarks);
        }

        // RIGHT hand
        if (!string.IsNullOrEmpty(rightData) && rightData.Split(',').Length == 63)
        {
            isRightTracked = UpdateHand(rightData, RightHandPoints, RightHandAnchor, RightLandmarks);
            if (isRightTracked)
                rightHandLastSeenTime = Time.time;
        }
        else
        {
            ClearLandmarks(RightLandmarks);
        }


        if (isLeftTracked && isRightTracked &&
            LeftLandmarks[0] != Vector3.zero &&
            RightLandmarks[0] != Vector3.zero)
        {
            float wristDist = Vector3.Distance(
                LeftLandmarks[0],
                RightLandmarks[0]
            );

            // smaller threshold + time-based decision
            if (wristDist < 0.01f)
            {
                if (leftHandLastSeenTime < rightHandLastSeenTime)
                {
                    isLeftTracked = false;
                    ClearLandmarks(LeftLandmarks);
                }
                else
                {
                    isRightTracked = false;
                    ClearLandmarks(RightLandmarks);
                }
            }
        }


        if (isLeftTracked || isRightTracked)
        {
            lastPacketTime = Time.time;
        }
        else
        {
            CheckTimeout();
        }

        ApplyHandLostTimeouts();
        SyncPublicState();
    }


    void SyncPublicState()
    {
        isLeftHandTracked = isLeftTracked;
        isRightHandTracked = isRightTracked;
    }

    void ApplyHandLostTimeouts()
    {
        if (Time.time - leftHandLastSeenTime > handLostTimeout)
            isLeftTracked = false;

        if (Time.time - rightHandLastSeenTime > handLostTimeout)
            isRightTracked = false;
    }

    void ClearLandmarks(Vector3[] landmarks)
    {
        for (int i = 0; i < landmarks.Length; i++)
            landmarks[i] = Vector3.zero;
    }

    bool UpdateHand(string csv, GameObject[] points, Transform anchor, Vector3[] landmarkArray)
    {
        string[] nums = csv.Split(',');
        if (nums.Length != 63) return false;

        float wristDistance = Vector3.Distance(LeftHandAnchor.position, RightHandAnchor.position);
        float dynamicBias = Mathf.Min(0.005f, wristDistance * 0.05f);

        for (int i = 0; i < 21; i++)
        {
            float x = (float.Parse(nums[i * 3]) - camWidth / 2f) / 500f;
            float y = (float.Parse(nums[i * 3 + 1]) - camHeight / 2f) / 500f;
            float z = -float.Parse(nums[i * 3 + 2]) / 500f;

            Vector3 worldPos = anchor.TransformPoint(new Vector3(x, y, z));

            if (worldPos.y < groundY)
                worldPos.y = groundY;

            Vector3 localPos = anchor.InverseTransformPoint(worldPos);

            float bias = (anchor == LeftHandAnchor) ? -dynamicBias : dynamicBias;
            localPos.x += bias;

            points[i].transform.localPosition = localPos;
            landmarkArray[i] = anchor.TransformPoint(localPos);
        }

        return true;
    }

   void CheckTimeout()
{
    if (Time.time - lastPacketTime > trackingTimeout)
    {
        isLeftTracked = false;
        isRightTracked = false;

        leftHandLastSeenTime = -1f;   
        rightHandLastSeenTime = -1f;  
        ClearLandmarks(LeftLandmarks);
        ClearLandmarks(RightLandmarks);
    }
}


    public Vector3 GetGrabPosition(string handName)
    {
        if (handName == "Left" && leftInteractor != null)
            return leftInteractor.position;

        if (handName == "Right" && rightInteractor != null)
            return rightInteractor.position;

        return Vector3.zero;
    }
} 