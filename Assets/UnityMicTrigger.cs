using UnityEngine;

public class UnityMicTrigger : MonoBehaviour
{
    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            Debug.Log("Mic detected: " + Microphone.devices[0]);
            Microphone.Start(null, false, 2, 44100);
        }
        else
        {
            Debug.Log("No microphone devices found");
        }
    }
}
