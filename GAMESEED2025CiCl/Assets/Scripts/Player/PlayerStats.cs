using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    public static event System.Action OnStatsChanged;
    public static event System.Action OnPlayerLevelUp;

    [Header("Live Stats - Read Only in Inspector")]
    // --- Attribute Upgrade Stats ---
    public float damageMultiplier = 1f;
    public float cooldownReduction = 0f; 
    public float areaOfEffectBonus = 1f;
    public float beatAccuracyBonus = 0f;
    public float maxHealthBonus = 0f;

    // --- Consumable Effects ---
    public float moveSpeedMultiplier = 1f;

    // --- In-Run Progression Tracking ---
    public int totalRupiahCollectedThisRun = 0;
    public int bossesKilledThisRun = 0;
    public float timePlayedThisRun = 0f;

    // --- Player Progression ---
    public int level = 1;
    public int money = 0; // This is now the temporary, in-run currency
    public int unlockedHoregSlots = 1;
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
        // This method is called at the start of every run
        ResetTemporaryStats();
        ApplyMetaUpgrades();
        maxExperience = CalculateMaxExperience();
    }

    private void ResetTemporaryStats()
    {
        // Reset in-run currency and progression
        money = 0;
        totalRupiahCollectedThisRun = 0;
        bossesKilledThisRun = 0;
        timePlayedThisRun = 0f;
        level = 1;
        unlockedHoregSlots = 1;
        currentExperience = 0;

        // Reset temporary combat stats to their base values
        damageMultiplier = 1f;
        cooldownReduction = 0f;
        areaOfEffectBonus = 1f;
        beatAccuracyBonus = 0f;
        maxHealthBonus = 0f;
        moveSpeedMultiplier = 1f;

        // Reset the dictionary that tracks the levels of temporary in-run upgrades
        upgradeLevels.Clear();
        foreach (AttributeType attribute in System.Enum.GetValues(typeof(AttributeType)))
        {
            upgradeLevels[attribute] = 0;
        }
        Debug.Log("Temporary in-run stats have been reset.");
    }

    private void ApplyMetaUpgrades()
    {
        if (GameManager.Instance == null) return;

        Debug.Log("Applying permanent meta-upgrades...");
        foreach (var upgrade in GameManager.Instance.metaUpgradeLevels)
        {
            string upgradeId = upgrade.Key;
            int level = upgrade.Value;

            // Assumes each level of a meta-upgrade provides a 1% bonus
            if (upgradeId.Contains("Damage"))
            {
                damageMultiplier += level * 0.01f;
            }
            else if (upgradeId.Contains("Cooldown"))
            {
                cooldownReduction += level * 0.01f;
            }
            else if (upgradeId.Contains("Area"))
            {
                areaOfEffectBonus += level * 0.01f;
            }
            else if (upgradeId.Contains("Health"))
            {
                maxHealthBonus += level * 10; // Example: +10 flat HP per level
            }
        }
        OnStatsChanged?.Invoke(); // Update UI with new base stats
    }

    void Update()
    {
        // Increment the run timer
        timePlayedThisRun += Time.deltaTime;

        // Debug: Add 100 EXP on Numpad 5
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
        int oldExperience = currentExperience;
        currentExperience += amount;
        Debug.Log($"Gained {amount} EXP! Current EXP: {currentExperience}/{maxExperience}");

        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.AnimateExperienceBar(oldExperience, currentExperience, maxExperience);
        }

        bool leveledUp = false;
        while (currentExperience >= maxExperience)
        {
            leveledUp = true;
            currentExperience -= maxExperience;
            level++;
            maxExperience = CalculateMaxExperience();
            Debug.Log($"LEVEL UP! Reached Level {level}! Next level at {maxExperience} EXP.");
        }

        OnStatsChanged?.Invoke(); // Notify UI or other systems

        if (leveledUp)
        {
            OnPlayerLevelUp?.Invoke(); // Fire the level up event
        }
    }

    public void UnlockHoregSlot(int slotIndex)
    {
        if (slotIndex > unlockedHoregSlots)
        {
            unlockedHoregSlots = slotIndex;
            Debug.Log($"Successfully unlocked Horeg slot {slotIndex}!");
            OnStatsChanged?.Invoke();
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
        if (amount > 0)
        {
            totalRupiahCollectedThisRun += amount;
        }
        Debug.Log($"Gained {amount} money! Current Rupiah: {money}");
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
