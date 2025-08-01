using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    public static event System.Action OnStatsChanged;

    [Header("Live Stats - Read Only in Inspector")]
    // --- Attribute Upgrade Stats ---
    public float damageMultiplier = 1f;
    public float cooldownReduction = 0f; 
    public float areaOfEffectBonus = 1f;
    public float beatAccuracyBonus = 0f;
    public float maxHealthBonus = 0f;

    // --- Consumable Effects ---
    public float moveSpeedMultiplier = 1f;

    // --- Upgrade Level Tracking ---
    [SerializeField]
    private Dictionary<AttributeType, int> upgradeLevels = new Dictionary<AttributeType, int>();

    void Awake()
    {
        foreach (AttributeType attribute in System.Enum.GetValues(typeof(AttributeType)))
        {
            upgradeLevels[attribute] = 0;
        }
    }

    public void ApplyUpgrade(AttributeUpgradeItem upgrade)
    {
        if (GetUpgradeLevel(upgrade.attributeToUpgrade) >= upgrade.maxLevel) return;

        upgradeLevels[upgrade.attributeToUpgrade]++;

        switch (upgrade.attributeToUpgrade)
        {
            case AttributeType.Damage:
                damageMultiplier += upgrade.upgradeValue;
                break;
            case AttributeType.Cooldown:
                cooldownReduction += upgrade.upgradeValue;
                break;
            case AttributeType.AreaOfEffect:
                areaOfEffectBonus += upgrade.upgradeValue;
                break;
            case AttributeType.BeatAccuracy:
                beatAccuracyBonus += upgrade.upgradeValue;
                break;
            case AttributeType.MaxHealth:
                maxHealthBonus += upgrade.upgradeValue;
                // TODO: notify Health component buat update max health
                break;
        }
        OnStatsChanged?.Invoke();
    }

    public void ApplyConsumable(ConsumableItem consumable)
    {
        switch (consumable.effect)
        {
            case ConsumableEffect.MoveSpeedBoost:
                StartCoroutine(TemporarySpeedBoost(consumable.effectValue, consumable.duration));
                break;
            case ConsumableEffect.HealHoreg:
                Health playerHealth = GetComponent<Health>();
                if (playerHealth != null)
                {
                    int healAmount = Mathf.FloorToInt(playerHealth.maxHealth * consumable.effectValue);
                    playerHealth.ChangeHealth(healAmount);
                    Debug.Log($"Healed for {healAmount} HP.");
                }
                break;
            case ConsumableEffect.DecreaseWantedLevel:
                // TODO: Konek ke WantedLevelManager 
                Debug.Log("Decreased Wanted Level");
                break;
            case ConsumableEffect.IncreaseWantedLevel:
                Debug.Log("Increased Wanted Level");
                break;
        }
        OnStatsChanged?.Invoke();
    }

    private System.Collections.IEnumerator TemporarySpeedBoost(float boostAmount, float duration)
    {
        moveSpeedMultiplier += boostAmount;
        yield return new WaitForSeconds(duration);
        moveSpeedMultiplier -= boostAmount;
    }

    public int GetUpgradeLevel(AttributeType attribute)
    {
        return upgradeLevels.ContainsKey(attribute) ? upgradeLevels[attribute] : 0;
    }
}
