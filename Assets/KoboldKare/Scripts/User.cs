using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;
using UnityEngine.UI;

public delegate void SpriteEvent(Sprite sprite);

public class User : MonoBehaviour {
    private NetworkedKobold internalKobold;
    [SerializeField] private GameObject panel;
    [SerializeField] private Image useImage;
    public NetworkedKobold kobold {
        get {
            if (internalKobold == null) {
                internalKobold = GetComponentInParent<NetworkedKobold>();
            }
            return internalKobold;
        }
    }

    public Sprite unknownUsableSprite;
    private HashSet<Tuple<NetworkedEntity,GameObject>> possibleUsables = new HashSet<Tuple<NetworkedEntity,GameObject>>();
    private NetworkedEntity closestUsable = null;
    private CapsuleCollider capsuleCollider;

    private void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    public void LateUpdate() {
        var ownedKobold = kobold;
        var networkedKobold = kobold.GetComponentInParent<NetworkedKobold>();
        // FIXME FISHNET
        //if (!photonView.IsMine || (Kobold)PhotonNetwork.LocalPlayer.TagObject != ownedKobold) return;
        transform.rotation = OrbitCamera.GetPlayerIntendedRotation();
        
        var desiredPosition = OrbitCamera.GetCamera().transform.position + transform.forward * (capsuleCollider.height*0.5f);
        float distance = Vector3.Distance(ownedKobold.transform.position, desiredPosition);
        transform.position = Vector3.MoveTowards(desiredPosition, ownedKobold.transform.position, Mathf.Max(distance - networkedKobold.baseSize.Value*0.2f, 0f));
    }

    public IEnumerator WaitAndThenTrigger(UnityEvent e) {
        yield return new WaitForSeconds(1f);
        yield return new WaitForEndOfFrame();
        e.Invoke();
    }
    private void OnTriggerEnter(Collider other) {
        NetworkedEntity g = other.GetComponentInParent<NetworkedEntity>();
        if (g!=null) {
            possibleUsables.Add(new Tuple<NetworkedEntity, GameObject>(g, other.gameObject));
        }
    }
    private void OnTriggerStay(Collider other) {
        NetworkedEntity g = other.GetComponentInParent<NetworkedEntity>();
        if (g!=null) {
            possibleUsables.Add(new Tuple<NetworkedEntity, GameObject>(g, other.gameObject));
        }
    }
    void FixedUpdate() {
        var networkedKobold = kobold.GetComponentInParent<NetworkedKobold>();
        capsuleCollider.height = networkedKobold.baseSize.Value * 0.20f;
        SortGrabbables();
        possibleUsables.Clear();
    }

    void SortGrabbables() {
        possibleUsables.RemoveWhere(o=>o == null || ((Component)o.Item1) == null || o.Item2 == null || !o.Item2.activeInHierarchy);
        float distance = float.MaxValue;
        NetworkedEntity closest = null;
        foreach( Tuple<NetworkedEntity,GameObject> u in possibleUsables ) {
            if (!u.Item1.CanUse(kobold) || u.Item1.transform.root == transform.root) {
                continue;
            }
            float d = Vector3.Distance(u.Item1.transform.position, transform.position);
            if (closest == null || d < distance) {
                closest = u.Item1;
                distance = d;
            }
        }
        closestUsable = closest;
        if (closestUsable != null) {
            if (!panel.activeInHierarchy) {
                panel.SetActive(true);
            }

            if (closestUsable.GetSprite() == null) {
                useImage.sprite = unknownUsableSprite;
            } else {
                useImage.sprite = closestUsable.GetSprite();
            }
        } else {
            closestUsable = null;
            useImage.sprite = unknownUsableSprite;
            if (panel.activeInHierarchy) {
                panel.SetActive(false);
            }
        }
    }
    public void Use() {
        if (closestUsable != null) {
            //closestUsable.photonView.RPC("RPCUse", RpcTarget.All, new object[]{photonView.ViewID});
            closestUsable.TryUse();
        }
    }
}
