using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

[RequireComponent(typeof(PhotonView))]
public class AnimationStationSet : MonoBehaviourPun {
    [SerializeField]
    private List<AnimationStation> stations;

    private ReadOnlyCollection<AnimationStation> readonlyStations;
    private SphereCollider sphereCollider;
    
    void Awake() {
        readonlyStations = stations.AsReadOnly();
        gameObject.layer = LayerMask.NameToLayer("AnimationSet");
        Bounds animationSetBounds = new Bounds(stations[0].transform.position, Vector3.zero);
        foreach (AnimationStation station in stations) {
            animationSetBounds.Encapsulate(station.transform.position);
        }
        sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = Mathf.Max(Mathf.Max(animationSetBounds.extents.x, animationSetBounds.extents.y),
            animationSetBounds.extents.z);
        sphereCollider.center = transform.InverseTransformPoint(animationSetBounds.center);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readonlyStations;
    }
}
