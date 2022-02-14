using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

public class GrinderManager : GenericUsable {
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;
    public AudioSource grindSound;
    public Animator animator;
    public Transform attachPoint;
    public AudioSource deny;
    public GenericReagentContainer container;
    private HashSet<GameObject> grindedThingsCache = new HashSet<GameObject>();
    private int usedCount;
    private bool on {
        get {
            return (usedCount % 2) != 0;
        }
    }
    public override Sprite GetSprite(Kobold k) {
        return on ? offSprite : onSprite;
    }
    public override bool CanUse(Kobold k) {
        return animator.isActiveAndEnabled;
    }
    [PunRPC]
    public override void Use() {
        usedCount++;
        if (on) {
            animator.SetTrigger("TurnOn");
            grindSound.Play();
        } else {
            grindSound.Pause();
            animator.SetTrigger("TurnOff");
        }
    }
    IEnumerator WaitAndThenClear() {
        yield return new WaitForSeconds(0.5f);
        grindedThingsCache.Clear();
    }
    void Grind(GameObject obj) {
        GenericGrabbable root = obj.GetComponentInParent<GenericGrabbable>();
        if (root == null) {
            return;
        }
        if ( grindedThingsCache.Contains(root.gameObject)) {
            return;
        }
        grindedThingsCache.Add(root.gameObject);
        foreach (GenericReagentContainer c in root.gameObject.GetComponentsInChildren<GenericReagentContainer>()) {
            container.TransferMix(c, c.volume, GenericReagentContainer.InjectType.Inject);
        }
        GenericDamagable d = obj.transform.root.GetComponent<GenericDamagable>();
        if (d != null) {
            d.Damage(d.health + 1);
        } else {
            PhotonView other = obj.GetComponentInParent<PhotonView>();
            if (other != null) {
                PhotonNetwork.Destroy(other.gameObject);
            } else {
                Destroy(root.gameObject);
            }
        }
        StopCoroutine("WaitAndThenClear");
        StartCoroutine("WaitAndThenClear");
    }
    private void HandleCollision(Collider other) {
        if (!on) {
            return;
        }
        if (other.isTrigger) {
            return;
        }
        GenericDamagable d = other.transform.root.GetComponent<GenericDamagable>();
        if (d != null && !d.removeOnDeath) {
            d.transform.position += Vector3.up * 1f;
            foreach (Rigidbody r in other.GetAllComponents<Rigidbody>()) {
                r?.AddExplosionForce(700f, transform.position+Vector3.down*5f, 100f);
            }
            if (!deny.isPlaying) {
                deny.Play();
            }
            d.Damage(d.health+1);
            return;
        }
        if ((other.GetComponentInParent<PhotonView>() != null && !other.GetComponentInParent<PhotonView>().IsMine)) {
            return;
        }
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        Grind(other.gameObject);
    }
    private void OnTriggerEnter(Collider other) {
        HandleCollision(other);
    }
    private void OnTriggerStay(Collider other) {
        HandleCollision(other);
    }
}
