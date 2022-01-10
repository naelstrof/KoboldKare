using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UVAnimationScroll : MonoBehaviour{
    public Vector2 scrollSpeed;
    Vector2 offset;
    Material material;

    void Start(){
        material = GetComponent<Image>().material;
    }
    void Update(){
        offset = new Vector2(offset.x + Time.time * scrollSpeed.x,offset.y + Time.time * scrollSpeed.y);        
        material.mainTextureOffset = offset;
    }
}
