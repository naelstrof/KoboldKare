using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class PlaceableRadioButton : GenericUsable{
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;
    [SerializeField]
    private Sprite nextTrack;
    public AudioSource aud;
    public List<AudioClip> tracks = new List<AudioClip>();
    int trackPos;
    public override Sprite GetSprite(Kobold k) {
        //return on ? onSprite : offSprite;
        return nextTrack;
    }

    public override void Use() {
        base.Use();
        trackPos = (int)Mathf.Repeat(trackPos+1,tracks.Count);
        aud.clip = tracks[trackPos];
        aud.Play();
    }
}
