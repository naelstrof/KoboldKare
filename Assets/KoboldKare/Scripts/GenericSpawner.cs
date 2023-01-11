using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;
using SimpleJSON;
using Random = UnityEngine.Random;

public class GenericSpawner : MonoBehaviourPun, IPunObservable, ISavable {
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
        lastSpawned = PhotonNetwork.InstantiateRoomObject(randomPrefab, transform.position, transform.rotation);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            if (lastSpawned != null) {
                stream.SendNext(lastSpawned.GetPhotonView().ViewID);
            } else {
                stream.SendNext(-1);
            }
        } else {
            int id = (int)stream.ReceiveNext();
            if (id != -1) {
                PhotonView view = PhotonNetwork.GetPhotonView(id);
                lastSpawned = view == null ? null : view.gameObject;
            } else {
                lastSpawned = null;
            }
        }
    }

    public void Save(JSONNode node) {
        if (lastSpawned != null) {
            node["lastSpawned"] = lastSpawned.GetPhotonView().ViewID;
        } else {
            node["lastSpawned"] = -1;
        }
    }

    public void Load(JSONNode node) {
        if (node.HasKey("lastSpawned")) {
            int id = node["lastSpawned"];
            if (id != -1) {
                PhotonView view = PhotonNetwork.GetPhotonView(id);
                lastSpawned = view == null ? null : view.gameObject;
            } else {
                lastSpawned = null;
            }
        }
    }
}
