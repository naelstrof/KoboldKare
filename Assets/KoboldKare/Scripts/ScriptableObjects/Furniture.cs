using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Furniture", menuName = "Data/Furniture")]
public class Furniture : ScriptableObject
{   
    
    public string shopName;
    public string description;
    public int price;
    public Sprite useSprite;
    public PhotonGameObjectReference prefab;

}
