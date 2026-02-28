#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

public class SpeechListener : AndroidJavaProxy
{
    private FreeNPCManager npcManager;
    private AndroidSpeechManager manager;
    public SpeechListener(FreeNPCManager npc, AndroidSpeechManager mgr = null)
        : base("android.speech.RecognitionListener")
    {
        npcManager = npc;
        manager = mgr;
    }

    public void onReadyForSpeech(AndroidJavaObject @params)
    {
        Debug.Log("[Android STT] Ready for speech");
    }

    public void onBeginningOfSpeech()
    {
        Debug.Log("[Android STT] Speech started");
    }

    public void onRmsChanged(float rmsdB) { }

    public void onBufferReceived(byte[] buffer) { }

    public void onEndOfSpeech()
    {
        Debug.Log("[Android STT] Speech ended");
    }

    public void onError(int error)
    {
        Debug.LogError("[Android STT] Error code: " + error);

        manager?.HandleSTTError(); // Safe even if manager is null
    }

    public void onResults(AndroidJavaObject results)
    {
        if (results == null)
        {
            Debug.LogError("[Android STT] Results null");
            return;
        }

        AndroidJavaObject matches =
            results.Call<AndroidJavaObject>(
                "getStringArrayList",
                "android.speech.extra.RESULTS_RECOGNITION"
            );

        if (matches == null)
        {
            Debug.LogError("[Android STT] Matches null");
            return;
        }

        List<string> texts = new List<string>();
        int size = matches.Call<int>("size");

        for (int i = 0; i < size; i++)
            texts.Add(matches.Call<string>("get", i));

        if (texts.Count > 0)
        {
            string resultText = texts[0];
            Debug.Log("[Android STT] Recognized: " + resultText);
            AndroidVoiceBridge.PushResult(resultText);
        }
        else
        {
            Debug.LogWarning("[Android STT] No speech recognized");
        }
    }

    public void onPartialResults(AndroidJavaObject partialResults) { }

    public void onEvent(int eventType, AndroidJavaObject @params) { }
}
#endif
