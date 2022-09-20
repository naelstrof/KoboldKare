using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

public class PenetratorPassiveAudioPack : PenetratorListener {
    private float lastDepth;
    [SerializeField]
    private AudioPack pack;

    private AudioSource source;

    public override void OnEnable(Penetrator newPenetrator) {
        base.OnEnable(newPenetrator);
        if (!Application.isPlaying || source != null) {
            return;
        }

        source = newPenetrator.gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.maxDistance = 8f;
        source.minDistance = 0f;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.enabled = false;
        source.hideFlags = HideFlags.DontSave;
    }

    public override void OnDisable() {
        if (Application.isPlaying) {
            Object.Destroy(source);
        }
    }

    public override void Update() {
        base.Update();
        if (!Application.isPlaying) {
            return;
        }
        source.volume = Mathf.MoveTowards(source.volume, 0f, Time.deltaTime*pack.GetVolume());
        source.pitch = Mathf.Lerp(0.5f, 1f, source.volume / Mathf.Max(pack.GetVolume(),0.01f));
        if (source.volume == 0f && lastDepth == 0f) {
            source.enabled = false;
        }
    }

    protected override void OnPenetrationDepthChange(float depth) {
        base.OnPenetrationDepthChange(depth);
        if (!Application.isPlaying) {
            return;
        }
        if (!source.enabled) {
            source.enabled = true;
        }

        if (!source.isPlaying) {
            pack.Play(source);
        }
        
        float diff = Mathf.Abs(depth - lastDepth);
        source.volume = Mathf.Min(source.volume+diff*50f*pack.GetVolume(), pack.GetVolume());
        lastDepth = depth;
    }
}
