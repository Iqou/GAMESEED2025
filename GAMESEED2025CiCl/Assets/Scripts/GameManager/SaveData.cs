using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Meta-progression currency
    public int soundChips; 

    // Permanent upgrade levels (e.g., "ToaRW_Damage_Lvl", 3)
    public Dictionary<string, int> metaUpgradeLevels;

    // Unlocked content
    public List<string> unlockedHoregs; // e.g., "SubwooferDugem"
    public string lastCharacterUsed;
}