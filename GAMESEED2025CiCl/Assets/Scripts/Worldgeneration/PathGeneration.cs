using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class PathGen : MonoBehaviour
{
    [Header("World Properties")]
    public int WorldChunks = 10;
    const int ChunkSize = 16;
    public int size => WorldChunks * ChunkSize;
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
    public List<BuildingOption> Type1; // Spawn hanya di dekat alley
    public List<BuildingOption> Type2; // Spawn di dekat alley dan main path
    public List<BuildingOption> Type3; // Spawn hanya di dekat main path

    [Header("Building Placement Settings")]
    [Range(1, 10)]
    public int minDistanceBetweenBuildings = 2;
    [Range(1, 10)]
    public int maxDistanceToPath = 3;

    [System.Serializable]
    public class BuildingData {
        public Vector2Int coordinate;  // Koordinat Grid (x,y)
        public Vector3 worldPosition;  // Posisi dunia (x,0,y)
        public Quaternion rotation;    // Rotasi terakhir
        public GameObject prefabUsed;  // Referensi prefab yang dipakai
        public string prefabName;      // Nama prefab (backup jika GameObject null)

        public BuildingData(Vector2Int coord, Vector3 pos, Quaternion rot, GameObject prefab) {
            coordinate = coord;
            worldPosition = pos;
            rotation = rot;
            prefabUsed = prefab;
            prefabName = prefab.name;
        }
    }

    [HideInInspector]public List<BuildingData> PlacedBuildings = new List<BuildingData>();

    private bool combineAfterGenerate = true;

    private int[,] Grid;
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

        DrawPrefabs();
        if (combineAfterGenerate) CombineAllTiles();
        SpawnBuildings();
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
                    Grid[gx, gy] = 1;
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
                        Grid[gx, gy] = 1;
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
                        Grid[gx, gy] = 1;
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
                    Grid[gx, gy] = 1;
                    pathPoints.Add(new Vector2Int(gx, gy));
                }
            }
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

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size && Grid[gx, gy] == 0)
                    Grid[gx, gy] = 2;
            }

        while (x != exitPosition.x)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int gx = x + dx;
                    int gy = y + dy;
                    if (gx >= 0 && gx < size && gy >= 0 && gy < size && Grid[gx, gy] == 0)
                        Grid[gx, gy] = 2;
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
                    if (gx >= 0 && gx < size && gy >= 0 && gy < size && Grid[gx, gy] == 0)
                        Grid[gx, gy] = 2;
                }
            y += (exitPosition.y > y) ? 1 : -1;
        }

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                int gx = x + dx;
                int gy = y + dy;
                if (gx >= 0 && gx < size && gy >= 0 && gy < size && Grid[gx, gy] == 0)
                    Grid[gx, gy] = 2;
            }
    }

    void DrawPrefabs()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Handle path tiles
                if (Grid[x, y] == 1 || Grid[x, y] == 2)
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
        return Grid[x, y] == 1 || Grid[x, y] == 2;
    }

    void SpawnBuildings()
    {
        if (buildingParent != null) DestroyImmediate(buildingParent.gameObject);
        buildingParent = new GameObject("Buildings").transform;
        buildingParent.parent = this.transform;
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
                GameObject instance = Instantiate(option.prefab, finalPosition, finalRotation, buildingParent);

                // Ganti pemanggilan MarkBuildingFootprint
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

            // This now checks the ENTIRE footprint, not just center
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
        // Move building away from path by depthOffset + minDistance
        int moveDistance = depthOffset + minDistanceBetweenBuildings;
        Vector2Int adjustedPos = initialPos;

        // Move away from path while staying within bounds
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

        // Search in expanding radius
        for (int r = 1; r <= maxDistanceToPath; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue; // edge of square only
                    
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
            
            if (minDistance < float.MaxValue) // Found a path
                return true;
        }

        return false;
    }

    bool IsValidBuildingPosition(Vector2Int center, Vector2 footprint)
    {
        int halfWidth = Mathf.CeilToInt(footprint.x / 2f);
        int halfDepth = Mathf.CeilToInt(footprint.y / 2f);

        // Check all tiles in the building's footprint
        for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++) {
            for (int y = center.y - halfDepth; y <= center.y + halfDepth; y++) {
                // Check if position is within grid bounds
                if (x < 0 || x >= size || y < 0 || y >= size)
                    return false;
                
                // Check if tile is a path (1 = main path, 2 = alley)
                if (Grid[x, y] != 0) // 0 is grass
                    return false;
                
                // Check against other buildings
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

        for (int x = center.x - halfWidth; x <= center.x + halfWidth; x++) {
            for (int y = center.y - halfDepth; y <= center.y + halfDepth; y++) {
                if (x >= 0 && x < size && y >= 0 && y < size) {
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

}
