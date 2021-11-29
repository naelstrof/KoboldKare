using KoboldKare;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedExtractor : MonoBehaviourPun {
    [System.Serializable]
    public class ReagentPrefabTuple {
        public ScriptableReagent reagentType;
        public PhotonGameObjectReference prefab;
        public float neededVolume;
    }
    public GenericUsable theExtractor;
    public AudioSource deny;
    public AudioSource done;
    public Animator grinderAnimator;
    public Task task;
    public Transform seedSpawnLocation;
    public List<ReagentPrefabTuple> serializedSpawnables = new List<ReagentPrefabTuple>();
    public GenericReagentContainer internalContents;

    private Dictionary<ScriptableReagent, ReagentPrefabTuple> spawnableLookup = new Dictionary<ScriptableReagent, ReagentPrefabTuple>();
    private void Awake() {
        foreach (var tuple in serializedSpawnables) {
            spawnableLookup.Add(tuple.reagentType, tuple);
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (other.isTrigger) {
            return;
        }
        if ((other.GetComponentInParent<PhotonView>() != null && !other.GetComponentInParent<PhotonView>().IsMine)) {
            return;
        }
        photonView.RequestOwnership();
        GenericDamagable damagable = other.GetComponentInParent<GenericDamagable>();
        if (damagable != null && damagable.removeOnDeath) {
            bool foundThing = false;
            foreach( GenericReagentContainer container in other.transform.root.GetComponentsInChildren<GenericReagentContainer>()) {
                internalContents.TransferMix(container, container.volume, GenericReagentContainer.InjectType.Inject);
                foundThing = true;
            }
            if (foundThing) {
                if (task != null) {
                    task.Stop();
                }
                task = new Task(OutputSeeds());
            }
        }
        if (damagable != null && !damagable.removeOnDeath) {
            //damagable.transform.position += Vector3.up * 1f;
            foreach (Rigidbody r in other.GetAllComponents<Rigidbody>()) {
                r?.AddExplosionForce(700f, transform.position+Vector3.down*5f, 100f);
            }
            if (!deny.isPlaying) {
                deny.Play();
            }
        }
        if (damagable) {
            damagable.Damage(damagable.maxHealth+1);
        }
    }
    private IEnumerator OutputSeeds() {
        yield return new WaitForSeconds(2f);
        foreach(var pair in spawnableLookup) {
            while (internalContents.GetVolumeOf(pair.Key)>= pair.Value.neededVolume) {
                internalContents.OverrideReagent(pair.Key, internalContents.GetVolumeOf(pair.Key));
                SaveManager.Instantiate(spawnableLookup[pair.Key].prefab.photonName, seedSpawnLocation.position, seedSpawnLocation.rotation);
                yield return new WaitForSeconds(2f);
            }
        }
        done.Play();
        grinderAnimator.SetTrigger("Open");
        gameObject.SetActive(false);
    }

    private void OnValidate() {
        foreach(var spawnable in serializedSpawnables) {
            spawnable.prefab.OnValidate();
        }
    }
}
