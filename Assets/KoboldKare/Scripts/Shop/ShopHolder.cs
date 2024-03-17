using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Shop Holder", menuName = "Data/Furniture Shop Holder")]
public class ShopHolder : ScriptableObject
{   [SerializeField]
    public GameObject shopMenuPrefab;
    private GameObject shopInstance;
    public GameObject GetPrefab(){
        return shopMenuPrefab;
    }
    public GameObject GetInstance(){
        return shopInstance;
    }
    public bool HasInstance(){
        return shopInstance!=null;
    }
    public bool SetInstance(GameObject instance){
        if(shopInstance!=null){
            return false;
        }else{
            shopInstance=instance;
            return true;
        }
    }
}
