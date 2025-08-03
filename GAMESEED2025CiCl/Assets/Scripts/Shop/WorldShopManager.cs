using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WorldShopManager : MonoBehaviour
{
    public static WorldShopManager Instance;

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
    
    private ShopTrigger currentShopTrigger; // Keep track of the currently active shop

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
        playerStats = FindObjectOfType<PlayerStats>(); 

        if (shopUI != null)
        {
            shopUI.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Shop UI reference is missing in WorldShopManager!", this);
        }
    }

    public void OpenShop(ShopTrigger shopTrigger)
    {
        if (isShopOpen || shopUI == null || mainCamera == null) return;

        currentShopTrigger = shopTrigger; // Set the active shop
        isShopOpen = true;

        // --- Get items from the trigger and refresh UI ---
        List<ShopItem> items = currentShopTrigger.GetOfferedItems();
        // Pass the sold out status to the UI
        shopUI.RefreshShop(items, playerStats, currentShopTrigger.IsSoldOut());

        // --- Camera Zoom ---
        originalCameraPos = mainCamera.transform.position;
        originalCameraRot = mainCamera.transform.rotation;
        originalFieldOfView = mainCamera.fieldOfView;

        StartCoroutine(ZoomToShop(shopTrigger.transform));
    }

    public void CloseShop()
    {
        if (!isShopOpen || shopUI == null) return;

        isShopOpen = false;
        currentShopTrigger = null; // Clear the active shop reference
        Time.timeScale = 1f;
        StartCoroutine(shopUI.FadeOut());
        StartCoroutine(ZoomToPlayer());
    }

    public void NotifyPurchaseMade()
    {
        if (currentShopTrigger != null)
        {
            currentShopTrigger.MarkAsSoldOut();
            shopUI.SetSoldOutState();
        }
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
        StartCoroutine(shopUI.FadeIn());
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
    }
}

