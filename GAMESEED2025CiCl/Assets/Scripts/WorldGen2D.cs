using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class SimplePathGen : MonoBehaviour
{
    public int gridSize = 20;
    public int seed = 42;

    public Tilemap tilemap;
    public TileBase grassTile;
    public TileBase pathTile;

    private System.Random rand;
    private int[,] grid;

    private Vector2Int[] directions = {
        new Vector2Int(0, 1),   // Atas
        new Vector2Int(1, 0),   // Kanan
        new Vector2Int(0, -1),  // Bawah
        new Vector2Int(-1, 0)   // Kiri
    };

    public void Generate()
    {
        if (tilemap == null || grassTile == null || pathTile == null)
        {
            Debug.LogWarning("Tilemap dan tile belum diisi.");
            return;
        }

        tilemap.ClearAllTiles();
        rand = new System.Random(seed);
        grid = new int[gridSize, gridSize];

        int pathCount = gridSize / 2;

        for (int i = 0; i < pathCount; i++)
        {
            GenerateSinglePath(i + 1); // kasih ID unik per path
        }

        DrawGrid();
    }

    void GenerateSinglePath(int id)
    {
        int side = rand.Next(4); // 0 = atas, 1 = kanan, 2 = bawah, 3 = kiri
        Vector2Int start = GetStartPositionFromSide(side);
        Vector2Int dir = GetInitialDirectionFromSide(side);
        Vector2Int pos = start;

        int belokTersisa = 2;

        if (!IsInside(pos)) return;

        grid[pos.x, pos.y] = id;

        while (IsInside(pos + dir))
        {
            pos += dir;
            grid[pos.x, pos.y] = id;

            if (belokTersisa > 0 && rand.Next(100) < 20)
            {
                dir = GetNewDirection(dir);
                belokTersisa--;
            }
        }
    }

    Vector2Int GetStartPositionFromSide(int side)
    {
        switch (side)
        {
            case 0: return new Vector2Int(rand.Next(1, gridSize - 1), gridSize - 1); // atas
            case 1: return new Vector2Int(gridSize - 1, rand.Next(1, gridSize - 1)); // kanan
            case 2: return new Vector2Int(rand.Next(1, gridSize - 1), 0);            // bawah
            case 3: return new Vector2Int(0, rand.Next(1, gridSize - 1));            // kiri
        }
        return Vector2Int.zero;
    }

    Vector2Int GetInitialDirectionFromSide(int side)
    {
        switch (side)
        {
            case 0: return directions[2]; // bawah
            case 1: return directions[3]; // kiri
            case 2: return directions[0]; // atas
            case 3: return directions[1]; // kanan
        }
        return Vector2Int.right;
    }

    Vector2Int GetNewDirection(Vector2Int current)
    {
        // Belok dari horizontal → ke vertikal, dari vertikal → ke horizontal
        if (current.x != 0)
            return rand.Next(2) == 0 ? directions[0] : directions[2]; // atas / bawah
        else
            return rand.Next(2) == 0 ? directions[1] : directions[3]; // kanan / kiri
    }

    bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < gridSize && p.y >= 0 && p.y < gridSize;
    }

    void DrawGrid()
    {
        for (int x = 0; x < gridSize; x++)
        for (int y = 0; y < gridSize; y++)
        {
            Vector3Int pos = new Vector3Int(x, y, 0);
            tilemap.SetTile(pos, grid[x, y] > 0 ? pathTile : grassTile);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimplePathGen))]
public class SimplePathGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SimplePathGen gen = (SimplePathGen)target;
        if (GUILayout.Button("Generate Path"))
        {
            gen.Generate();
        }
    }
}
#endif
