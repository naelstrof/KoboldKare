using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopCategoryLabel : MonoBehaviour
{
    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private GameObject Children;

     public void Toggle(){
        Children.SetActive(!Children.activeSelf);
    }
    public TMP_Text GetNameText(){
        return nameText;
    }
}
