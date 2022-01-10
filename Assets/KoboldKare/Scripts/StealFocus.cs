using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealFocus : MonoBehaviour{
    void Start(){
        StartCoroutine(waitABit());
    }

    IEnumerator waitABit(){
        yield return new WaitForSeconds(0.1f);
        GetComponent<Selectable>().Select();
    }
}
