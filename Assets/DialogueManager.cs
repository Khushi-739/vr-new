 using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public NPCProfile npcProfile;
    public MemoryManager memoryManager;
    public string GetResponse(IntentType intent, string playerText)
    {
        switch (intent)
        {
            case IntentType.Greeting:
                return npcProfile.greetingResponses.RandomItem() 
                       ?? "Hey there! I'm happy to see you.";

	    case IntentType.ThankYou:
    		return npcProfile.thankYouResponses.RandomItem()
           		?? "You’re welcome, sweetheart.";


            case IntentType.Achievement:
                return npcProfile.achievementResponses.RandomItem() 
                       ?? "You're doing amazing! Keep it up.";

            case IntentType.Failure:
                return npcProfile.failureResponses.RandomItem() 
                       ?? "It's okay. Everyone makes mistakes. Let's try again.";

            case IntentType.Silence:
                return npcProfile.silenceResponses.RandomItem() 
                       ?? "I’m right here, watching you. Take your time.";

            case IntentType.Distress:  // <-- New case for distress
                return npcProfile.distressResponses.RandomItem() 
                       ?? "Oh no! Are you okay? I’m right here.";

            case IntentType.Unknown:
                // Check memory first
                var relevantMemories = memoryManager?.GetRelevantMemories(playerText);
                if (relevantMemories != null && relevantMemories.Count > 0)
                    return $"Earlier you said: '{relevantMemories[0].playerText}'. Do you want to continue?";

                return npcProfile.unknownResponses.RandomItem() 
                       ?? "Hmm… tell me more. I'm listening.";
        }

        return "I’m right here with you.";
    }
}

public static class ListExtensions
{
    public static T RandomItem<T>(this List<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[Random.Range(0, list.Count)];
    }
}