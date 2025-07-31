using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class PathGen : MonoBehaviour
{
    [Header("World Properties")]
    public int size = 20;
    [Range(1, 100)]
    public int density = 50;

    [Header("Prefab Properties")]
    public GameObject grassPrefab;
    public GameObject pathPrefab;
    public GameObject TopSide;
    public GameObject BottomSide;
    public GameObject LeftSide;
    public GameObject RightSide;

    [Header("Corner Prefabs")]
    public GameObject outerCornerTopLeft; // ┛
    public GameObject outerCornerTopRight; // ┗
    public GameObject outerCornerBottomLeft; // ┓
    public GameObject outerCornerBottomRight; // ┏
    public GameObject innerCornerTopLeft; // ┍
    public GameObject innerCornerTopRight; // ┑
    public GameObject innerCornerBottomLeft; // ┕
    public GameObject innerCornerBottomRight; // ┙


    [System.Serializable]
    public class BuildingOption
    {
        public GameObject prefab;
        [Range(0f, 0.5f)] public float spawnChance = 0.1f;
        public Vector2 footprintSize = Vector2.one; // Add this to specify building size
    }

    [Header("Building Prefabs")]
    public List<BuildingOption> housePrefabs; // Spawn hanya di dekat alley
    public List<BuildingOption> shopPrefabs; // Spawn di dekat alley dan main path
    public List<BuildingOption> minimarketPrefabs; // Spawn hanya di dekat main path

    [Header("Building Placement Settings")]
    [Range(1, 10)]
    public int minDistanceBetweenBuildings = 2;
    [Range(1, 10)]
    public int maxDistanceToPath = 3;

    

    private bool combineAfterGenerate = true;

    private int[,] grid;
    private List<List<Vector2Int>> mainPaths = new List<List<Vector2Int>>();
    private Transform tileParent;
    private Transform buildingParent;


    void Start()
    {
        GeneratePaths();
    }

    [ContextMenu("Generate (Editor Preview)")]
    void GenerateEditorPreview()
    {
        GeneratePaths();
    }

    public void GeneratePaths()
    {
        // Clear previous
        if (tileParent != null) DestroyImmediate(tileParent.gameObject);
        tileParent = new GameObject("Tiles").transform;
        tileParent.parent = this.transform;
        GameObject existingCombined = GameObject.Find("CombinedTiles");
        if (existingCombined != null)
        {
            if (existingCombined.GetComponent<MeshFilter>() != null &&
                existingCombined.GetComponent<MeshRenderer>() != null)
            {
                DestroyImmediate(existingCombined);
            }
        }

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

        DrawPrefabs();
        if (combineAfterGenerate) CombineAllTiles();
        SpawnBuildings();
        AddGroundPlaneWithNavMesh();
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

        for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size)
                {
                    grid[gx, gy] = 1;
                    pathPoints.Add(new Vector2Int(gx, gy));
                }
            }

        while (x != exitPosition.x)
        {
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;
                    if (gx >= 0 && gx < size && gy >= 0 && gy < size)
                    {
                        grid[gx, gy] = 1;
                        pathPoints.Add(new Vector2Int(gx, gy));
                    }
                }
            x += (exitPosition.x > x) ? 1 : -1;
        }

        while (y != exitPosition.y)
        {
            for (int dx = -2; dx <= 2; dx++)
                for (int dy = -2; dy <= 2; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;
                    if (gx >= 0 && gx < size && gy >= 0 && gy < size)
                    {
                        grid[gx, gy] = 1;
                        pathPoints.Add(new Vector2Int(gx, gy));
                    }
                }
            y += (exitPosition.y > y) ? 1 : -1;
        }

        for (int dx = -2; dx <= 2; dx++)
            for (int dy = -2; dy <= 2; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size)
                {
                    grid[gx, gy] = 1;
                    pathPoints.Add(new Vector2Int(gx, gy));
                }
            }
        return pathPoints;
    }

    void AddGroundPlaneWithNavMesh()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "NavGround";
        ground.transform.position = new Vector3(size / 2f, -0.21f, size / 2f);
        ground.transform.localScale = new Vector3(size / 10f, 1f, size / 10f); // Plane is 10x10 units by default
        ground.isStatic = true;

        // Add MeshCollider if not present
        MeshCollider collider = ground.GetComponent<MeshCollider>();
        if (collider == null)
            ground.AddComponent<MeshCollider>();

        // Add NavMeshSurface
        if (ground.GetComponent<NavMeshSurface>() == null)
        {
            var navSurface = ground.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.Children; // or All
            navSurface.BuildNavMesh();
        }
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

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size && grid[gx, gy] == 0)
                    grid[gx, gy] = 2;
            }

        while (x != exitPosition.x)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;
                    if (gx >= 0 && gx < size && gy >= 0 && gy < size && grid[gx, gy] == 0)
                        grid[gx, gy] = 2;
                }
            x += (exitPosition.x > x) ? 1 : -1;
        }

        while (y != exitPosition.y)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;
                    if (gx >= 0 && gx < size && gy >= 0 && gy < size && grid[gx, gy] == 0)
                        grid[gx, gy] = 2;
                }
            y += (exitPosition.y > y) ? 1 : -1;
        }

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size && grid[gx, gy] == 0)
                    grid[gx, gy] = 2;
            }
    }

    void DrawPrefabs()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Handle path tiles
                if (grid[x, y] == 1 || grid[x, y] == 2)
                {
                    // Check adjacent tiles (4-way)
                    bool top = IsPathPrefab(x, y + 1);
                    bool bottom = IsPathPrefab(x, y - 1);
                    bool left = IsPathPrefab(x - 1, y);
                    bool right = IsPathPrefab(x + 1, y);

                    // Check diagonal tiles for inner corners
                    bool topLeft = IsPathPrefab(x - 1, y + 1);
                    bool topRight = IsPathPrefab(x + 1, y + 1);
                    bool bottomLeft = IsPathPrefab(x - 1, y - 1);
                    bool bottomRight = IsPathPrefab(x + 1, y - 1);

                    GameObject prefab = pathPrefab; // Default to center path

                    // Straight edges
                    if (!top && bottom && left && right) prefab = TopSide;
                    else if (top && !bottom && left && right) prefab = BottomSide;
                    else if (top && bottom && !left && right) prefab = LeftSide;
                    else if (top && bottom && left && !right) prefab = RightSide;

                    // Outer corners (L-shapes)
                    else if (!top && !left && bottom && right) prefab = outerCornerTopLeft;
                    else if (!top && !right && bottom && left) prefab = outerCornerTopRight;
                    else if (!bottom && !left && top && right) prefab = outerCornerBottomLeft;
                    else if (!bottom && !right && top && left) prefab = outerCornerBottomRight;

                    // Inner corners (concave)
                    else if (top && left && !topLeft && right && bottom) prefab = innerCornerTopLeft;
                    else if (top && right && !topRight && left && bottom) prefab = innerCornerTopRight;
                    else if (bottom && left && !bottomLeft && top && right) prefab = innerCornerBottomLeft;
                    else if (bottom && right && !bottomRight && top && left) prefab = innerCornerBottomRight;

                    // Spawn the selected path prefab
                    if (prefab != null)
                    {
                        GameObject obj = Instantiate(prefab, new Vector3(x, -0.2f, y), Quaternion.identity, tileParent);
                        obj.name = $"Path_{x}_{y}_{prefab.name}";
                        obj.isStatic = true;
                    }
                }
                else // Handle grass tiles
                {
                    // Spawn grass prefab
                    GameObject grassObj = Instantiate(grassPrefab, new Vector3(x, -0.2f, y), Quaternion.identity, tileParent);
                    grassObj.name = $"Grass_{x}_{y}";
                    grassObj.isStatic = true;
                }
            }
        }
    }
    void CombineAllTiles()
    {
        MeshFilter[] meshFilters = tileParent.GetComponentsInChildren<MeshFilter>();
        Dictionary<Material, List<CombineInstance>> materialToMesh = new Dictionary<Material, List<CombineInstance>>();

        foreach (MeshFilter mf in meshFilters)
        {
            MeshRenderer renderer = mf.GetComponent<MeshRenderer>();
            if (mf.sharedMesh == null || renderer == null || renderer.sharedMaterial == null)
                continue;

            Material mat = renderer.sharedMaterial;

            if (!materialToMesh.ContainsKey(mat))
                materialToMesh[mat] = new List<CombineInstance>();

            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            materialToMesh[mat].Add(ci);
        }

        if (materialToMesh.Count == 0)
        {
            Debug.LogWarning("Tidak ada mesh/material yang valid untuk digabung.");
            return;
        }

        List<CombineInstance> finalCombine = new List<CombineInstance>();
        List<Material> materials = new List<Material>();
        List<Mesh> subMeshes = new List<Mesh>();

        foreach (var kvp in materialToMesh)
        {
            Mesh subMesh = new Mesh();
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
            subMesh.CombineMeshes(kvp.Value.ToArray(), true, true);
            subMeshes.Add(subMesh);

            CombineInstance finalCI = new CombineInstance
            {
                mesh = subMesh,
                transform = Matrix4x4.identity
            };

            finalCombine.Add(finalCI);
            materials.Add(kvp.Key);
        }

        GameObject combinedGO = new GameObject("CombinedTiles", typeof(MeshFilter), typeof(MeshRenderer));
        combinedGO.transform.SetParent(transform, false);

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMesh.CombineMeshes(finalCombine.ToArray(), false, false); // false untuk multi submesh

        combinedGO.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        combinedGO.GetComponent<MeshRenderer>().materials = materials.ToArray();

        DestroyImmediate(tileParent.gameObject);

        Debug.Log("Semua tile berhasil digabung dengan material masing-masing.");
    }



    bool IsPathPrefab(int x, int y)
    {
        if (x < 0 || x >= size || y < 0 || y >= size) return false;
        return grid[x, y] == 1 || grid[x, y] == 2;
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
                if (grid[x, y] != 0) continue; // must be on grass

                Vector2Int pos = new Vector2Int(x, y);

                // Skip if too close to other building
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

                // Check alley dan main path terdekat
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
            int maxSearch = maxDistanceToPath;
            for (int r = 1; r <= maxSearch; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue; // edge of square only
                        int nx = from.x + dx;
                        int ny = from.y + dy;
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                        {
                            if (grid[nx, ny] == targetType)
                                return r;
                        }
                    }
                }
            }
            return -1; // not found
        }
        void MarkBuildingFootprint(Vector2Int center, Vector2 buildingSize)
        {
            // Calculate half building dimensions
            int halfWidth = Mathf.FloorToInt(buildingSize.x / 2);
            int halfHeight = Mathf.FloorToInt(buildingSize.y / 2);

            // Calculate footprint bounds
            int startX = Mathf.Clamp(center.x - halfWidth, 0, size - 1);
            int endX = Mathf.Clamp(center.x + halfWidth, 0, size - 1);
            int startY = Mathf.Clamp(center.y - halfHeight, 0, size - 1);
            int endY = Mathf.Clamp(center.y + halfHeight, 0, size - 1);

            // Mark all tiles in footprint
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    placedBuildings.Add(new Vector2Int(x, y));
                }
            }
        }

        bool IsValidBuildingPosition(Vector2Int pos, Vector2 footprintSize, List<Vector2Int> placedBuildings)
        {
            // Hitung bounding box dari building yang mau di-spawn
            int halfWidth = Mathf.FloorToInt(footprintSize.x / 2f);
            int halfHeight = Mathf.FloorToInt(footprintSize.y / 2f);

            int newStartX = pos.x - halfWidth - minDistanceBetweenBuildings;
            int newEndX = pos.x + halfWidth + minDistanceBetweenBuildings;
            int newStartY = pos.y - halfHeight - minDistanceBetweenBuildings;
            int newEndY = pos.y + halfHeight + minDistanceBetweenBuildings;

            // Bandingkan dengan semua posisi tile bangunan yang sudah ada
            foreach (var placed in placedBuildings)
            {
                if (placed.x >= newStartX && placed.x <= newEndX &&
                    placed.y >= newStartY && placed.y <= newEndY)
                {
                    return false; // Overlap atau terlalu dekat
                }
            }

            // Check if area akan menabrak tile non-0 (path atau obstacle)
            int startX = Mathf.Max(0, pos.x - halfWidth);
            int endX = Mathf.Min(size - 1, pos.x + halfWidth);
            int startY = Mathf.Max(0, pos.y - halfHeight);
            int endY = Mathf.Min(size - 1, pos.y + halfHeight);

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (grid[x, y] != 0)
                        return false;
                }
            }

            return true;
        }

        bool TrySpawnBuilding(List<BuildingOption> prefabList, Vector2Int pos, int targetType)
        {
            foreach (var option in prefabList)
            {
                if (Random.value <= option.spawnChance)
                {
                    // Check if building would fit without overlapping paths
                    if (!IsValidBuildingPosition(pos, option.footprintSize, placedBuildings))
                        return false;

                    Vector3 worldPos = new Vector3(pos.x, 0f, pos.y);

                    // Find direction to path
                    Vector3Int dir = Vector3Int.forward;
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
                        if (nx >= 0 && nx < size && ny >= 0 && ny < size)
                        {
                            if (grid[nx, ny] == targetType)
                            {
                                dir = d;
                                break;
                            }
                        }
                    }

                    Quaternion rot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
                    GameObject instance = Instantiate(option.prefab, worldPos, rot, buildingParent);
                    instance.isStatic = true;
                    instance.name = $"Building_{option.prefab.name}_{pos.x}_{pos.y}";
                    
                    // Mark the building's footprint as occupied
                    MarkBuildingFootprint(pos, option.footprintSize);
                    return true;
                }
            }
            return false;
        }
        
    }

}
