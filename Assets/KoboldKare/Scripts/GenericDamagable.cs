using KoboldKare;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;
using UnityEngine.Events;

public class GenericDamagable : MonoBehaviourPun {
    public float health { get; set; } = 100f;
    public float maxHealth = 100f;
    public bool removeOnDeath = true;
    public UnityEvent onDie;
    public GameObject gibPrefab;
    public Transform gibTarget;

    private void Start(){
        if (photonView == null) {
            Debug.LogWarning("Gameobject " + gameObject + " probably needs a photonview... Otherwise multiplayer interactions won't cause it to get deleted.");
        }
        health = maxHealth;
    }

    public void Damage(float amount) {
        if (health <= 0) {
            return;
        }
        health -= amount;
        if ( health <= 0 ) {
            onDie.Invoke();
            if (gibPrefab != null) {
                GameObject gibs = GameObject.Instantiate(gibPrefab, transform.position, Quaternion.identity);
                gibs.SendMessage("FitTo", gibTarget, SendMessageOptions.DontRequireReceiver);
            }
            if (removeOnDeath && photonView.IsMine) {
                if (photonView != null) {
                    PhotonNetwork.Destroy(photonView.gameObject);
                } else {
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
