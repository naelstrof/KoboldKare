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
    void Awake() {
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
            ReagentContents water = new ReagentContents();
            water.AddMix(ReagentDatabase.GetReagent("Water").GetReagent(20f));
            ReagentContents potassium = new ReagentContents();
            potassium.AddMix(ReagentDatabase.GetReagent("Potassium").GetReagent(20f));
            container.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, water, container.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
            container.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, potassium, container.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
        }
        fired = true;
    }

    public float GetHealth() {
        return 1f;
    }

    [PunRPC]
    public void Damage(float amount) {
        if (!fired) {
            Fire();
        } else {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void Heal(float amount) {
    }
}
