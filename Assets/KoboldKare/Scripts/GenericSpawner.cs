using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;
using Random = UnityEngine.Random;

public class GenericSpawner : MonoBehaviourPun {
    [SerializeField]
    private List<PhotonGameObjectReference> possibleSpawns = new List<PhotonGameObjectReference>();
    [SerializeField]
    private bool spawnOnLoad;
    private GameObject lastSpawned;
    private WaitUntil waitUntilCanSpawn;
    [SerializeField]
    private float minRespawnTime = 60f;
    [SerializeField]
    private float maxRespawnTime = 360f;
    private string GetRandomPrefab() {
        return possibleSpawns[Random.Range(0, possibleSpawns.Count)].photonName;
    }
    public virtual void Spawn() {
        if (!PhotonNetwork.IsMasterClient) {
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
        yield return waitUntilCanSpawn;
        string randomPrefab = GetRandomPrefab();
        lastSpawned = PhotonNetwork.InstantiateRoomObject(randomPrefab, transform.position, transform.rotation, 0, new object[] { new KoboldGenes().Randomize(), false });
    }

    private void OnEnable() {
        StartCoroutine(SpawnOccasionallyRoutine());
    }

    public virtual void Start() {
        waitUntilCanSpawn = new WaitUntil(CanSpawn);
        if (possibleSpawns.Count <= 0) {
            Debug.LogWarning("Spawner without anything to spawn...", gameObject);
        }
        if (spawnOnLoad) {
            Spawn();
        }
    }

    IEnumerator SpawnOccasionallyRoutine() {
        while (isActiveAndEnabled) {
            yield return new WaitForSeconds(Random.Range(minRespawnTime, maxRespawnTime));
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
