using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class FurnitureShopUI : MonoBehaviourPun
{

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
    private List<string> catNames;
    [SerializeField]
    private InputActionReference[] actionsToClose;
    // Start is called before the first frame update
    void Start()
    {
        

    }
    private void AddKeys(){
        foreach(InputActionReference iar in actionsToClose){
        iar.action.performed +=Hide;
    }
    }

    private void RemoveKeys(){
            foreach(InputActionReference iar in actionsToClose){
            iar.action.performed -=Hide;
    }
    }
    void Hide(InputAction.CallbackContext ctx){
        Hide();
    }
    public void Hide(){
            gameObject.SetActive(false);
    }

    public void Buy()
    {
        
        if(selected != null) { 
            Transform koboldTransform = kobold.hip.transform;
            if(kobold.GetComponent<MoneyHolder>().HasMoney(selected.price))
            {
                kobold.GetComponent<MoneyHolder>().ChargeMoney(selected.price);
                //GameObject temp =PhotonNetwork.InstantiateRoomObject(selected.prefab.photonName, koboldTransform.position + koboldTransform.forward+Vector3.up, Quaternion.identity);
                kobold.photonView.RPC("Spawn", RpcTarget.All,selected.prefab.photonName, koboldTransform.position + koboldTransform.forward+Vector3.up, Quaternion.identity);
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
            if(!isSetup)   //Using existing UI if it possible, if you sell the kobold that had it last youll probably need to re-initialize it, Maybe find some non-kobold object to attach itself to on disable to reduce this?
            {   
                await furnitureDatabase.Setup(); //Should probably be done during scene loading
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
                            catTemp=Instantiate(categoryLabel);
                            catTemp.transform.SetParent(contentTransform,false);
                            catTemp.GetComponent<CategoryLabel>().GetName().GetComponent<TMP_Text>().text=catName;
                            catPanelTransform=catTemp.transform;
                                catTemp.GetComponent<CategoryLabel>().GetName().GetComponent<Button>().onClick.AddListener( ()=>
                                            {

                                            catTemp.GetComponent<CategoryLabel>().Toggle();
                                            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
                                            } 
                                );
                            foreach(Furniture furn in furnitureDatabase.GetCategory(catName))
                            {   
                                int copyi = i;
                                int copyj = j;
                                temp = Instantiate(itemLabel);
                                temp.GetComponent<TMP_Text>().text = "  "+furn.name; //Pivots of labels need to be directly under eachother for auto navigation to work so i use this for indentation
                                temp.GetComponent<Button>().onClick.AddListener( ()=>{SetInfo(copyi,copyj);} );
                                temp.transform.SetParent(catPanelTransform,false);
                                temp.SetActive(false);
                                j++;
                            }
                            
                            i++;
                            j=0;
                        
                        }
                i = 0;
                
                loosePanel=Instantiate(loosePanelPrefab);
                loosePanel.transform.SetParent(contentTransform,false);
                loosePanel.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,0);
                foreach(Furniture furn in furnitureDatabase.GetList())
                {   
                    int copy = i;
                    temp = Instantiate(itemLabel);
                    temp.transform.SetParent(loosePanel.transform,false);

                    temp.GetComponent<TMP_Text>().text = furn.name;
                    temp.GetComponent<Button>().onClick.AddListener( ()=>{SetInfo(copy);} );
                    
                    i++;
                }

                

                isSetup=true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
                
                AddKeys();
            }else{
                
                RectTransform rt=GetComponent<RectTransform>();
                rt.localEulerAngles = Vector3.zero;     //UI would get rotated/moved/rescaled when moving from kobold to kobold, idk why, this resets it
                rt.localPosition=Vector3.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale=Vector3.one;
                AddKeys();
                
            }
        
    }
     void OnDisable()
    {   Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        RemoveKeys();
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
