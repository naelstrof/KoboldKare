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
    private bool on {
        get {
            return (usedCount % 2) != 0;
        }
    }
    public override Sprite GetSprite(Kobold k) {
        //return on ? onSprite : offSprite;
        return nextTrack;
    }

    public override void Use(Kobold k) {
        base.Use(k);
        trackPos = (int)Mathf.Repeat(trackPos+1,tracks.Count);
        aud.clip = tracks[trackPos];
        aud.Play();
    }
}
