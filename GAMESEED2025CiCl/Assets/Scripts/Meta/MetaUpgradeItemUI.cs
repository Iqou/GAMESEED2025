using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaUpgradeItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI upgradeNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Image iconImage;
    public Button purchaseButton;

    private MetaUpgradeItem currentItem;

    public void Setup(MetaUpgradeItem item)
    {
        currentItem = item;
        purchaseButton.onClick.AddListener(OnPurchase);
        UpdateUI();
    }

    private void OnPurchase()
    {
        if (MetaShopManager.Instance != null && currentItem != null)
        {
            MetaShopManager.Instance.PurchaseUpgrade(currentItem);
        }
    }

    private void UpdateUI()
    {
        if (currentItem == null || GameManager.Instance == null) return;

        int currentLevel = GameManager.Instance.GetMetaUpgradeLevel(currentItem.upgradeId);
        int cost = currentItem.CalculateCost(currentLevel);

        upgradeNameText.text = currentItem.upgradeName;
        descriptionText.text = currentItem.description;
        iconImage.sprite = currentItem.icon;
        levelText.text = $"LVL {currentLevel}";
        costText.text = $"{cost} SC";

        if (GameManager.Instance.soundChips < cost)
        {
            purchaseButton.interactable = false;
            costText.color = Color.red;
        }
        else
        {
            purchaseButton.interactable = true;
            costText.color = Color.white;
        }
    }
}