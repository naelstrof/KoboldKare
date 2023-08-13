using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;

public class FurnitureShopUI : MonoBehaviour
{   [SerializeField]
    private int categoryHeight;
    [SerializeField]
    private int itemHeight;
    [SerializeField]
    private int padding;
    private float paddingAdjusted;
    private float categoryHeightAdjusted;
    private float itemHeightAdjusted;
    [SerializeField]
    private int CanvasScalerResolutionHeight;
    private float scale;

    [SerializeField] 
    private Button buyButton;
        
    [SerializeField] 
     private Button exitButton;
    [SerializeField]
    private RectTransform contentTransform;
    [SerializeField]
    private GameObject loosePanelPrefab;
    private GameObject loosePanel;
    bool isSetup = false;
    private Furniture selected;
    [SerializeField]
    private GameObject categoryLabel;
    [SerializeField]
    private GameObject itemLabel;
    [SerializeField]
    private FurnitureDatabase furnitureDatabase;
    [SerializeField]
    private TMP_Text nameField;
    [SerializeField]
    private TMP_Text descriptionField;
    [SerializeField]
    private TMP_Text priceField;
    [SerializeField]
    private Image imageField;
    private Kobold kobold;
    private List<PhotonGameObjectReference> photonList = new List<PhotonGameObjectReference>();
    [SerializeField]
    private Sprite defaultSprite;
    [SerializeField]
    private Transform positionToSpawnCategories;
    private List<string> catNames;
    private GameObject first;
    private int lastResolution;
    private int contentHeight;
    // Start is called before the first frame update
    void Start()
    {
        buyButton.onClick.AddListener(() => { buy(); });
        exitButton.onClick.AddListener(() => { gameObject.SetActive(false); });
        
            
           

        }
    private void buy()
    {
        
        if(selected != null) { 
            Transform koboldTransform = kobold.hip.transform;
            if(kobold.GetComponent<MoneyHolder>().HasMoney(selected.price))
            {
                kobold.GetComponent<MoneyHolder>().ChargeMoney(selected.price);
                GameObject temp =PhotonNetwork.InstantiateRoomObject(selected.prefab.photonName, koboldTransform.position + koboldTransform.forward+Vector3.up, Quaternion.identity);
            }
        
        }

    }
    // Update is called once per frame

    public void SetUser(Kobold user)
    {
        kobold= user;
        
    }
    

