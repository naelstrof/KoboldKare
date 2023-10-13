using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item Pack", menuName = "Data/Shop Item Pack")]
public class ShopItemPack : ScriptableObject
{
    public string path;
    public List<ShopItem> items;
}
