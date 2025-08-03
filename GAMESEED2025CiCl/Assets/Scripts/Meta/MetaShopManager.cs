using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MetaShopManager : MonoBehaviour
{
    public static MetaShopManager Instance;

    [Header("Data")]
    public List<MetaUpgradeItem> availableUpgrades;

    [Header("UI")]
    public Transform contentParent;
    public GameObject metaItemUIPrefab;
    public TextMeshProUGUI soundChipText; // Assign this to show currency

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        UpdateSoundChipUI();

        // Clear existing items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Instantiate a UI element for each available upgrade
        foreach (var upgrade in availableUpgrades)
        {
            GameObject itemGO = Instantiate(metaItemUIPrefab, contentParent);
            MetaUpgradeItemUI uiScript = itemGO.GetComponent<MetaUpgradeItemUI>();
            if (uiScript != null)
            {
                uiScript.Setup(upgrade);
            }
        }
    }

    public void PurchaseUpgrade(MetaUpgradeItem upgrade)
    {
        int currentLevel = GameManager.Instance.GetMetaUpgradeLevel(upgrade.upgradeId);
        int cost = upgrade.CalculateCost(currentLevel);

        if (GameManager.Instance.soundChips >= cost)
        {
            GameManager.Instance.soundChips -= cost;
            GameManager.Instance.SetMetaUpgradeLevel(upgrade.upgradeId, currentLevel + 1);
            GameManager.Instance.SaveProgressToSlot(GameManager.Instance.currentSlot);

            Debug.Log($"Purchased '{upgrade.upgradeName}'. New Level: {currentLevel + 1}");
            RefreshUI(); // Refresh the entire shop to update all costs and buttons
        }
        else
        {
            Debug.Log("Not enough SoundChips!");
        }
    }

    public void UpdateSoundChipUI()
    {
        if (soundChipText != null && GameManager.Instance != null)
        {
            soundChipText.text = GameManager.Instance.soundChips.ToString();
        }
    }
}