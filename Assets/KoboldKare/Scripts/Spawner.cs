using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour {
    public float chance = .5f;
    public float overrideCost;
    //public GameObject prefab;
    public List<PhotonGameObjectReference> possibleSpawns = new List<PhotonGameObjectReference>();
    public bool spawnOnLoad = false;
    public LayerMask obstacles;
    public bool enforceMaximum;
    public int maxSpawnLimitPerDay, spawnedCount;
    private GameObject lastSpawned;
    public AudioClip spawnSound, spawnMaxReached;
    public AudioSource audioSource;
    public AudioMixer targetAudioMixer;
    public PurchaseHint hint;

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
                        PhotonNetwork.Destroy(lastSpawned);
                    }
                    lastSpawned = null;
                }
            }
        }
        yield return new WaitForEndOfFrame();
        if (Random.Range(0f, 1f) < chance) {
            if(enforceMaximum){
                if(spawnedCount < maxSpawnLimitPerDay){
                    string randomPrefab = GetRandomPrefab();
                    lastSpawned = PhotonNetwork.Instantiate(randomPrefab, transform.position, transform.rotation);
                    spawnedCount = Mathf.Clamp(spawnedCount+1, 0, maxSpawnLimitPerDay);

                    if (lastSpawned.GetComponent<Kobold>() != null) {
                        lastSpawned.GetComponent<Kobold>().RandomizeKobold();
                    }
                }
            }
            else{
                string randomPrefab = GetRandomPrefab();
                lastSpawned = PhotonNetwork.Instantiate(randomPrefab, transform.position, transform.rotation);
                
                if (lastSpawned.GetComponent<Kobold>() != null) {
                    lastSpawned.GetComponent<Kobold>().RandomizeKobold();
                }
            }

            //lastSpawned = GameObject.Instantiate(prefab, transform.position, transform.rotation);
            
        }
    }
    public void Spawn() {
        StopAllCoroutines();
        StartCoroutine(WaitAndThenSpawn());
        PlaySFX();
    }
    public void InstantSpawn() {
        PhotonView view = GetComponentInParent<PhotonView>();
        if (!((view != null && view.IsMine) || (view == null && PhotonNetwork.IsMasterClient) || PhotonNetwork.OfflineMode)) {
            return;
        }
        //lastSpawned = GameObject.Instantiate(prefab, transform.position, transform.rotation, null);
        string randomPrefab = GetRandomPrefab();
        lastSpawned = PhotonNetwork.Instantiate(randomPrefab, transform.position, transform.rotation);
        if (lastSpawned.GetComponent<Kobold>() != null) {
            lastSpawned.GetComponent<Kobold>().RandomizeKobold();
        }
    }
    public void Start() {
        if (possibleSpawns.Count <= 0) {
            Debug.LogWarning("Spawner without anything to spawn...", gameObject);
        }
        if (spawnOnLoad) {
            Spawn();
        }
        if(hint != null){
            if(overrideCost != 0){ hint.ChangeText(overrideCost.ToString()); }
        }
        audioSource.outputAudioMixerGroup = targetAudioMixer.FindMatchingGroups("SoundEffects")[0];
    }
    private void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "ico_spawn.png", true);
    }

    void PlaySFX(){
        if(spawnedCount < maxSpawnLimitPerDay){
            if(spawnSound != null)audioSource.PlayOneShot(spawnSound);
        }
        else
            if(spawnMaxReached != null) audioSource.PlayOneShot(spawnMaxReached);
    }

    public void OnValidate() {
#if UNITY_EDITOR
        foreach (var photonGameObject in possibleSpawns) {
            photonGameObject.OnValidate();
        }
#endif
    }

    public void ResetCount(){
        spawnedCount = 0;
    }

    public bool CanSpawn(){
        return spawnedCount < maxSpawnLimitPerDay;
    }
}
