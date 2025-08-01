using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;


[DefaultExecutionOrder(-100)]
public class PathGen : MonoBehaviour
{
    [Header("World Properties")]
    public int WorldChunks = 40;
    [HideInInspector]public int ChunkSize = 5;
    private int BorderWidth => 3 * ChunkSize;
    public int size => WorldChunks * ChunkSize;
    [Range(1, 100)]
    public int density = 50;



    [Header("Building Prefabs")]
    public List<BuildingOption> Type1;
    public List<BuildingOption> Type2;
    public List<BuildingOption> Type3;

    [Header("Building Placement Settings")]
    [Range(1, 10)]
    public int minDistanceBetweenBuildings = 2;
    [Range(1, 10)]
    public int maxDistanceToPath = 3;

    [Range(0f, 1f)]
    public float parkSpawnChance = 0.3f;
    public class ParkData
    {
        public Vector2Int startPosition;
        public int width;
        public int height;

        public ParkData(Vector2Int pos, int w, int h)
        {
            startPosition = pos;
            width = w;
            height = h;
        }
    }

    [HideInInspector]
    public List<ParkData> PlacedParks = new List<ParkData>();
    [HideInInspector]
    public List<BuildingData> PlacedBuildings = new List<BuildingData>();
    [HideInInspector]
    public int[,] Grid;
    
    private List<List<Vector2Int>> mainPaths = new List<List<Vector2Int>>();
    private Transform tileParent;
    private Transform buildingParent;

    public static PathGen Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

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
        tileParent.localPosition = Vector3.zero;

        GameObject existingCombined = GameObject.Find("CombinedWorld");
        if (existingCombined != null) DestroyImmediate(existingCombined);
        GameObject existingBuildings = GameObject.Find("Buildings");
        if (existingBuildings != null) DestroyImmediate(existingBuildings);
        GameObject navMesh = GameObject.Find("NavGround");
        if (navMesh != null) DestroyImmediate(navMesh);

        // 1. Generate Grid Data
        Grid = new int[size, size];
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

