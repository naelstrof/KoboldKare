using KoboldKare;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedExtractor : GenericUsable {
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;
    [System.Serializable]
    public class ReagentPrefabTuple {
        public ScriptableReagent reagentType;
        public PhotonGameObjectReference prefab;
        public float neededVolume;
    }

    public AudioSource source;
    public AudioPack deny;
    public Animator grinderAnimator;
    public Transform seedSpawnLocation;
    public List<ReagentPrefabTuple> serializedSpawnables = new List<ReagentPrefabTuple>();
    public GenericReagentContainer internalContents;
    public Coroutine routine;
    void Start() {
        grinderAnimator.SetBool("Open", true);
    }

    private bool on {
        get {
            return (usedCount % 2) != 0;
        }
    }
    public override Sprite GetSprite(Kobold k) {
        return on ? onSprite : offSprite;
    }
    public override void Use(Kobold k) {
        base.Use(k);
        if (on) {
            source.Play();
            if (routine != null) {
                StopCoroutine(routine);
            }
            grinderAnimator.SetBool("Open", false);
            routine = StartCoroutine(OutputSeeds());
        } else {
            if (routine != null) {
                StopCoroutine(routine);
            }
            source.Stop();
            grinderAnimator.SetBool("Open", true);
        }
    }

    private Dictionary<ScriptableReagent, ReagentPrefabTuple> spawnableLookup = new Dictionary<ScriptableReagent, ReagentPrefabTuple>();
    private void Awake() {
        foreach (var tuple in serializedSpawnables) {
            spawnableLookup.Add(tuple.reagentType, tuple);
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (!on) {
            return;
        }
        if (other.isTrigger) {
            return;
        }
        if ((other.GetComponentInParent<PhotonView>() != null && !other.GetComponentInParent<PhotonView>().IsMine)) {
            return;
        }
        photonView.RequestOwnership();
        GenericDamagable damagable = other.GetComponentInParent<GenericDamagable>();
        if (damagable != null && damagable.removeOnDeath) {
            foreach( GenericReagentContainer container in other.transform.root.GetComponentsInChildren<GenericReagentContainer>()) {
                internalContents.TransferMix(container, container.volume, GenericReagentContainer.InjectType.Inject);
            }
        }
        if (damagable != null && !damagable.removeOnDeath) {
            //damagable.transform.position += Vector3.up * 1f;
            foreach (Rigidbody r in other.GetAllComponents<Rigidbody>()) {
                r?.AddExplosionForce(700f, transform.position+Vector3.down*5f, 100f);
            }
            source.PlayOneShot(deny.GetRandomClip(), deny.volume);
        }
        if (damagable) {
            damagable.Damage(damagable.maxHealth+1);
        }
    }
    private IEnumerator OutputSeeds() {
        while(true) {
            yield return new WaitForSeconds(2f);
            foreach(var pair in spawnableLookup) {
                while (internalContents.GetVolumeOf(pair.Key) >= pair.Value.neededVolume) {
                    internalContents.OverrideReagent(pair.Key, internalContents.GetVolumeOf(pair.Key)-pair.Value.neededVolume);
                    PhotonNetwork.Instantiate(spawnableLookup[pair.Key].prefab.photonName, seedSpawnLocation.position, seedSpawnLocation.rotation);
                    yield return new WaitForSeconds(2f);
                }
            }
        }
    }

    private void OnValidate() {
        foreach(var spawnable in serializedSpawnables) {
            spawnable.prefab.OnValidate();
        }
    }
}
