using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class DrainUsable : UsableMachine {
    [SerializeField] private AudioPack drainSound;
    [SerializeField] private Sprite displaySprite;
    private AudioSource audioSource;
    private NetworkedEntity networkedEntity;
    
    protected override void Start() {
        base.Start();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.maxDistance = 10f;
            audioSource.minDistance = 0.2f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;
            audioSource.enabled = false;
        }

        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(displaySprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool draining;
    private bool OnUseRequested(NetworkedKobold k) {
        return constructed && networkedEntity.reagentContents.Value.volume > 0.01f;
    }

    private void OnUse(NetworkedKobold by) {
        StartCoroutine(Drain());
    }

    IEnumerator Drain() {
        draining = true;
        audioSource.enabled = true;
        drainSound.Play(audioSource);
        
        while (networkedEntity.reagentContents.Value.volume > 0.01f) {
            networkedEntity.Spill(Time.deltaTime * 10f);
            yield return null;
        }
        audioSource.enabled = false;
        draining = false;
    }

    // FIXME FISHNET
    /*public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(draining);
        } else {
            bool newDraining = (bool)stream.ReceiveNext();
            if (!draining && newDraining) {
                StartCoroutine(Drain());
            }
            draining = newDraining;
            PhotonProfiler.LogReceive(sizeof(bool));
        }
    }

    public override Task Load(JSONNode node) {
        base.Load(node);
        bool newDraining = node["draining"];
        if (!draining && newDraining) {
            StartCoroutine(Drain());
        }
        draining = newDraining;
        return Task.CompletedTask;
    }

    public override void Save(JSONNode node) {
        base.Save(node);
        node["draining"] = draining;
    }*/
}
