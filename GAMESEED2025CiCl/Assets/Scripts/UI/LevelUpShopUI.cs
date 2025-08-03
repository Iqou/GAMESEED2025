using UnityEngine;
using UnityEngine.UI;

public class LevelUpShopUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button closeButton;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        else
        {
            Debug.LogError("Close Button is not assigned in the LevelUpShopUI!", this);
        }
    }

    void CloseShop()
    {
        // Call the manager to handle closing logic
        if (LevelUpShopManager.Instance != null)
        {
            LevelUpShopManager.Instance.CloseShop();
        }
    }
}
