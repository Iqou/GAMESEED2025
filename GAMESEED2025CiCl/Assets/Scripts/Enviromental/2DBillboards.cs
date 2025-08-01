using UnityEngine;
using System.Collections.Generic;

public class BillboardSpawner : MonoBehaviour
{
    [System.Serializable]
    public class BillboardOption
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float spawnChance = 0.5f;
        public bool reverseFacing = false;
    }

    public List<BillboardOption> billboardPrefabs = new List<BillboardOption>();
    public int grassValue = 0;
    public float tileSize = 1f;
    public Transform billboardContainer;
    public float positionVariance = 0.3f;
    public float minDistanceFromBuildings = 1f;
    public Camera targetCamera;

    private int[,] grid;
    private List<BuildingData> placedBuildings;
    private List<BillboardData> spawnedBillboards = new List<BillboardData>();

    private class BillboardData
    {
        public GameObject billboard;
        public bool reverseFacing;
    }

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        PathGen pathGen = PathGen.Instance;
        grid = pathGen.Grid;
        placedBuildings = pathGen.PlacedBuildings;

        if (billboardContainer == null)
        {
            billboardContainer = new GameObject("BillboardContainer").transform;
        }

        SpawnBillboards();
    }

    void SpawnBillboards()
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == grassValue)
                {
                    TrySpawnBillboard(x, y);
                }
            }
        }
    }

    void TrySpawnBillboard(int x, int y)
    {
        Vector3 proposedPosition = new Vector3(x * tileSize, 0, y * tileSize);
        if (IsTooCloseToBuilding(proposedPosition))
        {
            return;
        }

        foreach (var option in billboardPrefabs)
        {
            if (option.prefab != null && Random.value <= option.spawnChance)
            {
                SpawnSingleBillboard(x, y, option);
                break;
            }
        }
    }

    bool IsTooCloseToBuilding(Vector3 position)
    {
        foreach (BuildingData building in placedBuildings)
        {
            if (Vector3.Distance(position, building.worldPosition) < minDistanceFromBuildings)
            {
                return true;
            }
        }
        return false;
    }

    void SpawnSingleBillboard(int x, int y, BillboardOption option)
    {
        Vector3 pos = new Vector3(
            x * tileSize + Random.Range(-positionVariance, positionVariance),
            0,
            y * tileSize + Random.Range(-positionVariance, positionVariance)
        );

        if (IsTooCloseToBuilding(pos))
        {
            return;
        }

        GameObject obj = Instantiate(option.prefab, pos, Quaternion.identity, billboardContainer);
        
        spawnedBillboards.Add(new BillboardData {
            billboard = obj,
            reverseFacing = option.reverseFacing
        });
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        foreach (var billboardData in spawnedBillboards)
        {
            if (billboardData.billboard != null && billboardData.billboard.activeInHierarchy)
            {
                UpdateBillboarding(billboardData.billboard.transform, billboardData.reverseFacing);
            }
        }
    }

    void UpdateBillboarding(Transform billboardTransform, bool reverseFacing)
    {
        Vector3 direction = reverseFacing ?
            (targetCamera.transform.position - billboardTransform.position) :
            (billboardTransform.position - targetCamera.transform.position);
            
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            billboardTransform.rotation = Quaternion.LookRotation(direction);
        }
    }
}