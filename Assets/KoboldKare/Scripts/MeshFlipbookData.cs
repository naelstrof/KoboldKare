using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mesh Flipbook Data", menuName = "Data/Mesh Flipbook Data", order = 1)]
public class MeshFlipbookData : ScriptableObject {
    public Mesh[] meshes;
    public float fps;
}