        Parkmaker();
        WorldBorderMaker();
        SpawnBuildings();
        AddGroundPlaneWithNavMesh();
    }


    void WorldBorderMaker()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (x < BorderWidth || x >= size - BorderWidth ||
                    y < BorderWidth || y >= size - BorderWidth)
                {
                    Grid[x, y] = 0;
                }
            }
        }
    }


    void Parkmaker()
    {
        int parkWidth = 4;
        int parkHeight = 3;
        PlacedParks.Clear();

        for (int x = BorderWidth; x <= size - BorderWidth - parkWidth; x++)
        {
            for (int y = BorderWidth; y <= size - BorderWidth - parkHeight; y++)
            {
                if (Random.value > parkSpawnChance) continue;

                if (CanPlaceParkAt(x, y, parkWidth, parkHeight))
                {
                    MarkParkArea(x, y, parkWidth, parkHeight);
                    PlacedParks.Add(new ParkData(new Vector2Int(x, y), parkWidth, parkHeight));
                    y += parkHeight - 1;
                }
            }
        }
    }
    bool CanPlaceParkAt(int startX, int startY, int width, int height)
    {
        // Minimal distance check
        foreach (var park in PlacedParks)
        {
            float distance = Vector2Int.Distance(new Vector2Int(startX, startY), park.startPosition);
            if (distance < 30f)
                return false;
        }

        if (!IsInsideBorderArea(startX, startY, width, height))
            return false;

        for (int x = startX - 1; x <= startX + width; x++)
        {
            for (int y = startY - 1; y <= startY + height; y++)
            {
                if (x < 0 || x >= size || y < 0 || y >= size)
                    continue;

                int tile = Grid[x, y];

                if ((x >= startX && x < startX + width && y >= startY && y < startY + height))
                {
                    if (tile != 0) return false; // hanya boleh grass
                }
                else
                {
                    if (tile == 1 || tile == 2) return false; // tidak boleh langsung bersisian dengan path
                }

                if (PlacedBuildings.Exists(b => b.coordinate == new Vector2Int(x, y)))
                    return false;
            }
        }

        return true;
    }



    bool IsInsideBorderArea(int x, int y, int width = 1, int height = 1)
    {
        return x >= BorderWidth &&
            y >= BorderWidth &&
            x + width <= size - BorderWidth &&
            y + height <= size - BorderWidth;
    }

    void MarkParkArea(int startX, int startY, int width, int height)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                Grid[x, y] = 3;
            }
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
                    entryPosition = new Vector2Int(Random.Range(BorderWidth, size - BorderWidth), BorderWidth);
                    exitPosition = new Vector2Int(Random.Range(BorderWidth, size - BorderWidth), size - BorderWidth - 1);
                    break;
                case 1:
                    entryPosition = new Vector2Int(BorderWidth, Random.Range(BorderWidth, size - BorderWidth));
                    exitPosition = new Vector2Int(size - BorderWidth - 1, Random.Range(BorderWidth, size - BorderWidth));
                    break;
                case 2:
                    entryPosition = new Vector2Int(Random.Range(BorderWidth, size - BorderWidth), size - BorderWidth - 1);
                    exitPosition = new Vector2Int(Random.Range(BorderWidth, size - BorderWidth), BorderWidth);
                    break;
                case 3:
                    entryPosition = new Vector2Int(size - BorderWidth - 1, Random.Range(BorderWidth, size - BorderWidth));
                    exitPosition = new Vector2Int(BorderWidth, Random.Range(BorderWidth, size - BorderWidth));
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
        List<Vector2Int> pathPoints = new();

        while (x != exitPosition.x)
        {
            DrawMainPathCell(x, y, pathPoints);
            x += (exitPosition.x > x) ? 1 : -1;
        }

        while (y != exitPosition.y)
        {
            DrawMainPathCell(x, y, pathPoints);
            y += (exitPosition.y > y) ? 1 : -1;
        }

        DrawMainPathCell(x, y, pathPoints);
        return pathPoints;
    }

    void DrawMainPathCell(int x, int y, List<Vector2Int> pathPoints)
    {
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= BorderWidth && gx < size - BorderWidth &&
                    gy >= BorderWidth && gy < size - BorderWidth)
                {
                    Grid[gx, gy] = 1;
                    pathPoints.Add(new Vector2Int(gx, gy));
                }
            }
        }
    }



    void PathmakerAlley()
    {
        if (mainPaths.Count < 2) return;

        int fromIdx = Random.Range(0, mainPaths.Count);
        int toIdx = Random.Range(0, mainPaths.Count);
        while (toIdx == fromIdx) toIdx = Random.Range(0, mainPaths.Count);

        Vector2Int entry = mainPaths[fromIdx][Random.Range(0, mainPaths[fromIdx].Count)];
        Vector2Int exit = mainPaths[toIdx][Random.Range(0, mainPaths[toIdx].Count)];

        int x = entry.x;
        int y = entry.y;

        while (x != exit.x)
        {
            DrawAlleyCell(x, y);
            x += (exit.x > x) ? 1 : -1;
        }

        while (y != exit.y)
        {
            DrawAlleyCell(x, y);
            y += (exit.y > y) ? 1 : -1;
        }

        DrawAlleyCell(x, y);
    }

    void DrawAlleyCell(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= BorderWidth && gx < size - BorderWidth &&
                    gy >= BorderWidth && gy < size - BorderWidth &&
                    Grid[gx, gy] == 0)
                {
                    Grid[gx, gy] = 2;
                }
            }
        }
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

    //BAGIAN BUILDING -- HANYA DATA TIDAK DI INSTANTIATE
    void SpawnBuildings()
    {
        if (buildingParent != null) DestroyImmediate(buildingParent.gameObject);
        buildingParent = new GameObject("Buildings").transform;
        buildingParent.parent = this.transform;
        buildingParent.localPosition = Vector3.zero;
        PlacedBuildings.Clear();

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (Grid[x, y] != 0) continue; // must be on grass

                Vector2Int pos = new Vector2Int(x, y);

                // Check alley and main path distances
                int distToAlley = FindNearestDistance(pos, 2);
                int distToMain = FindNearestDistance(pos, 1);

                bool nearAlley = distToAlley > 0 && distToAlley <= maxDistanceToPath;
                bool nearMain = distToMain > 0 && distToMain <= maxDistanceToPath;

                // Determine which building types to try based on proximity
                List<BuildingOption> buildingsToTry = new List<BuildingOption>();
                if (nearAlley) buildingsToTry.AddRange(Type1);
                if (nearAlley || nearMain) buildingsToTry.AddRange(Type2);
                if (nearMain && !nearAlley) buildingsToTry.AddRange(Type3);

                // Try to place a building
                TryPlaceBuildingAtPosition(pos, buildingsToTry);
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
                            if (Grid[nx, ny] == targetType)
                                return r;
                        }
                    }
                }
            }
            return -1; // not found
        }
    }

    bool TryPlaceBuildingAtPosition(Vector2Int pos, List<BuildingOption> buildingOptions)
    {
        Shuffle(buildingOptions);
        foreach (var option in buildingOptions)
        {
            if (TryFindValidPlacement(pos, option, out Vector3 finalPosition, out Quaternion finalRotation))
            {
                MarkBuildingFootprint(
                    new Vector2Int(Mathf.RoundToInt(finalPosition.x), Mathf.RoundToInt(finalPosition.z)),
                    option.footprintSize,
                    finalPosition,
                    finalRotation,
                    option.prefab
                );

                return true;
            }
        }
        return false;
    }

    bool TryFindValidPlacement(Vector2Int initialPos, BuildingOption buildingOption, out Vector3 finalPosition, out Quaternion finalRotation)
    {
        finalPosition = Vector3.zero;
        finalRotation = Quaternion.identity;

        if (!FindNearestPathDirection(initialPos, out Vector2Int pathPos, out Vector2 direction))
            return false;

        Vector2 footprint = buildingOption.footprintSize;
        int halfWidth = Mathf.CeilToInt(footprint.x / 2f);
        int halfDepth = Mathf.CeilToInt(footprint.y / 2f);

        Vector2Int adjustedPos = initialPos;
        int attempts = 0;
        int maxAttempts = 5;

        while (attempts < maxAttempts)
        {
            adjustedPos = CalculateAdjustedPosition(initialPos, pathPos, direction, halfDepth);

            if (IsValidBuildingPosition(adjustedPos, footprint))
            {
                finalPosition = new Vector3(adjustedPos.x, 0, adjustedPos.y);
                finalRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));
                return true;
            }

            initialPos = Vector2Int.RoundToInt(initialPos - direction);
            attempts++;
        }

        return false;
    }

    Vector2Int CalculateAdjustedPosition(Vector2Int initialPos, Vector2Int pathPos, Vector2 direction, int depthOffset)
    {
        int moveDistance = depthOffset + minDistanceBetweenBuildings;
        Vector2Int adjustedPos = initialPos;

        for (int i = 0; i < moveDistance; i++)
        {
            Vector2Int newPos = adjustedPos - Vector2Int.RoundToInt(direction);
            if (newPos.x < 0 || newPos.x >= size || newPos.y < 0 || newPos.y >= size)
                break;
            adjustedPos = newPos;
        }

        return adjustedPos;
    }

    bool FindNearestPathDirection(Vector2Int pos, out Vector2Int nearestPathPos, out Vector2 direction)
    {
        nearestPathPos = Vector2Int.zero;
        direction = Vector2.zero;
        float minDistance = float.MaxValue;

        for (int r = 1; r <= maxDistanceToPath; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;

                    int nx = pos.x + dx;
                    int ny = pos.y + dy;

                    if (nx >= 0 && nx < size && ny >= 0 && ny < size && (Grid[nx, ny] == 1 || Grid[nx, ny] == 2))
                    {
                        float distance = Vector2Int.Distance(pos, new Vector2Int(nx, ny));
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestPathPos = new Vector2Int(nx, ny);
                            direction = (new Vector2(nx, ny) - new Vector2(pos.x, pos.y)).normalized;
                        }
                    }
                }
            }

            if (minDistance < float.MaxValue)
                return true;
        }

        return false;
    }

    bool IsValidBuildingPosition(Vector2Int center, Vector2 footprint)
    {
        int halfWidth = Mathf.CeilToInt(footprint.x / 2f);
        int halfDepth = Mathf.CeilToInt(footprint.y / 2f);

        for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++)
        {
            for (int y = center.y - halfDepth; y <= center.y + halfDepth; y++)
            {
                if (x < 0 || x >= size || y < 0 || y >= size)
                    return false;

                if (Grid[x, y] != 0)
                    return false;

                if (PlacedBuildings.Exists(b => b.coordinate == new Vector2Int(x, y)))
                    return false;
            }
        }
        return true;
    }

    void MarkBuildingFootprint(Vector2Int center, Vector2 footprint, Vector3 worldPos, Quaternion rotation, GameObject prefab)
    {
        int halfWidth = Mathf.CeilToInt(footprint.x / 2f);
        int halfDepth = Mathf.CeilToInt(footprint.y / 2f);

        for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++)
        {
            for (int y = center.y - halfDepth; y <= center.y + halfDepth; y++)
            {
                if (x >= 0 && x < size && y >= 0 && y < size)
                {
                    PlacedBuildings.Add(new BuildingData(
                        new Vector2Int(x, y),
                        worldPos,
                        rotation,
                        prefab
                    ));
                }
            }
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    [ContextMenu("Debug Check Generated Data")]
    public void DebugCheckGeneratedData()
    {
        Debug.Log("=== DEBUG GENERATED DATA ===");
        
        // 1. Check Jalan Utama (Main Paths)
        Debug.Log($"Jumlah Main Paths: {mainPaths.Count}");
        for (int i = 0; i < mainPaths.Count; i++)
        {
            Debug.Log($"- Path {i}: {mainPaths[i].Count} titik");
            if (mainPaths[i].Count > 0)
            {
                Debug.Log($"  Titik pertama: {mainPaths[i][0]}, titik terakhir: {mainPaths[i][mainPaths[i].Count - 1]}");
            }
        }

        // 2. Check Taman (Parks)
        Debug.Log($"Jumlah Taman: {PlacedParks.Count}");
        foreach (var park in PlacedParks)
        {
            Debug.Log($"- Taman di {park.startPosition} (Ukuran: {park.width}x{park.height})");
        }

        // 3. Check Bangunan (Buildings)
        Debug.Log($"Jumlah Bangunan: {PlacedBuildings.Count}");
        if (PlacedBuildings.Count > 0)
        {
            var firstBuilding = PlacedBuildings[0];
            Debug.Log($"- Bangunan pertama: {firstBuilding.prefabName} di {firstBuilding.coordinate} (Rotasi: {firstBuilding.rotation.eulerAngles})");
        }

        // 4. Check Grid (Opsional: Hanya sample beberapa tile)
        Debug.Log("Sample Grid Tile:");
        Debug.Log($"- Tile (0,0): {Grid[0, 0]} (0=Grass, 1=MainPath, 2=Alley, 3=Park)");
        Debug.Log($"- Tile Tengah ({size/2},{size/2}): {Grid[size/2, size/2]}");
    }
}
