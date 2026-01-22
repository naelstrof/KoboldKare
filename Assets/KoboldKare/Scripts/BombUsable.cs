using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(Photon.Pun.PhotonView)), RequireComponent(typeof(GenericReagentContainer))]
public class BombUsable : MonoBehaviour, IDamagable {
    [SerializeField]
    private Sprite bombSprite;
    private bool fired = false;
    private Animator animator; 
    [SerializeField]
    private VisualEffect effect;
    private GenericReagentContainer container;
    private NetworkedEntity networkEntity;
    void Awake() {
        container = GetComponent<GenericReagentContainer>();
        animator = GetComponent<Animator>();
    }

    void Start() {
        networkEntity = GetComponentInParent<NetworkedEntity>();
        if (networkEntity != null) {
            networkEntity.SetSprite(bombSprite);
            networkEntity.useRequested += OnUseRequested;
            networkEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return !fired;
    }

    // FIXME FISHNET
    //[PunRPC]
    private void OnUse(NetworkedKobold by) {
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
    }

    public void Heal(float amount) {
    }
}
