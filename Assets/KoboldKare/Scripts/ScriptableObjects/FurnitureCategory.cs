using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Furniture Category", menuName = "Data/Furniture Category")]
public class FurnitureCategory : ScriptableObject
{
    public string catName;
    public List<Furniture> furniture;
}
