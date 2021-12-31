using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PachinkoBallZone : MonoBehaviour{
    public Pachinko pachinkoMachine;
    public int zoneID;
    void OnTriggerEnter(Collider other)    {
        if(other.tag == "Bullet"){
            pachinkoMachine.ReachedBottom(this);
        }
    }
}
