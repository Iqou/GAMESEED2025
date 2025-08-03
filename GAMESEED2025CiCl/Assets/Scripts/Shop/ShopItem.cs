
using UnityEngine;

public enum ItemRarity
{
    Common,
    Rare,
    Epic
}

public abstract class ShopItem : ScriptableObject
{
    [Header("Common Item Properties")]
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public int baseCost;
    public ItemRarity rarity;


    // Abstract yang nantinya harus diimplementasikan oleh setiap item
    public abstract bool Purchase(GameObject buyer);

    public virtual int GetCurrentCost(PlayerStats playerStats)
    {
        return baseCost;
    }

    public virtual string GetDescription(PlayerStats playerStats)
    {
        return description;
    }

    public virtual string GetFormattedDescription(PlayerStats playerStats)
    {
        return GetDescription(playerStats);
    }

    public virtual bool CanAfford(PlayerStats playerStats)
    {
        return playerStats.money >= GetCurrentCost(playerStats);
    }

    public int GetWeight()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return 100;
            case ItemRarity.Rare:
                return 25;
            case ItemRarity.Epic:
                return 5;
            default:
                return 100;
        }
    }
}
