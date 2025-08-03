using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class WorldShopUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button closeButton;
    public TextMeshProUGUI locationText;
    public TextMeshProUGUI soldOutText; // Assign a new TextMeshProUGUI for the "sold out" message
    public string locationName = "Warung";
    public float fadeDuration = 0.5f;

    [Header("Fixed Item Slots")]
    public ShopItemUI itemSlot1;
    public ShopItemUI itemSlot2;
    public ShopItemUI itemSlot3;

    private CanvasGroup canvasGroup;
    private Color originalTitleColor;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (locationText != null)
        {
            originalTitleColor = locationText.color;
        }
    }

    void OnEnable()
    {
        // Reset UI state every time it's enabled
        ResetUI();
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
        soldOutText.gameObject.SetActive(false); // Hide by default
    }

    public void RefreshShop(List<ShopItem> items, PlayerStats player, bool isSoldOut)
    {
        ResetUI(); // Ensure UI is in a clean state before refreshing

        // Assign items to the fixed slots
        if (itemSlot1 != null)
        {
            bool hasItem = items.Count > 0;
            itemSlot1.gameObject.SetActive(hasItem);
            if (hasItem) itemSlot1.Setup(items[0], player);
        }

        if (itemSlot2 != null)
        {
            bool hasItem = items.Count > 1;
            itemSlot2.gameObject.SetActive(hasItem);
            if (hasItem) itemSlot2.Setup(items[1], player);
        }

        if (itemSlot3 != null)
        {
            bool hasItem = items.Count > 2;
            itemSlot3.gameObject.SetActive(hasItem);
            if (hasItem) itemSlot3.Setup(items[2], player);
        }

        if (isSoldOut)
        {
            SetSoldOutState();
        }
    }

    public void SetSoldOutState()
    {
        locationText.text = "Terjual";
        locationText.color = Color.red;
        soldOutText.text = "Terjual!";
        soldOutText.gameObject.SetActive(true);

        // Disable purchase buttons but keep items visible
        if (itemSlot1 != null) itemSlot1.UpdatePurchaseButton(true);
        if (itemSlot2 != null) itemSlot2.UpdatePurchaseButton(true);
        if (itemSlot3 != null) itemSlot3.UpdatePurchaseButton(true);
    }

    private void ResetUI()
    {
        if (locationText != null)
        {
            locationText.text = locationName;
            locationText.color = originalTitleColor;
        }
        if (soldOutText != null)
        {
            soldOutText.gameObject.SetActive(false);
        }
    }

    public IEnumerator FadeIn()
    {
        canvasGroup.interactable = false;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    public IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    void CloseShop()
    {
        if (WorldShopManager.Instance != null)
        {
            WorldShopManager.Instance.CloseShop();
        }
    }
}