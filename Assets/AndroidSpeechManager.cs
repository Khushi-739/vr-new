using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class AndroidSpeechManager : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject speechRecognizer;
    private AndroidJavaObject activity;
    private SpeechListener listener;
    private bool isListening = false;
    private bool micWarmedUp = false;
    private bool restartPending = false;
#endif

    public FreeNPCManager npcManager;

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        Debug.Log("[Android STT] Activity ready");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void RequestAudioFocus()
    {
        using (AndroidJavaClass audioManagerClass =
               new AndroidJavaClass("android.media.AudioManager"))
        {
            AndroidJavaObject audioManager =
                activity.Call<AndroidJavaObject>("getSystemService", "audio");

            int result = audioManager.Call<int>(
                "requestAudioFocus",
                null,
                audioManagerClass.GetStatic<int>("STREAM_MUSIC"),
                audioManagerClass.GetStatic<int>("AUDIOFOCUS_GAIN_TRANSIENT")
            );

            Debug.Log("[Android STT] Audio focus request result: " + result);
        }
    }
#endif

    // TALK BUTTON
    public void StartListening()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (isListening)
        {
            Debug.Log("[Android STT] Already listening");
            return;
        }

        // PERMISSION
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Debug.Log("[Android STT] Requesting microphone permission");
            Permission.RequestUserPermission(Permission.Microphone);
            return;
        }

        if (!micWarmedUp)
        {
            Debug.Log("[Android STT] Forcing microphone warm-up");

            if (!Microphone.IsRecording(null))
                Microphone.Start(null, false, 1, 16000);

            Invoke(nameof(StopMicWarmupAndStartSTT), 0.3f);
            micWarmedUp = true;
            return;
        }

        StartSpeechRecognizer();
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void StopMicWarmupAndStartSTT()
    {
        if (Microphone.IsRecording(null))
            Microphone.End(null);

        Debug.Log("[Android STT] Mic warm-up complete");
        StartSpeechRecognizer();
    }

    private void StartSpeechRecognizer()
    {
        RequestAudioFocus(); 

        using (AndroidJavaClass srClass =
               new AndroidJavaClass("android.speech.SpeechRecognizer"))
        {
            bool available =
                srClass.CallStatic<bool>("isRecognitionAvailable", activity);

            if (!available)
            {
                Debug.LogError("[Android STT] Speech recognition NOT available on this device");
                return;
            }

            if (speechRecognizer == null)
            {
                speechRecognizer =
                    srClass.CallStatic<AndroidJavaObject>(
                        "createSpeechRecognizer", activity);

                listener = new SpeechListener(npcManager, this);
                speechRecognizer.Call("setRecognitionListener", listener);

                Debug.Log("[Android STT] SpeechRecognizer created");
            }
        }

        AndroidJavaObject intent =
            new AndroidJavaObject(
                "android.content.Intent",
                "android.speech.action.RECOGNIZE_SPEECH"
            );

        intent.Call<AndroidJavaObject>(
            "putExtra",
            "android.speech.extra.LANGUAGE_MODEL",
            "android.speech.extra.LANGUAGE_MODEL_FREE_FORM"
        );

        intent.Call<AndroidJavaObject>(
            "putExtra",
            "android.speech.extra.LANGUAGE",
            "en-US"
        );

        intent.Call<AndroidJavaObject>(
            "putExtra",
            "android.speech.extra.PARTIAL_RESULTS",
            true
        );

        speechRecognizer.Call("startListening", intent);
        isListening = true;

        Debug.Log("[Android STT] Listening started");
    }

    public void HandleSTTError()
    {
        if (restartPending) return;

        restartPending = true;
        Debug.Log("[Android STT] Handling recognizer failure");

        try
        {
            speechRecognizer?.Call("stopListening");
            speechRecognizer?.Call("cancel");
            speechRecognizer?.Call("destroy");
        }
        catch { }

        speechRecognizer = null;
        isListening = false;

        Invoke(nameof(RestartSpeechRecognizer), 0.6f);
    }

    private void RestartSpeechRecognizer()
    {
        restartPending = false;
        Debug.Log("[Android STT] Restarting SpeechRecognizer cleanly");
        StartListening();
    }
#endif

    public void StopListening()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isListening || speechRecognizer == null) return;

        speechRecognizer.Call("stopListening");
        isListening = false;

        Debug.Log("[Android STT] Listening stopped");
#endif
    }

    void OnDisable()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        speechRecognizer?.Call("destroy");
        speechRecognizer = null;
        isListening = false;
        micWarmedUp = false;
        restartPending = false;
#endif
    }
}
