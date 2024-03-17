using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using UnityEngine.VFX;
using KoboldKare;

public class Seller : GenericWeapon, IValuedGood, IGrabbable
{   
    public bool firing = false;
    [SerializeField]
    private Transform center;
    public GameObject scanBeam;
    public Transform laserEmitterLocation;
    public UnityEvent OnSuccess;
    public UnityEvent OnFailure;
    public float scanDelay = 0.09f;
    private static RaycastHit[] hits = new RaycastHit[32];
    private static RaycastHitComparer comparer = new RaycastHitComparer();
    [SerializeField] private VisualEffect poof;

    [SerializeField]
    private GameEventPhotonView soldGameEvent;

    private class RaycastHitComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
    
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
        if(hit.collider.gameObject.GetComponent<Freezable>()!=null )
                    { 
                    PhotonView view = hit.collider.gameObject.GetComponent<PhotonView>();
                    soldGameEvent.Raise(view);
                    poof.SendEvent("TriggerPoof");
                    float worth;
                    if(hit.collider.gameObject.GetComponent<InherentWorth>()!=null)
                        worth=hit.collider.gameObject.GetComponent<InherentWorth>().GetWorth();
                            else
                        worth=10f;
                    PhotonNetwork.Destroy(view.gameObject);
                    Kobold player =PhotonNetwork.GetPhotonView(playerViewID).GetComponentInParent<Kobold>();
                    player.GetComponent<MoneyHolder>().AddMoney(worth);
                    }


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
