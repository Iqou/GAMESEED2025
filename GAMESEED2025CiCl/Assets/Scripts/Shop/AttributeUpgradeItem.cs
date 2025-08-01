using UnityEngine;

public enum AttributeType
{
    Damage,
    Cooldown,
    AreaOfEffect,
    BeatAccuracy,
    MaxHealth
}

[CreateAssetMenu(fileName = "NewAttributeUpgrade", menuName = "Shop/Attribute Upgrade Item")]
public class AttributeUpgradeItem : ShopItem
{
    [Header("Attribute Upgrade Specifics")]
    public AttributeType attributeToUpgrade;
    public float upgradeValue;
    public int maxLevel = 10; 
    public int costIncreasePerLevel = 10000;


    public override bool Purchase(GameObject buyer)
    {
        PlayerStats playerStats = buyer.GetComponent<PlayerStats>();
        if (playerStats == null)
        {

            Debug.LogError("PlayerStats component not found !");
            return false;
        }

        if (playerStats.GetUpgradeLevel(attributeToUpgrade) >= maxLevel)
        {
            Debug.Log($"Attribute {attributeToUpgrade} is already at max level.");
            return false;
        }

        playerStats.ApplyUpgrade(this);

        Debug.Log($"Purchased and applied {itemName}. New level: {playerStats.GetUpgradeLevel(attributeToUpgrade)}");
        return true;
    }

    public override int GetCurrentCost()
    {
        // TODO: implementasi exponential cost increase
        if (GameManager.Instance != null)
        {
            // Bagian ini seharusnya mengambil level upgrade dari PlayerStats
            // int baseCost = GameManager.Instance.GetBaseCost(attributeToUpgrade);
            // int costIncreasePerLevel = GameManager.Instance.GetCostIncreasePerLevel(attributeToUpgrade);
            // int currentLevel = PlayerStats.Instance.GetUpgradeLevel(attributeToUpgrade);
            // return baseCost + (currentLevel * costIncreasePerLevel);
        }
        return baseCost; // Fallback
    }

    public string GetFormattedDescription(PlayerStats playerStats)
    {
        int currentLevel = playerStats.GetUpgradeLevel(attributeToUpgrade);
        return $"{description}\nLevel: {currentLevel}/{maxLevel}\nCost: {GetCurrentCost(playerStats)}";
    }

    public int GetCurrentCost(PlayerStats playerStats)
    {
        int currentLevel = playerStats.GetUpgradeLevel(attributeToUpgrade);
        return baseCost + (currentLevel * costIncreasePerLevel);
    }
}