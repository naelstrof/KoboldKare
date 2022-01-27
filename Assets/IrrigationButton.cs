using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class IrrigationButton : GenericUsable{
    [SerializeField]
    private Sprite spraySprite;
    public IrrigationZone iz;
    private int usedCount;

    public override Sprite GetSprite(Kobold k){
        return spraySprite;
    }

    public override void Use(){
        base.Use();
        usedCount++;
        iz.InjectReagent();
    }
}
