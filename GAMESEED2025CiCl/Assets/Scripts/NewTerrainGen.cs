using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class NewTerrainGen : MonoBehaviour
{
    [Header("World Properties")]
    public int size = 20;
    [Range(1, 100)]
    public int density = 50;

    [Header("Prefab Properties")]
    public GameObject pathPrefab;
    public GameObject TopSide;
    public GameObject BottomSide;
    public GameObject LeftSide;
    public GameObject RightSide;

    [System.Serializable]
    public class BuildingOption
    {
        public GameObject prefab;
        [Range(0f, 0.5f)] public float spawnChance = 0.1f;
    }

    [Header("Building Prefabs")]
    public List<BuildingOption> housePrefabs;
    public List<BuildingOption> shopPrefabs;
    public List<BuildingOption> minimarketPrefabs;

    [Header("Building Placement Settings")]
    [Range(1, 10)]
    public int minDistanceBetweenBuildings = 2;
    [Range(1, 10)]
    public int maxDistanceToPath = 3;

    [Header("Terrain Settings")]
    public float maxTerrainHeight = 20f;
    public float baseHeight = 0.1f;
    public float pathHeight = 0.15f;
    public float terrainNoiseScale = 0.1f;
    public float terrainNoiseAmount = 0.05f;
    public Material terrainMaterial;

    private int[,] grid;
    private List<List<Vector2Int>> mainPaths = new List<List<Vector2Int>>();
    private Transform buildingParent;
    private Terrain terrain;
    private TerrainData terrainData;
    private float[,] heightmap;

    void Start()
    {
        GenerateTerrainWorld();
    }

    [ContextMenu("Generate (Editor Preview)")]
    void GenerateEditorPreview()
    {
        GenerateTerrainWorld();
    }

    public void GenerateTerrainWorld()
    {
        ClearPrevious();
        InitializeTerrain();
        GeneratePathData();
        ApplyTerrainFeatures();
        SpawnBuildings();
    }

    void ClearPrevious()
    {
        if (buildingParent != null) DestroyImmediate(buildingParent.gameObject);
        buildingParent = new GameObject("Buildings").transform;
        buildingParent.parent = this.transform;

        Terrain existingTerrain = GetComponentInChildren<Terrain>();
        if (existingTerrain != null)
        {
            DestroyImmediate(existingTerrain.gameObject);
        }
    }

    void InitializeTerrain()
    {
        GameObject terrainObj = new GameObject("GeneratedTerrain");
        terrainObj.transform.parent = this.transform;
        terrainObj.transform.position = Vector3.zero;

        terrain = terrainObj.AddComponent<Terrain>();
        TerrainCollider terrainCollider = terrainObj.AddComponent<TerrainCollider>();

        terrainData = new TerrainData();
        terrainData.size = new Vector3(size, maxTerrainHeight, size);
        terrainData.heightmapResolution = size * 2;
        terrainData.baseMapResolution = 1024;
        terrainData.SetDetailResolution(1024, 32);

        heightmap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        
        terrain.terrainData = terrainData;
        terrainCollider.terrainData = terrainData;
        terrain.materialTemplate = terrainMaterial != null ? terrainMaterial : 
            Terrain.activeTerrain?.materialTemplate ?? new Material(Shader.Find("Nature/Terrain/Standard"));
    }

    void GeneratePathData()
    {
        grid = new int[size, size];
        mainPaths.Clear();

        int mainAmount = Mathf.Max(1, Mathf.RoundToInt((float)size * density / 100f * 0.2f));
        int alleyAmount = mainAmount * 5;

        List<Vector2Int> entryList = new List<Vector2Int>();
        List<Vector2Int> exitList = new List<Vector2Int>();

        for (int i = 0; i < mainAmount; i++)
        {
            var pathPoints = PathmakerMain(entryList, exitList);
            mainPaths.Add(pathPoints);
        }

        for (int i = 0; i < alleyAmount; i++)
        {
            PathmakerAlley();
        }
    }

    List<Vector2Int> PathmakerMain(List<Vector2Int> entryList, List<Vector2Int> exitList)
    {
        int side = Random.Range(0, 4);
        Vector2Int entryPosition = Vector2Int.zero;
        Vector2Int exitPosition = Vector2Int.zero;
        bool valid;
        int maxTry = 100;
        int tryCount = 0;

        do
        {
            switch (side)
            {
                case 0:
                    entryPosition = new Vector2Int(Random.Range(0, size), 0);
                    exitPosition = new Vector2Int(Random.Range(0, size), size - 1);
                    break;
                case 1:
                    entryPosition = new Vector2Int(0, Random.Range(0, size));
                    exitPosition = new Vector2Int(size - 1, Random.Range(0, size));
                    break;
                case 2:
                    entryPosition = new Vector2Int(Random.Range(0, size), size - 1);
                    exitPosition = new Vector2Int(0, Random.Range(0, size));
                    break;
                case 3:
                    entryPosition = new Vector2Int(size - 1, Random.Range(0, size));
                    exitPosition = new Vector2Int(Random.Range(0, size), 0);
                    break;
            }

            valid = true;
            foreach (var e in entryList)
                if (Vector2Int.Distance(entryPosition, e) < 4) valid = false;
            foreach (var ex in exitList)
                if (Vector2Int.Distance(exitPosition, ex) < 4) valid = false;

            tryCount++;
            if (tryCount > maxTry) break;
        }
        while (!valid);

        entryList.Add(entryPosition);
        exitList.Add(exitPosition);

        int x = entryPosition.x;
        int y = entryPosition.y;
        List<Vector2Int> pathPoints = new List<Vector2Int>();

        MarkGridArea(x, y, 2, 1, pathPoints);

        while (x != exitPosition.x)
        {
            MarkGridArea(x, y, 2, 1, pathPoints);
            x += (exitPosition.x > x) ? 1 : -1;
        }

        while (y != exitPosition.y)
        {
            MarkGridArea(x, y, 2, 1, pathPoints);
            y += (exitPosition.y > y) ? 1 : -1;
        }

        MarkGridArea(x, y, 2, 1, pathPoints);

        return pathPoints;
    }

    void PathmakerAlley()
    {
        if (mainPaths.Count < 2) return;

        int fromIdx = Random.Range(0, mainPaths.Count);
        int toIdx = Random.Range(0, mainPaths.Count);
        while (toIdx == fromIdx) toIdx = Random.Range(0, mainPaths.Count);

        var fromMain = mainPaths[fromIdx];
        var toMain = mainPaths[toIdx];

        Vector2Int entryPosition = fromMain[Random.Range(0, fromMain.Count)];
        Vector2Int exitPosition = toMain[Random.Range(0, toMain.Count)];

        int x = entryPosition.x;
        int y = entryPosition.y;

        MarkGridArea(x, y, 1, 2);

        while (x != exitPosition.x)
        {
            MarkGridArea(x, y, 1, 2);
            x += (exitPosition.x > x) ? 1 : -1;
        }

        while (y != exitPosition.y)
        {
            MarkGridArea(x, y, 1, 2);
            y += (exitPosition.y > y) ? 1 : -1;
        }

        MarkGridArea(x, y, 1, 2);
    }

    void MarkGridArea(int centerX, int centerY, int radius, int value, List<Vector2Int> pathPoints = null)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int gx = centerX + dx;
                int gy = centerY + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size)
                {
                    grid[gx, gy] = value;
                    if (pathPoints != null)
                    {
                        pathPoints.Add(new Vector2Int(gx, gy));
                    }
                }
            }
        }
    }

    void ApplyTerrainFeatures()
    {
        // Base terrain with noise
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                float noise = Mathf.PerlinNoise(x * terrainNoiseScale, y * terrainNoiseScale) * terrainNoiseAmount;
                heightmap[x, y] = baseHeight + noise;
            }
        }

        // Apply paths
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (grid[x, y] == 1 || grid[x, y] == 2)
                {
                    int hmX = Mathf.RoundToInt((float)x / size * (terrainData.heightmapResolution - 1));
                    int hmY = Mathf.RoundToInt((float)y / size * (terrainData.heightmapResolution - 1));

                    // Set path height with blending
                    int blendRadius = 3;
                    for (int bx = -blendRadius; bx <= blendRadius; bx++)
                    {
                        for (int by = -blendRadius; by <= blendRadius; by++)
                        {
                            int px = Mathf.Clamp(hmX + bx, 0, terrainData.heightmapResolution - 1);
                            int py = Mathf.Clamp(hmY + by, 0, terrainData.heightmapResolution - 1);
                            
                            float dist = Mathf.Sqrt(bx * bx + by * by);
                            float blendFactor = 1f - Mathf.Clamp01(dist / blendRadius);
                            heightmap[px, py] = Mathf.Lerp(heightmap[px, py], pathHeight, blendFactor);
                        }
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heightmap);

        // Apply textures using TerrainLayer system
        TerrainLayer[] terrainLayers = new TerrainLayer[2];

        // Grass layer
        terrainLayers[0] = new TerrainLayer();
        terrainLayers[0].diffuseTexture = Resources.Load<Texture2D>("TerrainTextures/Grass");
        terrainLayers[0].tileSize = new Vector2(15, 15); // Adjust texture tiling as needed

        // Path layer
        terrainLayers[1] = new TerrainLayer();
        terrainLayers[1].diffuseTexture = Resources.Load<Texture2D>("TerrainTextures/Path");
        terrainLayers[1].tileSize = new Vector2(10, 10); // Path textures typically need smaller tiling

        terrainData.terrainLayers = terrainLayers;

        // Create alphamap (now called splatmap)
        float[,,] splatmapData = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, terrainLayers.Length];

        for (int y = 0; y < terrainData.alphamapResolution; y++)
        {
            for (int x = 0; x < terrainData.alphamapResolution; x++)
            {
                // Normalize coordinates
                float normX = (float)x / (terrainData.alphamapResolution - 1);
                float normY = (float)y / (terrainData.alphamapResolution - 1);
                
                // Convert to grid coordinates
                int gridX = Mathf.RoundToInt(normX * size);
                int gridY = Mathf.RoundToInt(normY * size);
                
                if (gridX >= 0 && gridX < size && gridY >= 0 && gridY < size)
                {
                    if (grid[gridX, gridY] == 1 || grid[gridX, gridY] == 2)
                    {
                        // Path texture (layer 1)
                        splatmapData[x, y, 1] = 1f;
                        splatmapData[x, y, 0] = 0f;
                    }
                    else
                    {
                        // Grass texture (layer 0)
                        splatmapData[x, y, 0] = 1f;
                        splatmapData[x, y, 1] = 0f;
                    }
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
        
        for (int y = 0; y < terrainData.alphamapResolution; y++)
        {
            for (int x = 0; x < terrainData.alphamapResolution; x++)
            {
                float normX = (float)x / (terrainData.alphamapResolution - 1);
                float normY = (float)y / (terrainData.alphamapResolution - 1);
                
                int gridX = Mathf.RoundToInt(normX * size);
                int gridY = Mathf.RoundToInt(normY * size);
                
                if (gridX >= 0 && gridX < size && gridY >= 0 && gridY < size)
                {
                    if (grid[gridX, gridY] == 1 || grid[gridX, gridY] == 2)
                    {
                        splatmapData[x, y, 1] = 1f; // Path texture
                        splatmapData[x, y, 0] = 0f;
                    }
                    else
                    {
                        splatmapData[x, y, 0] = 1f; // Grass texture
                        splatmapData[x, y, 1] = 0f;
                    }
                }
            }
        }
        
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void SpawnBuildings()
    {
        if (buildingParent != null) DestroyImmediate(buildingParent.gameObject);
        buildingParent = new GameObject("Buildings").transform;
        buildingParent.parent = this.transform;

        List<Vector2Int> placedBuildings = new List<Vector2Int>();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (grid[x, y] != 0) continue;

                Vector2Int pos = new Vector2Int(x, y);

                bool tooClose = false;
                foreach (var placed in placedBuildings)
                {
                    if (Vector2Int.Distance(pos, placed) < minDistanceBetweenBuildings)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                int distToAlley = FindNearestDistance(pos, 2);
                int distToMain = FindNearestDistance(pos, 1);

                bool nearAlley = distToAlley > 0 && distToAlley <= maxDistanceToPath;
                bool nearMain = distToMain > 0 && distToMain <= maxDistanceToPath;

                if (nearAlley && TrySpawnBuilding(housePrefabs, pos, 2))
                {
                    placedBuildings.Add(pos);
                    continue;
                }

                if ((nearAlley || nearMain) && TrySpawnBuilding(shopPrefabs, pos, nearAlley ? 2 : 1))
                {
                    placedBuildings.Add(pos);
                    continue;
                }

                if (nearMain && !nearAlley && TrySpawnBuilding(minimarketPrefabs, pos, 1))
                {
                    placedBuildings.Add(pos);
                    continue;
                }
            }
        }

        int FindNearestDistance(Vector2Int from, int targetType)
        {
            for (int r = 1; r <= maxDistanceToPath; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    int dy = r - Mathf.Abs(dx);
                    if (CheckPosition(from.x + dx, from.y + dy, targetType) || 
                        CheckPosition(from.x + dx, from.y - dy, targetType))
                    {
                        return r;
                    }
                }
            }
            return -1;
        }

        bool CheckPosition(int x, int y, int targetType)
        {
            return x >= 0 && x < size && y >= 0 && y < size && grid[x, y] == targetType;
        }

        bool TrySpawnBuilding(List<BuildingOption> prefabList, Vector2Int pos, int targetType)
        {
            foreach (var option in prefabList)
            {
                if (Random.value <= option.spawnChance)
                {
                    // Get terrain height at this position
                    float terrainHeight = terrain.SampleHeight(new Vector3(pos.x, 0, pos.y));
                    
                    Vector3 worldPos = new Vector3(pos.x, terrainHeight, pos.y);
                    Vector3Int dir = FindDirectionToPath(pos, targetType);
                    Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
                    
                    GameObject instance = Instantiate(option.prefab, worldPos, rot, buildingParent);
                    instance.isStatic = true;
                    instance.name = $"Building_{option.prefab.name}_{pos.x}_{pos.y}";
                    return true;
                }
            }
            return false;
        }

        Vector3Int FindDirectionToPath(Vector2Int pos, int targetType)
        {
            Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),
                new Vector3Int(-1, 0, 0),
                new Vector3Int(0, 0, 1),
                new Vector3Int(0, 0, -1)
            };

            foreach (var d in directions)
            {
                int nx = pos.x + d.x;
                int ny = pos.y + d.z;
                if (nx >= 0 && nx < size && ny >= 0 && ny < size && grid[nx, ny] == targetType)
                {
                    return d;
                }
            }
            return Vector3Int.forward;
        }
    }
}