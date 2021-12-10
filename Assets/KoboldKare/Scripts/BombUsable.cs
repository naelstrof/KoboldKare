using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(Photon.Pun.PhotonView)), RequireComponent(typeof(GenericReagentContainer))]
public class BombUsable : GenericUsable {
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
    public override void Use(Kobold k) {
        base.Use(k);
        if (!fired) {
            effect.gameObject.SetActive(true);
            animator.SetTrigger("Burn");
            if (photonView.IsMine) {
                // Mix the water with the potassium...
                // It should sizzle and blow up.
                container.AddMix(ReagentDatabase.GetReagent("Water"), 40f, GenericReagentContainer.InjectType.Inject);
            }
            fired = true;
        }
    }
}
