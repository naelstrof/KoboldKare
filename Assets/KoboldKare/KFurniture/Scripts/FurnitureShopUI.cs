using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;

public class FurnitureShopUI : MonoBehaviour
{
    [SerializeField] 
    private Button buyButton;
        
    [SerializeField] 
     private Button exitButton;
    [SerializeField]
    private Transform contentTransform;
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
                PhotonNetwork.InstantiateRoomObject(selected.prefab.photonName, koboldTransform.position + koboldTransform.forward+Vector3.up, Quaternion.identity);
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
            {   await furnitureDatabase.Setup();
                
                GameObject temp;
                
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
                                catTemp.transform.SetParent(previous.GetComponent<CategoryLabel>().nextGet);
                                else
                                catTemp.transform.SetParent(contentTransform);
                            catTemp.GetComponent<TMP_Text>().text=catName;
                            catPanelTransform=catTemp.transform.GetChild(0);
                            previous=catTemp;
                            positionToSpawnCategories=catTemp.GetComponent<CategoryLabel>().nextGet.transform;
                                GameObject catPanel=catTemp.transform.GetChild(0).gameObject;
                                catTemp.GetComponent<Button>().onClick.AddListener( ()=>
                                            {
                                            catTemp.GetComponent<CategoryLabel>().Toggle();
                                            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.gameObject.GetComponent<RectTransform>());
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
                        catTemp.GetComponent<CategoryLabel>().Setup();
                        }
                i = 0;
                foreach(Furniture furn in furnitureDatabase.GetList())
                {   
                    int copy = i;
                    temp = Instantiate(itemLabel);
                    temp.GetComponent<TMP_Text>().text = furn.name;
                    temp.GetComponent<Button>().onClick.AddListener( ()=>{SetInfo(copy);} );
                    temp.transform.SetParent(contentTransform);
                    i++;
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform.gameObject.GetComponent<RectTransform>());
                
                isSetup=true;
                
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
   
}
