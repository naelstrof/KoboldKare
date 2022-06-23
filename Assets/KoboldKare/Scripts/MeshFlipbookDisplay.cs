using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshFlipbookDisplay : MonoBehaviour {
    private MeshFilter filter;
    [SerializeField]
    private MeshFlipbookData meshFlipbookData;
    void Start() {
        filter = GetComponent<MeshFilter>();
    }

    void Update() {
        int frame = Mathf.RoundToInt(Time.time * meshFlipbookData.fps) % meshFlipbookData.meshes.Length;
        filter.mesh = meshFlipbookData.meshes[frame];
    }
}
