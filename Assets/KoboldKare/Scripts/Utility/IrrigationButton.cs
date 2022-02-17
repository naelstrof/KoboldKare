using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class IrrigationButton : GenericUsable{
    [SerializeField]
    private Sprite spraySprite, disabledSprite;
    public IrrigationZone iz;
    private int usedCount;
    public bool interactable;

    public override Sprite GetSprite(Kobold k){
        if(interactable == true){
            return spraySprite;
        }
        else{
            return disabledSprite;
        }
    }

    public void ChangeInteractableState(bool toState){
        interactable = toState;
    }

    public override void Use(){
        base.Use();
        usedCount++;
        iz.InjectReagent();
    }
}
