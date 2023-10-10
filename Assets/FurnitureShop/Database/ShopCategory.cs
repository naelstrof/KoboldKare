using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Shop Category", menuName = "Data/Shop Category")]
public class ShopCategory : ShopObject
{
    public List<ShopObject> items;
}
