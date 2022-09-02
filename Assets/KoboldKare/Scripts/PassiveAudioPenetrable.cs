using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

public class PassiveAudioPenetrable : PenetrableListener {
    [SerializeField] private AudioPack playPack;
    [SerializeField] private float delayVariance = 2f;
    private float nextTime;
    
    private Penetrable pen;

    protected override void OnPenetrationDepthChange(float newDepth) {
        if (newDepth > 0f && Time.time > nextTime) {
            AudioClip clip = GameManager.instance.SpawnAudioClipInWorld(playPack, pen.GetSplinePath().GetPositionFromT(GetT(pen)));
            nextTime = Time.time + clip.length+Random.Range(0,delayVariance);
        }
    }

    public override void OnEnable(Penetrable p) {
        base.OnEnable(p);
        
        nextTime = Time.time;
        pen = p;
    }

    public override void AssertValid() {
        base.AssertValid();
        if (playPack == null) {
            throw new PenetrableListenerValidationException($"playPack is null on {this}");
        }
    }

    public override void NotifyPenetration(Penetrable penetrable, Penetrator penetrator, float worldSpaceDistanceToPenisRoot, Penetrable.SetClipDistanceAction clipAction) {
        NotifyPenetrationGDO(penetrable, penetrator, worldSpaceDistanceToPenisRoot, clipAction, PenData.Depth);
    }
}
