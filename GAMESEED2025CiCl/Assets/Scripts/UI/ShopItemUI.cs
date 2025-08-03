using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Button))]
public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemCostText;
    public Image itemIcon;
    
    private Button purchaseButton;
    private ShopItem currentItem;
    private PlayerStats playerStats;
    private Vector3 originalButtonScale;
    public float hoverScaleMultiplier = 1.1f;

    void Awake()
    {
        purchaseButton = GetComponent<Button>();
        originalButtonScale = transform.localScale;

        if (itemIcon == null)
        {
            Transform iconTransform = transform.Find("ItemIcon");
            if (iconTransform != null)
            {
                itemIcon = iconTransform.GetComponent<Image>();
            }
            
            if (itemIcon == null)
            {
                Debug.LogError("ShopItemUI: Could not find the 'ItemIcon' Image component on this GameObject or its children.", this);
            }
        }
    }

    public void Setup(ShopItem item, PlayerStats stats)
    {
        currentItem = item;
        playerStats = stats;

        itemNameText.text = currentItem.itemName;
        itemDescriptionText.text = currentItem.GetDescription(playerStats);
        itemCostText.text = $"Rp {currentItem.GetCurrentCost(playerStats)}";
        
        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.icon;
            itemIcon.enabled = currentItem.icon != null;
        }

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
                WorldShopManager.Instance.NotifyPurchaseMade();
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

    public void UpdatePurchaseButton(bool forceDisable = false)
    {
        if (forceDisable || currentItem == null || playerStats == null || !currentItem.CanAfford(playerStats))
        {
            purchaseButton.interactable = false;
            itemCostText.color = forceDisable ? Color.gray : Color.red;
        }
        else
        {
            purchaseButton.interactable = true;
            itemCostText.color = Color.white;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (purchaseButton.interactable)
        {
            transform.localScale = originalButtonScale * hoverScaleMultiplier;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalButtonScale;
    }
}

