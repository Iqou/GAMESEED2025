using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int level;
    public int money;
    public int currentExperience;
    public int maxExperience;
    public LongProgression progression = new LongProgression();
}

[System.Serializable]
public class SpeakerData
{
    public int desibelLevel;
    public int areaLevel;
    public int cooldownLevel;

    public float GetStat(float baseStat)
    {
        int totalUpgrade = desibelLevel + areaLevel + cooldownLevel;
        return baseStat * (1f + totalUpgrade * 0.01f);
    }
}

[System.Serializable]
public class LongProgression
{
    public int highestScore;
    public int totalSawerCollected;
    public int currentSoundchip;

    public int totalPlaytime;
    public int sessionCount;
    public string currentProfileName;
    public string lastPlayTimestamp;

    public SpeakerData speakerData = new SpeakerData();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int level;
    public int money;
    public int currentExperience;
    public int maxExperience;

    public LongProgression CurrentProgression = new LongProgression();

    public int totalCoins; // Used by ShopManager
    public string lastCharacterUsed; // Used by MetaPreviewUI
    public List<string> unlockedItems = new List<string>(); // Used by MetaPreviewUI

    private int currentSlot = 0;
    private float sessionStartTime;

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
            Application.quitting += OnAppQuit;

            sessionStartTime = Time.time;
            LoadProgressFromSlot(currentSlot);
            CurrentProgression.sessionCount++;
            Debug.Log("Session started. Total sessions: " + CurrentProgression.sessionCount);
        }
    }

    void Start()
    {
        // Example usage
        CurrentProgression.currentSoundchip += 200;
        CurrentProgression.speakerData.cooldownLevel++;
        float newStat = CurrentProgression.speakerData.GetStat(100f);
        Debug.Log("Current Speaker Power: " + newStat);
    }

    private void OnAppQuit()
    {
        int sessionDuration = Mathf.FloorToInt(Time.time - sessionStartTime);
        CurrentProgression.totalPlaytime += sessionDuration;
        CurrentProgression.lastPlayTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        SaveProgressToSlot(currentSlot);
        Debug.Log($"Session ended. Duration: {sessionDuration}s | Total Playtime: {CurrentProgression.totalPlaytime}s");
    }

    public void SaveProgressToSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"saveData_slot{slot}.json");
        SaveData data = new SaveData()
        {
            level = level,
            money = money,
            currentExperience = currentExperience,
            maxExperience = maxExperience,
            progression = CurrentProgression
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        currentSlot = slot;
        Debug.Log("Progress Saved to slot " + slot);
    }

    public void LoadProgressFromSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"saveData_slot{slot}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            level = data.level;
            money = data.money;
            currentExperience = data.currentExperience;
            maxExperience = data.maxExperience;
            CurrentProgression = data.progression;
            currentSlot = slot;

            Debug.Log("Progress Loaded from slot " + slot);
        }
        else
        {
            ResetProgress();
            Debug.Log("No save found, starting fresh.");
        }
    }

    public void ResetProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, $"saveData_slot{currentSlot}.json");
        if (File.Exists(path))
            File.Delete(path);

        level = 1;
        money = 0;
        currentExperience = 0;
        maxExperience = 150;
        CurrentProgression = new LongProgression();
        SaveProgressToSlot(currentSlot);
        Debug.Log("Progress Reset.");
    }

    public void OnPlayerScored(int score)
    {
        if (score > CurrentProgression.highestScore)
        {
            CurrentProgression.highestScore = score;
        }
    }

    public void OnSpendSoundchips(int amount)
    {
        if (CurrentProgression.currentSoundchip >= amount)
        {
            CurrentProgression.currentSoundchip -= amount;
        }
    }
}
