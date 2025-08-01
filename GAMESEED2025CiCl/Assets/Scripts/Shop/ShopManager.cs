using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Configuration")]
    public List<ShopItem> fullItemPool; 
    public int itemsToOffer = 5;

    private List<ShopItem> currentlyOfferedItems;

    // Notifikasi UI untuk ketika item shop sudah siap
    public static event System.Action<List<ShopItem>> OnShopItemsReady;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Akan dipanggil ketika player level up
    public void PrepareShopOffers(PlayerStats playerStats)
    {
        currentlyOfferedItems = SelectItemsForOffer(playerStats);
        OnShopItemsReady?.Invoke(currentlyOfferedItems);
        Debug.Log("Shop offers are ready. UI should now display them.");
    }

    private List<ShopItem> SelectItemsForOffer(PlayerStats playerStats)
    {
        List<ShopItem> availableItems = new List<ShopItem>(fullItemPool);
        List<ShopItem> offeredItems = new List<ShopItem>();

        availableItems.RemoveAll(item => {
            if (item is AttributeUpgradeItem upgradeItem)
            {
                return playerStats.GetUpgradeLevel(upgradeItem.attributeToUpgrade) >= upgradeItem.maxLevel;
            }
            return false;
        });

        int totalWeight = availableItems.Sum(item => item.GetWeight());

        for (int i = 0; i < itemsToOffer && availableItems.Count > 0; i++)
        {
            int randomWeight = Random.Range(0, totalWeight);
            ShopItem selectedItem = null;

            foreach (var item in availableItems)
            {
                randomWeight -= item.GetWeight();
                if (randomWeight < 0)
                {
                    selectedItem = item;
                    break;
                }
            }

            if (selectedItem != null)
            {
                offeredItems.Add(selectedItem);
                availableItems.Remove(selectedItem);
                totalWeight -= selectedItem.GetWeight();
            }
        }

        return offeredItems;
    }

    public bool PurchaseItem(ShopItem item, GameObject buyer)
    {
        PlayerStats playerStats = buyer.GetComponent<PlayerStats>();
        int cost = 0;

        if (item is AttributeUpgradeItem upgradeItem)
        {
            cost = upgradeItem.GetCurrentCost(playerStats);
        }
        else
        {
            cost = item.GetCurrentCost();
        }

        if (GameManager.Instance.totalCoins >= cost)
        {
            if (item.Purchase(buyer))
            {
                GameManager.Instance.totalCoins -= cost;
                Debug.Log($"Purchase successful: {item.itemName}. Remaining Rupiah: {GameManager.Instance.totalCoins}");

                return true;
            }
            else
            {
                Debug.Log($"Purchase failed for {item.itemName}, but player had enough money. The item's Purchase() method returned false.");
                return false;
            }
        }
        else
        {
            Debug.Log($"Not enough Rupiah to purchase {item.itemName}. Required: {cost}, Have: {GameManager.Instance.totalCoins}");
            return false;
        }
    }
}
