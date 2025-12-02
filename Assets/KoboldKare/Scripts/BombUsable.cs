using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
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
    
    // FIXME FISHNET
    //[PunRPC]
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
        // FIXME FISHNET
        /*
        if (photonView.IsMine) {
            // Mix the water with the potassium...
            // It should sizzle and blow up.
            ReagentContents water = new ReagentContents();
            if (ReagentDatabase.TryGetAsset("Water", out var waterReagent)) {
                water.AddMix(waterReagent.GetReagent(20f));
            }

            BitBuffer bufferOne = new BitBuffer(4);
            bufferOne.AddReagentContents(water);
            
            ReagentContents potassium = new ReagentContents();
            if (ReagentDatabase.TryGetAsset("Potassium", out var potassiumReagent)) {
                potassium.AddMix(potassiumReagent.GetReagent(20f));
            }
            BitBuffer bufferTwo = new BitBuffer(4);
            bufferTwo.AddReagentContents(potassium);
            
            container.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, bufferOne, container.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
            container.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, bufferTwo, container.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
        }*/
        fired = true;
    }

    public float GetHealth() {
        return 1f;
    }

    // FIXME FISHNET
    //[PunRPC]
    public void Damage(float amount) {
        if (!fired) {
            Fire();
        } else {
            // FIXME FISHNET
            //PhotonNetwork.Destroy(gameObject);
        }
        PhotonProfiler.LogReceive(sizeof(float));
    }

    public void Heal(float amount) {
    }
}
