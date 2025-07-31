using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int levelUnlocked = 1;
    public int totalCoins = 0;
    public string lastCharacterUsed = "Default";
    public List<string> unlockedItems = new List<string>();

    private string savePath;

    public int currentSlot = 0; // Default ke slot 0

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
            LoadProgress();
        }
    }

public void SaveProgress()
{
    SaveProgressToSlot(currentSlot);
}

public void LoadProgress()
{
    LoadProgressFromSlot(currentSlot);
}

    public void ResetProgress()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
        levelUnlocked = 1;
        totalCoins = 0;
        lastCharacterUsed = "Default";
        unlockedItems.Clear();
    }

    public void SaveProgressToSlot(int slot)
{
    string path = Path.Combine(Application.persistentDataPath, $"saveData_slot{slot}.json");
    SaveData data = new SaveData()
    {
        levelUnlocked = levelUnlocked,
        totalCoins = totalCoins,
        lastCharacterUsed = lastCharacterUsed,
        unlockedItems = unlockedItems
    };

    string json = JsonUtility.ToJson(data, true);
    File.WriteAllText(path, json);
    currentSlot = slot;
}

public void LoadProgressFromSlot(int slot)
{
    string path = Path.Combine(Application.persistentDataPath, $"saveData_slot{slot}.json");
    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        levelUnlocked = data.levelUnlocked;
        totalCoins = data.totalCoins;
        lastCharacterUsed = data.lastCharacterUsed;
        unlockedItems = data.unlockedItems ?? new List<string>();
        currentSlot = slot;
    }
}

}
