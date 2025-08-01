using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public GameObject shopPanel; // The main panel for the shop UI
    public GameObject itemOfferPrefab; // A prefab for a single item offer
    public Transform itemOfferContainer; // The parent transform for the item offers

    void OnEnable()
    {
        ShopManager.OnShopItemsReady += DisplayShopOffers;
    }

    void OnDisable()
    {
        ShopManager.OnShopItemsReady -= DisplayShopOffers;
    }

    void Start()
    {
        shopPanel.SetActive(false); // Hide the shop by default
    }

    // TEMPORARY: For testing purposes. Press 'P' to open the shop.
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (shopPanel.activeSelf)
            {
                CloseShop();
            }
            else
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    PlayerStats playerStats = player.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        ShopManager.Instance.PrepareShopOffers(playerStats);
                    }
                    else
                    {
                        Debug.LogError("Player does not have a PlayerStats component.");
                    }
                }
                else
                {
                    Debug.LogError("Could not find a GameObject with the 'Player' tag.");
                }
            }
        }
    }

    private void DisplayShopOffers(List<ShopItem> items)
    {
        // Clear any existing offers
        foreach (Transform child in itemOfferContainer)
        {
            Destroy(child.gameObject);
        }

        // Get the PlayerStats component from the player GameObject
        // This assumes you have a way to reference the player, e.g., through a tag or a GameManager reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("ShopUI: Player GameObject not found! Make sure the player has the 'Player' tag.");
            return;
        }
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("ShopUI: PlayerStats component not found on the Player GameObject!");
            return;
        }

        // Create a UI element for each offered item
        foreach (var item in items)
        {
            GameObject offerGO = Instantiate(itemOfferPrefab, itemOfferContainer);
            
            // Assuming the prefab has TextMeshProUGUI components for name, description, and cost
            // and a Button component.
            TextMeshProUGUI itemNameText = offerGO.transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemDescriptionText = offerGO.transform.Find("ItemDescriptionText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCostText = offerGO.transform.Find("ItemCostText").GetComponent<TextMeshProUGUI>();
            Button purchaseButton = offerGO.GetComponentInChildren<Button>();

            itemNameText.text = item.itemName;

            // Get the dynamic description and cost for attribute upgrades
            if (item is AttributeUpgradeItem upgradeItem)
            {
                itemDescriptionText.text = upgradeItem.GetFormattedDescription(playerStats);
                itemCostText.text = $"Rp {upgradeItem.GetCurrentCost(playerStats)}";
            }
            else
            {
                itemDescriptionText.text = item.description;
                itemCostText.text = $"Rp {item.GetCurrentCost()}";
            }

            // Set up the purchase button
            purchaseButton.onClick.AddListener(() => {
                bool success = ShopManager.Instance.PurchaseItem(item, player);
                if (success)
                {
                    // Refresh the shop offers to reflect the new state (e.g., updated costs, item removed from pool)
                    // This is a simple way to handle it. A more complex UI might just update the specific item.
                    ShopManager.Instance.PrepareShopOffers(playerStats);
                }
            });
        }

        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
}
