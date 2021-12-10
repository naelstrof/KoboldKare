using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;

public class GenericSpawner : MonoBehaviourPun {
    [SerializeField]
    private List<PhotonGameObjectReference> possibleSpawns = new List<PhotonGameObjectReference>();
    [SerializeField]
    private bool spawnOnLoad;
    private GameObject lastSpawned;
    private WaitUntil waitUntil;
    private string GetRandomPrefab() {
        return possibleSpawns[Random.Range(0, possibleSpawns.Count)].photonName;
    }
    public virtual void Spawn() {
        if (!photonView.IsMine) {
            return;
        }
        if (lastSpawned != null && lastSpawned.transform.DistanceTo(transform) < 1f) {
            return;
        }
        StartCoroutine(SpawnRoutine());
    }
    public virtual bool CanSpawn() {
        return PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady;
    }
    public virtual IEnumerator SpawnRoutine() {
        yield return waitUntil;
        string randomPrefab = GetRandomPrefab();
        lastSpawned = PhotonNetwork.Instantiate(randomPrefab, transform.position, transform.rotation);
        if (lastSpawned.GetComponent<Kobold>() != null) {
            lastSpawned.GetComponent<Kobold>().RandomizeKobold();
        }
    }
    public virtual void Start() {
        waitUntil = new WaitUntil(CanSpawn);
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
        foreach (var photonGameObject in possibleSpawns) {
            photonGameObject.OnValidate();
        }
    }
}
