using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingOption
{
    public GameObject prefab;
    [Range(0f, 0.5f)] public float spawnChance = 0.1f;
    public Vector2 footprintSize = Vector2.one;
}

[System.Serializable]
public class BuildingData
{
    public Vector2Int coordinate;
    public Vector3 worldPosition;
    public Quaternion rotation;
    public GameObject prefabUsed;
    public string prefabName;
    public bool isDestroyed;
    public bool isRendered;
    public bool isActive;

    public BuildingData(Vector2Int coord, Vector3 pos, Quaternion rot, GameObject prefab)
    {
        coordinate = coord;
        worldPosition = pos;
        rotation = rot;
        prefabUsed = prefab;
        prefabName = prefab.name;
        isDestroyed = false;
        isRendered = false;
        isActive = false;
    }
}

[System.Serializable]
public class FenceData
{
    public Vector3 position;
    public Quaternion rotation;
    public bool isGate;
    
    public FenceData(Vector3 pos, Quaternion rot, bool gate)
    {
        position = pos;
        rotation = rot;
        isGate = gate;
    }
}
