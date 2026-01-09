using System;
using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class Creature : MonoBehaviour, IDamagable {
    [SerializeField]
    private PhotonGameObjectReference spawnOnDeath;
    [SerializeField]
    private VisualEffect splashPrefab;
    [SerializeField]
    private float health = 1f;

    [SerializeField] private float speed = 4f;

    [SerializeField] private CreaturePath targetPath;

    private float distanceTravelled = 0f;
    private float distanceTravelledVel;
    private float networkedDistanceTravelled = 0f;
    private NetworkedEntity networkedEntity;

    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (!networkedEntity) {
            return;
        }

        networkedEntity.grabRequested += OnGrabRequested;
        networkedEntity.grabbed += OnGrab;
        networkedEntity.released += OnRelease;
        networkedEntity.SetGrabTransform(transform);
    }

    [SerializeField] private AudioPack gibSound;
    private bool OnGrabRequested(NetworkedKobold by) {
        return true;
    }
    private void OnGrab(NetworkedKobold by) {
        Die();
    }
    private void OnRelease(NetworkedKobold by, Vector3 velocity) {
    }

    public void Update() {
        if (targetPath == null) {
            return;
        }

        // FIXME FISHNET
        /*
        if (!photonView.IsMine) {
            if (Mathf.Abs(distanceTravelled - networkedDistanceTravelled) > 5f) {
                distanceTravelled = networkedDistanceTravelled;
            }
            distanceTravelled = Mathf.SmoothDamp(distanceTravelled, networkedDistanceTravelled, ref distanceTravelledVel, 1f);
        } else {
            distanceTravelled += Time.deltaTime*speed;
            if (distanceTravelled > targetPath.GetPath().arcLength) {
                distanceTravelled -= targetPath.GetPath().arcLength;
            }
        }

        transform.position = targetPath.GetPath().GetPositionFromDistance(distanceTravelled);
        transform.forward = targetPath.GetPath().GetVelocityFromDistance(distanceTravelled).normalized;*/
    }


    public float GetHealth() {
        return health;
    }

    public void Damage(float amount) {
        // FIXME FISHNET
        /*if (health < 0f || !photonView.IsMine) {
            return;
        }
        health -= amount;
        if (health < 0f) {
            photonView.RPC(nameof(Die), RpcTarget.All);
        }*/
    }

    // FIXME FISHNET
    //[PunRPC]
    private void Die() {
        var effect = GameObject.Instantiate(splashPrefab.gameObject, transform.position, Quaternion.identity);
        effect.GetComponent<VisualEffect>().SetVector4("Color", Color.red);
        GameManager.instance.SpawnAudioClipInWorld(gibSound, transform.position);
        Destroy(effect, 5f);
        
        /*
        if (!photonView.IsMine) {
            return;
        }
        PhotonNetwork.Instantiate(spawnOnDeath.photonName, transform.position, transform.rotation);
        PhotonNetwork.Destroy(gameObject);*/
    }

    public void Heal(float amount) {
        health += amount;
    }

    
    // FIXME FISHNET
    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(health);
            stream.SendNext(distanceTravelled);
        } else {
            // we sync death via RPC, so we just sync the health variable without triggering anything else.
            health = (float)stream.ReceiveNext();
            networkedDistanceTravelled = (float)stream.ReceiveNext();
            PhotonProfiler.LogReceive(sizeof(float) * 2);
        }
    }*/

    private void OnCollisionEnter(Collision collision) {
        if (collision.rigidbody != null && collision.impulse.magnitude > 1f) {
            Damage(collision.impulse.magnitude);
        }
    }

    // FIXME FISHNET
    /*public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData == null) {
            return;
        }

        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is int) {
            targetPath = PhotonNetwork.GetPhotonView((int)info.photonView.InstantiationData[0]).GetComponentInChildren<CreaturePath>();
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }*/
}
