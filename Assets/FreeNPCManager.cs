using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using UnityEngine.Windows.Speech;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class FreeNPCManager : MonoBehaviour
{
    [Header("Voice Input")]
    public MonoBehaviour speechToText;

    public void OnTalkButtonPressed()
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    // ---------- WINDOWS ----------
    if (speechToText != null && speechToText is SpeechToTextManager winManager)
    {
        if (!winManager.IsListening())
        {
            Debug.Log("[Windows STT] Voice interaction started");
            winManager.StartListening();
        }
        else
        {
            Debug.Log("[Windows STT] Voice interaction stopped");
            winManager.StopListening();
        }
    }
    else
    {
        Debug.LogError("[FreeNPCManager] SpeechToTextManager is NULL or not assigned");
    }

#elif UNITY_ANDROID && !UNITY_EDITOR
    // ---------- ANDROID ----------
    if (speechToText != null && speechToText is AndroidSpeechManager androidManager)
    {
        Debug.Log("[Android STT] Talk button pressed - starting listening");
        androidManager.StartListening();
    }
    else
    {
        Debug.LogError("[FreeNPCManager] AndroidSpeechManager is NULL or not assigned");
    }
#endif
}

    [Header("UI References")]
    public TMP_Text npcText;
    public TMP_InputField inputField;
    public TMP_Text androidConsole;

    [Header("System References")]
    public NPCProfile npcProfile;
    public IntentDetector intentDetector;

    private int conversationStep = 0;
    private int offScriptAttempts = 0;
    private string currentTopic = "";
    private List<string> playerMemory = new List<string>();

    private List<string> scriptedReplies = new List<string>
    {
        "Hello! How are you feeling today?",
        "That’s nice. What are you up to?",
        "Okay. I’ll sit here and watch.",
        "I’m watching.",
        "Take it easy.",
        "Hey, are you okay?",
        "That was a little tumble.",
        "It happens. Do you want to sit for a second?",
        "Okay, I’m right here.",
        "Let me look.",
        "That’s good. How does it feel now?",
        "Alright. You can rest or keep playing."
    };

    private List<string[]> expectedPlayerKeywords = new List<string[]>
    {
        new string[] { "good", "fine", "okay", "excited" },
        new string[] { "play", "playing", "game" },
        new string[] { "watch", "look" },
        new string[] { "trying", "new" },
        new string[] { "fell", "oops", "whoa" },
        new string[] { "okay", "fine" },
        new string[] { "surprised", "scared" },
        new string[] { "sit", "rest" },
        new string[] { "knee", "hurt", "pain" },
        new string[] { "better", "okay" },
        new string[] { "rest", "play" },
        new string[] { "knee", "hurt", "pain", "bleeding", "not bleeding", "okay" }
    };

    void Awake()
    {
        if (intentDetector == null)
        {
            intentDetector = UnityEngine.Object.FindAnyObjectByType<IntentDetector>();
            if (intentDetector == null)
                Debug.LogError("[FreeNPCManager] IntentDetector not found in scene");
        }
    }

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
if (Permission.HasUserAuthorizedPermission(Permission.Microphone) && speechToText != null)
{
    if (speechToText is AndroidSpeechManager androidManager)
        androidManager.StartListening();
}
#endif

        if (inputField != null)
            inputField.onEndEdit.AddListener(OnTextSubmitted);

        if (npcText == null)
            Debug.LogError("[FreeNPCManager] NPC Text not assigned!");

        npcText.text = scriptedReplies[0];
        npcText.ForceMeshUpdate();
    }


private bool IsMeaningfulInput(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return false;

    text = text.ToLower().Trim();

    if (text.Length < 2) return false;

    string[] noiseWords =
    {
        "um", "uh", "hmm", "erm", "ah", "oh", "..."
    };

    foreach (string noise in noiseWords)
    {
        if (text == noise)
            return false;
    }

    return true;
}


