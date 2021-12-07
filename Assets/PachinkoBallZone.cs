using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PachinkoBallZone : MonoBehaviour{
    public enum RewardTypes{ Equipment, Money, Upgrade };
    public Pachinko pachinkoMachine;
    RewardTypes myRewardType;
    public int zoneID;
    void Start(){
        myRewardType = RewardTypes.Money;
    }

    void OnTriggerEnter(Collider other)    {
        if(other.tag == "Bullet"){
            pachinkoMachine.ReachedBottom(this);
        }
    }
}
