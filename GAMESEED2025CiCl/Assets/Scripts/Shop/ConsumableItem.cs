using UnityEngine;

public enum ConsumableEffect
{
    MoveSpeedBoost,
    HealHoreg,
    DecreaseWantedLevel,
    IncreaseWantedLevel
}

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Shop/Consumable Item")]
public class ConsumableItem : ShopItem
{
    [Header("Consumable Specifics")]
    public ConsumableEffect effect;
    public float effectValue; 
    public float duration; // 0 untuk instant

    public override bool Purchase(GameObject buyer)
    {
        PlayerStats playerStats = buyer.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on the buyer!");
            return false;
        }

        playerStats.ApplyConsumable(this);

        Debug.Log($"Consumed {itemName}.");
        return true;
    }
}
