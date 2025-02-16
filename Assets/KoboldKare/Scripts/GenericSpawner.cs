using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;
using Photon.Realtime;
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
        if (lastSpawned != null && lastSpawned.transform.DistanceTo(transform) < 1f) {
            return;
        }
        StartCoroutine(SpawnRoutine());
    }
    public virtual bool CanSpawn() {
        return PhotonNetwork.IsMasterClient;
    }
    public virtual IEnumerator SpawnRoutine() {
        yield return waitUntilCanSpawn;
        string randomPrefab = GetRandomPrefab();
        lastSpawned = PhotonNetwork.InstantiateRoomObject(randomPrefab, transform.position, transform.rotation);
    }

    private void OnEnable() {
        StartCoroutine(SpawnOccasionallyRoutine());
    }

    private void Awake() {
        waitUntilCanSpawn = new WaitUntil(CanSpawn);
    }

    public virtual void Start() {
        if (possibleSpawns.Count <= 0) {
            Debug.LogWarning("Spawner without anything to spawn...", gameObject);
        }
        if (spawnOnLoad) {
            Spawn();
        }
    }

    private double lastAttempt;
    private void Update() {
        if (Time.timeAsDouble > lastAttempt) {
            lastAttempt = Time.timeAsDouble+10f;
            // Attempt to give it back to the master!! Not sure how this happens, apparently a disconnect randomly assigns owners..
            if (photonView != null && !Equals(photonView.Owner, PhotonNetwork.MasterClient) && Equals(photonView.Owner, PhotonNetwork.LocalPlayer)) {
                photonView.TransferOwnership(PhotonNetwork.MasterClient);
            }
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
