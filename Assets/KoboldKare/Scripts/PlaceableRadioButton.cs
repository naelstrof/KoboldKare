using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class PlaceableRadioButton : MonoBehaviour {
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;
    [SerializeField]
    private Sprite nextTrack;
    public AudioSource aud;
    public List<AudioClip> tracks = new List<AudioClip>();
    
    private NetworkedEntity networkedEntity;
    int trackPos;

    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(nextTrack);
            networkedEntity.useRequested += OnUseRequested;
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }

    private void OnUse(NetworkedKobold by) {
        trackPos = (int)Mathf.Repeat(trackPos+1,tracks.Count);
        aud.clip = tracks[trackPos];
        aud.Play();
    }
}
