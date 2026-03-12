using System;
using System.Collections;
using System.Collections.Generic;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class ElectricBlender : SuckingMachine {
    [SerializeField]
    private FluidStream stream;
    
    [SerializeField] private VisualEffect poof;
    [SerializeField] private AudioPack grindSound;
    private AudioSource source;
    public static event GrinderManager.GrindedObjectAction grindedObject;
    private NetworkedEntity networkedEntity;

    protected override void Awake() {
        base.Awake();
        if (source == null) {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.maxDistance = 10f;
            source.minDistance = 0.2f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.spatialBlend = 1f;
            source.loop = false;
            source.enabled = false;
        }
    }

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.reagentContents.OnChange += OnFluidChanged;
        }
    }

    private void OnDestroy() {
        if (networkedEntity) {
            networkedEntity.reagentContents.OnChange -= OnFluidChanged;
        }
    }

    void OnFluidChanged(ReagentContents contents, ReagentContents next, bool asServer) {
        stream.OnFire(networkedEntity);
    }

    protected override void OnTriggerEnter(Collider other) {
        if (!constructed) {
            return;
        }

        base.OnTriggerEnter(other);
    }

    // FIXME FISHNET
    //[PunRPC]
    protected override void OnSwallowed(NetworkedEntity ent) {
        if (!constructed) {
            return;
        }
        if(suckingIDs.Contains(ent.ObjectId)) {
            return;
        }
        suckingIDs.Add(ent.ObjectId);
        // FIXME FISHNET
        /*PhotonView view = PhotonNetwork.GetPhotonView(viewID);
        poof.SendEvent("TriggerPoof");
        source.enabled = true;
        grindSound.Play(source);
        StartCoroutine(WaitThenDisableSound());
        GenericReagentContainer otherContainer = view.GetComponent<GenericReagentContainer>();
        if (otherContainer != null) {
            BitBuffer tempBuffer = new BitBuffer(16);
            tempBuffer.AddReagentContents(otherContainer.GetContents());
            container.AddMixRPC(tempBuffer, viewID, (byte)GenericReagentContainer.InjectType.Inject);
            grindedObject?.Invoke(viewID, otherContainer.GetContents());
        }*/
        base.OnSwallowed(ent);
    }

    IEnumerator WaitThenDisableSound() {
        while(source.isPlaying) {
            yield return null;
        }
        source.enabled = false;
    }
}
