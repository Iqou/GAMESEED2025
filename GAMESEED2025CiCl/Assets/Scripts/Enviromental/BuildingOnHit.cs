using System.Collections.Generic;
using UnityEngine;

public class BuildingOnHit : MonoBehaviour
{
    [System.Serializable]
    public class NPCData
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnChance = 1f;
    }

    [Header("Building Health")]
    public int maxHealth = 100;

    [Header("NPC Spawned on Destroy")]
    public List<NPCData> NPC;
    public int spawnCount = 5;

    private int currentHealth;
    private Vector2Int coordinate;
    private void Start()
    {
        currentHealth = maxHealth;
        coordinate = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );
        GetComponent<Collider>().enabled = true;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            DestroyBuilding();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SoundWave"))
        {
            TakeDamage(10); // Adjust damage amount as needed
        }
    }

    private void DestroyBuilding()
    {
        BuildingData toRemove = null;
        foreach (BuildingData building in PathGen.Instance.PlacedBuildings)
        {
            if (building.coordinate == coordinate)
            {
                toRemove = building;
                break;
            }
        }

        if (toRemove != null)
        {
            PathGen.Instance.PlacedBuildings.Remove(toRemove);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        SpawnNPC();
    }

    private void SpawnNPC()
    {
        if (NPC.Count == 0 || spawnCount <= 0) return;

        Vector3 centerPosition = new Vector3(
            coordinate.x + 0.5f,
            transform.position.y,
            coordinate.y + 0.5f
        );
        float spawnRadius = 1.0f;

        // Hitung total bobot chance
        float totalChance = 0f;
        foreach (var npc in NPC)
        {
            totalChance += npc.spawnChance;
        }

        if (totalChance <= 0f) return;

        for (int i = 0; i < spawnCount; i++)
        {
            float randomValue = Random.Range(0f, totalChance);
            float cumulative = 0f;
            Debug.Log("For biasa dijalankan");

            foreach (var npc in NPC)
            {
                cumulative += npc.spawnChance;
                if (randomValue <= cumulative)
                {
                    // Posisi spawn acak dalam lingkaran radius 1
                    Vector2 offset2D = Random.insideUnitCircle * spawnRadius;
                    Vector3 spawnPosition = centerPosition + new Vector3(offset2D.x, 0f, offset2D.y);
                    Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                    Instantiate(npc.prefab, spawnPosition, rotation);
                    break;
                }
                Debug.Log("Foreach Dijalankan");
            }
        }
    }

}