using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using NetStack.Serialization;
using SimpleJSON;

public class MovableFurniture : GenericWeapon, IValuedGood, IGrabbable, ISavable, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField]
    private Transform center;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private bool needsConsistentViewId;

    [PunRPC]
    protected override void OnFireRPC(int playerViewID)
    { Freeze();
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
    public void Freeze(){
        rb.isKinematic=true;
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
        //rb.isKinematic=true;
        
    }


    // Update is called once per frame
    void Update()
    {

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            bool frozen=rb.isKinematic;
            stream.SendNext(frozen);
            
        } else {
            bool frozen = (bool)stream.ReceiveNext();
            rb.isKinematic=frozen;
            
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        
        if (info.photonView.InstantiationData == null) {
            return;
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is bool) {
            rb.isKinematic=(bool)info.photonView.InstantiationData[0];
        }
        if (info.photonView.InstantiationData.Length > 0 && info.photonView.InstantiationData[0] is not BitBuffer) {
            throw new UnityException("Unexpected spawn data for container");
        }
    }

    public void Save(JSONNode node) {
        
        JSONNode rootNode = JSONNode.Parse("{}");
        rootNode["frozen"] = rb.isKinematic.ToString();
        if(needsConsistentViewId)
        rootNode["id"] = photonView.ViewID;
        var rotation = GetComponent<Transform>().rotation;
        node["rotation.x"] = rotation.x;
        node["rotation.y"] = rotation.y;
        node["rotation.z"] = rotation.z;
        node["rotation.w"] = rotation.w;
        node["state"] = rootNode;
    }

    public void Load(JSONNode node) {
        
            JSONNode rootNode = node["state"];
            rb.isKinematic=rootNode["frozen"];
            if(needsConsistentViewId)
            GrabId(rootNode["id"]);

            float rx = node["rotation.x"];
            float ry = node["rotation.y"];
            float rz = node["rotation.z"];
            float rw = node["rotation.w"];
            GetComponent<Transform>().rotation = new Quaternion(rx, ry, rz, rw);
            }

    private void GrabId(int wantedId){
        if(photonView.ViewID==wantedId)
            return;
        PhotonView tempView=PhotonView.Find(wantedId);
        if(tempView!=null){
            int replaceId=wantedId;
            while(PhotonView.Find(replaceId)!=null){
                replaceId=replaceId+5;
            }
            tempView.ViewID=0;
            tempView.ViewID=replaceId;
            }
        photonView.ViewID=0;
        photonView.ViewID=wantedId;
    }
}
