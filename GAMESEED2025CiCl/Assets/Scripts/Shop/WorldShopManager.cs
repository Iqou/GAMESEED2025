using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WorldShopManager : MonoBehaviour
{
    public static WorldShopManager Instance;

    [Header("Data")]
    public ShopItemDatabase itemDatabase;
    public int itemsToShow = 4;

    [Header("UI")]
    public WorldShopUI shopUI; // Direct reference to the UI script

    [Header("Camera Control")]
    public float zoomDuration = 1.0f;
    public float shopFieldOfView = 30f;

    private Camera mainCamera;
    private PlayerStats playerStats;
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private float originalFieldOfView;
    private bool isShopOpen = false;

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
        mainCamera = Camera.main;
        playerStats = FindObjectOfType<PlayerStats>(); // Find the player stats

        if (shopUI != null)
        {
            shopUI.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Shop UI reference is missing in WorldShopManager!", this);
        }
    }

    public void OpenShop(Transform shopTarget)
    {
        if (isShopOpen || shopUI == null || mainCamera == null || itemDatabase == null) return;

        isShopOpen = true;
        
        // --- Refresh UI ---
        List<ShopItem> randomItems = GetRandomItems();
        shopUI.RefreshShop(randomItems, playerStats);
        
        // --- Camera Zoom ---
        originalCameraPos = mainCamera.transform.position;
        originalCameraRot = mainCamera.transform.rotation;
        originalFieldOfView = mainCamera.fieldOfView;

        StartCoroutine(ZoomToShop(shopTarget));
    }

    public void CloseShop()
    {
        if (!isShopOpen || shopUI == null) return;

        shopUI.gameObject.SetActive(false);
        Time.timeScale = 1f;

        StartCoroutine(ZoomToPlayer());
    }

    private List<ShopItem> GetRandomItems()
    {
        List<ShopItem> offeredItems = new List<ShopItem>();
        List<ShopItem> availablePool = new List<ShopItem>(itemDatabase.allItems);

        // --- Special Logic for Horeg Slot Unlocks ---
        // Check if the player is eligible for the next slot unlock and add it to the offers
        HoregSlotItem nextSlotUnlock = GetNextEligibleSlotUnlock(availablePool);
        if (nextSlotUnlock != null)
        {
            offeredItems.Add(nextSlotUnlock);
            availablePool.Remove(nextSlotUnlock); // Don't offer it twice
        }

        // --- Fill the rest of the shop with random items ---
        int itemsToFill = itemsToShow - offeredItems.Count;
        
        // Remove any other slot unlocks from the pool so they don't appear randomly
        availablePool.RemoveAll(item => item is HoregSlotItem);

        // Randomly select the remaining items
        if (itemsToFill > 0 && availablePool.Count > 0)
        {
            offeredItems.AddRange(availablePool.OrderBy(x => Random.value).Take(itemsToFill));
        }

        return offeredItems;
    }

    private HoregSlotItem GetNextEligibleSlotUnlock(List<ShopItem> pool)
    {
        // Find all slot unlock items in the pool
        var slotItems = pool.OfType<HoregSlotItem>().OrderBy(s => s.slotIndexToUnlock).ToList();

        foreach (var slotItem in slotItems)
        {
            // If the player has already unlocked this slot, it's not the next one.
            if (playerStats.unlockedHoregSlots >= slotItem.slotIndexToUnlock)
            {
                continue;
            }

            // If the player meets the level requirement for this slot, this is the one to offer.
            if (playerStats.level >= slotItem.requiredLevel)
            {
                return slotItem;
            }
        }

        // No eligible slot unlock found
        return null;
    }

    private IEnumerator ZoomToShop(Transform shopTarget)
    {
        Vector3 targetPosition = shopTarget.position - (shopTarget.forward * 5f) + (Vector3.up * 2f);
        Quaternion targetRotation = Quaternion.LookRotation(shopTarget.position - targetPosition);

        float elapsedTime = 0;
        while (elapsedTime < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(originalCameraPos, targetPosition, elapsedTime / zoomDuration);
            mainCamera.transform.rotation = Quaternion.Slerp(originalCameraRot, targetRotation, elapsedTime / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(originalFieldOfView, shopFieldOfView, elapsedTime / zoomDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
        mainCamera.fieldOfView = shopFieldOfView;
        
        shopUI.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    private IEnumerator ZoomToPlayer()
    {
        float elapsedTime = 0;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        while (elapsedTime < zoomDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, originalCameraPos, elapsedTime / zoomDuration);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, originalCameraRot, elapsedTime / zoomDuration);
            mainCamera.fieldOfView = Mathf.Lerp(shopFieldOfView, originalFieldOfView, elapsedTime / zoomDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalCameraPos;
        mainCamera.transform.rotation = originalCameraRot;
        mainCamera.fieldOfView = originalFieldOfView;
        isShopOpen = false;
    }
}
