using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoneyPickup : MonoBehaviour
{
    public int moneyValue = 10000;

    [Header("Collection")]
    public float collectionRadius = 2f;
    public AudioClip collectSound; // Sound to play on collection
    private Transform playerTransform;

    [Header("Visual Effects")]
    public float rotationSpeed = 100f;
    public float scaleSpeed = 1f;
    public float scaleAmount = 0.1f;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
        
        // Find the player once for efficiency
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Could not find GameObject with 'Player' tag. Money cannot be collected.", this);
        }
    }

    void Update()
    {
        // --- Visual Effects ---
        // Rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Scaling (zoom in/out)
        float scale = 1.0f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = initialScale * scale;

        // --- Collection Logic ---
        if (playerTransform != null)
        {
            // Check distance to the player
            if (Vector3.Distance(transform.position, playerTransform.position) <= collectionRadius)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        if (playerTransform != null)
        {
            PlayerStats playerStats = playerTransform.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddMoney(moneyValue);

                // Play sound at the pickup's location before destroying it
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);
                }

                // Destroy the object immediately to prevent it from being collected multiple times
                Destroy(gameObject); 
            }
        }
    }
}

