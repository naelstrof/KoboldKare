using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item Pack", menuName = "Data/Shop Item Pack")]
public class ShopItemPack : ScriptableObject
{   [SerializeField]
    private string path;

    [SerializeField]
    private List<ShopItem> items;
    public string[] GetPath(){
        return path.Split('/'); // '/' to make it look like url, maybe '\\' to make it look like file path? 
    }
    public List<ShopItem> GetItems(){
        return items;
    }
}
