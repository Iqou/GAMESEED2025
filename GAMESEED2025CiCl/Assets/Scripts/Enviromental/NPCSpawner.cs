using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-45)]
public class NPCSpawner : MonoBehaviour
{
    [Header("NPC Prefabs")]
    public GameObject[] npcBossPrefabs;        // Boss1 – Boss4
    public GameObject[] randomNpcPrefabs;      // Random NPCs (multiple)

    [Header("Spawn Settings")]
    public int maxBossParks = 4;
    public int randomNpcCount = 20;

    private void Start()
    {
        var pathGen = PathGen.Instance;
        if (pathGen == null)
        {
            Debug.LogError("PathGen.Instance not found!");
            return;
        }

        SpawnBossesInParks(pathGen);
        SpawnRandomNPCs(pathGen);
    }

    void SpawnBossesInParks(PathGen pathGen)
    {
        var parks = new List<PathGen.ParkData>(pathGen.PlacedParks);
        Shuffle(parks);

        int spawnCount = Mathf.Min(maxBossParks, npcBossPrefabs.Length, parks.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            var park = parks[i];
            GameObject bossPrefab = npcBossPrefabs[i];

            Vector3 worldPos = GetRandomPositionInArea(park.startPosition, park.width, park.height);
            Instantiate(bossPrefab, worldPos, Quaternion.identity, transform);
        }
    }

    void SpawnRandomNPCs(PathGen pathGen)
    {
        int attempts = 0;
        int spawned = 0;

        while (spawned < randomNpcCount && attempts < randomNpcCount * 10)
        {
            attempts++;

            int x = Random.Range(0, pathGen.size);
            int y = Random.Range(0, pathGen.size);
            Vector2Int pos = new Vector2Int(x, y);

            if (IsBuildingAt(pathGen, pos)) continue;

            Vector3 worldPos = new Vector3(x + 0.5f, 1f, y + 0.5f);
            GameObject prefab = GetRandomFromArray(randomNpcPrefabs);
            if (prefab != null)
            {
                Instantiate(prefab, worldPos, Quaternion.identity, transform);
                spawned++;
            }
        }
    }

    Vector3 GetRandomPositionInArea(Vector2Int start, int width, int height)
    {
        int localX = Random.Range(0, width);
        int localY = Random.Range(0, height);
        int finalX = start.x + localX;
        int finalY = start.y + localY;

        return new Vector3(finalX + 0.5f, 0f, finalY + 0.5f);
    }

    bool IsBuildingAt(PathGen pathGen, Vector2Int pos)
    {
        return pathGen.PlacedBuildings.Exists(b => b.coordinate == pos);
    }

    GameObject GetRandomFromArray(GameObject[] array)
    {
        if (array == null || array.Length == 0) return null;
        int index = Random.Range(0, array.Length);
        return array[index];
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
