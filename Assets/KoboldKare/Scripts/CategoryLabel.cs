using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryLabel : MonoBehaviour
{

    [SerializeField]
    private GameObject nameLabel;
    private bool showing=true;
    public void ShowHide(){
        if(showing){
            bool firstChild=true;
            foreach(Transform transf in transform){
                if(firstChild)
                {
                    firstChild=false;
                    continue;
                }
                transf.gameObject.SetActive(true);
            }
            showing=!showing;
        }
        else{
             bool firstChild=true;
             foreach(Transform transf in transform){
                if(firstChild)
                {
                    firstChild=false;
                    continue;
                }
                transf.gameObject.SetActive(false);
            }
            showing=!showing;
        }
    }
    public void Toggle(){
        ShowHide();
    }

    public GameObject GetName(){
        return nameLabel;
    }


}
