using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

public class KoboldDelivery : UsableMachine {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private RotateSelectorUsable priceSelector;
    [SerializeField] private Transform popOutLocation;
    [SerializeField] private VisualEffect poof;
    [SerializeField] private PhotonGameObjectReference koboldPrefab;
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private MoneyFloater floater;
    [SerializeField] private AudioPack popPack;
    private AudioSource source;
    private static readonly int Dispense = Animator.StringToHash("Dispense");
    private NetworkedKobold networkedKobold;

    public delegate void SpawnedKoboldAction(Kobold kob);

    public static event SpawnedKoboldAction spawnedKobold;
    
    private float GetPrice() {
        return 50f + priceSelector.GetSelected() * 50f * priceSelector.GetSelected();
    }

    private void Awake() {
        priceSelector.rotated += OnRotated;
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
        networkedKobold = GetComponentInParent<NetworkedKobold>();
        if (networkedKobold) {
            networkedKobold.SetSprite(useSprite);
            networkedKobold.used += OnUse;
            networkedKobold.useRequested += OnUseRequested;
        }
    }

    void OnRotated(int newRotation) {
        floater.SetText(GetPrice().ToString(CultureInfo.CurrentCulture));
    }

    private bool OnUseRequested(NetworkedKobold k) {
        if (k.TryGetKobold(out Kobold kobold)) {
            MoneyHolder holder = kobold.GetComponent<MoneyHolder>();
            return constructed && holder.HasMoney(GetPrice());
        }

        return false;
    }

    private void OnUse(NetworkedKobold k) {
        if (!OnUseRequested(k)) {
            return;
        }

        if (k.TryGetKobold(out Kobold kobold)) {
            MoneyHolder holder = kobold.GetComponent<MoneyHolder>();
            holder.ChargeMoney(GetPrice());
            StartCoroutine(DispenseKobold());
        }
    }

    private IEnumerator DispenseKobold() {
        targetAnimator.SetTrigger(Dispense);
        yield return new WaitForSeconds(2f);
        
        // FIXME FISHNET
        /*
        if (!photonView.IsMine) {
            yield break;
        }

        poof.SendEvent("TriggerPoof");
        string koboldName = koboldPrefab.photonName;
        KoboldGenes genes =
            new KoboldGenes().Randomize(koboldName, 0.4f + priceSelector.GetSelected() * priceSelector.GetSelected());
        BitBuffer playerSpawnData = new BitBuffer(16);
        playerSpawnData.AddKoboldGenes(genes);
        playerSpawnData.AddBool(false);
        
        GameObject obj = PhotonNetwork.InstantiateRoomObject(koboldName, popOutLocation.transform.position,
            Quaternion.identity, 0, new object[] { playerSpawnData });
        spawnedKobold?.Invoke(obj.GetComponent<Kobold>());
        source.enabled = true;
        popPack.Play(source);
        yield return new WaitForSeconds(source.clip.length+0.1f);
        source.enabled = false;*/
    }

}
