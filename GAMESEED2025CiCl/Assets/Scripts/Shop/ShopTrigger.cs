using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRadius = 5f;

    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void OnMouseDown()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player not found. Cannot interact with shop.");
            return;
        }

        // Check if the player is within the interaction radius
        if (Vector3.Distance(transform.position, playerTransform.position) <= interactionRadius)
        {
            // Tell the manager to open the shop, passing this shop's transform for the camera to focus on
            if (WorldShopManager.Instance != null)
            {
                WorldShopManager.Instance.OpenShop(transform);
            }
        }
        else
        {
            Debug.Log("Player is too far away to interact with the shop.");
            // Optional: Add a UI message here to inform the player
        }
    }

    // Draw a visual representation of the interaction radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
