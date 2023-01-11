using System;
using System.Collections;
using System.IO;
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

    public override bool CanUse(Kobold k) {
        return constructed;
    }

    public override Sprite GetSprite(Kobold k) {
        return useSprite;
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

    public override void Use() {
        SetSelected(selectedMode + 1);
        StopAllCoroutines();
        source.enabled = true;
        selectPack.Play(source);
        StartCoroutine(DisableAfterTime());
    }

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

    public override void Load(JSONNode node) {
        base.Load(node);
        SetSelected(node["selected"]);
    }

    IEnumerator DisableAfterTime() {
        yield return new WaitForSeconds(source.clip.length+0.1f);
        source.enabled = false;
    }
}
