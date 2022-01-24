using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class ConstructionContract : GenericUsable {
    [SerializeField]
    private Sprite displaySprite;
    [SerializeField]
    private UnityEvent purchased;
    [SerializeField]
    private ScriptableFloat money;
    [SerializeField]
    private float cost;
    [SerializeField]
    private MoneyFloater floater;

    [SerializeField]
    public PhotonView photonView;

    void Start() {
        Bounds bound = new Bounds(transform.position, Vector3.one);
        foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
            bound.Encapsulate(r.bounds);
        }
        floater.SetBounds(bound);
        floater.SetText(cost.ToString());
    }
    public override Sprite GetSprite(Kobold k) {
        return displaySprite;
    }
    public override bool CanUse(Kobold k) {
        return money.has(cost);
    }
    [PunRPC]
    public override void Use() {
        base.Use();
        money.charge(cost);
        purchased.Invoke();
        gameObject.SetActive(false);
    }
}
