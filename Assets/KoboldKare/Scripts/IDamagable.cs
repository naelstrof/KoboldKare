using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public interface IDamagable {
    public float GetHealth();
    [PunRPC]
    public void Damage(float amount);
    public void Heal(float amount);
    public PhotonView photonView { get; }
}
