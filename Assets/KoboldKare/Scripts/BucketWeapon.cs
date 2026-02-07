using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using KoboldKare;
using NetStack.Quantization;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class BucketWeapon : GenericWeapon {
    public delegate void FoodCreateAction(BucketWeapon bucket, ScriptableReagent food);

    public static event FoodCreateAction foodCreated;
    [SerializeField]
    private PhotonGameObjectReference bucketSplashProjectile;
    [SerializeField]
    private Animator bucketAnimator;
    [SerializeField]
    private GameObject defaultBucketDisplay;

    [SerializeField]
    private Rigidbody body;

    private static readonly int Fire = Animator.StringToHash("Fire");
    private NetworkedKobold playerFired;

    [SerializeField] private AudioPack bucketSlosh;
    private AudioSource audioSource;

    private WaitForSeconds waitForSeconds;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float projectileVolume = 10f;
    [SerializeField] private float projectileSpeed = 10f;

    private GameObject currentDisplay;

    protected override void Start() {
        base.Start();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.maxDistance = 20f;
            audioSource.minDistance = 0.2f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialBlend = 1f;
            audioSource.loop = false;
        }

        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange += OnReagentsChanged;
            OnReagentsChanged(networkedEntity.reagentContents.Value, networkedEntity.reagentContents.Value, true);
        }
        
        audioSource.enabled = false;
        waitForSeconds = new WaitForSeconds(5f);
        defaultBucketDisplay.SetActive(true);
    }

    protected override void OnDestroy() {
        if (networkedEntity != null) {
            networkedEntity.reagentContents.OnChange -= OnReagentsChanged;
        }
    }

    void OnReagentsChanged(ReagentContents contents, ReagentContents next, bool asServer) {
        GameObject bestDisplay = null;
        float bestVolume = 0f;
        byte bestID = 0;
        foreach (var reagent in contents) {
            if (!ReagentDatabase.TryGetAsset(reagent.id, out var match) || !match.GetDisplayPrefab()) {
                continue;
            }
            if (reagent.volume < 5f) {
                continue;
            }
            if (reagent.volume > bestVolume) {
                bestDisplay = match.GetDisplayPrefab();
                bestVolume = reagent.volume;
                bestID = reagent.id;
            }
        }

        if ((!bestDisplay && currentDisplay) || (currentDisplay && bestDisplay && !currentDisplay.name.Contains(bestDisplay.name))) {
            Destroy(currentDisplay);
            currentDisplay = null;
            defaultBucketDisplay.SetActive(true);
        }

        if (bestDisplay && !currentDisplay) {
            if (ReagentDatabase.TryGetAsset(bestID, out var match)) {
                foodCreated?.Invoke(this, match);
            }
            currentDisplay = Instantiate(bestDisplay, transform);
            defaultBucketDisplay.SetActive(false);
        }
    }

    protected override void OnFire(NetworkedKobold kobold) {
        playerFired = kobold;
        bucketAnimator.SetTrigger(Fire);
    }

    protected override void OnEndFire(NetworkedKobold player) {
        if (networkedEntity.volume < 0.1f) {
            return;
        }
        if (!player.TryGetKobold(out var kobold)) {
            return;
        }
        
        for (int i = 0; i < projectileCount; i++) {
            Vector3 velocity = GetWeaponBarrelTransform().forward * projectileSpeed;
            velocity += kobold.body.velocity * 0.5f;
            velocity += Random.insideUnitSphere * i * 2f;

            KoboldEntitySpawner.NetworkedEntityInstantiationData data = KoboldEntitySpawner.NetworkedEntityInstantiationData.Default();
            data.assetName = "FluidProjectile";
            data.groupName = "NetworkedPrefab";
            data.velocity = velocity;
            data.position = GetWeaponBarrelTransform().position;
            data.rotation = GetWeaponBarrelTransform().rotation;
            data.CopyGenesFrom(networkedEntity);

            var spilled = networkedEntity.Spill(projectileVolume);
            data.reagentContents = spilled;

            var networkObject = InstanceFinder.NetworkManager.GetComponent<KoboldEntitySpawner>().SpawnAsServer(data, false);
            //networkObject.GetComponentInChildren<Projectile>().LaunchFrom(body);
        }

        audioSource.enabled = true;
        bucketSlosh.Play(audioSource);
    }

    IEnumerator WaitSomeTimeThenDisableAudio() {
        yield return waitForSeconds;
        audioSource.enabled = false;
    }
}
