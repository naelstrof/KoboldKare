using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DragonMailHandler : MonoBehaviour{
    public GameObject selectOnStart;
    public ScriptableFloat dragonMoneyGoal;
    public GameObject viewMain, viewMoneyDonate, viewMoneyConfirm, viewKoboldSend, viewKoboldRetrieve, viewKoboldReceipt;
    public Slider mainviewMoneySlider, donationSlider;
    public TextMeshProUGUI moneyLabel;
    public GameObject koboldBoxPrefab, koboldBoxInstance;

    void OnEnable(){   
        SwitchToMain();
    }

    public void SwitchToMain(){
        EventSystem.current.SetSelectedGameObject(selectOnStart);
        TurnOn(viewMain);
    }

    public void SwitchToMoneyDonate(){
        TurnOn(viewMoneyDonate);
    }

    public void SwitchToMoneyConfirm(){
        TurnOn(viewMoneyConfirm);
    }

    public void SwitchToKoboldDonate(){
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
        TurnOff(viewMain);
        TurnOff(viewMoneyDonate);
        TurnOff(viewMoneyConfirm);
        TurnOff(viewKoboldReceipt);
        TurnOff(viewKoboldRetrieve);
        TurnOff(viewKoboldSend);
    }

    void TurnOff(GameObject go){
        go.GetComponent<Animator>().SetTrigger("Close");
        go.GetComponent<CanvasGroup>().interactable = false;
        go.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    void TurnOn(GameObject go){
        TurnOffAll();
        go.GetComponent<Animator>().SetTrigger("Open");
        go.GetComponent<CanvasGroup>().interactable = true;
        go.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }


    public void AddMoneyToGoal(){ //TODO: Needs to handle network environment
        dragonMoneyGoal.give(donationSlider.value);
        if(dragonMoneyGoal.value >= dragonMoneyGoal.max){
            //Handle dragon spawning with accompanying UI fanfare etc
        }
        UpdateMoneyGoal();
    }

    public void UpdateMoneyGoal(){
        mainviewMoneySlider.value = dragonMoneyGoal.value;
        moneyLabel.text = dragonMoneyGoal.value.ToString("C")+"/"+dragonMoneyGoal.max.ToString("C");
    }

    public void Close(){ //TODO : Make animated
        gameObject.SetActive(false);
    }
}
