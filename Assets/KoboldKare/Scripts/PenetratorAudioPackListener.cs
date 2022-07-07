using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

[PenetratorListener(typeof(PenetratorAudioPackListener), "Play Audio Pack Listener")]
[System.Serializable]
public class PenetratorAudioPackListener : PenetratorListener {
    [SerializeField]
    private AudioPack pack;
    [SerializeField]
    private bool activateOnEnter;
    [SerializeField]
    private bool activateOnExit;
    private Penetrator penetrator;

    private float oldDepth = 0f;
    public override void OnEnable(Penetrator p) {
        base.OnEnable(p);
        penetrator = p;
        oldDepth = 0f;
    }

    protected override void OnPenetrationDepthChange(float newDepth) {
        base.OnPenetrationDepthChange(newDepth);
        if (!Application.isPlaying) {
            return;
        }

        if ((newDepth > oldDepth && oldDepth == 0f && activateOnEnter) || (newDepth == 0f && oldDepth != 0f && activateOnExit)) {
            GameManager.instance.SpawnAudioClipInWorld(pack, penetrator.transform.position);
        }
        oldDepth = newDepth;
    }
}
