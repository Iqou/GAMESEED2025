using UnityEngine;
using System.Collections.Generic;

public class BillboardRendering : MonoBehaviour
{
    [HideInInspector] public List<BuildingData> placedBillboards;
    public int renderDistance = 2;
    
    private int chunkSize;
    private int size;
    private GameObject player;
    private Vector2Int lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
    
    private Dictionary<Vector2Int, List<BuildingData>> activeBillboards = new Dictionary<Vector2Int, List<BuildingData>>();
    private Dictionary<string, Queue<GameObject>> billboardPools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<GameObject, BuildingData> billboardToDataMap = new Dictionary<GameObject, BuildingData>();

    void Start()
    {
        PathGen pathGen = PathGen.Instance;

        if (pathGen == null)
        {
            Debug.LogError("PathGen.Instance not found!");
            return;
        }

        chunkSize = pathGen.ChunkSize;
        size = pathGen.size;
        player = GameObject.Find("Player");
        placedBillboards = pathGen.PlacedBillboards;

        InitializeBillboardPools();
        Debug.Log("[BillboardRendering] Placed Billboards Count: " + placedBillboards.Count);
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            if (Camera.main.orthographic)
            {
                Debug.Log("Camera is in Orthographic mode");
                foreach (var kvp in billboardToDataMap)
                {
                    GameObject billboardObj = kvp.Key;
                    if (billboardObj.activeInHierarchy)
                    {
                        billboardObj.transform.rotation = Quaternion.Euler(80f, 0, 0);
                    }
                }
                
            }
            else
            {
                Debug.Log("Camera is in Perspective mode");
                Quaternion tiltRotation = Quaternion.Euler(0, 0, 0);

                foreach (var kvp in billboardToDataMap)
                {
                    GameObject billboardObj = kvp.Key;
                    if (billboardObj.activeInHierarchy)
                    {
                        Vector3 lookDirection = Camera.main.transform.position - billboardObj.transform.position;
                        lookDirection.y = 0;

                        if (lookDirection != Vector3.zero)
                        {
                            Quaternion faceRotation = Quaternion.LookRotation(-lookDirection);
                            billboardObj.transform.rotation = faceRotation * tiltRotation;
                        }
                    }
                }
            }
        }
        
        
    }
    void InitializeBillboardPools()
    {
        foreach (var billboard in placedBillboards)
        {
            if (!billboardPools.ContainsKey(billboard.prefabName))
            {
                billboardPools[billboard.prefabName] = new Queue<GameObject>();
            }
        }
    }

    void Update()
    {
        Vector2Int currentChunk = GetPlayerChunk(player.transform.position, chunkSize);
        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            RenderBillboardsAroundPlayer(currentChunk);
        }
    }

    Vector2Int GetPlayerChunk(Vector3 position, int chunkSize)
    {
        float clampedX = Mathf.Clamp(position.x, 0, size - 1);
        float clampedZ = Mathf.Clamp(position.z, 0, size - 1);

        return new Vector2Int(
            Mathf.FloorToInt(clampedX / chunkSize),
            Mathf.FloorToInt(clampedZ / chunkSize)
        );
    }

    void RenderBillboardsAroundPlayer(Vector2Int centerChunk)
    {
        HashSet<Vector2Int> newChunks = new HashSet<Vector2Int>();

        int minChunkX = Mathf.Max(0, centerChunk.x - renderDistance);
        int maxChunkX = Mathf.Min((size / chunkSize) - 1, centerChunk.x + renderDistance);
        int minChunkY = Mathf.Max(0, centerChunk.y - renderDistance);
        int maxChunkY = Mathf.Min((size / chunkSize) - 1, centerChunk.y + renderDistance);

        // Activate billboards in nearby chunks
        for (int x = minChunkX; x <= maxChunkX; x++)
        {
            for (int y = minChunkY; y <= maxChunkY; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                newChunks.Add(chunkCoord);

                if (!activeBillboards.ContainsKey(chunkCoord))
                {
                    List<BuildingData> billboardsInChunk = GetBillboardsInChunk(chunkCoord);
                    activeBillboards[chunkCoord] = billboardsInChunk;
                    RenderBillboardsInChunk(billboardsInChunk);
                }
                else
                {
                    // Reactivate billboards if needed
                    foreach (var billboard in activeBillboards[chunkCoord])
                    {
                        if (!billboard.isActive)
                        {
                            GetPooledBillboard(billboard);
                            billboard.isActive = true;
                        }
                    }
                }
            }
        }

        // Deactivate billboards outside range
        foreach (var kvp in activeBillboards)
        {
            if (!newChunks.Contains(kvp.Key))
            {
                foreach (var billboard in kvp.Value)
                {
                    if (billboard.isActive)
                    {
                        ReturnBillboardToPool(billboard);
                        billboard.isActive = false;
                    }
                }
            }
        }
    }

    List<BuildingData> GetBillboardsInChunk(Vector2Int chunkCoord)
    {
        List<BuildingData> billboardsInChunk = new List<BuildingData>();
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;
        int endX = startX + chunkSize;
        int endY = startY + chunkSize;

        foreach (var billboard in placedBillboards)
        {
            if (billboard.coordinate.x >= startX && billboard.coordinate.x < endX &&
                billboard.coordinate.y >= startY && billboard.coordinate.y < endY)
            {
                billboardsInChunk.Add(billboard);
            }
        }

        return billboardsInChunk;
    }

    void RenderBillboardsInChunk(List<BuildingData> billboards)
    {
        foreach (var billboard in billboards)
        {
            if (!billboard.isDestroyed && !billboard.isActive)
            {
                GetPooledBillboard(billboard);
                billboard.isRendered = true;
                billboard.isActive = true;
            }
        }
    }

    void GetPooledBillboard(BuildingData billboard)
    {
        if (!billboardPools.ContainsKey(billboard.prefabName))
        {
            billboardPools[billboard.prefabName] = new Queue<GameObject>();
        }

        GameObject billboardObj;
        if (billboardPools[billboard.prefabName].Count > 0)
        {
            billboardObj = billboardPools[billboard.prefabName].Dequeue();
            billboardObj.transform.position = billboard.worldPosition;
            billboardObj.transform.rotation = billboard.rotation;
            billboardObj.SetActive(true);
        }
        else
        {
            billboardObj = Instantiate(billboard.prefabUsed, billboard.worldPosition, billboard.rotation);
        }

        billboardObj.name = $"{billboard.prefabName}_Billboard_{billboard.coordinate.x}_{billboard.coordinate.y}";
        billboardToDataMap[billboardObj] = billboard;
    }

    void ReturnBillboardToPool(BuildingData billboard)
    {
        string billboardName = $"{billboard.prefabName}_Billboard_{billboard.coordinate.x}_{billboard.coordinate.y}";
        GameObject billboardObj = GameObject.Find(billboardName);
        
        if (billboardObj != null)
        {
            billboardObj.SetActive(false);
            billboardPools[billboard.prefabName].Enqueue(billboardObj);
            billboardToDataMap.Remove(billboardObj);
        }
    }

    // Optional: Add methods to handle billboard interaction
    public void OnBillboardClicked(GameObject billboardObj)
    {
        if (billboardToDataMap.TryGetValue(billboardObj, out BuildingData billboardData))
        {
            Debug.Log($"Billboard clicked at {billboardData.coordinate}");
            // Handle billboard interaction here
        }
    }
}