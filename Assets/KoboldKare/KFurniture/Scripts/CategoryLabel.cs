using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryLabel : MonoBehaviour
{   [SerializeField]
    private RectTransform next;
    public RectTransform nextGet =>next;
    [SerializeField]
    private GameObject panel;
    public GameObject panelGet=>panel;
    private bool showing=true;
    private float categoryHeightAdjusted;
    private float itemHeightAdjusted;
    private float paddingAdjusted;
    private float nextPositionY;
    public bool hasChild=false;
    public void ShowHide(bool show){
        if(show){
            panel.SetActive(true);
            next.anchoredPosition=new Vector2(0,-CalculateHeight());
            showing=!showing;
        }
        else{
             panel.SetActive(false);
            next.anchoredPosition=new Vector2(0,-paddingAdjusted);
            showing=!showing;
        }

    }
    public void Toggle(){
        ShowHide(showing);
    }
    private float CalculateHeight(){
        return itemHeightAdjusted*panel.transform.childCount+paddingAdjusted;

    }
    public void Start(){
        nextPositionY=next.anchoredPosition.y;
    }
    public void SetupSize(float categoryHeight,float itemHeight,float padding,float scale){
            categoryHeightAdjusted=categoryHeight;
            itemHeightAdjusted=itemHeight;
            paddingAdjusted=padding;
            next.anchoredPosition=new Vector2(next.anchoredPosition.x,nextPositionY*scale);
            GetComponent<RectTransform>().sizeDelta=new Vector2(200,categoryHeightAdjusted);
            foreach(Transform transf in panel.transform){
                transf.GetComponent<RectTransform>().sizeDelta=new Vector2(200,itemHeightAdjusted);
            }

            if(hasChild){
                next.GetChild(0).GetComponent<CategoryLabel>().SetupSize(categoryHeight,itemHeight,padding,scale);
            }

    }
}
