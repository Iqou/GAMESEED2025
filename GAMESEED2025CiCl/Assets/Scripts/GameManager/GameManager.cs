using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- Meta Progression Data ---
    public int soundChips = 0;
    public Dictionary<string, int> metaUpgradeLevels = new Dictionary<string, int>();
    public List<string> unlockedHoregs = new List<string> { "ToaRW" }; // Dimulai dari ToaRW
    public string lastCharacterUsed = "Default";
    
    private string savePath;
    public int currentSlot = 0;
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
            // This line will likely cause an error, it is commented out for now
            // CurrentProgression.sessionCount++; 
            // Debug.Log("Session started. Total sessions: " + CurrentProgression.sessionCount);
        }
    }

    void Start()
    {
        // Example usage
        // These lines will likely cause an error, they are commented out for now
        // CurrentProgression.currentSoundchip += 200;
        // CurrentProgression.speakerData.cooldownLevel++;
        // float newStat = CurrentProgression.speakerData.GetStat(100f);
        // Debug.Log("Current Speaker Power: " + newStat);
    }

    private void OnAppQuit()
    {
        int sessionDuration = Mathf.FloorToInt(Time.time - sessionStartTime);
        // This line will likely cause an error, it is commented out for now
        // CurrentProgression.totalPlaytime += sessionDuration;
        // CurrentProgression.lastPlayTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

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
        
        soundChips = 0;
        metaUpgradeLevels.Clear();
        unlockedHoregs = new List<string> { "ToaRW" };
        lastCharacterUsed = "Default";
    }

    public void SaveProgressToSlot(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"saveData_slot{slot}.json");
        SaveData data = new SaveData()
        {
            soundChips = this.soundChips,
            metaUpgradeLevels = this.metaUpgradeLevels,
            unlockedHoregs = this.unlockedHoregs,
            lastCharacterUsed = this.lastCharacterUsed
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

            soundChips = data.soundChips;
            metaUpgradeLevels = data.metaUpgradeLevels ?? new Dictionary<string, int>();
            unlockedHoregs = data.unlockedHoregs ?? new List<string> { "ToaRW" };
            lastCharacterUsed = data.lastCharacterUsed;
            currentSlot = slot;

            Debug.Log("Progress Loaded from slot " + slot);
        }
        else
        {
            ResetProgress();
            Debug.Log("No save found, starting fresh.");
        }
    }

    // Helper method buat meta upgrades
    public int GetMetaUpgradeLevel(string upgradeId)
    {
        return metaUpgradeLevels.ContainsKey(upgradeId) ? metaUpgradeLevels[upgradeId] : 0;
    }

    public void SetMetaUpgradeLevel(string upgradeId, int level)
    {
        metaUpgradeLevels[upgradeId] = level;
    }

    public void OnPlayerScored(int score)
    {
        // This line will likely cause an error, it is commented out for now
        // if (score > CurrentProgression.highestScore)
        // {
        //     CurrentProgression.highestScore = score;
        // }
    }

    public void OnSpendSoundchips(int amount)
    {
        // This line will likely cause an error, it is commented out for now
        // if (CurrentProgression.currentSoundchip >= amount)
        // {
        //     CurrentProgression.currentSoundchip -= amount;
        // }
    }
}