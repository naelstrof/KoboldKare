using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealFocus : MonoBehaviour{
    void Start(){
        StartCoroutine(WaitABit());
    }

    IEnumerator WaitABit(){
        yield return new WaitForSeconds(0.1f);
        GetComponent<Selectable>().Select();
    }
}
