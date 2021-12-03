using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseHint : MonoBehaviour{
    public List<GameObject> hint = new List<GameObject>();

    void Start(){
        foreach(GameObject item in hint){
            item.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other){
        foreach(GameObject item in hint){
            item.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other){
        foreach(GameObject item in hint){
            item.SetActive(false);
        }
    }

    public void ChangeText(string text){
        foreach(GameObject go in hint){
            go.GetComponent<TextMesh>().text = text;
        }
    }
}
