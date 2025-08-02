using UnityEngine;
using System.Collections.Generic;
using System;

public class Rendering : MonoBehaviour
{
    public GameObject pathPrefab;
    public GameObject grassPrefab;
    public GameObject parkPrefab;

    public GameObject TopSide, BottomSide, LeftSide, RightSide;
    public GameObject outerCornerTopLeft, outerCornerTopRight, outerCornerBottomLeft, outerCornerBottomRight;
    public GameObject innerCornerTopLeft, innerCornerTopRight, innerCornerBottomLeft, innerCornerBottomRight;

    [HideInInspector] public Transform tileParent;
    [HideInInspector] public List<BuildingData> placedBuildings;
    

    public int renderDistance = 2;

    private int[,] Grid;
    private int size;
    private int chunkSize;
    private GameObject player;
    private Vector2Int lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);

    private Dictionary<Vector2Int, GameObject> chunkLoaded = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, List<BuildingData>> activeBuildings = new Dictionary<Vector2Int, List<BuildingData>>();
    private Dictionary<string, Queue<GameObject>> buildingPools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<GameObject, BuildingData> buildingToDataMap = new Dictionary<GameObject, BuildingData>(); 

    void Start()
    {
        PathGen pathGen = PathGen.Instance;

        if (pathGen == null)
        {
            Debug.LogError("PathGen.Instance not found!");
            return;
        }

        Grid = pathGen.Grid;
        chunkSize = pathGen.ChunkSize;
        size = pathGen.size;
        player = GameObject.Find("Player");
        player.transform.position = new Vector3(size / 2f, 0, size / 2f);
        placedBuildings = pathGen.PlacedBuildings;

        // Initialize building pools
        InitializeBuildingPools();

        Debug.Log("[Rendering]Tile at (5,5): " + Grid[5, 5]);
        Debug.Log("[Rendering] World size: " + size);
        Debug.Log("[Rendering] Placed Buildings Count: " + placedBuildings.Count);
    }

    void InitializeBuildingPools()
    {
        foreach (var building in placedBuildings)
        {
            if (!buildingPools.ContainsKey(building.prefabName))
            {
                buildingPools[building.prefabName] = new Queue<GameObject>();
            }
        }
    }

    void Update()
    {
        Vector2Int currentChunk = GetPlayerChunk(player.transform.position, chunkSize);
        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            RenderChunksAroundPlayer(currentChunk);
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

    void RenderChunksAroundPlayer(Vector2Int centerChunk)
    {
        HashSet<Vector2Int> newChunks = new HashSet<Vector2Int>();

        int minChunkX = Mathf.Max(0, centerChunk.x - renderDistance);
        int maxChunkX = Mathf.Min((size / chunkSize) - 1, centerChunk.x + renderDistance);
        int minChunkY = Mathf.Max(0, centerChunk.y - renderDistance);
        int maxChunkY = Mathf.Min((size / chunkSize) - 1, centerChunk.y + renderDistance);

        for (int x = minChunkX; x <= maxChunkX; x++)
        {
            for (int y = minChunkY; y <= maxChunkY; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                newChunks.Add(chunkCoord);

                if (!chunkLoaded.ContainsKey(chunkCoord))
                {
                    GameObject chunkParent = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
                    chunkParent.transform.parent = tileParent;
                    
                    // Draw terrain
                    DrawPrefabsInChunk(chunkCoord, chunkSize, Grid, chunkParent.transform);
                    
                    // Draw buildings
                    List<BuildingData> buildingsInChunk = GetBuildingsInChunk(chunkCoord);
                    RenderBuildingsInChunk(buildingsInChunk, chunkParent.transform);
                    
                    GameObject mesh = CombineChunkToMesh(chunkParent, chunkParent.name);
                    chunkLoaded[chunkCoord] = mesh;
                    activeBuildings[chunkCoord] = buildingsInChunk;
                }
                else
                {
                    chunkLoaded[chunkCoord].SetActive(true);
                    
                    // Reactivate buildings if needed
                    foreach (var building in activeBuildings[chunkCoord])
                    {
                        if (!building.isActive)
                        {
                            GetPooledBuilding(building);
                            building.isActive = true;
                        }
                    }
                }
            }
        }

        // Deactivate chunks outside range
        foreach (var kvp in chunkLoaded)
        {
            if (!newChunks.Contains(kvp.Key))
            {
                kvp.Value.SetActive(false);
                
                // Return buildings to pool
                foreach (var building in activeBuildings[kvp.Key])
                {
                    if (building.isActive)
                    {
                        ReturnBuildingToPool(building);
                        building.isActive = false;
                    }
                }
            }
        }
    }

    List<BuildingData> GetBuildingsInChunk(Vector2Int chunkCoord)
    {
        List<BuildingData> buildingsInChunk = new List<BuildingData>();
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;
        int endX = startX + chunkSize;
        int endY = startY + chunkSize;

        foreach (var building in placedBuildings)
        {
            if (building.coordinate.x >= startX && building.coordinate.x < endX &&
                building.coordinate.y >= startY && building.coordinate.y < endY)
            {
                buildingsInChunk.Add(building);
            }
        }

        return buildingsInChunk;
    }

    void RenderBuildingsInChunk(List<BuildingData> buildings, Transform parent)
    {
        foreach (var building in buildings)
        {
            if (!building.isDestroyed)
            {
                GetPooledBuilding(building);
                building.isRendered = true;
                building.isActive = true;
            }
        }
    }

    void GetPooledBuilding(BuildingData building)
    {
        if (!buildingPools.ContainsKey(building.prefabName))
        {
            buildingPools[building.prefabName] = new Queue<GameObject>();
        }

        GameObject buildingObj;
        if (buildingPools[building.prefabName].Count > 0)
        {
            buildingObj = buildingPools[building.prefabName].Dequeue();
            buildingObj.transform.position = building.worldPosition;
            buildingObj.transform.rotation = building.rotation;
            buildingObj.SetActive(true);
        }
        else
        {
            buildingObj = Instantiate(building.prefabUsed, building.worldPosition, building.rotation);
        }

        buildingObj.name = $"{building.prefabName}_{building.coordinate.x}_{building.coordinate.y}";
        buildingToDataMap[buildingObj] = building;
    }

    void ReturnBuildingToPool(BuildingData building)
    {
        // Find the building instance
        string buildingName = $"{building.prefabName}_{building.coordinate.x}_{building.coordinate.y}";
        GameObject buildingObj = GameObject.Find(buildingName);
        
        if (buildingObj != null)
        {
            buildingObj.SetActive(false);
            buildingPools[building.prefabName].Enqueue(buildingObj);
            buildingToDataMap.Remove(buildingObj);
        }
    }

    public void DrawPrefabsInChunk(Vector2Int chunkCoord, int chunkSize, int[,] grid, Transform parent)
    {
        int startX = chunkCoord.x * chunkSize;
        int startY = chunkCoord.y * chunkSize;

        for (int x = startX; x < startX + chunkSize; x++)
        {
            for (int y = startY; y < startY + chunkSize; y++)
            {
                if (x < 0 || x >= size || y < 0 || y >= size) continue;

                int tileType = grid[x, y];
                Vector3 pos = new Vector3(x, -0.2f, y);

                GameObject prefab = null;
                string name = "";

                if (tileType == 1 || tileType == 2)
                {
                    prefab = DeterminePathPrefab(x, y, grid, size);
                    name = $"Path_{x}_{y}_{prefab.name}";
                }
                else if (tileType == 3)
                {
                    prefab = parkPrefab;
                    name = $"Park_{x}_{y}";
                }
                else
                {
                    prefab = grassPrefab;
                    name = $"Grass_{x}_{y}";
                }

                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, pos, Quaternion.identity, parent);
                    obj.name = name;
                    obj.isStatic = true;
                }
            }
        }
    }

    GameObject DeterminePathPrefab(int x, int y, int[,] grid, int size)
    {
        bool top = IsPathPrefab(x, y + 1, grid, size);
        bool bottom = IsPathPrefab(x, y - 1, grid, size);
        bool left = IsPathPrefab(x - 1, y, grid, size);
        bool right = IsPathPrefab(x + 1, y, grid, size);
        bool topLeft = IsPathPrefab(x - 1, y + 1, grid, size);
        bool topRight = IsPathPrefab(x + 1, y + 1, grid, size);
        bool bottomLeft = IsPathPrefab(x - 1, y - 1, grid, size);
        bool bottomRight = IsPathPrefab(x + 1, y - 1, grid, size);

        if (!top && bottom && left && right) return TopSide;
        else if (top && !bottom && left && right) return BottomSide;
        else if (top && bottom && !left && right) return LeftSide;
        else if (top && bottom && left && !right) return RightSide;
        else if (!top && !left && bottom && right) return outerCornerTopLeft;
        else if (!top && !right && bottom && left) return outerCornerTopRight;
        else if (!bottom && !left && top && right) return outerCornerBottomLeft;
        else if (!bottom && !right && top && left) return outerCornerBottomRight;
        else if (top && left && !topLeft && right && bottom) return innerCornerTopLeft;
        else if (top && right && !topRight && left && bottom) return innerCornerTopRight;
        else if (bottom && left && !bottomLeft && top && right) return innerCornerBottomLeft;
        else if (bottom && right && !bottomRight && top && left) return innerCornerBottomRight;

        return pathPrefab;
    }

    bool IsPathPrefab(int x, int y, int[,] grid, int size)
    {
        if (x < 0 || x >= size || y < 0 || y >= size) return false;
        return grid[x, y] == 1 || grid[x, y] == 2;
    }

    public GameObject CombineChunkToMesh(GameObject chunkParent, string chunkName)
    {
        Dictionary<Material, List<CombineInstance>> materialToMesh = new Dictionary<Material, List<CombineInstance>>();

        foreach (Transform tile in chunkParent.transform)
        {
            MeshFilter mf = tile.GetComponent<MeshFilter>();
            MeshRenderer mr = tile.GetComponent<MeshRenderer>();

            if (mf == null || mr == null || mf.sharedMesh == null || mr.sharedMaterial == null)
                continue;

            Material mat = mr.sharedMaterial;
            if (!materialToMesh.ContainsKey(mat))
                materialToMesh[mat] = new List<CombineInstance>();

            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            materialToMesh[mat].Add(ci);
        }

        if (materialToMesh.Count == 0) return null;

        List<CombineInstance> finalCombine = new List<CombineInstance>();
        List<Material> materials = new List<Material>();

        foreach (var kvp in materialToMesh)
        {
            Mesh subMesh = new Mesh();
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);

            CombineInstance finalCI = new CombineInstance
            {
                mesh = subMesh,
                transform = Matrix4x4.identity
            };

            finalCombine.Add(finalCI);
            materials.Add(kvp.Key);
        }

        GameObject combinedGO = new GameObject(chunkName, typeof(MeshFilter), typeof(MeshRenderer));
        combinedGO.transform.position = Vector3.zero;

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(finalCombine.ToArray(), false, false);

        combinedGO.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        combinedGO.GetComponent<MeshRenderer>().materials = materials.ToArray();
        combinedGO.AddComponent<MeshCollider>().sharedMesh = finalMesh;

        combinedGO.isStatic = true;
        chunkParent.SetActive(false);

        return combinedGO;
    }
}