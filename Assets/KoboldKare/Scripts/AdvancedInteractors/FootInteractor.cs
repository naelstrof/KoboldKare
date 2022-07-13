using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class FootInteractor : MonoBehaviour, IAdvancedInteractable {
    private Kobold kobold;
    private static WaitForSeconds waitForSeconds = new WaitForSeconds(3f);
    public void OnInteract(Kobold k) {
        kobold.ragdoller.PushRagdoll();
    }
    public void Start() {
        kobold = GetComponentInParent<Kobold>();
    }
    public void OnEndInteract(Kobold k) {
        StopAllCoroutines();
        StartCoroutine(WaitThenStand());
    }
    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
    }
    public bool ShowHand() {
        return true;
    }
    public bool PhysicsGrabbable() {
        return true;
    }

    private IEnumerator WaitThenStand() {
        yield return waitForSeconds;
        kobold.ragdoller.PopRagdoll();
    }
}
