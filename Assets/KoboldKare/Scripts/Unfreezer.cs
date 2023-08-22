using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class Unfreezer : GenericWeapon, IValuedGood, IGrabbable
{
    public bool firing = false;
    [SerializeField]
    private Transform center;
    public GameObject scanBeam;
    public Transform laserEmitterLocation;

    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    {
        base.OnFireRPC(playerViewID);
        if (firing)
        {
            return;
        }
        firing = true;
        RaycastHit hit;
        Physics.Raycast(laserEmitterLocation.position - laserEmitterLocation.forward * 0.25f, laserEmitterLocation.forward, out hit, 10f, 
                        GameManager.instance.waterSprayHitMask, QueryTriggerInteraction.Ignore);
        hit.collider.GetComponentInParent<Freezable>()?.Unfreeze();

    }
    [PunRPC]
    protected override void OnEndFireRPC(int viewID)
    {
        firing = false;
       
    }
    public Vector3 GetWeaponPositionOffset(Transform grabber)
    {
        return (grabber.up * 0.1f + grabber.right * 0.5f - grabber.forward * 0.25f);
    }

    public bool ShouldSave()
    {
        return true;
    }
    public float GetWorth()
    {
        return 15f;
    }

    public bool CanGrab(Kobold kobold)
    {
        return true;
    }

    [PunRPC]
    public void OnGrabRPC(int koboldID)
    {
        scanBeam.SetActive(true);
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity)
    {
        scanBeam.SetActive(false);
    }

    public Transform GrabTransform()
    {
        return center;
    }
}
