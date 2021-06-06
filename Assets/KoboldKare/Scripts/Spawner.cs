using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour {
    public float chance = .5f;
    //public GameObject prefab;
    public List<PhotonGameObjectReference> possibleSpawns = new List<PhotonGameObjectReference>();
    public bool spawnOnLoad = false;
    public LayerMask obstacles;
    private GameObject lastSpawned;
    private string GetRandomPrefab() {
        return possibleSpawns[Random.Range(0, possibleSpawns.Count)].photonName;
    }
    public IEnumerator WaitAndThenSpawn() {
        float timeOut = Time.timeSinceLevelLoad+3f;
        yield return new WaitUntil(() => PhotonNetwork.IsConnected || Time.timeSinceLevelLoad>timeOut);
        PhotonView view = GetComponentInParent<PhotonView>();
        if (!((view != null && view.IsMine) || (view == null && PhotonNetwork.IsMasterClient) || PhotonNetwork.OfflineMode)) {
            yield break;
        }
        if (lastSpawned != null && lastSpawned.transform != null) {
            foreach (Collider c in Physics.OverlapSphere(transform.position, 0.5f, obstacles, QueryTriggerInteraction.Collide)) {
                if (c == null || c.transform == null || c.transform.root == null) {
                    continue;
                }
                if (c.transform.root != null && (lastSpawned != null && lastSpawned.transform != null && c.transform.root == lastSpawned.transform.root)) {
                    if (lastSpawned.GetComponentInParent<PhotonView>().IsMine) {
                        SaveManager.Destroy(lastSpawned);
                    }
                    lastSpawned = null;
                }
            }
        }
        yield return new WaitForEndOfFrame();
        if (Random.Range(0f, 1f) < chance) {
            //lastSpawned = GameObject.Instantiate(prefab, transform.position, transform.rotation);
            string randomPrefab = GetRandomPrefab();
            if (randomPrefab != "GrabbableKobold4") {
                lastSpawned = SaveManager.Instantiate(randomPrefab, transform.position, transform.rotation);
            } else {
                lastSpawned = SaveManager.Instantiate(randomPrefab, transform.position, transform.rotation, 0, new object[] { PlayerKoboldLoader.GetRandomKobold() });
            }
        }
    }
    public void Spawn() {
        StopAllCoroutines();
        StartCoroutine(WaitAndThenSpawn());
    }
    public void InstantSpawn() {
        PhotonView view = GetComponentInParent<PhotonView>();
        if (!((view != null && view.IsMine) || (view == null && PhotonNetwork.IsMasterClient) || PhotonNetwork.OfflineMode)) {
            return;
        }
        //lastSpawned = GameObject.Instantiate(prefab, transform.position, transform.rotation, null);
        string randomPrefab = GetRandomPrefab();
        if (randomPrefab != "GrabbableKobold4") {
            lastSpawned = SaveManager.Instantiate(randomPrefab, transform.position, transform.rotation);
        } else {
            lastSpawned = SaveManager.Instantiate(randomPrefab, transform.position, transform.rotation, 0, new object[] { PlayerKoboldLoader.GetRandomKobold() });
        }
    }
    public void Start() {
        if (possibleSpawns.Count <= 0) {
            Debug.LogWarning("Spawner without anything to spawn...", gameObject);
        }
        if (spawnOnLoad) {
            Spawn();
        }
    }
    private void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ico_spawn.png", true);
    }

    public void OnValidate() {
#if UNITY_EDITOR
        foreach (var photonGameObject in possibleSpawns) {
            photonGameObject.OnValidate();
        }
#endif
    }
}
