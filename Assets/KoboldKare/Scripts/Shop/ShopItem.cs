using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Shop Item", menuName = "Data/Shop Item")]
public class ShopItem : ScriptableObject
{   [SerializeField]
    private string path;
    [SerializeField]
    private string itemName;
    [SerializeField]
    private string description;
    [SerializeField]
    private int price;
    [SerializeField]
    private Sprite useSprite;
    [SerializeField]
    private PhotonGameObjectReference prefab;

    // public string GetPath(){
    //     return path;
    // }
    public string[] GetPath(){
        return path.Split('/'); // '/' to make it look like url, maybe '\\' to make it look like file path? 
    }
    public string GetName(){
        return itemName;
    }
    public string GetDescription(){
        return description;
    }
    public int GetPrice(){
        return price;
    }
    public Sprite GetSprite(){
        return useSprite;
    }
    public PhotonGameObjectReference GetPrefab(){
        return prefab;
    }

}
