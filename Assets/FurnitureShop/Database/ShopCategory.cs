using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Shop Category", menuName = "Data/Shop Item Category")]
public class ShopCategory : ScriptableObject
{   public string categoryName;
    public List<ShopCategory> subCategories;
    public List<ShopItem> items;
}
