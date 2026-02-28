using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{
    private string filePath;

    void Awake()
    {
        filePath = Application.persistentDataPath + "/npc_memory.json";
    }

    public void SaveMemory(string playerText, string npcText)
    {
        MemoryBank bank = LoadMemory();

        bank.memories.Add(new MemoryItem
        {
            playerText = playerText,
            npcText = npcText,
            time = System.DateTime.Now.ToString()
        });

        string json = JsonUtility.ToJson(bank, true);
        File.WriteAllText(filePath, json);
    }

    public MemoryBank LoadMemory()
    {
        if (!File.Exists(filePath))
            return new MemoryBank();

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<MemoryBank>(json);
    }

    public List<MemoryItem> GetRelevantMemories(string currentInput, int limit = 3)
    {
        MemoryBank bank = LoadMemory();

        return bank.memories
            .Where(m => m.playerText.ToLower().Contains(currentInput.ToLower()))
            .Take(limit)
            .ToList();
    }
} 