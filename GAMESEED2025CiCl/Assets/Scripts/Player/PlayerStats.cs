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

    // --- Player Progression ---
    public int level = 1;
    public int money = 0;
    public int currentExperience = 0;
    public int maxExperience = 150;

    // --- EXP Formula Constants ---
    private const float BASE_EXP = 150f;
    private const float EXPONENT = 1.5f;


    // --- Upgrade Level Tracking ---
    [SerializeField]
    private Dictionary<AttributeType, int> upgradeLevels = new Dictionary<AttributeType, int>();

    void Awake()
    {
        foreach (AttributeType attribute in System.Enum.GetValues(typeof(AttributeType)))
        {
            upgradeLevels[attribute] = 0;
        }
        maxExperience = CalculateMaxExperience();
    }

    void Update()
    {
        // Debug: Add 100 EXP 
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AddExperience(100);
        }
    }

    private int CalculateMaxExperience()
    {
        return Mathf.RoundToInt(BASE_EXP * Mathf.Pow(level, EXPONENT));
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        Debug.Log($"Gained {amount} EXP! Current EXP: {currentExperience}/{maxExperience}");

        while (currentExperience >= maxExperience)
        {
            currentExperience -= maxExperience;
            level++;
            maxExperience = CalculateMaxExperience();
            Debug.Log($"LEVEL UP! Reached Level {level}! Next level at {maxExperience} EXP.");
        }
        OnStatsChanged?.Invoke(); // Notify UI or other systems
    }

    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"Gained {amount} money! Total Money: {money}");
        OnStatsChanged?.Invoke(); // Notify UI or other systems
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