    async void OnEnable()
    {       
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if(!isSetup)
            {   
                await furnitureDatabase.Setup();
                first=null;
                GameObject temp;
                CalculateSize();
                lastResolution=Screen.height;
                Transform catPanelTransform;
                catNames=new List<string>();
                foreach(string name in furnitureDatabase.GetCategoryNames())
                {catNames.Add(name);}
                int i = 0;
                
                GameObject previous=null;
                        foreach(string catName in catNames){
                            
                            int j=0;
                            GameObject catTemp;
                            catTemp=Instantiate(categoryLabel,positionToSpawnCategories.position,Quaternion.identity);
                            if(previous != null)
                                {   previous.GetComponent<CategoryLabel>().hasChild=true;
                                    catTemp.transform.SetParent(previous.GetComponent<CategoryLabel>().nextGet);}
                                else
                                {catTemp.transform.SetParent(positionToSpawnCategories);
                                first=catTemp;}
                            catTemp.GetComponent<TMP_Text>().text=catName;
                            catPanelTransform=catTemp.transform.GetChild(0);
                            previous=catTemp;
                            positionToSpawnCategories=catTemp.GetComponent<CategoryLabel>().nextGet.transform;
                                GameObject catPanel=catTemp.transform.GetChild(0).gameObject;
                                catTemp.GetComponent<Button>().onClick.AddListener( ()=>
                                            {
                                            //catTemp.GetComponent<CategoryLabel>().Toggle();
                                            ChangeContentHeight((int)catTemp.GetComponent<CategoryLabel>().ToggleGetHeightChange());
                                            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
                                            } 
                                );
                            
                            foreach(Furniture furn in furnitureDatabase.GetCategory(catName))
                            {   
                                int copyi = i;
                                int copyj = j;
                                temp = Instantiate(itemLabel);
                                temp.GetComponent<TMP_Text>().text = furn.name;
                                temp.GetComponent<Button>().onClick.AddListener( ()=>{SetInfo(copyi,copyj);} );
                                temp.transform.SetParent(catPanelTransform);
                                j++;
                            }
                            
                            i++;
                            j=0;
                        
                        }
                i = 0;
                if(previous !=null){
                    loosePanel=Instantiate(loosePanelPrefab);
                loosePanel.transform.SetParent(previous.GetComponent<CategoryLabel>().nextGet);
                loosePanel.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,0);}
                foreach(Furniture furn in furnitureDatabase.GetList())
                {   
                    int copy = i;
                    temp = Instantiate(itemLabel);
                    temp.transform.SetParent(loosePanel.transform);
                    temp.GetComponent<RectTransform>().sizeDelta=new Vector2(200,itemHeightAdjusted);
                    temp.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-(paddingAdjusted+itemHeightAdjusted)*(i));
                    temp.GetComponent<TMP_Text>().text = furn.name;
                    temp.GetComponent<Button>().onClick.AddListener( ()=>{SetInfo(copy);} );
                    
                    i++;
                }

                
                first.GetComponent<CategoryLabel>().SetupSize(categoryHeightAdjusted,itemHeightAdjusted,paddingAdjusted,scale);
                ResetContentHeight();
                isSetup=true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
                
                
            }else{
                if(lastResolution!=Screen.height){
                    CalculateSize();
                    first.GetComponent<CategoryLabel>().SetupSize(categoryHeightAdjusted,itemHeightAdjusted,paddingAdjusted,scale);
                    foreach(RectTransform transf in loosePanel.transform){
                        transf.sizeDelta=new Vector2(200,itemHeightAdjusted);
                    }
                    ResetContentHeight();
                    lastResolution=Screen.height;

                }
            }
        
    }
     void OnDisable()
    {   Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void SetInfo(string name)
    {   
        SetInfo(furnitureDatabase.GetFurniture(name));
    }
    
    void SetInfo(int id)
    {    
        SetInfo(furnitureDatabase.GetFurniture((short)id));
    }
      void SetInfo(int cat,int id)
    {   
        SetInfo(furnitureDatabase.GetFurniture(catNames[cat],id));
    }
    private void SetInfo(Furniture temp){
         nameField.text = temp.name;
        descriptionField.text = temp.description;
        priceField.text = temp.price.ToString();
        if(temp.useSprite!=null)
        {
            imageField.sprite=temp.useSprite;
        }
        else{
            imageField.sprite=defaultSprite;
        }
        selected = temp;
    }

    private void CalculateSize(){
            scale=(float)Screen.height/CanvasScalerResolutionHeight;
            categoryHeightAdjusted=(float)categoryHeight*scale;
            itemHeightAdjusted=(float)itemHeight*scale;
            paddingAdjusted=(float)padding*scale;
            
    }
    private void ResetContentHeight(){
        Debug.Log("Scale: "+scale);
        contentHeight=(int)(//first.GetComponent<RectTransform>().sizeDelta.y+
                            (((categoryHeightAdjusted+paddingAdjusted)*furnitureDatabase.GetKeyCount())+
                            ((itemHeightAdjusted+paddingAdjusted)*furnitureDatabase.GetFurnitureCount()))/scale);
        contentTransform.sizeDelta=new Vector2(contentTransform.sizeDelta.x,contentHeight);
        Debug.Log("catHeight "+(categoryHeightAdjusted+paddingAdjusted)+" "+furnitureDatabase.GetKeyCount()+"Times is "+(categoryHeightAdjusted+paddingAdjusted)*furnitureDatabase.GetKeyCount());
        Debug.Log("iHeight"+(itemHeightAdjusted+paddingAdjusted)+" "+furnitureDatabase.GetFurnitureCount()+"times is "+((itemHeightAdjusted+paddingAdjusted)*furnitureDatabase.GetFurnitureCount()));
        Debug.Log("New Height: "+contentHeight);
    }
    private void ChangeContentHeight(int change){
        Debug.Log("Changed by "+change);
        contentHeight=(int)(contentHeight+(change/scale));
        contentTransform.sizeDelta=new Vector2(contentTransform.sizeDelta.x,contentHeight);
        Debug.Log("New Height: "+contentHeight);
    }
   
}
