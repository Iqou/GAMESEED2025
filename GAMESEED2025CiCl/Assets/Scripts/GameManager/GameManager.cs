using UnityEngine;
using System.Collections.Generic;
using System.IO;

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

    void Start()
    {
        SaveManager.CurrentProgression.currentSoundchip += 200;

        SaveManager.CurrentProgression.speakerData.cooldownLevel++;

        float newStat = SaveManager.CurrentProgression.speakerData.GetStat(100f);
        Debug.Log("Current Speaker Power: " + newStat);
    }

    public void OnPlayerScored(int score)
    {
        if (score > SaveManager.CurrentProgression.highestScore)
        {
            SaveManager.CurrentProgression.highestScore = score;
        }
    }

    public void OnSpendSoundchips(int amount)
    {
        if (SaveManager.CurrentProgression.currentSoundchip >= amount)
        {
            SaveManager.CurrentProgression.currentSoundchip -= amount;
        }
    }
}
