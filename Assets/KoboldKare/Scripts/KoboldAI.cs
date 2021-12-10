using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoboldAI : MonoBehaviourPun {
    public GameObject playerController;
    public LayerMask usableMask;
    private Collider[] colliders = new Collider[6];
    private WaitForSeconds wait = new WaitForSeconds(1f);
    private void Start() {
        StartCoroutine(Think());
    }
    public IEnumerator Think() {
        yield return wait;
        while(isActiveAndEnabled && !playerController.activeInHierarchy && photonView.IsMine) {
            yield return wait;
            int hits = Physics.OverlapSphereNonAlloc(transform.position, 1.3f, colliders);
            for(int i=0;i<hits;i++) {
                Collider hitCollider = colliders[i];
                GenericUsable g = hitCollider.GetComponentInParent<GenericUsable>();
                PenetrationTech.Penetrator d = hitCollider.GetComponentInParent<PenetrationTech.Penetrator>();
                if (g != null && g.gameObject != photonView.gameObject) {
                    // Skip dicks that are penetrating stuff, don't use those!
                    if (d!=null) {
                        if (d.holeTarget != null) {
                            continue;
                        }
                    }
                    if (g.CanUse(GetComponentInParent<Kobold>())) {
                        g.Use(GetComponentInParent<Kobold>());
                        break;
                    }
                }
            }
        }
    }
}
