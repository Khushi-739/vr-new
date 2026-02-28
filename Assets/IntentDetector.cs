using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IntentDetector : MonoBehaviour
{
    public IntentType Detect(string input, NPCProfile profile)
    {
        if (string.IsNullOrWhiteSpace(input))
            return IntentType.Silence;

        input = input.ToLower();

        // GREETING
        List<string> greetingKeywords = new List<string> { "hello", "hi", "hey", "how are you" };
        if (greetingKeywords.Any(k => input.Contains(k)))
            return IntentType.Greeting;

        // THANK YOU
        List<string> thankYouKeywords = new List<string> { "thank you", "thanks", "thx", "ty", "appreciate", "grateful" };
        if (thankYouKeywords.Any(k => input.Contains(k)))
            return IntentType.ThankYou;

        // ACHIEVEMENT
        List<string> achievementKeywords = new List<string> { "i did it", "i won", "look", "goal", "ball", "success", "trying", "playing" };
        if (achievementKeywords.Any(k => input.Contains(k)))
            return IntentType.Achievement;

        // FAILURE
        List<string> failureKeywords = new List<string> { "can't", "failed", "missed", "lost", "not working", "error" };
        if (failureKeywords.Any(k => input.Contains(k)))
            return IntentType.Failure;

        // DISTRESS
        List<string> distressKeywords = new List<string> { "fell", "hurt", "boo-boo", "knee", "pain", "ouch", "tumble" };
        if (distressKeywords.Any(k => input.Contains(k)))
            return IntentType.Distress;

        // SILENCE
        if (string.IsNullOrWhiteSpace(input))
            return IntentType.Silence;

        // DEFAULT: UNKNOWN
        return IntentType.Unknown;
    }
}
