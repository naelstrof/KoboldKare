using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(Photon.Pun.PhotonView)), RequireComponent(typeof(GenericReagentContainer))]
public class BombUsable : GenericUsable, IDamagable {
    [SerializeField]
    private Sprite bombSprite;
    private bool fired = false;
    private Animator animator; 
    [SerializeField]
    private VisualEffect effect;
    private GenericReagentContainer container;
    void Start() {
        container = GetComponent<GenericReagentContainer>();
        animator = GetComponent<Animator>();
    }
    public override Sprite GetSprite(Kobold k) {
        return bombSprite;
    }
    [PunRPC]
    public override void Use() {
        base.Use();
        Fire();
    }

    private void Fire() {
        if (fired) {
            return;
        }

        effect.gameObject.SetActive(true);
        animator.SetTrigger("Burn");
        if (photonView.IsMine) {
            // Mix the water with the potassium...
            // It should sizzle and blow up.
            container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All,
                ReagentDatabase.GetReagent("Water").GetReagent(40f), container.photonView.ViewID);
        }
        fired = true;
    }

    public float GetHealth() {
        return 1f;
    }

    public void Damage(float amount) {
        if (!fired) {
            Fire();
        } else {
            if (photonView.IsMine) {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public void Heal(float amount) {
    }
}
