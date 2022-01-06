using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ABatteryLid : MonoBehaviour
{
    public PhotonView photonView;
    private void OnTriggerEnter(Collider other)
    {
        GameObject collidingObject = other.gameObject;
        ABatteryCan c = collidingObject.GetComponent<ABatteryCan>();
        if(c == null) return;
        if(photonView.IsMine && c.photonView.IsMine){
            GameObject newbat = PhotonNetwork.Instantiate("ABatteryNormal", collidingObject.transform.position, collidingObject.transform.rotation, 0);
            newbat.GetComponent<ABattery>().Fuel(collidingObject.GetComponent<GenericReagentContainer>());
            newbat.GetComponent<AudioSource>().Play();
            PhotonNetwork.Destroy(collidingObject);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
