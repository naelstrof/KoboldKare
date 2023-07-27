using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryLabel : MonoBehaviour
{
    [SerializeField]
    private int categoryHeight;
    public int categoryHeightGet=>categoryHeight;
    [SerializeField]
    private int itemHeight;
    public int itemHeightGet=>itemHeight;
    [SerializeField]
    private RectTransform next;
    public RectTransform nextGet =>next;
    [SerializeField]
    private GameObject panel;
    public GameObject panelGet=>panel;
    private bool showing=true;
    [SerializeField]
    private int padding;
    private float paddingAdjusted;
    private float categoryHeightAdjusted;
    private float itemHeightAdjusted;
    CanvasScaler canvasScaler;
    public void ShowHide(bool show){
        if(show){
            panel.SetActive(true);
            next.anchoredPosition=new Vector2(0,-CalculateHeight());
            showing=!showing;
        }
        else{
             panel.SetActive(false);
            next.anchoredPosition=new Vector2(0,-categoryHeightAdjusted/2);
            showing=!showing;
        }

    }
    public void Toggle(){
        ShowHide(showing);
    }
    private float CalculateHeight(){
        return itemHeightAdjusted*panel.transform.childCount+paddingAdjusted;

    }
    public void Setup(){
        SetupSize();
       
    }
    private void SetupSize(){

            float scale=Screen.height/600f;
            categoryHeightAdjusted=(categoryHeight*scale);
            itemHeightAdjusted=(itemHeight*scale);
            paddingAdjusted=padding*scale;
            next.anchoredPosition=new Vector2(next.anchoredPosition.x,next.anchoredPosition.y*scale);
            GetComponent<RectTransform>().sizeDelta=new Vector2(200,categoryHeightAdjusted);
            foreach(Transform transf in panel.transform){
                Debug.Log("Doing "+transf.gameObject.name);
                transf.GetComponent<RectTransform>().sizeDelta=new Vector2(200,itemHeightAdjusted);
            }

    }
}
