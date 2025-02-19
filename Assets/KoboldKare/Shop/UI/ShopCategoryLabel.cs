using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopCategoryLabel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private GameObject name;
    [SerializeField]
    private GameObject children;

    public GameObject GetChildren(){
        return children;
    }
    public GameObject GetName(){
        return name;
    }
     public void Toggle(){
        children.SetActive(!children.activeSelf);
    }
    public void SetName(string name){
        nameText.text=name;
    }
    public TMP_Text GetNameText(){
        return nameText;
    }
}
