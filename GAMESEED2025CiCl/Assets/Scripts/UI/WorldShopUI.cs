using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorldShopUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button closeButton;
    public TextMeshProUGUI locationText;
    public string locationName = "Warung";

    [Header("Fixed Item Slots")]
    public ShopItemUI itemSlot1;
    public ShopItemUI itemSlot2;
    public ShopItemUI itemSlot3;

    void OnEnable()
    {
        if (locationText != null)
        {
            locationText.text = locationName;
        }
    }

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        else
        {
            Debug.LogError("Close Button is not assigned in the WorldShopUI!", this);
        }
    }

    public void RefreshShop(List<ShopItem> items, PlayerStats player)
    {
        // Assign items to the fixed slots
        if (itemSlot1 != null && items.Count > 0)
        {
            itemSlot1.gameObject.SetActive(true);
            itemSlot1.Setup(items[0], player);
        }
        else if (itemSlot1 != null)
        {
            itemSlot1.gameObject.SetActive(false);
        }

        if (itemSlot2 != null && items.Count > 1)
        {
            itemSlot2.gameObject.SetActive(true);
            itemSlot2.Setup(items[1], player);
        }
        else if (itemSlot2 != null)
        {
            itemSlot2.gameObject.SetActive(false);
        }

        if (itemSlot3 != null && items.Count > 2)
        {
            itemSlot3.gameObject.SetActive(true);
            itemSlot3.Setup(items[2], player);
        }
        else if (itemSlot3 != null)
        {
            itemSlot3.gameObject.SetActive(false);
        }
    }

    void CloseShop()
    {
        if (WorldShopManager.Instance != null)
        {
            WorldShopManager.Instance.CloseShop();
        }
    }
}
