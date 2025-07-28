using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class RoadGeneration : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 20;
    public int seed = 42;
    public float tileSpacing = 3.2f;

    [Header("Prefab Selection")]
    public GameObject emptyTilePrefab;
    public GameObject pathTilePrefab;

    [Header("Optional House")]
    public GameObject housePrefab;

    private int[,] grid;
    private int[,] dirGrid;
    private bool[,] visitedGlobal;

    // Arah: 0 = Atas, 1 = Kanan, 2 = Bawah, 3 = Kiri
    private int[] dx = { -1, 0, 1, 0 };
    private int[] dy = { 0, 1, 0, -1 };

    private System.Random rand;

    public void GenerateAndDraw()
    {
        ClearPreviousTiles();
        GeneratePaths();
        DrawGrid();
    }

    void ClearPreviousTiles()
    {
#if UNITY_EDITOR
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
#endif
    }

    void GeneratePaths()
    {
        rand = new System.Random(seed);
        grid = new int[gridSize, gridSize];
        dirGrid = new int[gridSize, gridSize];
        visitedGlobal = new bool[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
            dirGrid[x, y] = -1;

        int pathCount = Mathf.Max(1, gridSize / 4);

        for (int i = 0; i < pathCount; i++)
        {
            RunSinglePath();
        }
    }

    void RunSinglePath()
    {
        bool[,] visitedPath = new bool[gridSize, gridSize];
        (int x, int y, int dir, string entrySide) = GetEntryPointAndDirection();

        int step = 0;

        while (IsInside(x, y) && !visitedPath[x, y])
        {
            grid[x, y] = 1;
            dirGrid[x, y] = dir;
            visitedPath[x, y] = true;
            visitedGlobal[x, y] = true;

            string currentSide = GetSideName(x, y);
            if (currentSide != null && currentSide != entrySide) break;

            int[] directions = GetDirectionOptions(dir);
            int nextDir = dir;
            bool preferSafe = rand.Next(100) < 90;

            foreach (int candidateDir in directions)
            {
                int nx = x + dx[candidateDir];
                int ny = y + dy[candidateDir];

                if (IsInside(nx, ny) && !visitedPath[nx, ny])
                {
                    bool isSafe = IsSafeDistance(nx, ny);
                    if (!preferSafe || isSafe)
                    {
                        nextDir = candidateDir;
                        break;
                    }
                }
            }

            int tx = x + dx[nextDir];
            int ty = y + dy[nextDir];
            if (!IsInside(tx, ty) || visitedPath[tx, ty]) break;

            x = tx;
            y = ty;
            dir = nextDir;

            step++;
            if (step > 500) break;
        }
    }

    (int, int, int, string) GetEntryPointAndDirection()
    {
        int side = rand.Next(4);
        int x = 0, y = 0, dir = 0;
        string entry = "";

        switch (side)
        {
            case 0: x = 0; y = rand.Next(gridSize); dir = 2; entry = "top"; break;
            case 1: x = rand.Next(gridSize); y = gridSize - 1; dir = 3; entry = "right"; break;
            case 2: x = gridSize - 1; y = rand.Next(gridSize); dir = 0; entry = "bottom"; break;
            case 3: x = rand.Next(gridSize); y = 0; dir = 1; entry = "left"; break;
        }

        return (x, y, dir, entry);
    }

    string GetSideName(int x, int y)
    {
        if (x == 0) return "top";
        if (x == gridSize - 1) return "bottom";
        if (y == 0) return "left";
        if (y == gridSize - 1) return "right";
        return null;
    }

    int[] GetDirectionOptions(int currentDir)
    {
        int roll = rand.Next(100);
        if (roll < 80) return new int[] { currentDir, (currentDir + 1) % 4, (currentDir + 3) % 4 };
        else if (roll < 90) return new int[] { (currentDir + 1) % 4, currentDir, (currentDir + 3) % 4 };
        else return new int[] { (currentDir + 3) % 4, currentDir, (currentDir + 1) % 4 };
    }

    bool IsSafeDistance(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        for (int j = -1; j <= 1; j++)
        {
            int nx = x + i;
            int ny = y + j;
            if (IsInside(nx, ny) && visitedGlobal[nx, ny] && (i != 0 || j != 0))
                return false;
        }
        return true;
    }

    bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < gridSize && y < gridSize;
    }

    void DrawGrid()
    {
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing);

                if (grid[x, y] == 1)
                {
                    if (pathTilePrefab != null)
                    {
                        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(pathTilePrefab, transform);
                        go.transform.position = position;

                        int d = dirGrid[x, y];
                        if (d >= 0)
                        {
                            Vector3 forward = new Vector3(dx[d], 0f, dy[d]);
                            if (forward == Vector3.zero)
                                forward = Vector3.forward;

                            Quaternion originalRot = Quaternion.LookRotation(forward);
                            Quaternion adjustedRot = originalRot * Quaternion.Euler(0, -90f, 0); // Kurangi 90 derajat
                            go.transform.rotation = adjustedRot;
                        }
                    }
                }
                else
                {
                    if (emptyTilePrefab != null)
                    {
                        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(emptyTilePrefab, transform);
                        go.transform.position = position;
                    }
                }
            }
            // Rumah
            for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
            {
                if (grid[x, y] != 0 || housePrefab == null)
                    continue;

                // Cek Direction
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];

                    if (IsInside(nx, ny) && grid[nx, ny] == 1)
                    {
                        Vector3 pos = new Vector3(x * tileSpacing, 0, y * tileSpacing);
                        GameObject house = (GameObject)PrefabUtility.InstantiatePrefab(housePrefab, transform);
                        house.transform.position = pos;

                        Vector3 forward = new Vector3(dx[d], 0, dy[d]);
                        Quaternion rotation = Quaternion.LookRotation(forward);
                        house.transform.rotation = rotation;
                        break;
                    }
                }
            }

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RoadGeneration))]
public class RoadGenerationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoadGeneration generator = (RoadGeneration)target;

        if (GUILayout.Button("Generate Grid"))
        {
            generator.GenerateAndDraw();
        }
    }
}
#endif
