using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class MovableFurniture : GenericWeapon, IValuedGood, IGrabbable
{
    [SerializeField]
    private Transform center;
    [SerializeField]
    private Freezable freezable;
    [SerializeField]
    private float worth;

    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    { 
        freezable.Freeze();
    }

    
    public float GetWorth()
    {
        return worth;
    }

    public bool CanGrab(Kobold kobold)
    {   
        return !freezable.IsFrozen;
    }
   

    [PunRPC]
    public void OnGrabRPC(int koboldID)
    { 
        PhotonProfiler.LogReceive(sizeof(int));
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity)
    {
        PhotonProfiler.LogReceive(sizeof(int)+sizeof(float)*3);
    }

    public Transform GrabTransform()
    {
        return center;
    }

}
