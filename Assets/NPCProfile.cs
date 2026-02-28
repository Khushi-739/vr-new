using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCProfile", menuName = "NPC/NPC Profile")]
public class NPCProfile : ScriptableObject
{
    public string npcName = "Mom";
    public string role = "Warm, caring mother";

    [TextArea(3,10)]
    public string description = 
        "Mom is nurturing, patient, and emotionally expressive. She praises effort, smiles often, and claps enthusiastically.";

    [Header("Intent-Based Responses")]
    public List<string> greetingResponses = new List<string>() {
        "Hello! How are you feeling today?",
        "Hey sweetheart! I'm right here.",
        "Hi! I’m so happy to see you."
    };

    public List<string> thankYouResponses = new List<string>() {
        "You’re very welcome, sweetheart.",
        "Anytime, love. I’m happy to help you.",
        "Of course! That means a lot to me."
    };

    public List<string> achievementResponses = new List<string>() {
        "That’s nice. What are you up to?",
        "Okay. I’ll sit here and watch.",
        "I’m watching.",
        "Take it easy."
    };

    public List<string> failureResponses = new List<string>() {
        "That’s okay, love. Not everything works the first time. Try again when you’re ready.",
        "It’s fine, mistakes happen. I know you’ll do better next time.",
        "Don’t worry, sweetie. I believe in you!"
    };

    public List<string> distressResponses = new List<string>() {
        "Hey, are you okay?",
        "That was a little tumble.",
        "Let me look.",
        "It’s okay, that happens sometimes.",
        "I’m here for you. Tell me what happened."
    };

    public List<string> silenceResponses = new List<string>() {
        "I’m right here, watching you. Take your time.",
        "No rush, I’m just here for you.",
        "It’s okay, I’ll wait. Focus at your pace."
    };

    [Header("Unknown / General Responses")]
    public List<string> unknownResponses = new List<string>() {
        "Hmm… tell me more. I’m listening.",
        "I’m curious, can you explain that?",
        "Interesting… keep going.",
        "Can you tell me what happened exactly?"
    };
}
