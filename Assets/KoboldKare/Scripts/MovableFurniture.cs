using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


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
        //animator.SetBool("Open", true);
    }

    [PunRPC]
    public void OnReleaseRPC(int koboldID, Vector3 velocity)
    {
        //animator.SetBool("Open", false);
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
