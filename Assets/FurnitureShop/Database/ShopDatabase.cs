using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
[CreateAssetMenu(fileName = "New Shop Database", menuName = "Data/Shop Database")]
public class ShopDatabase : ScriptableObject
{   [SerializeField]
    private List<ShopCategory> categoriesToLoad;
    [SerializeField]
    private List<ShopItem> furnitureToLoad;
    
    [SerializeField]
    private bool shouldLoadAdressable =false;
    [SerializeField]
    private string AdressableCategoryTag;
    [SerializeField]
    private string AdressableItemTag;

     public async Task  Setup(){


        }


    
}
