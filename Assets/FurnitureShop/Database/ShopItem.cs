using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Shop Item", menuName = "Data/Shop Item")]
public class ShopItem : ShopObject
{
    public string description;
    public int price;
    public Sprite useSprite;
    public PhotonGameObjectReference prefab;
}
