using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Photon.Pun;
using SimpleJSON;
using UnityEngine;

public class RotateSelectorUsable : UsableMachine {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private Transform spinnyWheel;
    [SerializeField] private AudioPack selectPack;
    private AudioSource source;
    private int selectedMode;
    private const int maxSelections = 4;
    private Quaternion startRotation;
    
    private NetworkedEntity networkedEntity;
    

    public delegate void RotatedAction(int newValue);

    public event RotatedAction rotated;

    private void Awake() {
        startRotation = spinnyWheel.localRotation;
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
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
            networkedEntity.SetSprite(useSprite);
        }
    }

    private bool OnUseRequested(NetworkedKobold k) {
        return constructed;
    }

    private void SetSelected(int select) {
        int newValue = select % maxSelections;
        if (newValue == selectedMode) {
            return;
        }
        selectedMode = select % maxSelections;
        spinnyWheel.localRotation = Quaternion.AngleAxis(selectedMode * 360f / maxSelections, -Vector3.right) * startRotation;
        rotated?.Invoke(select);
    }

    public int GetSelected() {
        return selectedMode;
    }

    private void OnUse(NetworkedKobold k) {
        SetSelected(selectedMode + 1);
        StopAllCoroutines();
        source.enabled = true;
        selectPack.Play(source);
        StartCoroutine(DisableAfterTime());
    }

    
    // FIXME FISHNET
    /*
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting) {
            stream.SendNext(GetSelected());
        } else {
            SetSelected((int)stream.ReceiveNext());
            PhotonProfiler.LogReceive(sizeof(int));
        }
    }

    public override void Save(JSONNode node) {
        base.Save(node);
        node["selected"] = GetSelected();
    }

    public override Task Load(JSONNode node) {
        base.Load(node);
        SetSelected(node["selected"]);
        return Task.CompletedTask;
    }*/

    IEnumerator DisableAfterTime() {
        yield return new WaitForSeconds(source.clip.length+0.1f);
        source.enabled = false;
    }
}
