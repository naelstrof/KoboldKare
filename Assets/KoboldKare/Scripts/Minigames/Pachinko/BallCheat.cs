using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallCheat : MonoBehaviourPun, IPunInstantiateMagicCallback {
    Rigidbody rb;
    Pachinko pachinko;
    public float sensitivity;
    private WaitForFixedUpdate waitForFixedUpdate;

    void Awake(){
        rb = GetComponent<Rigidbody>();
        waitForFixedUpdate = new WaitForFixedUpdate();
    }
    void Start() {
        StartCoroutine(StuckCheckRoutine());
    }

    void OnCollisionEnter(Collision collisionInfo){
        pachinko.HitPin();
    }

    //public void SetMachine(Pachinko Pachinko){
        //pachinko = Pachinko;
    //}
    IEnumerator StuckCheckRoutine() {
        int stuckCount = 0;
        while(stuckCount < 60) {
            while (rb.velocity.magnitude > sensitivity ) {
                yield return waitForFixedUpdate;
            }
            stuckCount++;
            yield return waitForFixedUpdate;
        }
        pachinko.BallStuck();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        pachinko = PhotonNetwork.GetPhotonView((int)(info.photonView.InstantiationData[0])).GetComponent<Pachinko>();
        PhotonProfiler.LogReceive(sizeof(int));
    }
}
