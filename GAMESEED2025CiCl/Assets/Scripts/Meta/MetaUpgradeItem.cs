using UnityEngine;

[CreateAssetMenu(fileName = "New Meta Upgrade", menuName = "Meta/Meta Upgrade Item")]
public class MetaUpgradeItem : ScriptableObject
{
    [Header("Info")]
    public string upgradeId; // Must be unique, e.g., "ToaRW_Damage"
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Cost Formula")]
    public float baseCost = 100f;
    public float exponent = 1.25f;

    public int CalculateCost(int currentLevel)
    {
        // Level 0 (first purchase) should have a cost based on level 1 for the formula
        int levelForCalc = currentLevel == 0 ? 1 : currentLevel;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(levelForCalc, exponent));
    }
}