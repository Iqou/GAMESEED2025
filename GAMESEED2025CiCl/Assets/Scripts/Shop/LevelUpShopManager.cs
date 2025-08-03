using UnityEngine;

public class LevelUpShopManager : MonoBehaviour
{
    public static LevelUpShopManager Instance;

    [Header("Shop UI Panel")]
    public GameObject shopPanel;

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
        // Ensure the shop is closed at the start
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // Subscribe to the level up event
        PlayerStats.OnPlayerLevelUp += OpenShop;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        PlayerStats.OnPlayerLevelUp -= OpenShop;
    }

    public void OpenShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("Shop Panel is not assigned in the LevelUpShopManager!", this);
            return;
        }

        Debug.Log("Opening Level Up Shop and pausing game.");
        shopPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;

        Debug.Log("Closing Level Up Shop and resuming game.");
        shopPanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game
    }
}
