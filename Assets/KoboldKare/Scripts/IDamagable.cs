using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public interface IDamagable {
    public float GetHealth();
    
    // FIXME FISHNET
    //[PunRPC]
    public void Damage(float amount);
    public void Heal(float amount);
}
