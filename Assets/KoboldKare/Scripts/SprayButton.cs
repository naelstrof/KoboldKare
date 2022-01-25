using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayButton : GenericUsable {
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;
    public List<FluidOutputMozzarellaSquirt> squirter = new List<FluidOutputMozzarellaSquirt>();
    public AudioSource aud;
    public GenericReagentContainer targetContainer;
    private int usedCount;
    public override Sprite GetSprite(Kobold k) {
        return on ? onSprite : offSprite;
    }
    private bool on {
        get {
            return (usedCount % 2) != 0;
        }
    }
    public override void Use() {
        base.Use();
        usedCount++;
        if (on) {
            foreach (var item in squirter){
                item.Fire(targetContainer);
            }
            if(aud != null) {
                aud.Play();
            }
        } else {
            foreach (var item in squirter){
                item.StopFiring();
            }
            if(aud != null) {
                aud.Stop();
            }
        }
    }
}
