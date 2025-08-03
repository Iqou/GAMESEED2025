using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemCostText;
    public Image itemIcon;
    public Button purchaseButton;

    private ShopItem currentItem;
    private PlayerStats playerStats;

    public void Setup(ShopItem item, PlayerStats stats)
    {
        currentItem = item;
        playerStats = stats;

        itemNameText.text = currentItem.itemName;
        itemDescriptionText.text = currentItem.description;
        itemCostText.text = $"Rp {currentItem.GetCurrentCost(playerStats)}";
        itemIcon.sprite = currentItem.icon;

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnPurchase);

        UpdatePurchaseButton();
    }

    private void OnPurchase()
    {
        if (currentItem == null || playerStats == null) return;

        int cost = currentItem.GetCurrentCost(playerStats);
        if (playerStats.money >= cost)
        {
            playerStats.AddMoney(-cost);
            bool success = currentItem.Purchase(playerStats.gameObject);
            if (success)
            {
                Debug.Log($"Successfully purchased {currentItem.itemName}");
                Setup(currentItem, playerStats); 
            }
            else
            {
                playerStats.AddMoney(cost);
                Debug.LogWarning($"Purchase of {currentItem.itemName} failed, money refunded.");
            }
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    private void UpdatePurchaseButton()
    {
        if (!currentItem.CanAfford(playerStats))
        {
            purchaseButton.interactable = false;
            itemCostText.color = Color.red;
        }
        else
        {
            purchaseButton.interactable = true;
            itemCostText.color = Color.white;
        }
    }
}
