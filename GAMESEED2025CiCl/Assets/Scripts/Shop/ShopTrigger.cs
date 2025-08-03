using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopTrigger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRadius = 5f;
    public int itemsToShow = 3;

    [Header("Data")]
    public ShopItemDatabase itemDatabase;

    // --- Per-Shop State ---
    private List<ShopItem> offeredItems;
    private bool isSoldOut = false;
    // --------------------

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Generate this shop's unique, persistent inventory ONCE.
        GenerateInventory();
    }

    private void GenerateInventory()
    {
        offeredItems = new List<ShopItem>();
        if (itemDatabase == null || itemDatabase.allItems == null || itemDatabase.allItems.Count == 0)
        {
            Debug.LogError("Item Database is not assigned or is empty!", this);
            return;
        }

        List<ShopItem> availablePool = new List<ShopItem>(itemDatabase.allItems);
        
        // For now, we'll just pick randomly. This could be expanded with rarity later.
        for (int i = 0; i < itemsToShow && availablePool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePool.Count);
            offeredItems.Add(availablePool[randomIndex]);
            availablePool.RemoveAt(randomIndex);
        }
        Debug.Log($"Shop {gameObject.name} generated inventory with {offeredItems.Count} items.");
    }

    void OnMouseDown()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player not found. Cannot interact with shop.");
            return;
        }

        if (Vector3.Distance(transform.position, playerTransform.position) <= interactionRadius)
        {
            if (WorldShopManager.Instance != null)
            {
                // Pass this specific shop trigger to the manager.
                WorldShopManager.Instance.OpenShop(this);
            }
        }
        else
        {
            Debug.Log("Player is too far away to interact with the shop.");
        }
    }

    // --- Public methods for the Manager to use ---
    public List<ShopItem> GetOfferedItems()
    {
        return offeredItems;
    }

    public bool IsSoldOut()
    {
        return isSoldOut;
    }

    public void MarkAsSoldOut()
    {
        isSoldOut = true;
        Debug.Log($"Shop {gameObject.name} is now sold out.");
    }
    // -----------------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}

