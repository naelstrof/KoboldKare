using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ABattery : MonoBehaviour
{
    public GameObject itemTearDrop;
    public GameObject interactor;
    public GenericReagentContainer container;
    public Transform rootTransform;
    public PhotonView photonView;
    public GenericAttachable attachable;
    public float fuel;
    public float slowUpdateRate = 2f;
    private float nextSlowUpdate = 0f;

    public void Fuel(GenericReagentContainer c){
        fuel = c.GetVolumeOf(ReagentDatabase.GetReagent("Cum"));
    }

    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // // Update is called once per frame
    void Update()
    {
        if(nextSlowUpdate <= Time.timeSinceLevelLoad){
            slowUpdate();
            nextSlowUpdate = Time.timeSinceLevelLoad + slowUpdateRate;
        }
    }

    void slowUpdate(){
        container.OverrideReagent(ReagentDatabase.GetReagent("Cum"), fuel);
    }
}
