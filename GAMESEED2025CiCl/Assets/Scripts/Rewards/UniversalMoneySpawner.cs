using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UniversalMoneySpawner : MonoBehaviour
{
    public static UniversalMoneySpawner Instance;

    [Header("Money Prefab")]
    public GameObject moneyPrefab;

    [Header("Spawn Settings")]
    public float launchForce = 10f;
    public string moneyTexturePath = "MoneyTextures"; 

    private List<MoneyDenomination> denominations;
    private PlayerStats playerStats;

    private struct MoneyDenomination
    {
        public int value;
        public Texture2D texture;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadDenominations();
    }

    private void LoadDenominations()
    {
        denominations = new List<MoneyDenomination>();
        Texture2D[] moneyTextures = Resources.LoadAll<Texture2D>(moneyTexturePath);

        if (moneyTextures.Length == 0)
        {
            Debug.LogError($"No money textures found in 'Assets/Resources/{moneyTexturePath}'. Please create this folder and place your correctly named money PNGs there.", this);
            return;
        }

        foreach (var texture in moneyTextures)
        {
            string name = texture.name.ToLower();
            if (name.EndsWith("k"))
            {
                string numberPart = name.TrimEnd('k');
                if (int.TryParse(numberPart, out int value))
                {
                    denominations.Add(new MoneyDenomination { value = value * 1000, texture = texture });
                }
                else
                {
                    Debug.LogWarning($"Could not parse number from texture name: '{texture.name}'.", this);
                }
            }
            else
            {
                Debug.LogWarning($"Texture name '{texture.name}' does not end with 'k'. Please name files like '10k.png'.", this);
            }
        }

        denominations = denominations.OrderByDescending(d => d.value).ToList();
        Debug.Log($"Loaded {denominations.Count} money denominations automatically.");
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (playerStats != null)
            {
                SpawnMoney(playerStats.transform.position, 180000);
            }
            else
            {
                Debug.LogError("PlayerStats not found in scene for debug spawning!", this);
            }
        }
    }

    public void SpawnMoney(Vector3 position, int totalAmount)
    {
        if (moneyPrefab == null || denominations == null || denominations.Count == 0)
        {
            Debug.LogError("Money Prefab not assigned or no denominations were loaded!", this);
            return;
        }

        int remainingAmount = totalAmount;

        foreach (var denomination in denominations)
        {
            if (denomination.value <= 0) continue;
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

        MoneyPickup pickup = moneyGO.GetComponent<MoneyPickup>();
        if (pickup != null)
        {
            pickup.moneyValue = denomination.value;
        }

        Renderer renderer = moneyGO.GetComponent<Renderer>();
        if (renderer != null && denomination.texture != null)
        {
            renderer.material.mainTexture = denomination.texture;
        }

        Rigidbody rb = moneyGO.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1f, Random.Range(-0.5f, 0.5f)).normalized;
            rb.AddForce(randomDirection * launchForce, ForceMode.Impulse);
        }
    }
}

