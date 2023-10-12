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
            Transform koboldTransform = kobold.hip.transform;
            if(kobold.GetComponent<MoneyHolder>().HasMoney(selected.price))
            {
                kobold.GetComponent<MoneyHolder>().ChargeMoney(selected.price);
                kobold.photonView.RPC("Spawn", RpcTarget.All,selected.prefab.photonName, koboldTransform.position + koboldTransform.forward+Vector3.up, Quaternion.identity);
            }
        
        }
    }
    public void SetUser(Kobold user){
        kobold= user;
    }

    async void OnEnable()
    {       
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
                if(!isSetup)   //Using existing UI if it possible, if you sell the kobold that had it last youll probably need to re-initialize it, Maybe find some non-kobold object to attach itself to on disable to reduce this?
            {   
                await shopDatabase.Setup(); //Should probably be done during scene loading
                foreach(ShopCategory category in shopDatabase.subCategories){
                    SetupCategory(contentTransform,category);
                }
                SetupItems(contentTransform,shopDatabase.items);
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
    
    private void SetupCategory(Transform parent,ShopCategory category){
                            Transform catTemp;
                            catTemp=Instantiate(categoryLabel).transform;
                            catTemp.SetParent(parent.transform,false);
                            catTemp.GetComponent<ShopCategoryLabel>().GetNameText().text=category.categoryName;
                            catTemp.GetChild(0).GetComponent<Button>().onClick.AddListener( ()=>
                                            {
                                            catTemp.GetComponent<ShopCategoryLabel>().Toggle();
                                            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
                                            } 
                                );
                            foreach(ShopCategory childCategory in category.subCategories){
                                SetupCategory(catTemp.GetChild(1),childCategory);
                            }
                            SetupItems(catTemp.GetChild(1),category.items);
    }
    private void SetupItems(Transform parent, List<ShopItem> items){
            foreach(ShopItem childItem in items){
            GameObject itemTemp;
            itemTemp=Instantiate(itemLabel);
            itemTemp.transform.SetParent(parent,false);
            itemTemp.GetComponent<TMP_Text>().text=childItem.itemName;
            itemTemp.GetComponent<Button>().onClick.AddListener( ()=>{
                SetInfo(childItem);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
            } );
            }
    }

}
