using UnityEngine;
using System.Collections.Generic;

public class AndroidVoiceBridge : MonoBehaviour
{
    private static AndroidVoiceBridge instance;
    private readonly Queue<string> pendingTexts = new Queue<string>();

    public FreeNPCManager npcManager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public static void PushResult(string text)
    {
        if (instance == null) return;

        lock (instance.pendingTexts)
        {
            instance.pendingTexts.Enqueue(text);
        }
    }


    void Update()
    {
        if (npcManager == null) return;

        lock (pendingTexts)
        {
            while (pendingTexts.Count > 0)
            {
                string text = pendingTexts.Dequeue();
                Debug.Log("[Android STT → MainThread] " + text);
                npcManager.HandlePlayerInput(text);
            }
        }
    }
}
