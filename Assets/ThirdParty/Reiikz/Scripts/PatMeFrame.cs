using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PatMeFrame : GenericUsable
{
    public Sprite patSprite;
    public float amont = 1f;
    public float useEach = 1f;
    Dictionary<int, float> usedby = new Dictionary<int, float>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override Sprite GetSprite(Kobold k) {
        return patSprite;
    }

    public override void Use(Kobold k) {
        base.Use(k);
        KoboldInventory inventory = k.GetComponent<KoboldInventory>();
        if (inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) == null){
            while(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch) != null) {
                inventory.RemoveEquipment(inventory.GetEquipmentInSlot(Equipment.EquipmentSlot.Crotch),false);
            }
            inventory.PickupEquipment(EquipmentDatabase.GetEquipment("KandiDick"), null);
        }
        int pid = k.GetComponent<PhotonView>().ViewID;
        if(usedby.ContainsKey(pid)){
            if((usedby[pid] + useEach) < Time.timeSinceLevelLoad){
                if(k.baseDickSize > 30){
                    k.baseDickSize *= 1.05f;
                }else{
                    k.baseDickSize += amont;
                }
                usedby[pid] = Time.timeSinceLevelLoad;
            }
        }else{
            if(k.baseDickSize > 30){
                k.baseDickSize *= 1.05f;
            }else{
                k.baseDickSize += amont;
            }
            usedby.Add(pid, Time.timeSinceLevelLoad);
        }
    }
}
