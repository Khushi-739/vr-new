// ---------------- SpeechToTextManager.cs ----------------
using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using UnityEngine.Windows.Speech;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class SpeechToTextManager : MonoBehaviour
{
    public FreeNPCManager npcManager;
    private bool isListening = false;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    private DictationRecognizer recognizer;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject speechRecognizer;
    private AndroidJavaObject activity;
    private SpeechListener speechListener;
#endif

    public bool IsListening() => isListening;

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Request mic permission first
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            Debug.Log("[Android STT] Requested microphone permission");
        }
#endif
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.LogWarning("[Android STT] Microphone permission not granted yet. Wait before starting STT.");
            return;
        }

        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            using (AndroidJavaClass srClass = new AndroidJavaClass("android.speech.SpeechRecognizer"))
            {
                if (srClass.CallStatic<bool>("isRecognitionAvailable", activity))
                {
                    speechRecognizer = srClass.CallStatic<AndroidJavaObject>("createSpeechRecognizer", activity);
                    Debug.Log("[Android STT] SpeechRecognizer created");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[Android STT Init Error] " + e);
        }
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        recognizer = new DictationRecognizer();
        recognizer.DictationResult += (text, confidence) =>
{
    Debug.Log("[Windows STT] " + text);

    MainThreadDispatcher.RunOnMainThread(() =>
    {
        npcManager?.HandlePlayerInput(text);
    });
};

        recognizer.DictationError += (error, hresult) => Debug.LogError("[Windows STT Error] " + error);
        recognizer.DictationComplete += (cause) =>
        {
            Debug.Log("[Windows STT] Complete: " + cause);
            isListening = false;
        };
#endif
    }

    public void StartListening()
    {
        if (isListening) return;
        isListening = true;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (recognizer != null && recognizer.Status != SpeechSystemStatus.Running)
        {
            recognizer.Start();
            Debug.Log("[Windows STT] Listening");
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.LogWarning("[Android STT] Cannot start. Microphone permission not granted.");
            isListening = false;
            return;
        }

        if (speechRecognizer == null || activity == null)
        {
            Debug.LogError("[Android STT] Not initialized");
            isListening = false;
            return;
        }

        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.speech.action.RECOGNIZE_SPEECH");
        intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE_MODEL", "android.speech.extra.LANGUAGE_MODEL_FREE_FORM");
        intent.Call<AndroidJavaObject>("putExtra", "android.speech.extra.LANGUAGE", "en-US");

        speechListener = new SpeechListener(npcManager);
        speechRecognizer.Call("setRecognitionListener", speechListener);
        speechRecognizer.Call("startListening", intent);

        Debug.Log("[Android STT] Listening...");
#endif
    }

    public void StopListening()
    {
        if (!isListening) return;
        isListening = false;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (recognizer != null && recognizer.Status == SpeechSystemStatus.Running)
        {
            recognizer.Stop();
            Debug.Log("[Windows STT] Stopped");
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        speechRecognizer?.Call("stopListening");
        Debug.Log("[Android STT] Stopped");
#endif
    }

    private void OnDestroy()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        recognizer?.Dispose();
        recognizer = null;
#endif
    }
}
