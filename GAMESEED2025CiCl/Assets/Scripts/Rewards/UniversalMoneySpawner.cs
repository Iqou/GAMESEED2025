using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Added for sorting

public class UniversalMoneySpawner : MonoBehaviour
{
    public static UniversalMoneySpawner Instance;

    [Header("Money Prefab")]
    public GameObject moneyPrefab; // Assign a prefab with MoneyPickup.cs and a Rigidbody

    [System.Serializable]
    public struct MoneyDenomination
    {
        public int value;
        public Texture2D texture; // Changed from Material to Texture2D
    }

    [Header("Money Denominations (Order does not matter)")]
    public List<MoneyDenomination> denominations;

    [Header("Spawn Settings")]
    public float launchForce = 10f;

    private PlayerStats playerStats;

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

        // Automatically sort the list from highest to lowest value to ensure the logic works
        denominations = denominations.OrderByDescending(d => d.value).ToList();
    }

    void Start()
    {
        // Cache player stats for debug spawning
        playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update()
    {
        // Debug: Spawn money
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (playerStats != null)
            {
                SpawnMoney(playerStats.transform.position, 180000); 
            }
            else
            {
                Debug.LogError("PlayerStats not found in scene for debug spawning!");
            }
        }
    }

    public void SpawnMoney(Vector3 position, int totalAmount)
    {
        if (moneyPrefab == null || denominations.Count == 0)
        {
            Debug.LogError("Money Prefab or Denominations not assigned in UniversalMoneySpawner!");
            return;
        }

        int remainingAmount = totalAmount;

        foreach (var denomination in denominations)
        {
            int billCount = remainingAmount / denomination.value;
            for (int i = 0; i < billCount; i++)
            {
                SpawnSingleBill(position, denomination);
            }
            remainingAmount %= denomination.value;
        }
    }

    private void SpawnSingleBill(Vector3 position, MoneyDenomination denomination)
    {
        float spawnHeight = Random.Range(2f, 3f);
        Vector3 spawnPosition = position + Vector3.up * spawnHeight;
        GameObject moneyGO = Instantiate(moneyPrefab, spawnPosition, Quaternion.identity);

        // Set the money value
        MoneyPickup pickup = moneyGO.GetComponent<MoneyPickup>();
        if (pickup != null)
        {
            pickup.moneyValue = denomination.value;
        }

        // Assign the texture to the object's material
        Renderer renderer = moneyGO.GetComponent<Renderer>();
        if (renderer != null && denomination.texture != null)
        {
            renderer.material.mainTexture = denomination.texture;
        }

        // Launch the money upwards
        Rigidbody rb = moneyGO.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(randomDirection * launchForce, ForceMode.Impulse);
        }
    }
}

