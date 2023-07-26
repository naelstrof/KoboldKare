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
    private List<string> catNames;
    // Start is called before the first frame update
    void Start()
    {
        buyButton.onClick.AddListener(() => { buy(); });
        exitButton.onClick.AddListener(() => { gameObject.SetActive(false); });
        
            
           

        }
    private void buy()
    {
        //Debug.Log("Bought!");
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
                Debug.Log("Finished");
                GameObject temp;
                GameObject catTemp;
                Transform catPanelTransform;
                catNames=new List<string>();
                foreach(string name in furnitureDatabase.GetCategoryNames())
                {catNames.Add(name);}
                int i = 0;
                        foreach(string catName in catNames){
                            int j=0;
                            catTemp=Instantiate(categoryLabel);
                            catPanelTransform=catTemp.transform.Find("Panel").transform;
                            catTemp.GetComponent<TMP_Text>().text=catName;
                            catTemp.transform.SetParent(contentTransform);
                                GameObject catPanel=catTemp.transform.Find("Panel").gameObject;
                                catTemp.GetComponent<Button>().onClick.AddListener( ()=>
                                            {
                                            catPanel.SetActive(!catPanel.activeSelf);
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
