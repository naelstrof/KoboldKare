using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class PenetratorAudioPackListener : PenetratorListener {
    [SerializeField, Range(0f,1f)]
    private float localDistanceFromTipOfDick = 0f;
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

        if ((newDepth > 0f && oldDepth <= 0f && activateOnEnter) || (newDepth <= 0f && oldDepth > 0f && activateOnExit)) {
            GameManager.instance.SpawnAudioClipInWorld(pack, penetrator.transform.position);
        }
        oldDepth = newDepth;
    }
    public override void OnDrawGizmosSelected(Penetrator p) {
#if UNITY_EDITOR
        Handles.color = Color.blue;
        float dist = (1f-localDistanceFromTipOfDick) * p.GetWorldLength();
        Vector3 position = p.GetSplinePath().GetPositionFromDistance(dist);
        Handles.Label(position, GetType().Name);
        Handles.DrawWireDisc(position, p.GetSplinePath().GetVelocityFromDistance(dist).normalized, p.GetWorldGirthRadius(dist));
#endif
    }
    public override void NotifyPenetrationUpdate(Penetrator a, Penetrable b, float distToHole) {
        float realDist = (1f - localDistanceFromTipOfDick) * a.GetWorldLength();
        float penetrateDist = realDist - distToHole;
        OnPenetrationDepthChange(penetrateDist);
    }
}
