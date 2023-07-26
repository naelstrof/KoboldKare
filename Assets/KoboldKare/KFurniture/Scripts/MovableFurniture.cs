using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MovableFurniture : GenericWeapon, IValuedGood, IGrabbable
{
    [SerializeField]
    private Transform center;
    [SerializeField]
    private Rigidbody rb;
    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    { rb.isKinematic=true;
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
        return !rb.isKinematic;
    }
    public void Unfreeze()
    {
        rb.isKinematic=false;
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
    {   if(rb==null)
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
