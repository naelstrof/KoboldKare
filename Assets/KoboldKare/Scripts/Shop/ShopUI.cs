using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShopUI : MonoBehaviourPun
{   [SerializeField]
    private RectTransform contentTransform;
    bool isSetup = false;
    private ShopItem selected;
    [SerializeField]
    private GameObject categoryLabel;
    [SerializeField]
    private GameObject itemLabel;
    [SerializeField]
    private ShopDatabase shopDatabase;

    [SerializeField]
    private TMP_Text nameField;
    [SerializeField]
    private TMP_Text descriptionField;
    [SerializeField]
    private TMP_Text priceField;
    [SerializeField]
    private Image imageField;
    [SerializeField]
    private Sprite defaultSprite;

    private Kobold kobold;
    private ShopKeeper shopKeeper;
    private bool usingShop;

    [SerializeField]
    private InputActionReference[] actionsToClose;
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

    public void Buy(){     
        if(selected != null) { 
            if(!usingShop)
            {    Transform koboldTransform = kobold.hip.transform;
                if(kobold.GetComponent<MoneyHolder>().HasMoney(selected.GetPrice()))
                {
                kobold.GetComponent<MoneyHolder>().ChargeMoney(selected.GetPrice());
                PhotonNetwork.InstantiateRoomObject(selected.GetPrefab().photonName, koboldTransform.position + koboldTransform.forward, Quaternion.identity);
                }
            }
            else{
                if(kobold.GetComponent<MoneyHolder>().HasMoney(selected.GetPrice()))
                {
                     kobold.GetComponent<MoneyHolder>().ChargeMoney(selected.GetPrice());
                     PhotonNetwork.InstantiateRoomObject(selected.GetPrefab().photonName, shopKeeper.GetSpawnPosition(), Quaternion.identity);
                }


            }
        
        }
    }
    public void SetUser(Kobold user){
        kobold= user;
        usingShop=false;
    }
    public void SetUser(Kobold user,ShopKeeper shop){
        kobold=user;
        shopKeeper=shop;
        usingShop=true;
    }

    async void OnEnable()
    {       
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
                if(!isSetup)   //Using existing UI if it possible, if you sell the kobold that had it last it will get destroyed with it and youll need to re-initialize it, Maybe find some non-kobold object to attach itself to on disable to reduce this?
            {   
                await shopDatabase.Setup(); //Should probably be done during scene loading
                foreach(ShopCategory category in shopDatabase.GetRootCategories()){
                    SetupCategory(contentTransform,category);
                }
                SetupItems(contentTransform,shopDatabase.GetRootItems());
                AddKeys();
                isSetup=true;
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
    private void SetInfo(ShopItem temp){
        nameField.text = temp.GetName();
        descriptionField.text = temp.GetDescription();
        priceField.text = temp.GetPrice().ToString();
        if(temp.GetSprite()!=null)
        {
            imageField.sprite=temp.GetSprite();
        }
        else{
            imageField.sprite=defaultSprite;
        }
        selected = temp;
    }
    
    private void SetupCategory(Transform parent,ShopCategory category){
            Transform catTemp;
            catTemp=Instantiate(categoryLabel).transform;
            catTemp.SetParent(parent.transform,false);
            catTemp.GetComponent<ShopCategoryLabel>().GetNameText().text=category.GetName();
            catTemp.GetComponent<ShopCategoryLabel>().GetName().GetComponent<Button>().onClick.AddListener( ()=>{
                    catTemp.GetComponent<ShopCategoryLabel>().Toggle();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
                    } );
            foreach(ShopCategory childCategory in category.GetSubcategories()){
                SetupCategory(catTemp.GetComponent<ShopCategoryLabel>().GetChildren().transform,childCategory);
            }
            SetupItems(catTemp.GetComponent<ShopCategoryLabel>().GetChildren().transform,category.GetItems());
    }
    private void SetupItems(Transform parent, List<ShopItem> items){
            foreach(ShopItem childItem in items){
            GameObject itemTemp;
            itemTemp=Instantiate(itemLabel);
            itemTemp.transform.SetParent(parent,false);
            itemTemp.GetComponent<TMP_Text>().text=childItem.GetName();
            itemTemp.GetComponent<Button>().onClick.AddListener( ()=>{
                SetInfo(childItem);
                //LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
            } );
            }
    }

}
