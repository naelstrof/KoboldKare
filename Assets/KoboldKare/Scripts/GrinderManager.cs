using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using ExitGames.Client.Photon;

public class GrinderManager : MonoBehaviourPun {
    public GameObject clearDebrisButton;
    public GameObject debris;
    public Transform attachPoint;
    public AudioSource deny;
    public StreamRenderer stream;
    public GenericReagentContainer container;
    private HashSet<GameObject> grindedThingsCache = new HashSet<GameObject>();
    public bool on { get; set; }
    public IEnumerator WaitAndThenClear() {
        yield return new WaitForSeconds(0.5f);
        grindedThingsCache.Clear();
    }
    void Grind(GameObject obj) {
        if ( grindedThingsCache.Contains(obj.transform.root.gameObject)) {
            return;
        }
        if (obj.transform.root == this.transform.root) { return; }
        photonView.RequestOwnership();
        grindedThingsCache.Add(obj.transform.root.gameObject);
        foreach (GenericReagentContainer c in obj.GetAllComponents<GenericReagentContainer>()) {
            container.contents.Mix(c.contents);
        }
        GenericDamagable d = obj.transform.root.GetComponent<GenericDamagable>();
        if (d != null) {
            d.Damage(d.health + 1);
        } else {
            PhotonView other = obj.GetComponentInParent<PhotonView>();
            if (other != null) {
                SaveManager.Destroy(other.gameObject);
            } else {
                Destroy(obj.transform.root);
            }
        }
        StopCoroutine("WaitAndThenClear");
        StartCoroutine("WaitAndThenClear");
        stream.Fire(container);
    }
    private void HandleCollision(Collider other) {
        if (!on || debris.activeInHierarchy ) {
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
        if ( debris.activeInHierarchy != true) {
            Grind(other.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other) {
        HandleCollision(other);
    }
    private void OnTriggerStay(Collider other) {
        HandleCollision(other);
    }
}
