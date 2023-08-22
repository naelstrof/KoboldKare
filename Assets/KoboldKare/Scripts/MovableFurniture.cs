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
    
    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    { freezable.Freeze();
    }

    
    public float GetWorth()
    {
        return 15f;
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
    // Start is called before the first frame update
    void Start()
    { 

        
    }


    // Update is called once per frame
    void Update()
    {

    }
    

}
