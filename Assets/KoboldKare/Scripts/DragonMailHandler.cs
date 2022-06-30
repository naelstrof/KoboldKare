using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class DragonMailHandler : MonoBehaviour, IPunObservable{
    public GameObject selectOnStart;
    public ScriptableFloat dragonMoneyGoal;
    public GameObject viewMain, viewMoneyDonate, viewMoneyConfirm, viewKoboldSend, viewKoboldRetrieve, viewKoboldReceipt;
    public Slider mainviewMoneySlider, donationSlider;
    public TextMeshProUGUI moneyLabel, moneyValue;
    public GameObject koboldBoxPrefab, koboldBoxInstance;
    public Transform boxSpawnPoint;
    PhotonView pView;
    public bool dmActive;
    public static DragonMailHandler inst;
    public Canvas dmMainCanvas;
    void Start(){
        pView = GetComponent<PhotonView>();
        RefreshMoneyGoal();
        inst = this;
    }

    public void RefreshMoneyGoal(){
        moneyValue.text = string.Format("{0}/{1} {2}",dragonMoneyGoal.value.ToString(),dragonMoneyGoal.max.ToString(), (dragonMoneyGoal.value/dragonMoneyGoal.max).ToString("P"));
        mainviewMoneySlider.maxValue = dragonMoneyGoal.max;
        mainviewMoneySlider.value = dragonMoneyGoal.value;
    }

    public void SwitchToMain(){
        Debug.Log("switching to Main");
        EventSystem.current.SetSelectedGameObject(selectOnStart);
        TurnOn(viewMain);
        RefreshMoneyGoal();
        dmActive = true;
    }

    public void SwitchToMoneyDonate(){
        Debug.Log("switching to viewMoneyDonate");
        TurnOn(viewMoneyDonate);
    }

    public void SwitchToMoneyConfirm(){
        TurnOn(viewMoneyConfirm);
    }

    public void SwitchToKoboldDonate(){
        Debug.Log("switching to KoboldDonate");
        if(koboldBoxInstance == null){
            TurnOn(viewKoboldSend);
        }
        else{ 
            TurnOn(viewKoboldRetrieve);
        }
    }

    public void SwitchToKoboldReceipt(){
        TurnOn(viewKoboldReceipt);
    }

    void TurnOffAll(){
        Debug.Log("Turning off all");
        TurnOff(viewMain);
        TurnOff(viewMoneyDonate);
        TurnOff(viewMoneyConfirm);
        TurnOff(viewKoboldReceipt);
        TurnOff(viewKoboldRetrieve);
        TurnOff(viewKoboldSend);
    }

    void TurnOff(GameObject go){
        Debug.Log("turned off"+go.name+" with GUID "+go.GetInstanceID().ToString());
        go.GetComponent<Animator>().SetBool("Open", false);
        go.GetComponent<CanvasGroup>().interactable = false;
        go.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    void TurnOn(GameObject go){
        TurnOffAll();
        Debug.Log("turning on"+go.name+" with GUID "+go.GetInstanceID().ToString());
        go.GetComponent<Animator>().SetBool("Open", true);
        go.GetComponent<CanvasGroup>().interactable = true;
        go.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }


    public void AddMoneyToGoal(){ //TODO: Needs to handle network environment
        dragonMoneyGoal.give(donationSlider.value);
        if(dragonMoneyGoal.value >= dragonMoneyGoal.max){
            //Handle dragon spawning with accompanying UI fanfare etc
        }
        RefreshMoneyGoal();
    }

    public void Close(){
        dmActive = false;
        dmMainCanvas.GetComponent<Animator>().SetBool("Open", dmActive);
        dmMainCanvas.GetComponent<CanvasGroup>().blocksRaycasts = dmActive;
        dmMainCanvas.GetComponent<CanvasGroup>().interactable = dmActive;
        Cursor.lockState = CursorLockMode.None;
        dmMainCanvas.enabled = dmActive;
    }

    public void Open(){
        dmActive = true;
        dmMainCanvas.GetComponent<Animator>().SetBool("Open", dmActive);
        dmMainCanvas.GetComponent<CanvasGroup>().blocksRaycasts = dmActive;
        dmMainCanvas.GetComponent<CanvasGroup>().interactable = dmActive;
        Cursor.lockState = CursorLockMode.None;
        dmMainCanvas.enabled = dmActive;
    }

    public void Toggle(){
        if(dmActive){ Close(); }
        else if (!dmActive){
            Open();
            SwitchToMain();
        }
        
    }

    public void SendDonationBox(){
        if(koboldBoxInstance != null){
            PhotonNetwork.Destroy(koboldBoxInstance.GetComponent<PhotonView>());
        }
        koboldBoxInstance = PhotonNetwork.Instantiate("DonationBox",boxSpawnPoint.position,Quaternion.identity);
    }
    public void RetrieveDonationBox(){
        //Handle retrieval of box data-wise, awarding appropriately
        if(koboldBoxInstance != null){
            PhotonNetwork.Destroy(koboldBoxInstance);
        }
        else{
            Debug.LogWarning("[DragonMailHandler] :: Attempted to destroy SendBox which did not exist");
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
    }
}
