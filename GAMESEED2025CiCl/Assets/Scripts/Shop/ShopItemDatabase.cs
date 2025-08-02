using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Shop Item Database", menuName = "Shop/Item Database")]
public class ShopItemDatabase : ScriptableObject
{
    public List<ShopItem> allItems;
}