private bool IsBlockingEmotion(IntentType intent, string text = "")
{
    // Core emotional blocking by intent
    switch (intent)
    {
        case IntentType.Distress:
        case IntentType.Failure:
            return true;
    }


    if (!string.IsNullOrEmpty(text))
    {
        text = text.ToLower();

        if (text.Contains("hurt") ||
            text.Contains("pain") ||
            text.Contains("cry") ||
            text.Contains("scared") ||
            text.Contains("bleeding"))
        {
            return true;
        }
    }

    return false;
}


    private void OnTextSubmitted(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        HandlePlayerInput(text);
        inputField.text = "";
        inputField.DeactivateInputField();
    }

    public void HandlePlayerInput(string playerText)
{
    playerText = playerText?.Trim();
    if (string.IsNullOrEmpty(playerText)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
    if (androidConsole != null)
    {
        androidConsole.text += "\n[INPUT] " + playerText;
        androidConsole.ForceMeshUpdate();
    }
#endif

    if (intentDetector == null || npcProfile == null || npcText == null)
    {
        Debug.LogError("[FreeNPCManager] Missing references");
        return;
    }

    Debug.Log("[NPC] Processing input: " + playerText);

    IntentType intent = intentDetector.Detect(playerText, npcProfile);
    Debug.Log("[NPC] Detected intent: " + intent);

    UpdateCurrentTopic(playerText);

    string npcReply = GenerateNPCResponse(intent, playerText);

    if (string.IsNullOrWhiteSpace(npcReply))
    {
        npcReply = "I heard you. Can you tell me more?";
        Debug.LogWarning("[NPC] Empty reply generated – using fallback");
    }

    npcText.text = npcReply;
    npcText.ForceMeshUpdate();

    playerMemory.Add(playerText.ToLower());

#if UNITY_ANDROID && !UNITY_EDITOR
    if (androidConsole != null)
        androidConsole.text += "\n[NPC] " + npcReply;
#endif

    Debug.Log("[NPC] " + npcReply);
}


    private void UpdateCurrentTopic(string playerText)
    {
        playerText = playerText.ToLower();

        if (playerText.Contains("home")) currentTopic = "home";
        else if (playerText.Contains("tired") || playerText.Contains("sleep")) currentTopic = "tired";
        else if (playerText.Contains("play") || playerText.Contains("game")) currentTopic = "play";
        else if (playerText.Contains("school") || playerText.Contains("homework")) currentTopic = "school";
        else if (playerText.Contains("hurt") || playerText.Contains("pain") || playerText.Contains("fell")) currentTopic = "injury";
        else if (playerText.Contains("happy") || playerText.Contains("fun")) currentTopic = "happy";
        else if (playerText.Contains("baka") || playerText.Contains("stupid")) currentTopic = "teasing";
        else if (playerText.Contains("hello") || playerText.Contains("hi")) currentTopic = "greeting";
        else currentTopic = "unknown";
    }

    private string GenerateNPCResponse(IntentType intent, string playerText)
    {
        playerText = playerText.ToLower();

        if (!IsMeaningfulInput(playerText))
            return "Hmm… can you tell me a little more, sweetheart?";

        if (conversationStep < expectedPlayerKeywords.Count)
        {
            bool matchesExpectation =
                expectedPlayerKeywords[conversationStep].Any(k => playerText.Contains(k));

            if (matchesExpectation)
            {
                string reply = scriptedReplies[conversationStep];
                conversationStep++;
                offScriptAttempts = 0;
                return reply;
            }
            else if (offScriptAttempts < 2 && !IsBlockingEmotion(intent, playerText))
            {
                offScriptAttempts++;
                return GenerateSmartOffScriptResponse(playerText, intent);
            }
        }

        return GenerateSmartOffScriptResponse(playerText, intent);
    }

    private string GenerateSmartOffScriptResponse(string text, IntentType intent)
    {
        text = text.ToLower();

        switch (currentTopic)
        {
            case "tired": return "You look tired. Do you want to take a little break?";
            case "happy": return "I love seeing you happy! What’s making it fun?";
            case "injury": return "Oh no! Are you hurt? I’m right here for you.";
            case "teasing": return "Hey now, that’s not very nice! I’m listening.";
            case "home": return "You want to go home? Do you feel like leaving now or just need a break first?";
            case "play": return "Are you having fun playing? Tell me more!";
            case "school": return "How’s school going? Need help with homework or something?";
            case "greeting": return "Hey! Good to see you. How’s it going?";
        }

        switch (intent)
        {
            case IntentType.Greeting: return "Hey there! How are you today?";
            case IntentType.Achievement: return "Wow! That’s amazing. Tell me more!";
            case IntentType.Failure: return "It’s okay. We all make mistakes sometimes.";
            case IntentType.Distress: return "I’m here for you. What happened?";
            case IntentType.Silence: return "Take your time, I’m right here.";
        }

        if (playerMemory.Count > 0)
        {
            string last = playerMemory.Last();
            if (last.Contains("game") || last.Contains("play")) return "How’s the game going now?";
            if (last.Contains("school") || last.Contains("homework")) return "Do you need help with that?";
            if (last.Contains("home")) return "Do you really want to go home now or just thinking about it?";
        }

        return GenerateHumanLikeResponse(text);
    }

    private string GenerateHumanLikeResponse(string playerText)
    {
        playerText = playerText.ToLower();

        if (playerText.Contains("hello") || playerText.Contains("hi") || playerText.Contains("hey"))
            return "Hello! How’s your day going so far?";
        if (playerText.Contains("thank") || playerText.Contains("thanks"))
            return "You’re welcome, sweetie!";
        if (playerText.Contains("bored"))
            return "Oh no! Want to play a game or do something fun?";
        if (playerText.Contains("hungry"))
            return "Do you want a snack? What do you feel like eating?";
        if (playerText.Contains("sleepy"))
            return "Maybe it’s time for a little nap. Want me to tuck you in?";
        if (playerText.Contains("sad") || playerText.Contains("upset"))
            return "I’m here with you. Do you want a hug?";
        if (playerText.Contains("angry") || playerText.Contains("mad"))
            return "It’s okay to be upset. Can you tell me what made you feel this way?";
        if (playerText.Contains("play") || playerText.Contains("game"))
            return "That sounds fun! Can you show me what you’re playing?";
        if (playerText.Contains("home"))
            return "Do you want to go home now, or just take a little break first?";

        return "I’m listening. Tell me more about that, sweetie.";
    }

    private string GenerateIntentBasedResponse(IntentType intent, string playerText)
    {
        switch (intent)
        {
            case IntentType.Greeting:
                return npcProfile.greetingResponses.FirstOrDefault() ?? "Hello, dear!";
            case IntentType.ThankYou:
                return npcProfile.thankYouResponses.FirstOrDefault() ?? "Anytime, love.";
            case IntentType.Achievement:
                if (playerText.Contains("playing") || playerText.Contains("trying"))
                    return "That’s great! Tell me more about what you’re doing.";
                return npcProfile.achievementResponses.FirstOrDefault() ?? "Good job!";
            case IntentType.Failure:
                return npcProfile.failureResponses.FirstOrDefault() ?? "Don’t worry, you’ll get it next time.";
            case IntentType.Distress:
                if (playerText.Contains("fell") || playerText.Contains("knee"))
                    return "Oh no! Are you hurt? Let me help.";
                return npcProfile.distressResponses.FirstOrDefault() ?? "I’m here, it’s okay.";
            case IntentType.Silence:
                return npcProfile.silenceResponses.FirstOrDefault() ?? "Take your time, I’m right here.";
            default:
                return npcProfile.unknownResponses.FirstOrDefault() ?? "Hmm… tell me more.";
        }
    }
}
