using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ABatteryInteractable : GenericUsable
{
    public Sprite icon;
    public GameObject rootObject;
    public ABattery Battery;
    public PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override Sprite GetSprite(Kobold k) {
        return icon;
    }

    public override void Use(Kobold k) {
        base.Use(k);
        if(photonView.IsMine){
            GameObject batteryCan = PhotonNetwork.Instantiate("ABatteryCan", rootObject.transform.position, rootObject.transform.rotation, 0);
            PhotonNetwork.Instantiate("ABatteryLid", rootObject.transform.position + Vector3.right + Vector3.up, rootObject.transform.rotation, 0);
            GenericReagentContainer container = batteryCan.GetComponent<GenericReagentContainer>();
            container.OverrideReagent(ReagentDatabase.GetReagent("Cum"), Battery.fuel);
            batteryCan.GetComponent<AudioSource>().Play();
            PhotonNetwork.Destroy(rootObject);
        }
    }
}
