using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Horeg Database", menuName = "Shop/Horeg Database")]
public class HoregDatabase : ScriptableObject
{
    public List<GameObject> allHoregPrefabs;
}
